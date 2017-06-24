namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;

    public class Study
    {
        public Study()
        {
            this.Lessons = new List<LessonItem>();
        }

        public int Year { get; set; }

        public string Culture { get; set; }

        public string Name { get; set; }

        public IList<LessonItem> Lessons { get; set; }
    }
}
