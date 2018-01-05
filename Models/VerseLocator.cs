namespace Church.BibleStudyFellowship.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class VerseLocator
    {
        const string VerseFormat = @"([{3}]* *(\d+ *[{0}]( *\d+ *(([{1}] *\d+ *([{0}] *\d+)?)?)?))( *[{2}](( *\d+ *(([{1}] *\d+ *([{0}] *\d+)?)?)?)))*)+";

        private readonly string books;

        private readonly string chapterSeparators;

        private readonly string verseConnectors;

        private readonly string verseSeparators;

        private readonly string groupSeparators;

        private readonly string chapterPattern;

        private readonly Regex chapterRegex;

        private readonly string versePattern;

        private readonly Regex verseRegex;

        internal VerseLocator(string books, string chapterSeparators, string verseConnectors, string verseSeparators, string groupSeparators, string chapterPattern)
        {
            this.books = books;
            this.chapterSeparators = chapterSeparators;
            this.verseConnectors = verseConnectors;
            this.verseSeparators = verseSeparators;
            this.groupSeparators = groupSeparators;
            this.chapterPattern = chapterPattern;
            this.chapterRegex = new Regex(chapterPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            this.versePattern = string.Format(
                VerseLocator.VerseFormat,
                chapterSeparators,
                verseConnectors,
                verseSeparators,
                groupSeparators);
            this.verseRegex = new Regex(this.versePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public static VerseLocator Create(IEnumerable<BibleBook> books, bool includeShorthand = false)
        {
            ExceptionUtilities.ThrowArgumentNullExceptionIfNull(books, nameof(books));

            var bibleBooks = books.ToArray();
            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(bibleBooks.Length > 0, "Need at least one books.");

            var bookString = includeShorthand ? string.Join("|", bibleBooks.SelectMany(book => new[] { book.Name, book.Shorthand }))
                : string.Join("|", bibleBooks.Select(book => book.Name));

            switch (bibleBooks[0].Culture)
            {
                case "zh-CN":
                case "zh-TW":
                    return new VerseLocatorZh(bookString);
                case "es-MX":
                    return new VerseLocatorEs(bookString);
                case "en-US":
                    return new VerseLocatorEn(bookString);
                default:
                    throw new NotSupportedException($"The culture '{bibleBooks[0].Culture}' is not supported.");
            }
        }

        public string GetPattern()
        {
            return "(" + this.books + ")" + "(" + this.versePattern + "|" + this.chapterPattern + ")";
        }

        public IList<VerseItem> GetVerses(Match match)
        {
            ExceptionUtilities.ThrowArgumentNullExceptionIfNull(match, nameof(match));

            var book = match.Groups[1].Value.Trim();
            var verses = match.Groups[2].Value.Trim();
            if (!this.verseRegex.IsMatch(verses))
            {
                return this.TryGetChapters(book, verses);
            }

            var items = new List<VerseItem>();
            var groups = verses.Replace(" ", string.Empty).Split(this.groupSeparators.ToCharArray());
            var chapterSeparatorSet = new HashSet<char>(this.chapterSeparators.ToArray());
            foreach (var group in groups)
            {
                if (group.Count(chapterSeparatorSet.Contains) > 1)
                {
                    var item = new VerseItem
                    {
                        Book = book,
                        Verse = group,
                    };
                    items.Add(item);
                }
                else
                {
                    var parts = group.Split(chapterSeparatorSet.ToArray());
                    var chapter = parts[0];
                    var sections = parts[1].Split(this.verseSeparators.ToCharArray());
                    foreach (var section in sections)
                    {
                        var item = new VerseItem
                        {
                            Book = book,
                            Verse = chapter + ":" + section,
                        };
                        items.Add(item);
                    }
                }
            }

            return items;
        }

        private IList<VerseItem> TryGetChapters(string book, string text)
        {
            var items = new List<VerseItem>();
            var parts = this.chapterRegex.Split(text);
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
