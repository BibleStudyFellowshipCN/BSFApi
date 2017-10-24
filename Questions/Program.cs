namespace Questions
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Church.BibleStudyFellowship.Models;
    using Church.BibleStudyFellowship.Models.PdfBox;
    using Church.BibleStudyFellowship.Models.Storage;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 7)
            {
                Console.WriteLine("Questions <culture> <filename> <year> <date> <title> <lesson format> <lesson number>");
                return;
            }

            var appSettings = ConfigurationManager.AppSettings;

            var culture = CultureInfo.CreateSpecificCulture(args[0]);
            var input = args[1];
            var year = int.Parse(args[2]);
            var date = DateTime.Parse(args[3]);
            var title = args[4];
            var lessonFormat = args[5];
            var lessonNumber = int.Parse(args[6]);

            var text = Utilities.ReadFromPdf(input);
            var repository = Repository.Create(appSettings["ConnectionString"]);
            var parser = Program.GetParser(culture, year, repository);
            var lesson = parser.Parse(text);
            repository.UpsertLessonAsync(lesson).Wait();

            var item = new LessonItem
            {
                Id = parser.Year + "_" + lessonNumber,
                Order = string.Format(lessonFormat, lessonNumber),
                Name = lesson.Name,
                ProposedDate = date.ToString(parser.Culture.DateTimeFormat.LongDatePattern)
            };
            var currentStudy = repository.GetStudies(culture.Name).FirstOrDefault(study => study.Title == title);
            if(currentStudy==null)
            {
                currentStudy = new Study { Culture = parser.Culture.Name, Title = title, Lessons = new[] { item }.ToList() };
            }
            else
            {
                var currentLesson = currentStudy.Lessons.FirstOrDefault(l => l.Id == item.Id);
                if(currentLesson==null)
                {
                    currentStudy.Lessons.Add(item);
                }
                else
                {
                    currentStudy.Lessons.Remove(currentLesson);
                    currentStudy.Lessons.Add(item);
                }
            }

            repository.UpsertStudyAsync(currentStudy).Wait();
        }

        private static AbstractParser GetParser(CultureInfo cultureInfo, int year, IRepository repository)
        {
            switch(cultureInfo.Name)
            {
                case "en-US":
                    return TextParseEnUs.Create(year, repository);
                case "es-MX":
                    return TextParseEsMx.Create(year, repository);
                case "zh-CN":
                    return TextParseZhCn.Create(year, repository);
                case "zh-TW":
                    return TextParseZhTw.Create(year, repository);
                default:
                    throw new NotImplementedException("Could not find the culture");
            }
        }

        private static Regex GetBibleVersePattern(IRepository repository, string culture)
        {
            var suffix = " *([0-9]+ *: *[0-9]+ *((- *[0-9]+ *(: *[0-9]+)?)?)?)";
            var books = repository.GetBibleBooks(culture);
            var pattern = "(" + string.Join("|", books.SelectMany(book => new[] { book.Name, book.Shorthand })) + ")" + suffix;
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}
