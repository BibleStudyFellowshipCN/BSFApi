namespace Church.BibleStudyFellowship.Models.GetBible
{
    using Church.BibleStudyFellowship.Models;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Text.RegularExpressions;

    public class BibleClient : IBible
    {
        private readonly IDictionary<string, string> bookMappings;

        private readonly Regex verseRegex;

        internal BibleClient(VerseLocator verseLocator, IDictionary<string, string> bookMappings)
        {
            var pattern = verseLocator.GetPattern();
            this.verseRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            this.bookMappings = bookMappings;
        }

        public static BibleClient Create(string culture, IRepository repository)
        {
            var books = repository.GetBibleBooks(culture);
            var bookDictionary = books.ToDictionary(book => book.Order, book => book.Name);
            var bookMappings = repository.GetBibleBooks("en-US").OrderBy(book=>book.Order)
                .ToDictionary(book => bookDictionary[book.Order], book => book.Name);
            var verseLocator = VerseLocator.Create(books);
            return new BibleClient(verseLocator, bookMappings);
        }

        public Task<IEnumerable<BibleVersion>> GetVersionsAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<BibleChapter>> GetVersesAsync(string passage)
        {
            var match = this.verseRegex.Match(passage);
            var verses = match.Groups[2].Value.Trim();
            var englishBook = this.bookMappings[match.Groups[1].Value.Trim()];
            var newPassage = englishBook + "+" + verses;
            var url = $"http://getbible.net/json?passage={newPassage}&version=cus";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent",
                                 "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident / 6.0)");
                var response = await client.GetStringAsync(url);
                var trimmed = response.Trim('(', ')', ';');
                var message = JsonConvert.DeserializeObject<Passage>(trimmed);

                return message.Book.Select(book => new BibleChapter
                {
                    Version = "cus",
                    BookOrder = book.Book_nr,
                    Order = book.Chapter_nr,
                    Culture = "zh-CN",
                    Verses = book.chapter.Select(chapter => new BibleVerse
                    {
                        Order = chapter.Value.Verse_nr,
                        Text = chapter.Value.Verse,
                    }),
                });
            }
        }
    }
}
