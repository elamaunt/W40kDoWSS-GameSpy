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
    }
}
