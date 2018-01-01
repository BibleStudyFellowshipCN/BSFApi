namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class VerseLocator
    {
        // (;* *(\d+ *:( *\d+ *((- *\d+ *(: *\d+)?)?)?))( *,(( *\d+ *((- *\d+ *(: *\d+)?)?)?)))*)+
        const string VerseFormat = @"(?:[{3}]* *(?:\d+ *[{0}](?: *\d+ *(?:(?:[{1}] *\d+ *(?:[{0}] *\d+)?)?)?))(?: *[{2}](?:(?: *\d+ *(?:(?:[{1}] *\d+ *([{0}] *\d+)?)?)?)))*)+";

        private readonly string books;

        private readonly string chapterSeparators;

        private readonly string verseConnectors;

        private readonly string verseSeparators;

        private readonly string groupSeparators;

        internal VerseLocator(string books, string chapterSeparators, string verseConnectors, string verseSeparators, string groupSeparators)
        {
            this.books = books;
            this.chapterSeparators = chapterSeparators;
            this.verseConnectors = verseConnectors;
            this.verseSeparators = verseSeparators;
            this.groupSeparators = groupSeparators;
        }

        public static VerseLocator Create(IEnumerable<BibleBook> books, bool includeShorthand = false)
        {
            ExceptionUtilities.ThrowArgumentNullExceptionIfNull(books, nameof(books));

            var bibleBooks = books.ToArray();
            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(bibleBooks.Length > 0, "Need at least one books.");

            var bookString = includeShorthand ? string.Join("|", bibleBooks.SelectMany(book => new[] { book.Name, book.Shorthand }))
                : string.Join("|", bibleBooks.Select(book => book.Name));

            string chapterSeparators, verseConnectors, verseSeparators, groupSeparators;
            switch (bibleBooks[0].Culture)
            {
                case "zh-CN":
                case "zh-TW":
                    chapterSeparators = ":：";
                    verseConnectors = "-";
                    verseSeparators = ",，、";
                    groupSeparators = ";；";
                    break;
                default:
                    chapterSeparators = ":";
                    verseConnectors = "-";
                    verseSeparators = ",";
                    groupSeparators = ";";
                    break;
            }

            return new VerseLocator(bookString, chapterSeparators, verseConnectors, verseSeparators, groupSeparators);
        }

        public string GetPattern(bool hasSingleGroup)
        {
            var versePattern = string.Format(
                VerseLocator.VerseFormat,
                this.chapterSeparators,
                this.verseConnectors,
                this.verseSeparators,
                this.groupSeparators);
            return hasSingleGroup
                ? "((?:" + this.books + ")" + "(?:" + versePattern + "))"
                : "(" + this.books + ")" + "(" + versePattern + ")";
        }

        public IList<VerseItem> GetVerses(Match match)
        {
            var items = new List<VerseItem>();
            var book = match.Groups[1].Value.Trim();
            var verses = match.Groups[2].Value.Replace(" ", string.Empty);
            var groups = verses.Split(this.groupSeparators.ToCharArray());
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
    }
}
