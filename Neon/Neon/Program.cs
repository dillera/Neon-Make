using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;


namespace Neon
{
    class Program
    {
        static void Main(string[] args)
        {
            ADF.Compiler C = new ADF.Compiler();
 
            bool param = false;

            bool Result = true;
           
            string Source = "";
            string Destination = "";
            string FileFormat = "";

            ADF.FileFormat Newline;

            if (args.Length > 0)
            {
                param = true;

                Source = args[0];

                if (args.Length > 1)
                {
                    Destination = args[1];
                }

                if (args.Length > 2)
                {
                    FileFormat = args[2];
                }
                else
                {
                    FileFormat = "ATASCII";
                }
            }

            else
            {              
                Console.WriteLine("You can pass the Source, Destination, and File Format (optional).");
                Console.WriteLine("neon index.adf index.doc atascii");
            }

            do
            {
                if (string.IsNullOrEmpty(Source))
                {
                    Console.WriteLine();
                    Console.Write("Source file:");
                    Source = Console.ReadLine();
                }

                if (string.IsNullOrEmpty(Destination))
                {                 
                    Console.Write("Destination file:");
                    Destination = Console.ReadLine();
                }

                if (string.IsNullOrEmpty(FileFormat))
                {
                    Console.WriteLine("Enter File format (ATASCII, CRLF, LF)");
                    Console.Write("File Format:");
                    FileFormat = Console.ReadLine();
                }


                switch (FileFormat.Trim().ToUpper())
                {
                    case "ATASCII":
                        Newline = ADF.FileFormat.ATASCII;
                        break;

                    case "CRLF":
                        Newline = ADF.FileFormat.CRLF;
                        break;

                    case "LF":
                        Newline = ADF.FileFormat.LF;
                        break;

                    default:
                        Newline = ADF.FileFormat.ATASCII;
                        break;
                }


                Result = C.ProcessFile(Source, Newline);

                if (Result)
                {
                    C.SaveDocument(Destination);
                    Console.WriteLine("Document created");
                }
                else
                {
                    Console.WriteLine("Errors found!!");
                }

                Source = "";
                Destination = "";
                FileFormat = "";


            } while (!param);
        }
    }
}

