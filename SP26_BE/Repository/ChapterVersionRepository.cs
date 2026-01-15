using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository
{
    public class ChapterVersionRepository
    {
        private readonly StoryAI_DBContext _context;

        public ChapterVersionRepository()
        {
            _context = new StoryAI_DBContext();
        }

        public ChapterVersionRepository(StoryAI_DBContext context)
        {
            _context = context;
        }

        public async Task<ChapterVersion> CreateAsync(ChapterVersion version)
        {
            _context.ChapterVersions.Add(version);
            await _context.SaveChangesAsync();

            // Load relationships
            await _context.Entry(version)
                .Reference(v => v.Chapter)
                .LoadAsync();

            if (version.Chapter != null)
            {
                await _context.Entry(version.Chapter)
                    .Reference(c => c.Project)
                    .LoadAsync();
            }

            await _context.Entry(version)
                .Collection(v => v.Aijobs)
                .LoadAsync();

            await _context.Entry(version)
                .Collection(v => v.AnalysisReports)
                .LoadAsync();

            await _context.Entry(version)
                .Collection(v => v.ManuscriptChunks)
                .LoadAsync();

            return version;
        }

        public async Task<ChapterVersion?> GetByIdAsync(int versionId)
        {
            return await _context.ChapterVersions
                .Include(v => v.Chapter)
                    .ThenInclude(c => c.Project)
                        .ThenInclude(p => p.Author)
                .Include(v => v.Aijobs)
                .Include(v => v.AnalysisReports)
                .Include(v => v.ManuscriptChunks)
                .AsTracking()
                .FirstOrDefaultAsync(v => v.VersionId == versionId);
        }

        public async Task<List<ChapterVersion>> GetByChapterIdAsync(int chapterId)
        {
            return await _context.ChapterVersions
                .Include(v => v.Chapter)
                .Include(v => v.Aijobs)
                .Include(v => v.AnalysisReports)
                .Include(v => v.ManuscriptChunks)
                .Where(v => v.ChapterId == chapterId)
                .OrderByDescending(v => v.VersionNumber)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ChapterVersion?> GetActiveVersionByChapterIdAsync(int chapterId)
        {
            return await _context.ChapterVersions
                .Include(v => v.Chapter)
                .Include(v => v.Aijobs)
                .Include(v => v.AnalysisReports)
                .Include(v => v.ManuscriptChunks)
                .Where(v => v.ChapterId == chapterId && v.IsActive == true)
                .OrderByDescending(v => v.VersionNumber)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<ChapterVersion> UpdateAsync(ChapterVersion version)
        {
            var existingEntry = _context.ChangeTracker.Entries<ChapterVersion>()
                .FirstOrDefault(e => e.Entity.VersionId == version.VersionId);

            if (existingEntry != null)
            {
                existingEntry.State = EntityState.Detached;
            }

            _context.ChapterVersions.Update(version);
            await _context.SaveChangesAsync();

            // Reload relationships
            await _context.Entry(version)
                .Reference(v => v.Chapter)
                .LoadAsync();

            if (version.Chapter != null)
            {
                await _context.Entry(version.Chapter)
                    .Reference(c => c.Project)
                    .LoadAsync();
            }

            await _context.Entry(version)
                .Collection(v => v.Aijobs)
                .LoadAsync();

            await _context.Entry(version)
                .Collection(v => v.AnalysisReports)
                .LoadAsync();

            await _context.Entry(version)
                .Collection(v => v.ManuscriptChunks)
                .LoadAsync();

            return version;
        }

        public async Task<bool> DeleteAsync(int versionId)
        {
            var version = await _context.ChapterVersions.FindAsync(versionId);
            if (version == null)
                return false;

            _context.ChapterVersions.Remove(version);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsOwnerAsync(int versionId, int userId)
        {
            return await _context.ChapterVersions
                .Include(v => v.Chapter)
                    .ThenInclude(c => c.Project)
                .AnyAsync(v => v.VersionId == versionId && v.Chapter.Project.AuthorId == userId);
        }

        public async Task<int> GetMaxVersionNumberAsync(int chapterId)
        {
            var maxVersionNumber = await _context.ChapterVersions
                .Where(v => v.ChapterId == chapterId)
                .MaxAsync(v => (int?)v.VersionNumber);

            return maxVersionNumber ?? 0;
        }

        public async Task<bool> SetActiveVersionAsync(int versionId)
        {
            var version = await _context.ChapterVersions
                .Include(v => v.Chapter)
                .FirstOrDefaultAsync(v => v.VersionId == versionId);

            if (version == null)
                return false;

            // Deactivate all versions of this chapter
            var allVersions = await _context.ChapterVersions
                .Where(v => v.ChapterId == version.ChapterId)
                .ToListAsync();

            foreach (var v in allVersions)
            {
                v.IsActive = false;
            }

            // Activate the selected version
            version.IsActive = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CalculateWordCountAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return 0;

            // Simple word count (can be improved)
            var words = content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }
    }
}