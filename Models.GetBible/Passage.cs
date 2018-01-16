namespace Church.BibleStudyFellowship.Models.GetBible
{
    using System.Collections.Generic;

    internal class Passage
    {
        public string Version { get; set; }

        public string Type { get; set; }

        public string direction { get; set; }

        public IList<PassageBook> Book { get; set; }
    }
}
