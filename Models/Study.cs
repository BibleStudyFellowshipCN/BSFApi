namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;

    public class Study
    {
        public Study()
        {
            this.Lessons = new List<LessonItem>();
        }

        public string Culture { get; set; }

        public string Title { get; set; }

        public IList<LessonItem> Lessons { get; set; }
    }
}
