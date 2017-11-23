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

        private readonly VerseLocator verseLocator;

        private readonly Regex versePattern;

        internal AbstractParser(int year, CultureInfo culture, IDictionary<Regex, MethodInfo> methodMappings, VerseLocator verseLocator)
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
    }
}
