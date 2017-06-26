namespace Church.BibleStudyFellowship.Models.PdfBox
{
    using org.apache.pdfbox.pdmodel;
    using org.apache.pdfbox.util;

    public static partial class Utilities
    {
        public static string ReadFromPdf(string filename)
        {
            PDDocument doc = null;

            try
            {
                doc = PDDocument.load(filename);
                var stripper = new PDFTextStripper();
                return stripper.getText(doc);
            }
            finally
            {
                doc?.close();
            }
        }
    }
}
