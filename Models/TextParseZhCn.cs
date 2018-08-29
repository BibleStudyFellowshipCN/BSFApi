namespace Church.BibleStudyFellowship.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class TextParseZhCn : AbstractTextParser
    {
        private static Regex DayPattern = new Regex("^第(.)天：", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex QuestionPattern = new Regex(@"^ *(\d+) *(\. *a)?\. ", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        internal TextParseZhCn(int year, CultureInfo culture, IDictionary<Regex, MethodInfo> methodMappings, VerseLocator verseLocator)
            : base(year, culture, methodMappings, verseLocator)
        {
        }

        public static TextParseZhCn Create(int year, IRepository repository)
        {
            const string CultureName = "zh-CN";

            var methods = typeof(TextParseZhCn).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttributes(false).OfType<SectionAttribute>().Any());
            var methodMappings = methods.ToDictionary(TextParseZhCn.GetRegex);
            var culture = CultureInfo.CreateSpecificCulture(CultureName);
            var verseLocator = VerseLocator.Create(repository.GetBibleBooksAsync(CultureName).Result);

            return new TextParseZhCn(year, culture, methodMappings, verseLocator);
        }

        [Section(@"^BSF®第\d+课")]
        protected void ParseFotter1(Lesson lesson, IList<string> lines)
        {
            const string Prefix = "BSF®";

            ExceptionUtilities.ThrowInvalidOperationExceptionIfFalse(lines.Count > 1, "At least 2 lines.");
            var name = lines[0].Substring(Prefix.Length).Trim();
            var order = AbstractTextParser.ExtractOrder(name);
            lesson.Id = this.Year + "_" + order.ToString("D2");
            lesson.Name = lines[1].Split(' ')[1] + " " + name;
        }

        [Section("^®$")]
        protected void ParseEmpty(Lesson lesson, IList<string> lines)
        {
        }

        [Section(@" \| www\.bsfinternational\.org")]
        protected void ParseFotter2(Lesson lesson, IList<string> lines)
        {
        }

        [Section(@"背诵经文")]
        protected void ParseMemoryVerse(Lesson lesson, IList<string> lines)
        {
            ExceptionUtilities.ThrowInvalidOperationExceptionIfFalse(lines.Count() > 1, "At least two lines.");
            lesson.MemoryVerse = string.Join(string.Empty, lines.Skip(2));
        }

        [Section("^第.天：")]
        protected void ParseDay(Lesson lesson, IList<string> lines)
        {
            var match = TextParseZhCn.DayPattern.Match(lines[0]);
            var title = lines[0].Substring(match.Value.Length).Trim();
            if (lines.Count > 1)
            {
                title += " " + string.Join(" ", lines.Skip(1).Select(line => line.Trim()));
            }

            var day = new Day
            {
                Tab = TextParseZhCn.OrdinalMapping[match.Groups[1].Value],
                Title = title,
                ReadVerse = this.ExtractVerse(title),
            };
            lesson.DayQuestions.Add(day);
        }

        [Section(@"^ *\d+ *\. ")]
        protected void ParseQuestion(Lesson lesson, IList<string> lines)
        {
            const string Separator = "_";

            var match = TextParseZhCn.QuestionPattern.Match(lines[0]);
            var questionOrder = match.Groups[1].Value;
            lines[0] = lines[0].Substring(match.Value.Length);
            var questions = TextParseZhCn.GetSubquestions(lines);
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

            var match = TextParseZhCn.SubQuestionPattern.Match(lines[0]);
            lines[0] = lines[0].Substring(match.Value.Length);
            var questions = TextParseZhCn.GetSubquestions(lines);
            var parts = lesson.DayQuestions.Last().Questions.Last().Id.Split(new[] { Separator }, StringSplitOptions.None).ToList();
            var count = int.Parse(parts.Last());
            foreach (var line in questions)
            {
                var id = string.Join(Separator, parts[0], parts[1], parts[2], ++count);
                this.AddQuestion(lesson, parts[2] + ". " + match.Groups[0].Value + line, id);
            }
        }

        [Section(@"^\（组长及班务同工的讲道培训：本周暂停）$")]
        protected void ParseLeader(Lesson lesson, IList<string> lines)
        {
        }

        [Section(@"^COPYRIGHT.Bible Study Fellowship")]
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
            while (index < lines.Count && !TextParseZhCn.SuffixSet.Contains(lastChar));

            if (index >= lines.Count - 1)
            {
                return new[] { string.Join(" ", lines.Select(TextParseZhCn.TrimEnding)) };
            }

            var questions = new List<string> { string.Join(" ", lines.Take(index + 1).Select(TextParseZhCn.TrimEnding)) };
            questions.AddRange(lines.Skip(index + 1).Select(TextParseZhCn.TrimEnding));
            return questions;
        }

        private static string TrimEnding(string line)
        {
            return line.TrimEnd('_', ' ');
        }
    }
}