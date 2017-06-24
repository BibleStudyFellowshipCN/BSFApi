namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;

    public class Question
    {
        public Question()
        {
            this.Verses = new List<string>();
        }

        public string Id { get; set; }

        public string Text { get; set; }

        public string Answer { get; set; }

        public IList<string> Verses { get; set; }
    }
}
