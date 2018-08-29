namespace Church.BibleStudyFellowship.Models
{
    using System.Linq;

    internal class VerseLocatorZh : VerseLocator
    {
        const string ChapterPattern = @"第 *([一二三四五六七八九十]+|\d+) *([章篇]? *[、和及\-至] *第? *([一二三四五六七八九十]+|\d+) *)*[章篇]";

        internal VerseLocatorZh(string books)
            : base(books, ":：", "-", ",，、", ";；和", VerseLocatorZh.ChapterPattern, "-至")
        {
        }

        protected override int MapChapter(string value)
        {
            const string Index = "一二三四五六七八九十";

            if(int.TryParse(value, out var number))
            {
                return number;
            }
            else
            {
                return Index.IndexOf(value.First()) + 1;
            }
        }
    }
}
