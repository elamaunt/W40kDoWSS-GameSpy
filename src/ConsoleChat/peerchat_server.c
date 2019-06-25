/*
    Copyright 2005-2011 Luigi Auriemma

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA

    http://www.gnu.org/licenses/gpl-2.0.txt
*/

#define _CRT_SECURE_NO_WARNINGS

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include "gs_peerchat.h"
#include "pch.h"

#ifdef WIN32
    #include <winsock.h>
    #include "winerr.h"

    #define close   closesocket
    #define sleep   Sleep
    #define ONESEC  1000
#else
    #include <unistd.h>
    #include <sys/socket.h>
    #include <sys/types.h>
    #include <arpa/inet.h>
    #include <netinet/in.h>
    #include <netdb.h>
    #include <pthread.h>

    #define ONESEC  1
#endif

#ifdef WIN32
    #define quick_thread(NAME, ARG) DWORD WINAPI NAME(ARG)
    #define thread_id   DWORD
#else
    #define quick_thread(NAME, ARG) void *NAME(ARG)
    #define thread_id   pthread_t
#endif

thread_id quick_threadx(void *func, void *data) {
    thread_id       tid;
#ifdef WIN32
    if(!CreateThread(NULL, 0, func, data, 0, &tid)) return(0);
#else
    pthread_attr_t  attr;
    if(pthread_attr_init(&attr)) return(0);
    pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED);
    pthread_attr_setstacksize(&attr, 1<<18); //PTHREAD_STACK_MIN);
    if(pthread_create(&tid, &attr, func, data)) return(0);
#endif
    return(tid);
}

typedef uint8_t     u8;
typedef uint16_t    u16;
typedef uint32_t    u32;



#define VER         "0.1.3b"
#define BUFFSZ      1024
#define PORT        6667
#define CHALL       "0000000000000000"
#define KEYS        ":s 705 * "CHALL" "CHALL"\r\n"
#define USRIP       ":s 302  :=+@0.0.0.0\r\n"
#define LOGIN       ":s 707 * 12345678 87654321\r\n"
#define LOCALHOST   "127.0.0.1"
#define GSLIST      "E:\\Coding\\Projects\\SoulstormServer\\src\\Debug\\gslist.cfg"
#define GSLISTSZ    256 // was 80
#define CNAMEOFF    54
#define CKEYOFF     73
#define MAXTIMEOUT  300

#define LOGIT       if(debug) fwrite(buff, len, 1, stdout);



int bind_job(u32 iface, u16 port);
quick_thread(peerchat_client, int sock);
int get_key(u8 *gamename, u8 *gamekey, int gamekeysz);
int recv_tcp(int sock, u8 *data, int len);
int recv_tcp_dec(int sock, u8 *data, int len, gs_peerchat_ctx *decrypt);
int timeout(int sock);
u32 resolv(char *host);
void std_err(void);



struct  sockaddr_in irc;
//#ifdef DEBUG
//int     debug   = 1;
//#else
int     debug   = 0;
//#endif



int main(int argc, char *argv[])
{
    struct  sockaddr_in peer;
    int     i,
            sdl,
            sda,
            psz;
    u16     port;
	u8      *host = LOCALHOST;//"irc.azzurra.org";//LOCALHOST;

#ifdef WIN32
    WSADATA    wsadata;
    WSAStartup(MAKEWORD(1,0), &wsadata);
#endif

    setbuf(stdout, NULL);

	argc = 3;
	char* args[3] = { "6667", "irc.azzurra.org","-v" };
	argv = args;

    fputs("\n"
        "GS peerchat server emulator "VER"\n"
        "by Luigi Auriemma\n"
        "e-mail: aluigi@autistici.org\n"
        "web:    aluigi.org\n"
        "\n", stdout);

    if(argc < 2) {
        printf("\n"
            "Usage: %s <IRC_port> [IRC_server(%s)] [-v]\n"
            "\n"
            " You need a normal IRC server for using this proxy tool\n"
            " The port %d is already used by this program so if your IRC server runs on\n"
            " this same computer you need to assign a different port to it (like 6668)\n"
            " You need also to have "GSLIST" in this same folder:\n"
            "   http://aluigi.org/papers/"GSLIST"\n"
            "\n", argv[0], host, PORT);
        exit(1);
    }

	/*for (i = 2; i < argc; i++)
	{
		if (argv[i][0] == '-')
		{
			if (argv[i][1] == 'v') debug = 1;
		}
		else {
			host = argv[i];
		}
	}*/

	//host = "irc.azzurra.org";
	debug = 1;
	//port = 6667;
	port = 7777;

    //if(argc > 2) host = argv[2];
    //port = atoi(argv[1]);
    get_key(NULL, NULL, 0);

    printf("- verbose mode: %s\n", debug ? "on" : "off");

    printf("- Resolv hostname %s ... ", host);
    irc.sin_addr.s_addr  = resolv(host);
    irc.sin_port         = htons(port);
    irc.sin_family       = AF_INET;
    printf("ok\n");

    if((irc.sin_addr.s_addr == inet_addr(LOCALHOST)) && (ntohs(irc.sin_port) == PORT))
	{
        printf("\n"
            "Error: you cannot connect to this same program!\n"
            "       Your local IRC server must run on a different port\n"
            "\n");

        exit(1);
    }

    printf("\n"
        "IRC server  %s:%hu\n"
        "local port  %hu\n"
        "\n",
        inet_ntoa(irc.sin_addr), port,
        PORT);

    sdl = bind_job(INADDR_ANY, PORT);
    printf("- wait connections:\n");

    for(;;) {
        psz = sizeof(struct sockaddr_in);
        sda = accept(sdl, (struct sockaddr *)&peer, &psz);
        if(sda < 0) {
            printf("- accept() failed, continue within one second\n");
            close(sdl);
            sleep(ONESEC);
            sdl = bind_job(INADDR_ANY, PORT);
            continue;
        }

        printf("  %s:%hu\n", inet_ntoa(peer.sin_addr), ntohs(peer.sin_port));

        if(!quick_threadx(peerchat_client, (void *)sda)) close(sda);
    }

    close(sdl);
    return(0);
}



