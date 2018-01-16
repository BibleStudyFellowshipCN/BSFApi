namespace Church.BibleStudyFellowship.Models
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBible
    {
        Task<IEnumerable<BibleVersion>> GetVersionsAsync();

        Task<IEnumerable<BibleChapter>> GetVersesAsync(string passage);
    }
}
