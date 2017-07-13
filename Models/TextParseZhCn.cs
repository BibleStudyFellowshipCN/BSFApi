﻿namespace Church.BibleStudyFellowship.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class TextParseZhCn : ITextParser
    {
        private static Regex DayPattern = new Regex("^第(.)天：", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex QuestionPattern = new Regex(@"^ *(\d+)\. ", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex BulletinPattern = new Regex(@"^ *[a-z]\. ", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex QuotationPattern = new Regex(@"^“.+”$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static IDictionary<string, string> OrdinalMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"一", "一" },
            {"二", "二" },
            {"三", "三" },
            {"四", "四" },
            {"五", "五" },
            {"六", "六" },
        };

        private readonly IDictionary<Regex, MethodInfo> methodMappings;

        private readonly Regex versePattern;

        internal TextParseZhCn(int year, CultureInfo culture, Regex versePattern, IDictionary<Regex, MethodInfo> methodMappings)
        {
            this.Year = year;
            this.Culture = culture;
            this.versePattern = versePattern;
            this.methodMappings = methodMappings;
        }

        public int Year { get; }

        public CultureInfo Culture { get; }

        public static TextParseZhCn Create(int year, IRepository repository)
        {
            var methods = typeof(TextParseZhCn).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttributes(false).OfType<SectionAttribute>().Any());
            var methodMappings = methods.ToDictionary(TextParseZhCn.GetRegex);
            var culture = CultureInfo.CreateSpecificCulture("zh-CN");
            var versePattern = TextParseZhCn.GetBibleVersePattern(repository, culture.Name);

            return new TextParseZhCn(year, culture, versePattern, methodMappings);
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

        [Section(@"^BSF⑧第\d课")]
        protected void ParseFotter1(Lesson lesson, IList<string> lines)
        {
            const string Prefix = "BSF⑧第";

            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(lines.Count > 3, "At least 4 lines.");
            lesson.Id = string.Join("_", this.Year, lines[0].Substring(Prefix.Length, 1).Trim());
            lesson.Name = lines[2];
        }

        [Section("^®$")]
        protected void ParseEmpty(Lesson lesson, IList<string> lines)
        {
        }

        [Section(@"www\.bsfinternational\.org \| ")]
        protected void ParseFotter2(Lesson lesson, IList<string> lines)
        {
        }

        [Section("^背诵经文")]
        protected void ParseMemoryVerse(Lesson lesson, IList<string> lines)
        {
            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(lines.Count() > 1, "At least two lines.");
            lesson.MemoryVerse = string.Join(string.Empty, lines.Skip(1));
        }

        [Section("^第.天：")]
        protected void ParseDay(Lesson lesson, IList<string> lines)
        {
            var match = TextParseZhCn.DayPattern.Match(lines[0]);
            var title = lines[0].Substring(match.Value.Length).Trim();
            var day = new Day
            {
                Tab = TextParseZhCn.OrdinalMapping[match.Groups[1].Value],
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

            var match = TextParseZhCn.QuestionPattern.Match(lines[0]);
            var questionOrder = match.Groups[1].Value;
            lines[0] = lines[0].Substring(match.Value.Length);
            var numberOfQuestions = 1;
            if ((numberOfQuestions = TextParseZhCn.GetNumberOfBulletins(lines)) > 1)
            {
                var top = 0;
                while(!TextParseZhCn.BulletinPattern.IsMatch(lines[top]))
                {
                    top++;
                    break;
                }

                var questions = new List<string>();
                foreach (var line in lines.Skip(top))
                {
                    if (TextParseZhCn.BulletinPattern.IsMatch(line))
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
            else if ((numberOfQuestions = TextParseZhCn.GetNumberOfQuotations(lines)) > 1)
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

        [Section(@"^\（组长及班务同工的讲道培训：本周暂停）$")]
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
                if (TextParseZhCn.BulletinPattern.IsMatch(line))
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
                if (TextParseZhCn.QuotationPattern.IsMatch(line))
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