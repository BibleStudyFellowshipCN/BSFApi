namespace Church.BibleStudyFellowship.Models
{
    public interface ITextParser
    {
        Lesson Parse(string input);
    }
}
