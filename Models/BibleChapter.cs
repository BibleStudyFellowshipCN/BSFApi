namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;

    public class BibleChapter
    {
        public string Culture { get; set; }

        public string Version { get; set; }

        public int BookOrder { get; set; }

        public int Order { get; set; }

        public IEnumerable<BibleVerse> Verses { get; set; }
    }
}
