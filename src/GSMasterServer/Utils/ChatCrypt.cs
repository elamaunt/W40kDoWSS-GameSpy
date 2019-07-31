using System.Runtime.CompilerServices;

namespace GSMasterServer.Utils
{
    public unsafe static class ChatCrypt
    {
        /// <summary>
        /// gs_peerchat_ctx same
        /// </summary>
        public class GDCryptKey
        {
            /// <summary>
            /// gs_peerchat_1 same
            /// </summary>
            public byte X;

            /// <summary>
            ///  gs_peerchat_2 same
            /// </summary>
            public byte Y;

            /// <summary>
            /// gs_peerchat_crypt same
            /// </summary>
            public byte[] State = new byte[256];
        }

        static void SwapByte(byte* array, int index1, int index2)
        {
            var t = array[index1];
            array[index1] = array[index2];
            array[index2] = t;
        }

        // gs_peerchat same
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void GSEncodeDecode(GDCryptKey ctx, byte* data, int size)
        {
            byte num1, num2, t;

            fixed (byte* crypt = ctx.State)
            {
                num1 = ctx.X;
                num2 = ctx.Y;

                while (size-- > 0)
                {
                    t = crypt[++num1];
                    num2 += t;
                    crypt[num1] = crypt[num2];
                    crypt[num2] = t;
                    t += crypt[num1];
                    *data++ ^= crypt[t];
                }

                ctx.X = num1;
                ctx.Y = num2;
            }
        }

        // gs_peerchat init same
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void GSCryptKeyInit(GDCryptKey ctx, byte* chall, byte* gamekey, int gameKeySize)
        {
            byte* challenge = stackalloc byte[16];
            byte* l;
            byte* l1;
            byte* p;
            byte* p1;
            byte t;
            byte t1;

            ctx.X = 0;
            ctx.Y = 0;

            // Setup start 'M' chars to make it same with C code
            for (int i = 0; i < 16; i++)
                challenge[i] = 204;

            fixed (byte* crypt = ctx.State)
            {

                p = challenge;
                l = challenge + 16;
                p1 = gamekey;
                int counter = 0;

                do
                {
                    if (counter == gameKeySize)
                    {
                        p1 = gamekey;
                        counter = 0;
                    }

                    counter++;

                    *(p++) = (byte)(*(chall++) ^ *(p1++));
                }
                while (p != l);


                t1 = 255;
                p1 = crypt;
                l1 = crypt + 256;
                do
                {
                    *p1++ = t1--;
                }
                while (p1 != l1);

                t1++;       // means t1 = 0;
                p = crypt;
                p1 = challenge;

                do
                {
                    t1 = (byte)(t1 + *p1 + *p);
                    t = crypt[t1];
                    crypt[t1] = *p;
                    *p = t;
                    p++;
                    p1++;

                    if (p1 == l)
                        p1 = challenge;
                }
                while (p != l1);
            }
        }

        /*static const char* piStagingRoomHash(unsigned int publicIP, unsigned int privateIP, unsigned short port, char * buffer)
        {

            unsigned int result;

                publicIP = ntohl(publicIP);
                privateIP = ntohl(privateIP);

                result = (((privateIP >> 24) & 0xFF) | ((privateIP >> 8) & 0xFF00) | ((privateIP << 8) & 0xFF0000) | ((privateIP << 24) & 0xFF000000));
	        result ^= publicIP;
	        result ^= (port | (port << 16));

	        return EncodeIP(result, buffer, PEERTrue);
        }*/


        //public static byte* EncodeIP(uint ip, byte* buffer, )


        /*
        static const char* EncodeIP(unsigned int ip, char * buffer, PEERBool newCrypt)
        {

            const char* crypt = newCrypt ? new_digits_crypt : digits_crypt;
                int i;
                char* str;
                int digit_idx;

                // XOR the IP address.
                ip ^= ip_xormask;

            // Print out the ip addr in hex form.
            sprintf(cryptbuffer, "%08x", ip);

            // Translate chars in positions 0 through 7 from hex digits to "crypt" digits.
            for(i = 0 ; i< 8 ; i++)
            {
                str = strchr(digits_hex, cryptbuffer[i]);
                digit_idx = (str - digits_hex);

                if((digit_idx< 0) || (digit_idx > 15)) // sanity check
                {
                    strcpy(cryptbuffer, "14saFv19"); // equivalent to 0.0.0.0
                    break;
                }

            cryptbuffer[i] = crypt[digit_idx];
            }

            if(buffer)
            {
                strcpy(buffer, cryptbuffer);
                return buffer;
            }

            return cryptbuffer;
        }*/

        /*static const char* piStagingRoomHash(unsigned int publicIP, unsigned int privateIP, unsigned short port, char * buffer)
        {

            unsigned int result;

            publicIP = ntohl(publicIP);
            privateIP = ntohl(privateIP);

            result = (((privateIP >> 24) & 0xFF) | ((privateIP >> 8) & 0xFF00) | ((privateIP << 8) & 0xFF0000) | ((privateIP << 24) & 0xFF000000));
                        result ^= publicIP;
                        result ^= (port | (port << 16));

            return EncodeIP(result, buffer, PEERTrue);
         }*/

        /*static unsigned int DecodeIP(const char* buffer, PEERBool newCrypt)
        {
	        const char* crypt = newCrypt ? new_digits_crypt : digits_crypt;
                unsigned int ip;
                char* str;
                int digit_idx;
                int i;

	        if(!buffer)
		        return 0;
	
	        // Translate chars from hex digits to "crypt" digits.
	        for(i = 0 ; i< 8 ; i++)
	        {
		        str = strchr(crypt, buffer[i]);
                digit_idx = (str - crypt);

		        if((digit_idx< 0) || (digit_idx > 15))
			        return 0;

		        cryptbuffer[i] = digits_hex[digit_idx];
	        }

            // Cap the buffer.
            cryptbuffer[i] = '\0';

	        // Convert the string to an unsigned long (the XORd ip addr).
	        sscanf(cryptbuffer, "%x", &ip);

            // re-XOR the IP address.
            ip ^= ip_xormask;

	        return ip;
        }*/
    }
}
