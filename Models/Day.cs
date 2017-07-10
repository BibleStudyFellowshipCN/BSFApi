namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;

    public class Day
    {
        public Day()
        {
            this.ReadVerse = new List<VerseItem>();
            this.Questions = new List<Question>();
        }

        public string Tab { get; set; }

        public string Title { get; set; }

        public IList<VerseItem> ReadVerse { get; set; }

        public IList<Question> Questions { get; set; }
    }
}
