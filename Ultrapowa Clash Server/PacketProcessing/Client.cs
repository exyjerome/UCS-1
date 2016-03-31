using Sodium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using UCS.Logic;
using UCK;

namespace UCS.PacketProcessing
{
    internal class Client
    {
        public static ClashKeyPair GenerateKeyPair()
        {
            var keyPair = PublicKeyBox.GenerateKeyPair();
            return new ClashKeyPair(keyPair.PublicKey, keyPair.PrivateKey);
        }

        private readonly byte[] m_vEnDecryptKey =
        {
        };

        private Level m_vLevel;

        private readonly byte[] m_vPrivateKey =
        {
        };

        private readonly long m_vSocketHandle;

        public Client(Socket so)
        {
            Socket = so;
            m_vSocketHandle = so.Handle.ToInt64();
            IncomingPacketsKey = new byte[m_vEnDecryptKey.Length];
            Array.Copy(m_vEnDecryptKey, IncomingPacketsKey, m_vEnDecryptKey.Length);
            OutgoingPacketsKey = new byte[m_vEnDecryptKey.Length];
            Array.Copy(m_vEnDecryptKey, OutgoingPacketsKey, m_vEnDecryptKey.Length);
            DataStream = new List<byte>();
            CState = 0;
        }

        public int ClientSeed { get; set; }

        public byte[] CPublicKey { get; set; }

        public byte[] CSessionKey { get; set; }

        public byte[] CSNonce { get; set; }

        public int CState { get; set; }

        public byte[] CRNonce { get; set; }

        public byte[] CSharedKey { get; set; }

        public List<byte> DataStream { get; set; }

        public byte[] IncomingPacketsKey { get; set; }

        public byte[] OutgoingPacketsKey { get; set; }

        public Socket Socket { get; set; }

        /*public static int extract_number(int[] buffer, int ix)
        {
            if (ix == 0)
            {
                generate_numbers(buffer);
            }

            var y = buffer[ix];
            y ^= y >> 11;
            y ^= (int) (y << 7 & 2636928640); // 0x9d2c5680
            y ^= (int) (y << 15 & 4022730752); // 0xefc60000
            y ^= y >> 18;

            if ((y & (1 << 31)) != 0)
            {
                y = ~y + 1;
            }

            ix = (ix + 1)%624;
            return y%256;
        }*/

        // Extract a tempered pseudorandom number based on the index-th value,
        // calling generate_numbers() every 624 numbers
        // Generate an array of 624 untempered numbers
        public static void generate_numbers(int[] buffer)
        {/*
            for (var i = 0; i < 624; i++)
            {
                var y = (int) ((buffer[i] & 0x80000000) + (buffer[(i + 1)%624] & 0x7fffffff));
                buffer[i] = buffer[(i + 397)%624] ^ (y >> 1);
                if (y%2 != 0)
                {
                    buffer[i] = (int) (buffer[i] ^ 2567483615);
                }
            }*/
        }

        // Initialize the generator from a seed
        public static void initialize_generator(int seed, int[] buffer)
        {/*
            buffer[0] = seed;
            for (var i = 1; i < 624; ++i)
            {
                buffer[i] = 1812433253*((buffer[i - 1] ^ (buffer[i - 1] >> 30)) + 1);
            }*/
        }

        public static byte[] GenerateSessionKey()
        {
            return PublicKeyBox.GenerateNonce();
        }

        public static void TransformSessionKey(int clientSeed, byte[] sessionKey)
        {/*
            var buffer = new int[624];
            initialize_generator(clientSeed, buffer);
            var byte100 = 0;
            for (var i = 0; i < 100; i++)
            {
                byte100 = extract_number(buffer, i);
            }

            for (var i = 0; i < sessionKey.Length; i++)
            {
                sessionKey[i] ^= (byte) (extract_number(buffer, i + 100) & byte100);
            }*/
        }

        public void Decrypt(byte[] data)
        {
            EnDecrypt(IncomingPacketsKey, data);
        }

        public void Encrypt(byte[] data)
        {
            EnDecrypt(OutgoingPacketsKey, data);
        }

