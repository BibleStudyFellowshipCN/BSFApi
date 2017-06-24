namespace Church.BibleStudyFellowship.Models
{
    public class BibleVerse
    {
        public string Version { get; set; }

        public int BookOrder { get; set; }

        public int ChapterOrder { get; set; }

        public int Order { get; set; }

        public string Culture { get; set; }

        public string Text { get; set; }
    }
}
