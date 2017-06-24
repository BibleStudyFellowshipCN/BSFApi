using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Church.BibleStudyFellowship.Models
{
    public class SectionWrapper
    {
        public SectionWrapper()
        {
            this.Sections = new List<SectionItem>();
        }

        public string Title { get; set; }

        public string Verse { get; set; }

        public IList<SectionItem> Sections { get; set; }
    }
}
