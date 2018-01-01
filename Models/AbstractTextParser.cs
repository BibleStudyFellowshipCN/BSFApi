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
            var pattern = verseLocator.GetPattern(false);
            this.versePattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
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

        protected static int ExtractOrder(string line)
        {
            const string Pattern = @"(\d+)";

            var regex = new Regex(Pattern);
            var match = regex.Match(line);
            if(!match.Success)
            {
                throw new FormatException($"Could not find lesson number from {line}.");
            }

            return int.Parse(match.Value);
        }

        protected void AddQuestion(Lesson lesson, string line, string id)
        {
            var question = new Question
            {
                Id = id,
                QuestionText = line,
                Text = line,
                Quotes = this.ExtractVerse(line),
            };

            lesson.DayQuestions.Last().Questions.Add(question);
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
