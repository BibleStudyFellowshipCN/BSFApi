namespace Church.BibleStudyFellowship.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class VersePatternCollection : IReadOnlyDictionary<string, Regex>
    {
        private static HashSet<string> Cultures = new HashSet<string> { "en-US", "zh-CN", "zh-TW", "es-MX" };

        private static IDictionary<string, Regex> patters;

        private IRepository repository;

        static VersePatternCollection()
        {
            VersePatternCollection.patters = new Dictionary<string, Regex>(StringComparer.OrdinalIgnoreCase);
        }

        internal VersePatternCollection(IRepository repository)
        {
            this.repository = repository;
        }

        public Regex this[string key]
        {
            get
            {
                return VersePatternCollection.patters.TryGetValue(key, out var value) ?
                    value : this.GeneratePattern(key);
            }
        }

        public IEnumerable<string> Keys => VersePatternCollection.Cultures;

        public IEnumerable<Regex> Values => this.Keys.Select(key => this[key]);

        public int Count => VersePatternCollection.patters.Count;

        public static VersePatternCollection Create(IRepository repository)
        {
            return new VersePatternCollection(repository);
        }

        public bool ContainsKey(string key)
        {
            return VersePatternCollection.Cultures.Contains(key);
        }

        public IEnumerator<KeyValuePair<string, Regex>> GetEnumerator()
        {
            this.Values.ToArray();
            return VersePatternCollection.patters.GetEnumerator();
        }

        public bool TryGetValue(string key, out Regex value)
        {
            if(VersePatternCollection.Cultures.Contains(key))
            {
                value = this[key];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return VersePatternCollection.Cultures.GetEnumerator();
        }

        private Regex GeneratePattern(string culture)
        {
            var suffix = "( *(([0-9]+) *:( *[0-9]+ *((- *[0-9]+ *(: *[0-9]+)?)?)?))( *,(( *[0-9]+ *((- *[0-9]+ *(: *[0-9]+)?)?)?)))*( *;( *(([0-9]+) *:( *[0-9]+ *((- *[0-9]+ *(: *[0-9]+)?)?)?))( *,(( *[0-9]+ *((- *[0-9]+ *(: *[0-9]+)?)?)?)))*))*)";
            var books = this.repository.GetBibleBooks(culture);
            var pattern = "(" + string.Join("|", books.SelectMany(book => new[] { book.Name, book.Shorthand })) + ")" + suffix;
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}
