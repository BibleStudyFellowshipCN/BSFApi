namespace Church.BibleStudyFellowship.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class TextParseEnUs : AbstractParser
    {
        private static Regex DayPattern = new Regex("^([A-Z]+) DAY:", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex QuestionPattern = new Regex(@"^ *(\d+)\. ", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex BulletinPattern = new Regex(@"^ *[a-z]\. ", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex QuotationPattern = new Regex(@"^“.+”$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static IDictionary<string, string> OrdinalMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"First", "1ST" },
            {"Second", "2ND" },
            {"Third", "3RD" },
            {"Fourth", "4TH" },
            {"Fifth", "5TH" },
            {"Sixth", "6TH" },
        };

        internal TextParseEnUs(int year, CultureInfo culture, IDictionary<Regex, MethodInfo> methodMappings, Regex versePattern)
            : base(year, culture, methodMappings, versePattern)
        {
        }

        public static TextParseEnUs Create(int year, IRepository repository)
        {
            const string CultureName = "en-US";

            var methods = typeof(TextParseEnUs).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttributes(false).OfType<SectionAttribute>().Any());
            var methodMappings = methods.ToDictionary(TextParseEnUs.GetRegex);
            var culture = CultureInfo.CreateSpecificCulture(CultureName);
            var versePattern = TextParseEnUs.GetBibleVersePattern(repository, culture.Name);

            return new TextParseEnUs(year, culture, methodMappings, versePattern);
        }

        [Section(@"Lesson \d+ \| www\.bsfinternational\.org")]
        protected void ParseFotter1(Lesson lesson, IList<string> lines)
        {
            const string Prefix = "BSF® Lesson ";

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

        [Section("^Scripture Memory Verse")]
        protected void ParseMemoryVerse(Lesson lesson, IList<string> lines)
        {
            ExceptionUtilities.ThowInvalidOperationExceptionIfFalse(lines.Count() > 1, "At least two lines.");
            lesson.MemoryVerse = string.Join(string.Empty, lines.Skip(1));
        }

        [Section("^[A-Z]+ DAY:")]
        protected void ParseDay(Lesson lesson, IList<string> lines)
        {
            var match = TextParseEnUs.DayPattern.Match(lines[0]);
            var title = lines[0].Substring(match.Value.Length).Trim();
            var day = new Day
            {
                Tab = TextParseEnUs.OrdinalMapping[match.Groups[1].Value],
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

            var match = TextParseEnUs.QuestionPattern.Match(lines[0]);
            var questionOrder = match.Groups[1].Value;
            lines[0] = lines[0].Substring(match.Value.Length);
            var numberOfQuestions = 1;
            if ((numberOfQuestions = TextParseEnUs.GetNumberOfBulletins(lines)) > 1)
            {
                var top = 0;
                while(!TextParseEnUs.BulletinPattern.IsMatch(lines[top]))
                {
                    top++;
                    break;
                }

                var questions = new List<string>();
                foreach (var line in lines.Skip(top))
                {
                    if (TextParseEnUs.BulletinPattern.IsMatch(line))
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
            else if ((numberOfQuestions = TextParseEnUs.GetNumberOfQuotations(lines)) > 1)
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

        [Section(@"^\(No homiletics for group and administrative leaders\)$")]
        protected void ParseLeader(Lesson lesson, IList<string> lines)
        {
        }

        [Section(@"^Copyright © Bible Study Fellowship")]
        protected void ParseEnding(Lesson lesson, IList<string> lines)
        {
        }

        private static int GetNumberOfBulletins(IList<string> lines)
        {
            var count = 0;
            foreach (var line in lines)
            {
                if (TextParseEnUs.BulletinPattern.IsMatch(line))
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
                if (TextParseEnUs.QuotationPattern.IsMatch(line))
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
    }
}