namespace Questions
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Church.BibleStudyFellowship.Models;
    using Church.BibleStudyFellowship.Models.Storage;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Questions <culture> <filename> <year> <date> <title>");
                return;
            }

            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            var culture = CultureInfo.CreateSpecificCulture(args[0]);
            var input = args[1];
            var year = int.Parse(args[2]);
            var date = DateTime.Parse(args[3]);
            var title = args[4];

            var text = File.ReadAllText(input);
            var repository = Repository.Create(configuration["AzureStorage:ConnectionString"]);
            var parser = Program.GetParser(culture, year, repository);
            var lesson = parser.Parse(text);
            repository.UpsertLessonAsync(lesson).Wait();
            Console.WriteLine(JsonConvert.SerializeObject(lesson));

            var item = new LessonItem
            {
                Id = lesson.Id,
                Name = lesson.Name,
                ProposedDate = date.ToString(parser.Culture.DateTimeFormat.LongDatePattern)
            };
            var currentStudy = repository.GetStudies(culture.Name).FirstOrDefault(study => study.Title == title);
            if (currentStudy == null)
            {
                currentStudy = new Study { Culture = parser.Culture.Name, Title = title, Lessons = new[] { item }.ToList() };
            }
            else
            {
                var currentLesson = currentStudy.Lessons.FirstOrDefault(l => l.Id == item.Id);
                if (currentLesson == null)
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

        private static AbstractTextParser GetParser(CultureInfo cultureInfo, int year, IRepository repository)
        {
            switch (cultureInfo.Name)
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
    }
}
