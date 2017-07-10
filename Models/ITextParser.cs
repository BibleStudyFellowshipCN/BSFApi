using System.Globalization;

namespace Church.BibleStudyFellowship.Models
{
    public interface ITextParser
    {
        int Year { get; }

        CultureInfo Culture { get; }

        Lesson Parse(string input);
    }
}
