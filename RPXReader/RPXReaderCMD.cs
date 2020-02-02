using System;
using System.IO;

namespace RPXReader
{
    public class RPXReaderCMD
    {
        public void Run(string[] args)
        {
            Console.WriteLine("ELF/RPL/RPX Reader v1.0");
            if (args.Length == 1)
            {
                ELF file = ELF.Open(args[0]);
                if (file != null)
                    Console.Write("File: \"" + args[0] + "\"\n\n" + file.ToString());
                else
                    Console.WriteLine("File: \"" + args[0] + "\"\n\nIt is not an ELF, RPX or RPL file.");
            }
            else if (args.Length == 2)
            {
                if (args[0] == "decompress")
                {
                    ELF file = ELF.Open(args[1]);
                    if (file != null)
                    {
                        Console.Write("File: \"" + args[1] + "\"\n\n" + file.ToString());
                        string destination = Path.GetDirectoryName(args[1]).Length > 0 ? Path.GetDirectoryName(args[1]) + "\\" : "" +
                            Path.GetFileNameWithoutExtension(args[1]) +
                            "_decompressed" + Path.GetExtension(args[1]);
                        try
                        {
                            RPX.Decompress(args[1], destination);
                            Console.WriteLine("\nDecompressed!");
                            Console.WriteLine("Output: \"" + destination + "\"");
                        }
                        catch
                        {
                            Console.WriteLine("\nIt is not an RPX or RPL file");
                        }
                    }
                    else
                        Console.WriteLine("File: \"" + args[1] + "\"\n\nIt is not an ELF, RPX or RPL file.");
                }
                else if (args[0] == "compress")
                {
                    ELF file = ELF.Open(args[1]);
                    if (file != null)
                    {
                        Console.Write("File: \"" + args[1] + "\"\n\n" + file.ToString());
                        string destination = Path.GetDirectoryName(args[1]).Length > 0 ? Path.GetDirectoryName(args[1]) + "\\" : "" +
                           Path.GetFileNameWithoutExtension(args[1]) +
                           "_compressed" + Path.GetExtension(args[1]);
                        try
                        {
                            RPX.Compress(args[1], destination);
                            Console.WriteLine("\nCompressed!");
                            Console.WriteLine("Output: \"" + destination + "\"");
                        }
                        catch
                        {
                            Console.WriteLine("\nIt is not an RPX or RPL file");
                        }
                    }
                    else
                        Console.WriteLine("File: \"" + args[1] + "\"\n\nIt is not an ELF, RPX or RPL file.");
                }
                else if (args[0] == "extractrom")
                {
                    ELF file = ELF.Open(args[1]);
                    if (file != null)
                    {
                        Console.Write("File: \"" + args[1] + "\"\n\n" + file.ToString());
                        if (file is RPXNES)
                        {
                            RPXNES vc = file as RPXNES;
                            string filename = Path.GetDirectoryName(args[1]).Length > 0 ? Path.GetDirectoryName(args[1]) + "\\" : "" + vc.GetROMFileName();
                            FileStream fs = File.Open(filename, FileMode.Create);
                            if (vc.ROM.IsFDS)
                                fs.Write(vc.ROM.Data, 0, vc.ROM.RawSize);
                            else
                            {
                                fs.Write(vc.ROM.Data, 0, vc.ROM.RawSize + 16);
                                fs.Position = 3;
                                fs.WriteByte(0x1A);
                            }
                            fs.Close();
                            Console.WriteLine("\nROM extracted!");
                            Console.WriteLine("Output: \"" + filename + "\"");
                        }
                        else if (file is RPXSNES)
                        {
                            RPXSNES vc = file as RPXSNES;
                            string filename = Path.GetDirectoryName(args[1]) + "\\" + vc.GetROMFileName();
                            FileStream fs = File.Open(filename, FileMode.Create);
                            fs.Write(vc.ROM.Data, 0, vc.ROM.Data.Length);
                            fs.Close();
                            Console.WriteLine("\nROM extracted!");
                            Console.WriteLine("Output: \"" + filename + "\"");
                        }
                        else
                            Console.WriteLine("\nIt is not an VC NES RPX or VC SNES RPX file.");
                    }
                    else
                        Console.WriteLine("File: \"" + args[1] + "\"\n\nIt is not an ELF, RPX or RPL file.");
                }
                else
                {
                    Console.WriteLine("Use: <file path>");
                    Console.WriteLine("Or:  decompress <file path>");
                    Console.WriteLine("Or:  compress <file path>");
                    Console.WriteLine("Or:  extractrom <file path>");
                }
            }
            else
            {
                Console.WriteLine("Use: <file path>");
                Console.WriteLine("Or:  decompress <file path>");
                Console.WriteLine("Or:  compress <file path>");
                Console.WriteLine("Or:  extractrom <file path>");
            }
        }
    }
}
