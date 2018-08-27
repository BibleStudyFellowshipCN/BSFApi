namespace Church.BibleStudyFellowship.Models
{
    internal class VerseLocatorZh : VerseLocator
    {
        const string ChapterPattern = @"第 *([一二三四五六七八九十百]+|\d+) *([章篇]?[、和-]第? *([一二三四五六七八九十百]+|\d+) *)*[章篇]";

        internal VerseLocatorZh(string books)
            : base(books, ":：", "-", ",，、", ";；", VerseLocatorZh.ChapterPattern)
        {
        }
    }
}
