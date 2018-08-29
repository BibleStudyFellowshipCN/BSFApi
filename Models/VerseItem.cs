namespace Church.BibleStudyFellowship.Models
{
    public class VerseItem
    {
        public string Book { get; set; }

        public string Verse { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as VerseItem;
            return other != null && this.Book == other.Book && this.Verse == other.Verse;
        }

        public override int GetHashCode()
        {
            return (this.Book + this.Verse).GetHashCode();
        }
    }
}
