namespace Church.BibleStudyFellowship.Models
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    internal class SectionAttribute : Attribute
    {
        public SectionAttribute(string mark)
        {
            this.Mark = mark;
        }

        public string Mark { get; }
    }
}