namespace Church.BibleStudyFellowship.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public abstract class AbstractParser : ITextParser
    {
        private readonly IDictionary<Regex, MethodInfo> methodMappings;

        private readonly Regex versePattern;

        internal AbstractParser(int year, CultureInfo culture, IDictionary<Regex, MethodInfo> methodMappings, Regex versePattern)
        {
            this.Year = year;
            this.Culture = culture;
            this.methodMappings = methodMappings;
            this.versePattern = versePattern;
        }

        public int Year { get; }

        public CultureInfo Culture { get; }

        public Lesson Parse(string input)
        {
            input = input.Replace('\t', ' ');
            var lesson = new Lesson { Culture = this.Culture.Name };

            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            var sectionLines = new List<string>();
            MethodInfo currentSection = null;
            foreach (var line in lines)
            {
                foreach (var mapping in this.methodMappings)
                {
                    if (mapping.Key.IsMatch(line))
                    {
                        if (currentSection != null)
                        {
                            currentSection.Invoke(this, new object[] { lesson, sectionLines });
                            sectionLines.Clear();
                        }

                        currentSection = mapping.Value;
                        break;
                    }
                }

                sectionLines.Add(line);
            }

            if (currentSection != null)
            {
                currentSection.Invoke(this, new object[] { lesson, sectionLines });
            }

            return lesson;
        }

        protected static Regex GetBibleVersePattern(IRepository repository, string culture)
        {
            var suffix = "( *(([0-9]+) *:( *[0-9]+ *((- *[0-9]+ *(: *[0-9]+)?)?)?))( *,(( *[0-9]+ *((- *[0-9]+ *(: *[0-9]+)?)?)?)))*( *;( *(([0-9]+) *:( *[0-9]+ *((- *[0-9]+ *(: *[0-9]+)?)?)?))( *,(( *[0-9]+ *((- *[0-9]+ *(: *[0-9]+)?)?)?)))*))*)";
            var books = repository.GetBibleBooks(culture);
            var pattern = "(" + string.Join("|", books.SelectMany(book => new[] { book.Name, book.Shorthand })) + ")" + suffix;
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        protected static Regex GetRegex(MethodInfo methodInfo)
        {
            var pattern = methodInfo.GetCustomAttributes(false).OfType<SectionAttribute>().First().Mark;

            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        protected int GetNumberOfVerses(IList<string> lines)
        {
            var count = 0;
            foreach (var line in lines)
            {
                var match = this.versePattern.Match(line);
                if (match.Success && match.Value == line)
                {
                    count++;
                }
            }

            return count;
        }

        protected IList<VerseItem> ExtractVerse(string text)
        {
            var items = new List<VerseItem>();
            var collection = this.versePattern.Matches(text);
            foreach (Match match in collection)
            {
                var book = match.Groups[1].Value.Trim();
                var verses = match.Groups[2].Value.Replace(" ", string.Empty);
                var groups = verses.Split(';');
                foreach(var group in groups)
                {
                    var parts = group.Split(':');
                    var chapter = parts[0];
                    var sections = parts[1].Split(',');
                    foreach(var section in sections)
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