        public static void EnDecrypt(byte[] key, byte[] data)
        {/*
            int dataLen;

            if (data != null)
            {
                dataLen = data.Length;

                if (dataLen >= 1)
                {
                    do
                    {
                        dataLen--;
                        var index = (byte) (key[0] + 1);
                        key[0] = index;
                        var num2 = (byte) (key[4] + key[index + 8]);
                        key[4] = num2;
                        var num3 = key[index + 8];
                        key[index + 8] = key[num2 + 8];
                        key[key[4] + 8] = num3;
                        var num4 = key[(byte) (key[key[4] + 8] + key[key[0] + 8]) + 8];
                        data[data.Length - dataLen - 1] = (byte) (data[data.Length - dataLen - 1] ^ num4);
                    } while (dataLen > 0);
                }
            }
            */
        }

        public Level GetLevel()
        {
            return m_vLevel;
        }

        public long GetSocketHandle()
        {
            return m_vSocketHandle;
        }

        public void SetLevel(Level l)
        {
            m_vLevel = l;
        }

        public bool TryGetPacket(out Message p)
        {
            p = null;
            var result = false;

            if (DataStream.Count() >= 5)
            {
                var length = (0x00 << 24) | (DataStream[2] << 16) | (DataStream[3] << 8) | DataStream[4];
                var type = (ushort) ((DataStream[0] << 8) | DataStream[1]);

                if (DataStream.Count - 7 >= length)
                {
                    object obj = null;
                    var packet = DataStream.Take(7 + length).ToArray();

                    using (var br = new BinaryReader(new MemoryStream(packet)))
                    {
                        obj = MessageFactory.Read(this, br, type);
                    }

                    if (obj != null)
                    {
                        p = (Message) obj;
                        result = true;
                    }
                    else
                    {
                        var data = DataStream.Skip(7).Take(length).ToArray();
                        Decrypt(data);
                    }
                    DataStream.RemoveRange(0, 7 + length);
                }
            }
            return result;
        }

        public void UpdateKey(byte[] sessionKey)
        {/*
            TransformSessionKey((int) ClientSeed, sessionKey);

            var newKey = new byte[264];
            var clientKey = sessionKey;
            var v7 = m_vPrivateKey.Length;
            //byte[] v8 = privateKey;
            var v9 = m_vPrivateKey.Length + sessionKey.Length;
            var completeSessionKey = new byte[m_vPrivateKey.Length + sessionKey.Length];
            Array.Copy(m_vPrivateKey, 0, completeSessionKey, 0, v7); //memcpy(v10, v8, v7);
            Array.Copy(clientKey, 0, completeSessionKey, v7, sessionKey.Length);
                //memcpy(v10 + v7, clientKey, sessionKeySize);
            uint v11 = 0;
            uint v16;
            uint v12; //attention type
            byte v13; //attention type
            uint v14;
            byte* v15;
            uint v17;
            uint v18;
            byte v19;
            byte* v20;
            byte v21;
            uint v22;
            byte* v23;

            fixed (byte* v5 = newKey, v8 = m_vPrivateKey, v10 = completeSessionKey)
            {
                do
                {
                    *(v5 + v11 + 8) = (byte) v11;
                    ++v11;
                } while (v11 != 256);
                *v5 = 0;
                *(v5 + 4) = 0;
                while (true)
                {
                    v16 = *v5;
                    //if (v16 == 255)//if ( *v5 > 255 )
                    //    break;
                    v12 = *(v10 + v16%v9) + *(uint*) (v5 + 4);
                    *(uint*) v5 = v16 + 1;
                    v13 = *(v5 + v16 + 8);
                    v14 = (byte) (v12 + *(v5 + v16 + 8));
                    *(uint*) (v5 + 4) = v14;
                    v15 = v5 + v14;
                    *(v5 + v16 + 8) = *(v15 + 8);
                    *(v15 + 8) = v13;
                    if (v16 == 255) //if ( *v5 > 255 )
                        break;
                }
                v17 = 0;
                *v5 = 0;
                *(v5 + 4) = 0;
                while (v17 < v9)
                {
                    ++v17;
                    v18 = *(uint*) (v5 + 4);
                    v19 = (byte) (*(uint*) v5 + 1);
                    *(uint*) v5 = v19;
                    v20 = v5 + v19;
                    v21 = *(v20 + 8);
                    v22 = (byte) (v18 + v21);
                    *(uint*) (v5 + 4) = v22;
                    v23 = v5 + v22;
                    *(v20 + 8) = *(v23 + 8);
                    *(v23 + 8) = v21;
                }
            }
            Array.Copy(newKey, IncomingPacketsKey, newKey.Length);
            Array.Copy(newKey, OutgoingPacketsKey, newKey.Length);*/
        }
    }
}