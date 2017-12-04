namespace Church.BibleStudyFellowship.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public abstract class AbstractTextParser : ITextParser
    {
        private readonly IDictionary<Regex, MethodInfo> methodMappings;

        private readonly VerseLocator verseLocator;

        private readonly Regex versePattern;

        internal AbstractTextParser(int year, CultureInfo culture, IDictionary<Regex, MethodInfo> methodMappings, VerseLocator verseLocator)
        {
            this.Year = year;
            this.Culture = culture;
            this.methodMappings = methodMappings;
            this.verseLocator = verseLocator;
            this.versePattern = verseLocator.GetPattern();
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

        protected static Regex GetRegex(MethodInfo methodInfo)
        {
            var pattern = methodInfo.GetCustomAttributes(false).OfType<SectionAttribute>().First().Mark;

            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        protected IList<VerseItem> ExtractVerse(string text)
        {
            var items = new List<VerseItem>();
            var collection = this.versePattern.Matches(text);
            foreach (Match match in collection)
            {
                items.AddRange(this.verseLocator.GetVerses(match));
            }

            return items;
        }

        protected IList<TextPart> ExtractParts(string text)
        {
            var items = new List<TextPart>();
            var collection = this.versePattern.Matches(text);

            var loop = 0;
            var lastPosition = 0;
            while (loop < collection.Count)
            {
                var match = collection[loop];
                AbstractTextParser.AddSimpleText(items, text.Substring(lastPosition, match.Index - lastPosition));
                var verses = this.verseLocator.GetVerses(match);
                items.AddRange(verses.Select(verse => new TextPart
                {
                    Style = TextPartStyle.Verse,
                    Text = verse.Book + " " + verse.Verse,
                }));
                //// Sample code for a single item for verses
                ////items.Add(new TextPart
                ////{
                ////    Style = TextPartStyle.Verse,
                ////    Text = match.Value,
                ////});
                loop++;
                lastPosition = match.Index + match.Length;
            }

            AbstractTextParser.AddSimpleText(items, text.Substring(lastPosition));
            return items;
        }

        private static void AddSimpleText(IList<TextPart> items, string text)
        {
            if(!string.IsNullOrEmpty(text))
            {
                items.Add(new TextPart { Style = TextPartStyle.Normal, Text = text });
            }
        }
    }
}
