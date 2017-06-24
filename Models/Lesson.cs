namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;

    public class Lesson
    {
        public Lesson()
        {
            this.DayQuestions = new List<Day>();
        }

        public string Culture { get; set; }

        public string Id { get; set; }

        public string Audio { get; set; }

        public string Name { get; set; }

        public string MemoryVerse { get; set; }

        public IList<Day> DayQuestions { get; set; }
    }
}