int bind_job(u32 iface, u16 port) 
{
    struct  sockaddr_in peerx;
    int     sdl,
            on = 1;

    peerx.sin_addr.s_addr = iface;
    peerx.sin_port        = htons(port);
    peerx.sin_family      = AF_INET;

    sdl = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

    if(sdl < 0)
		std_err();

    if(setsockopt(sdl, SOL_SOCKET, SO_REUSEADDR, (char *)&on, sizeof(on)) < 0)
		std_err();

    if(bind(sdl, (struct sockaddr *)&peerx, sizeof(struct sockaddr_in)) < 0)
		std_err();

    listen(sdl, SOMAXCONN);
    return(sdl);
}



quick_thread(peerchat_client, int sock) 
{
    gs_peerchat_ctx client,
                    server;
    struct  timeval tout;
    fd_set  readset;
    int     sd       = -1,
            len,
            selectsock;
    u8      *buff    = NULL,
            gamekey[16],
            *p,
            *l;

    sd = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

    if(sd < 0)
		goto give_up;

    if(connect(sd, (struct sockaddr *)&irc, sizeof(struct sockaddr_in)) < 0)
	{
        printf("- no free sockets or the destination server is down\n");
        goto give_up;
    }

    buff = malloc(BUFFSZ + 1);

    if(!buff) 
		goto give_up;

    //gamekey = buff;

    len = recv_tcp(sock, buff, BUFFSZ);

    if(len <= 0)
		goto give_up;

    LOGIT

    memset(&server, 0, sizeof(server)); // any common non-crypted IRC client is welcome, just like a proxy!
    memset(&client, 0, sizeof(client));

    p = buff;
    l = strchr(p, ' ');

    if(l && !memcmp(p, "CRYPT", l - p))
	{   
		// CRYPT
        p = l + 1;
        l = strchr(p, ' ');     // des

        if(!l || memcmp(p, "des", l - p)) 
		{
            printf("  Error: no des field received\n");
            goto give_up;
        }

        p = l + 1;
        l = strchr(p, ' ');     // type?

        if(!l)
		{
            printf("  Error: no type received\n");
            goto give_up;
        }

        for(p = ++l; *l > '\r'; l++);
        *l = 0;

        printf("  gamename: %s   ", p);

        if(!get_key(p, gamekey, sizeof(gamekey)))
		{
            printf("  Error: no gamekey found for this game\n");
            goto give_up;
        }

        printf("gamekey: %s\n", gamekey);

        if(send(sock, KEYS, sizeof(KEYS) - 1, 0) <= 0) 
			goto give_up;

        gs_peerchat_init(&client, CHALL, gamekey);
        gs_peerchat_init(&server, CHALL, gamekey);

        // if(recv_tcp(sd, buff, BUFFSZ) <= 0) goto give_up;   // skip server NOTICE

        for(;;)
		{
            len = recv_tcp_dec(sock, buff, BUFFSZ, &client);    // initial Peerchat commands

            if(len <= 0) 
				goto give_up;                          // ... lame solution
            LOGIT

            if(!memcmp(buff, "USRIP", 5))
			{                     // USRIP
                memcpy(buff, USRIP, sizeof(USRIP) - 1);

                gs_peerchat(&server, buff, sizeof(USRIP) - 1);
				
                if(send(sock, buff, sizeof(USRIP) - 1, 0) <= 0) 
					goto give_up;

            } 
			else if(!memcmp(buff, "LOGIN", 5))
			{              // LOGIN
                memcpy(buff, LOGIN, sizeof(LOGIN) - 1);

                gs_peerchat(&server, buff, sizeof(LOGIN) - 1);

                if(send(sock, buff, sizeof(LOGIN) - 1, 0) <= 0) 
					goto give_up;

            } 
			else
			{
                break;
            }
        }
    }

    if(!memcmp(buff, "USRIP", 5)) 
	{
        memcpy(buff, USRIP, sizeof(USRIP) - 1);
        gs_peerchat(&server, buff, sizeof(USRIP) - 1);

        if(send(sock, buff, sizeof(USRIP) - 1, 0) <= 0) 
			goto give_up;
    } 
	else 
	{
        //gs_peerchat(&server, buff, len);
        if(send(sd, buff, len, 0) <= 0) goto give_up;
    }

    selectsock = ((sock > sd) ? sock : sd) + 1;

    for(;;)
	{
        tout.tv_sec  = MAXTIMEOUT;
        tout.tv_usec = 0;
        FD_ZERO(&readset);
        FD_SET(sock, &readset);
        FD_SET(sd, &readset);

        if(select(selectsock, &readset, NULL, NULL, &tout) < 0)
			break;

        if(FD_ISSET(sd, &readset)) 
		{
            len = recv(sd, buff, BUFFSZ, 0);

            if(len <= 0)
				break;

            LOGIT

            gs_peerchat(&server, buff, len);

            if(send(sock, buff, len, 0) != len) 
				break;

        } 
		else if(FD_ISSET(sock, &readset))
		{
            len = recv(sock, buff, BUFFSZ, 0);

            if(len <= 0) 
				break;

            gs_peerchat(&client, buff, len);

            LOGIT

            if(send(sd, buff, len, 0) != len) 
				break;
        }
    }

give_up:
    if(buff)
		free(buff);
    if(sd > 0) 
		close(sd);

    close(sock);

    printf("  disconnected\n");
    return(0);
}



