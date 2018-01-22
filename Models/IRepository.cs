namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository
    {
        Task UpsertStudyAsync(Study study);

        Task<IEnumerable<Study>> GetStudiesAsync(string culture);

        Task<Study> GetStudyAsync(string culture, string title);

        Task UpsertLessonAsync(Lesson lesson);

        Task<IEnumerable<Lesson>> GetLessonsAsync(string culture);

        Task<Lesson> GetLessonAsync(string culture, string id);

        Task AddFeedbackAsync(Feedback feedback);

        IEnumerable<Feedback> GetFeedback();

        Task AddBibleBooksAsync(IEnumerable<BibleBook> books);

        Task<IEnumerable<BibleBook>> GetBibleBooksAsync(string culture);
    }
}
