﻿namespace Church.BibleStudyFellowship.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class TextParseEnUs : AbstractTextParser
    {
        private static Regex DayPattern = new Regex("^([A-Z]+) DAY:", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex QuestionPattern = new Regex(@"^ *(\d+) *(\. *a)?\. ", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex SubQuestionPattern = new Regex(@"^ *([b-e])\. ", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static IDictionary<string, string> OrdinalMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"First", "1ST" },
            {"Second", "2ND" },
            {"Third", "3RD" },
            {"Fourth", "4TH" },
            {"Fifth", "5TH" },
            {"Sixth", "6TH" },
        };

        private static HashSet<char> SuffixSet = new HashSet<char> { '.', '?', ')', ':' };

        internal TextParseEnUs(int year, CultureInfo culture, IDictionary<Regex, MethodInfo> methodMappings, VerseLocator verseLocator)
            : base(year, culture, methodMappings, verseLocator)
        {
        }

        public static TextParseEnUs Create(int year, IRepository repository)
        {
            const string CultureName = "en-US";

            var methods = typeof(TextParseEnUs).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttributes(false).OfType<SectionAttribute>().Any());
            var methodMappings = methods.ToDictionary(TextParseEnUs.GetRegex);
            var culture = CultureInfo.CreateSpecificCulture(CultureName);
            var verseLocator = VerseLocator.Create(repository.GetBibleBooksAsync(CultureName).Result);

            return new TextParseEnUs(year, culture, methodMappings, verseLocator);
        }

        [Section(@"Lesson \d+ \| www\.bsfinternational\.org")]
        protected void ParseFotter1(Lesson lesson, IList<string> lines)
        {
            const string Prefix = "BSF® ";

            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(lines.Count > 3, "At least 4 lines.");
            var name = lines[1].Substring(Prefix.Length).Trim();
            var order = AbstractTextParser.ExtractOrder(name);
            lesson.Id = this.Year + "_" + order.ToString("D2");
            lesson.Name = lines[3] + " "+ name;
        }

        [Section("^®$")]
        protected void ParseEmpty(Lesson lesson, IList<string> lines)
        {
        }

        [Section(@"www\.bsfinternational\.org \| ")]
        protected void ParseFotter2(Lesson lesson, IList<string> lines)
        {
        }

        [Section("^Scripture Memory Verse")]
        protected void ParseMemoryVerse(Lesson lesson, IList<string> lines)
        {
            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(lines.Count() > 1, "At least two lines.");
            lesson.MemoryVerse = string.Join(string.Empty, lines.Skip(1));
        }

        [Section("^ *[A-Z]+ DAY:")]
        protected void ParseDay(Lesson lesson, IList<string> lines)
        {
            var firstLine = lines[0].Trim();
            var match = TextParseEnUs.DayPattern.Match(firstLine);
            var title = firstLine.Substring(match.Value.Length).Trim();
            if (lines.Count > 1)
            {
                title += " " + string.Join(" ", lines.Skip(1).Select(line => line.Trim()));
            }

            var day = new Day
            {
                Tab = TextParseEnUs.OrdinalMapping[match.Groups[1].Value],
                Title = title,
                ReadVerse = this.ExtractVerse(title),
            };
            lesson.DayQuestions.Add(day);
        }

        [Section(@"^ *\d+ *\. ")]
        protected void ParseQuestion(Lesson lesson, IList<string> lines)
        {
            const string Separator = "_";

            var match = TextParseEnUs.QuestionPattern.Match(lines[0]);
            var questionOrder = match.Groups[1].Value;
            lines[0] = lines[0].Substring(match.Value.Length);
            var questions = TextParseEnUs.GetSubquestions(lines);
            var count = 1;
            foreach (var line in questions)
            {
                var id = string.Join(Separator, this.Year, lesson.DayQuestions.Count(), questionOrder, count++);
                this.AddQuestion(lesson, match.Groups[0].Value + line, id);
            }
        }

        [Section(@"^ *[b-e]\. ")]
        protected void ParseSubquestion(Lesson lesson, IList<string> lines)
        {
            const string Separator = "_";

            var match = TextParseEnUs.SubQuestionPattern.Match(lines[0]);
            lines[0] = lines[0].Substring(match.Value.Length);
            var questions = TextParseEnUs.GetSubquestions(lines);
            var parts = lesson.DayQuestions.Last().Questions.Last().Id.Split(new[] { Separator }, StringSplitOptions.None).ToList();
            var count = int.Parse(parts.Last());
            foreach (var line in questions)
            {
                var id = string.Join(Separator, parts[0], parts[1], parts[2], ++count);
                this.AddQuestion(lesson, parts[2] + ". " + match.Groups[0].Value + line, id);
            }
        }

        [Section(@"^\(No homiletics for group and administrative leaders\)$")]
        protected void ParseLeader(Lesson lesson, IList<string> lines)
        {
        }

        [Section(@"^Copyright © Bible Study Fellowship")]
        protected void ParseEnding(Lesson lesson, IList<string> lines)
        {
        }

        private static IList<string> GetSubquestions(IList<string> lines)
        {
            var index = 0;
            char lastChar;
            do
            {
                lastChar = lines[index].TrimEnd().LastOrDefault();
                index++;
            }
            while (index < lines.Count && !TextParseEnUs.SuffixSet.Contains(lastChar));

            if (index >= lines.Count - 1)
            {
                return new[] { string.Join(" ", lines.Select(TextParseEnUs.TrimEnding)) };
            }

            var questions = new List<string> { string.Join(" ", lines.Take(index + 1).Select(TextParseEnUs.TrimEnding)) };
            questions.AddRange(lines.Skip(index + 1).Select(TextParseEnUs.TrimEnding));
            return questions;
        }

        private static string TrimEnding(string line)
        {
            return line.TrimEnd('_', ' ');
        }
    }
}