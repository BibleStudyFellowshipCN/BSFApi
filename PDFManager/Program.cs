namespace PDFManager
{
    using Church.BibleStudyFellowship.Models.PdfBox;
    using System;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("PDFManager <option> <filename>");
                return;
            }

            string filename, result;

            switch(args[0].ToUpper())
            {
                case "TEXT":
                    filename = Path.ChangeExtension(args[1], ".txt");
                    result = Utilities.ReadPdfAsText(args[1]);
                    break;
                case "HTML":
                    filename = Path.ChangeExtension(args[1], ".html");
                    result = Utilities.ReadPdfAsHtml(args[1]);
                    break;
                default:
                    throw new NotSupportedException($"Not support {args[0]}");
            }

            File.WriteAllText(filename, result);
        }
    }
}
