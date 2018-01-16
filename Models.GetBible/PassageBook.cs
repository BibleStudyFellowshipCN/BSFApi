namespace Church.BibleStudyFellowship.Models.GetBible
{
    using System.Collections.Generic;

    internal class PassageBook
    {
        public string Book_name { get; set; }

        public int Book_nr { get; set; }

        public string Book_ref { get; set; }

        public IDictionary<int, PassageChapter> chapter { get; set; }

        public int Chapter_nr { get; set; }
    }
}
