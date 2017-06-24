namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;

    public class SectionItem
    {
        public SectionItem()
        {
            this.Questions = new List<string>();
        }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public IList<string> Questions { get; set; }
    }
}
