namespace Church.BibleStudyFellowship.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class TextParseZhTw : AbstractTextParser
    {
        private static Regex DayPattern = new Regex("^第(.)天：", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex QuestionPattern = new Regex(@"^ *(\d+)(\. *a)?\. ", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex SubQuestionPattern = new Regex(@"^ *([b-e])\. ", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static IDictionary<string, string> OrdinalMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"一", "一" },
            {"二", "二" },
            {"三", "三" },
            {"四", "四" },
            {"五", "五" },
            {"六", "六" },
        };

        private static HashSet<char> SuffixSet = new HashSet<char> { '.', '。', '?', '？', ')', '）', ':', '：' };

        internal TextParseZhTw(int year, CultureInfo culture, IDictionary<Regex, MethodInfo> methodMappings, VerseLocator verseLocator)
            : base(year, culture, methodMappings, verseLocator)
        {
        }

        public static TextParseZhTw Create(int year, IRepository repository)
        {
            const string CultureName = "zh-TW";

            var methods = typeof(TextParseZhTw).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttributes(false).OfType<SectionAttribute>().Any());
            var methodMappings = methods.ToDictionary(TextParseZhTw.GetRegex);
            var culture = CultureInfo.CreateSpecificCulture(CultureName);
            var verseLocator = VerseLocator.Create(repository.GetBibleBooks(CultureName));

            return new TextParseZhTw(year, culture, methodMappings, verseLocator);
        }

        [Section(@"^BSF⑧第\d課")]
        protected void ParseFotter1(Lesson lesson, IList<string> lines)
        {
            const string Prefix = "BSF⑧第";

            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(lines.Count > 3, "At least 4 lines.");
            lesson.Id = string.Join("_", this.Year, lines[2].Substring(Prefix.Length, lines[2].Length - Prefix.Length - 1).Trim());
            ////lesson.Id = string.Join("_", this.Year, lines[4].Substring(Prefix.Length, lines[4].Length - Prefix.Length - 1).Trim());
            lesson.Name = lines[6].Split(' ')[1];
        }

        [Section("^®$")]
        protected void ParseEmpty(Lesson lesson, IList<string> lines)
        {
        }

        [Section(@" \| www\.bsfinternational\.org")]
        protected void ParseFotter2(Lesson lesson, IList<string> lines)
        {
        }

        [Section(@"\| Adult Questions \| Lesson")]
        protected void ParseMemoryVerse(Lesson lesson, IList<string> lines)
        {
            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(lines.Count() > 1, "At least two lines.");
            lesson.MemoryVerse = string.Join(string.Empty, lines.Skip(1));
        }

        [Section("^第.天：")]
        protected void ParseDay(Lesson lesson, IList<string> lines)
        {
            var match = TextParseZhTw.DayPattern.Match(lines[0]);
            var title = lines[0].Substring(match.Value.Length).Trim();
            var day = new Day
            {
                Tab = TextParseZhTw.OrdinalMapping[match.Groups[1].Value],
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

            var match = TextParseZhTw.QuestionPattern.Match(lines[0]);
            var questionOrder = match.Groups[1].Value;
            lines[0] = lines[0].Substring(match.Value.Length);
            var questions = TextParseZhTw.GetSubquestions(lines);
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

            var match = TextParseZhTw.SubQuestionPattern.Match(lines[0]);
            lines[0] = lines[0].Substring(match.Value.Length);
            var questions = TextParseZhTw.GetSubquestions(lines);
            var parts = lesson.DayQuestions.Last().Questions.Last().Id.Split(new[] { Separator }, StringSplitOptions.None).ToList();
            var count = int.Parse(parts.Last());
            foreach (var line in questions)
            {
                var id = string.Join(Separator, parts[0], parts[1], parts[2], ++count);
                this.AddQuestion(lesson, parts[2] + ". " + match.Groups[0].Value + line, id);
            }
        }

        [Section(@"^\（組長及班務同工的講道培訓：本周暫停）$")]
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

            // Remove empty lines.
            lines = lines.Where(line => !string.IsNullOrEmpty(line)).ToList();

            char lastChar;
            do
            {
                lastChar = lines[index].TrimEnd().LastOrDefault();
                index++;
            }
            while (index < lines.Count && !TextParseZhTw.SuffixSet.Contains(lastChar));

            if (index >= lines.Count - 1)
            {
                return new[] { string.Join(" ", lines.Select(TextParseZhTw.TrimEnding)) };
            }

            var questions = new List<string> { string.Join(" ", lines.Take(index + 1).Select(TextParseZhTw.TrimEnding)) };
            questions.AddRange(lines.Skip(index + 1).Select(TextParseZhTw.TrimEnding));
            return questions;
        }

        private static string TrimEnding(string line)
        {
            return line.TrimEnd('_', ' ');
        }

        private void AddQuestion(Lesson lesson, string line, string id)
        {
            var question = new Question
            {
                Id = id,
                QuestionText = line,
                Quotes = this.ExtractVerse(line),
            };

            lesson.DayQuestions.Last().Questions.Add(question);
        }
    }
}