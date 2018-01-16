namespace Church.BibleStudyFellowship.Models.GetBible
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Linq;
    using System.Threading.Tasks;
    using Church.BibleStudyFellowship.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class BibleClient : IBible
    {
        public Task<IEnumerable<BibleVersion>> GetVersionsAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<BibleChapter>> GetVersesAsync(string passage)
        {
            passage = "John+3:16-17;4:5-10";
            var url = $"http://getbible.net/json?passage={passage}&version=cus";
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
