namespace Church.BibleStudyFellowship.Models
{
    internal class VerseLocatorEn : VerseLocator
    {
        const string ChapterPattern = @" +(\d+)( +and +(\d+))?";

        internal VerseLocatorEn(string books)
            : base(books, ":", "-", ",", ";", VerseLocatorEn.ChapterPattern)
        {
        }
    }
}
