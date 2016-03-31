using SevenZip.SDK;
using System;
using System.IO;

namespace UCC
{
    internal class Program
    {
        private static void Decompress(string[] args)
        {
            SevenZip.SDK.Compress.LZMA.Decoder decoder = new SevenZip.SDK.Compress.LZMA.Decoder();
            string[] filePaths = Directory.GetFiles(args[0], args[2]);
            Directory.CreateDirectory(args[1]);
            foreach (string filePath in filePaths)
            {
                FileInfo f = new FileInfo(filePath);
                using (FileStream input = new FileStream(filePath, FileMode.Open))
                {
                    using (FileStream output = new FileStream(Path.Combine(args[1], Path.GetFileName(filePath)), FileMode.Create))
                    {
                        // Read the decoder properties
                        byte[] properties = new byte[5];
                        input.Read(properties, 0, 5);

                        // Read in the decompress file size.
                        byte[] fileLengthBytes = new byte[4];
                        input.Read(fileLengthBytes, 0, 4);
                        int fileLength = BitConverter.ToInt32(fileLengthBytes, 0);

                        decoder.SetDecoderProperties(properties);
                        decoder.Code(input, output, input.Length, fileLength, null);
                        output.Flush();
                        output.Close();
                    }
                    input.Close();
                }
            }
        }

        private static void Compress(string[] args)
        {
            SevenZip.SDK.Compress.LZMA.Encoder coder = new SevenZip.SDK.Compress.LZMA.Encoder();
            string[] filePaths = Directory.GetFiles(args[0], args[2]);
            Directory.CreateDirectory(args[1]);
            foreach (string filePath in filePaths)
            {
                FileInfo f = new FileInfo(filePath);
                using (FileStream input = new FileStream(filePath, FileMode.Open))
                {
                    using (FileStream output = new FileStream(Path.Combine(args[1], Path.GetFileName(filePath)), FileMode.Create))
                    {
                        //properties
                        //byte[] properties = new byte[5] { 0x5d, 0x00, 0x00, 0x04, 0x00};

                        CoderPropID[] propIDs =
                        {
                            CoderPropID.DictionarySize,
                            CoderPropID.PosStateBits,
                            CoderPropID.LitContextBits,
                            CoderPropID.LitPosBits,
                            CoderPropID.Algorithm,
                            CoderPropID.NumFastBytes,
                            CoderPropID.MatchFinder,
                            CoderPropID.EndMarker
                        };

                        Int32 dictionary = 1 << 18;
                        Int32 posStateBits = 2;
                        Int32 litContextBits = 3; // for normal files
                        // UInt32 litContextBits = 0; // for 32-bit data
                        Int32 litPosBits = 0;
                        // UInt32 litPosBits = 2; // for 32-bit data
                        Int32 algorithm = 2;
                        Int32 numFastBytes = 32;
                        string mf = "bt4";
                        bool eos = false;

                        object[] properties =
                        {
                            (Int32)(dictionary),
                            (Int32)(posStateBits),
                            (Int32)(litContextBits),
                            (Int32)(litPosBits),
                            (Int32)(algorithm),
                            (Int32)(numFastBytes),
                            mf,
                            eos
                        };

                        coder.SetCoderProperties(propIDs, properties);
                        coder.WriteCoderProperties(output);
                        output.Write(BitConverter.GetBytes(input.Length), 0, 4);

                        coder.Code(input, output, input.Length, -1, null);
                        output.Flush();
                        output.Close();
                    }
                    input.Close();
                }
            }
        }

        private static void Main(string[] args)
        {
            // args[0]: source directory
            // args[1]: target directory
            // args[2]: filename (ex: *.csv)
            // args[3]: "-d" for decompress

            if (args.Length == 4)
            {
                if (args[3] == "-d")
                    Decompress(new string[] { args[0], args[1], args[2] });
            }
            else if (args.Length > 0)
                Compress(new string[] { args[0], args[1], args[2] });
            else
                Environment.Exit(0x00);
        }
    }
}