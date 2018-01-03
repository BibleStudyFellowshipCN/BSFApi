namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    internal class VerseLocatorZh : VerseLocator
    {
        const string ChapterPattern = @"第 *([一二三四五六七八九十百]+|\d+) *章(和第 *([一二三四五六七八九十百]+|\d+) *章)?";

        private static Regex ChapterRegex = new Regex(VerseLocatorZh.ChapterPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal VerseLocatorZh(string books)
            : base(books, ":：", "-", ",，、", ";；", VerseLocatorZh.ChapterPattern.Replace("(","(?:"))
        {
        }

        protected override IList<VerseItem> TryGetChapters(string book, string text)
        {
            var items = new List<VerseItem>();
            var parts = ChapterRegex.Split(text);
            int loop = 1;
            while (loop < parts.Length)
            {
                var item = new VerseItem
                {
                    Book = book,
                    Verse = parts[loop] + ":1-999",
                };
                items.Add(item);
                loop += 2;
            }

            return items;
        }
    }
}
