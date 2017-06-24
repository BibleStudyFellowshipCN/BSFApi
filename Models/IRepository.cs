namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository
    {
        Task AddStudyAsync(Study study);

        IEnumerable<Study> GetStudies(string culture);

        Task AddLessonAsync(Lesson lesson);

        Lesson GetLesson(string culture, string id);

        Task AddFeedbackAsync(Feedback feedback);

        IEnumerable<Feedback> GetFeedback();

        Task AddBibleBooksAsync(IEnumerable<BibleBook> books);

        IEnumerable<BibleBook> GetBibleBooks(string culture);

        Task AddBibleVersesAsync(IEnumerable<BibleVerse> verses);
    }
}
