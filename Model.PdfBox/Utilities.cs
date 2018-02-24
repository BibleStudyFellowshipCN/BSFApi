namespace Church.BibleStudyFellowship.Models.PdfBox
{
    using org.apache.pdfbox.pdmodel;
    using org.apache.pdfbox.util;

    public static partial class Utilities
    {
        public static string ReadPdfAsText(string filename)
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

        public static string ReadPdfAsHtml(string filename)
        {
            PDDocument doc = null;

            try
            {
                doc = PDDocument.load(filename);
                var stripper = new PDFText2HTML(null);
                return stripper.getText(doc);
            }
            finally
            {
                doc?.close();
            }
        }
    }
}
