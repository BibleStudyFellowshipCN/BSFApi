namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;

    public class Question
    {
        public Question()
        {
            this.TextParts = new List<TextPart>();
            this.Quotes = new List<VerseItem>();
        }

        public string Id { get; set; }

        public IList<TextPart> TextParts { get; set; }

        public string QuestionText { get; set; }

        public string Answer { get; set; }

        public IList<VerseItem> Quotes { get; set; }
    }
}
