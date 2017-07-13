﻿namespace Church.BibleStudyFellowship.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class TextParseEsMx : ITextParser
    {
        private static Regex DayPattern = new Regex("^([A-Z]+) DÍA:", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex QuestionPattern = new Regex(@"^ *(\d+)\. ", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex BulletinPattern = new Regex(@"^ *[a-z]\. ", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex QuotationPattern = new Regex(@"^“.+”$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static IDictionary<string, string> OrdinalMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"PRIMER", "1ER" },
            {"SEGUNDO", "2DO" },
            {"TERCER", "3TE" },
            {"CUARTO", "4TO" },
            {"QUINTO", "5TO" },
            {"SEXTO", "6TO" },
        };

        private readonly IDictionary<Regex, MethodInfo> methodMappings;

        private readonly Regex versePattern;

        internal TextParseEsMx(int year, CultureInfo culture, Regex versePattern, IDictionary<Regex, MethodInfo> methodMappings)
        {
            this.Year = year;
            this.Culture = culture;
            this.versePattern = versePattern;
            this.methodMappings = methodMappings;
        }

        public int Year { get; }

        public CultureInfo Culture { get; }

        public static TextParseEsMx Create(int year, IRepository repository)
        {
            var methods = typeof(TextParseEsMx).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttributes(false).OfType<SectionAttribute>().Any());
            var methodMappings = methods.ToDictionary(TextParseEsMx.GetRegex);
            var culture = CultureInfo.CreateSpecificCulture("es-MX");
            var versePattern = TextParseEsMx.GetBibleVersePattern(repository, culture.Name);

            return new TextParseEsMx(year, culture, versePattern, methodMappings);
        }

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

        [Section(@"^Lección \d+: ")]
        protected void ParseFotter1(Lesson lesson, IList<string> lines)
        {
            const string Prefix = "BSF® Lección ";

            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(lines.Count > 3, "At least 4 lines.");
            lesson.Id = string.Join("_", this.Year, lines[1].Substring(Prefix.Length).Trim());
            lesson.Name = lines[3];
        }

        [Section("^®$")]
        protected void ParseEmpty(Lesson lesson, IList<string> lines)
        {
        }

        [Section(@"www\.bsfinternational\.org \| ")]
        protected void ParseFotter2(Lesson lesson, IList<string> lines)
        {
        }

        [Section("^Versículo de las Escrituras para memorizar")]
        protected void ParseMemoryVerse(Lesson lesson, IList<string> lines)
        {
            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(lines.Count() > 1, "At least two lines.");
            lesson.MemoryVerse = string.Join(string.Empty, lines.Skip(1));
        }

        [Section("^[A-Z]+ DÍA:")]
        protected void ParseDay(Lesson lesson, IList<string> lines)
        {
            var match = TextParseEsMx.DayPattern.Match(lines[0]);
            var title = lines[0].Substring(match.Value.Length).Trim();
            var day = new Day
            {
                Tab = TextParseEsMx.OrdinalMapping[match.Groups[1].Value],
                Title = title,
                ReadVerse = this.ExtractVerse(title),
            };
            if (lines.Count > 1)
            {
                day.Title += " " + string.Join(string.Empty, lines.Skip(1));
            }

            lesson.DayQuestions.Add(day);
        }

        [Section(@"^ *\d+\. ")]
        protected void ParseQuestion(Lesson lesson, IList<string> lines)
        {
            const string Separator = "_";

            var match = TextParseEsMx.QuestionPattern.Match(lines[0]);
            var questionOrder = match.Groups[1].Value;
            lines[0] = lines[0].Substring(match.Value.Length);
            var numberOfQuestions = 1;
            if ((numberOfQuestions = TextParseEsMx.GetNumberOfBulletins(lines)) > 1)
            {
                var top = 0;
                while(!TextParseEsMx.BulletinPattern.IsMatch(lines[top]))
                {
                    top++;
                    break;
                }

                var questions = new List<string>();
                foreach (var line in lines.Skip(top))
                {
                    if (TextParseEsMx.BulletinPattern.IsMatch(line))
                    {
                        questions.Add(line.Trim());
                    }
                    else
                    {
                        questions[questions.Count() - 1] += " " + line.Trim();
                    }
                }

                var count = 1;
                var id = string.Join(Separator, this.Year, lesson.DayQuestions.Count(), questionOrder, count);
                var firstQuestion = lines.Take(top).Union(questions.Take(1));
                this.AddQuestion(lesson, firstQuestion, id, questionOrder);
                foreach (var line in questions.Skip(1))
                {
                    id = string.Join(Separator, this.Year, lesson.DayQuestions.Count(), questionOrder, ++count);
                    this.AddQuestion(lesson, new[] { line }, id, questionOrder);
                }
            }
            else if ((numberOfQuestions = this.GetNumberOfVerses(lines)) > 1)
            {
                var top = lines.Count - numberOfQuestions;
                var count = 1;
                var id = string.Join(Separator, this.Year, lesson.DayQuestions.Count(), questionOrder, count);
                this.AddQuestion(lesson, lines.Take(top), id, questionOrder);
                lesson.DayQuestions.Last().Questions.Last().QuestionText += "\n" + lines[top + 1];
                foreach(var line in lines.Skip(top + 1))
                {
                    id = string.Join(Separator, this.Year, lesson.DayQuestions.Count(), questionOrder, ++count);
                    this.AddQuestion(lesson, new[] { line }, id, questionOrder);
                }
            }
            else if ((numberOfQuestions = TextParseEsMx.GetNumberOfQuotations(lines)) > 1)
            {
                var top = lines.Count - numberOfQuestions;
                var count = 1;
                var id = string.Join(Separator, this.Year, lesson.DayQuestions.Count(), questionOrder, count);
                this.AddQuestion(lesson, lines.Take(top), id, questionOrder);
                lesson.DayQuestions.Last().Questions.Last().QuestionText += "\n" + lines[top + 1];
                foreach (var line in lines.Skip(top + 1))
                {
                    id = string.Join(Separator, this.Year, lesson.DayQuestions.Count(), questionOrder, ++count);
                    this.AddQuestion(lesson, new[] { line }, id, questionOrder);
                }
            }
            else
            {
                var id = string.Join(Separator, this.Year, lesson.DayQuestions.Count(), questionOrder);
                this.AddQuestion(lesson, lines, id, questionOrder);
            }
        }

        [Section(@"^\(No hay homilética para líderes de grupo y administrativos\)$")]
        protected void ParseLeader(Lesson lesson, IList<string> lines)
        {
        }

        [Section(@"^Copyright © Bible Study Fellowship")]
        protected void ParseEnding(Lesson lesson, IList<string> lines)
        {
        }

        private static Regex GetBibleVersePattern(IRepository repository, string culture)
        {
            var suffix = " *([0-9]+ *: *[0-9]+ *((- *[0-9]+ *(: *[0-9]+)?)?)?)";
            var books = repository.GetBibleBooks(culture);
            var pattern = "(" + string.Join("|", books.SelectMany(book => new[] { book.Name, book.Shorthand })) + ")" + suffix;
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private static Regex GetRegex(MethodInfo methodInfo)
        {
            var pattern = methodInfo.GetCustomAttributes(false).OfType<SectionAttribute>().First().Mark;

            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private static int GetNumberOfBulletins(IList<string> lines)
        {
            var count = 0;
            foreach (var line in lines)
            {
                if (TextParseEsMx.BulletinPattern.IsMatch(line))
                {
                    count++;
                }
            }

            return count;
        }

        private int GetNumberOfVerses(IList<string> lines)
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

        private static int GetNumberOfQuotations(IList<string> lines)
        {
            var count = 0;
            foreach (var line in lines)
            {
                if (TextParseEsMx.QuotationPattern.IsMatch(line))
                {
                    count++;
                }
            }

            return count;
        }

        private void AddQuestion(Lesson lesson, IEnumerable<string> lines, string id, string questionOrder)
        {
            var text = questionOrder + ". " + string.Join(string.Empty, lines);
            var question = new Question
            {
                Id = id,
                QuestionText = text,
                Quotes = this.ExtractVerse(text),
            };

            lesson.DayQuestions.Last().Questions.Add(question);
        }

        private IList<VerseItem> ExtractVerse(string text)
        {
            var items = new List<VerseItem>();
            var collection = this.versePattern.Matches(text);
            foreach (Match match in collection)
            {
                var item = new VerseItem
                {
                    Book = match.Groups[1].Value.Trim(),
                    Verse = match.Groups[2].Value.Replace(" ", string.Empty),
                };
                items.Add(item);
            }

            return items;
        }
    }
}