namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;

    public class Day
    {
        public Day()
        {
            this.Verses = new List<string>();
            this.Questions = new List<Question>();
        }

        public string Tab { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public IList<string> Verses { get; set; }

        public IList<Question> Questions { get; set; }
    }
}
