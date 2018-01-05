namespace Church.BibleStudyFellowship.Models
{
    internal class VerseLocatorEs : VerseLocator
    {
        const string ChapterPattern = @" +(\d+)( +y +(\d+))?";

        internal VerseLocatorEs(string books)
            : base(books, ":", "-", ",", ";", VerseLocatorEs.ChapterPattern)
        {
        }
    }
}