int get_key(u8 *gamename, u8 *gamekey, int gamekeysz) {
    FILE    *fd;
    u8      buff[GSLISTSZ + 1],
            *p,
            *l;

    fd = fopen(GSLIST, "rb");
    if(!fd) {
        printf("\n"
            "- open %s\n"
            "  you must download it from:\n"
            "    http://aluigi.org/papers/"GSLIST"\n",
            GSLIST);
        std_err();
        //printf("\n- press RETURN to quit\n");
        //fgets(buff, sizeof(buff), stdin);
        //exit(1);
    }

    if(!gamename && !gamekey) 
		return(0);

    gamekey[0] = 0;
    
    p = buff + CNAMEOFF;
    while(fgets(buff, sizeof(buff), fd)) {
        for(l = buff; *l && (*l != '\r') && (*l != '\n'); l++);
        *l = 0;
        l = strchr(p, ' ');
        if(l) *l = 0;
        if(!strcmp(gamename, p)) {
            fclose(fd);

            //memcpy(gamekey, buff + CKEYOFF, 6);
            strncpy(gamekey, buff + CKEYOFF, gamekeysz);    // Juiced has 7 chars
            for(l = gamekey; *l && (l < gamekey+gamekeysz-1); l++);
            *l = 0; // gamekey[6] = 0;
            return(1);
        }
    }
    fclose(fd);
    return(0);
}



int recv_tcp(int sock, u8 *data, int len) 
{
    u8      *p, *limit;

    p = data;
    limit = data + len - 1;

    while(p < limit)
	{

        if(timeout(sock) < 0)
			return(-1);

        if(recv(sock, p, 1, 0) <= 0)
			return(-1);

        if(*p++ == '\n') 
			break;
    }

    *p = 0;
    return(p - data);
}

int recv_tcp_dec(int sock, u8 *data, int len, gs_peerchat_ctx *decrypt)
{
    u8  *p, *limit;

    p = data;
    limit = data + len - 1;

    while(p < limit) 
	{
        if(timeout(sock) < 0) 
			return(-1);

        if(recv(sock, p, 1, 0) <= 0) 
			return(-1);

		//char a = p[0];

        gs_peerchat(decrypt, p, 1);

		//char b = p[0];

        if(*p++ == '\n') 
			break;
    }

    *p = 0;
    return(p - data);
}

int timeout(int sock)
{
    struct  timeval tout;
    fd_set  fd_read;
    int     err;

    tout.tv_sec  = MAXTIMEOUT;
    tout.tv_usec = 0;
    FD_ZERO(&fd_read);
    FD_SET(sock, &fd_read);
    err = select(sock + 1, &fd_read, NULL, NULL, &tout);
    if(err < 0) return(-1);
    if(!err) return(-1);
    return(0);
}

u32 resolv(char *host)
{
	struct  hostent *hp;
	u32     host_ip;

	host_ip = inet_addr(host);
	if (host_ip == INADDR_NONE)
	{
		hp = gethostbyname(host);
		if (!hp)
		{
			printf("\nError: Unable to resolve hostname (%s)\n", host);
			exit(1);
		}
		else host_ip = *(u32 *)hp->h_addr;
	}
	return(host_ip);
}



#ifndef WIN32
    void std_err(void)
	{
        perror("\nError");
        exit(1);
    }
#endif



