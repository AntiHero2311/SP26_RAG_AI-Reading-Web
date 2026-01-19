using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository
{
    public class ChapterRepository
    {
        private readonly StoryAI_DBContext _context;

        public ChapterRepository()
        {
            _context = new StoryAI_DBContext();
        }

        public ChapterRepository(StoryAI_DBContext context)
        {
            _context = context;
        }

        public async Task<Chapter> CreateAsync(Chapter chapter)
        {
            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();

            await _context.Entry(chapter)
                .Reference(c => c.Project)
                .LoadAsync();

            if (chapter.Project != null)
            {
                await _context.Entry(chapter.Project)
                    .Reference(p => p.Author)
                    .LoadAsync();
            }

            await _context.Entry(chapter)
                .Collection(c => c.ChapterVersions)
                .LoadAsync();

            return chapter;
        }

        public async Task<Chapter?> GetByIdAsync(int chapterId)
        {
            return await _context.Chapters
                .Include(c => c.Project)
                    .ThenInclude(p => p.Author)
                .Include(c => c.ChapterVersions)
                .AsTracking()
                .FirstOrDefaultAsync(c => c.ChapterId == chapterId);
        }

        public async Task<List<Chapter>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Chapters
                .Include(c => c.Project)
                .Include(c => c.ChapterVersions)
                .Where(c => c.ProjectId == projectId)
                .OrderBy(c => c.ChapterNo)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Chapter> UpdateAsync(Chapter chapter)
        {
            var existingEntry = _context.ChangeTracker.Entries<Chapter>()
                .FirstOrDefault(e => e.Entity.ChapterId == chapter.ChapterId);

            if (existingEntry != null)
            {
                existingEntry.State = EntityState.Detached;
            }

            chapter.UpdatedAt = DateTime.UtcNow;
            _context.Chapters.Update(chapter);
            await _context.SaveChangesAsync();

            await _context.Entry(chapter)
                .Reference(c => c.Project)
                .LoadAsync();

            if (chapter.Project != null)
            {
                await _context.Entry(chapter.Project)
                    .Reference(p => p.Author)
                    .LoadAsync();
            }

            await _context.Entry(chapter)
                .Collection(c => c.ChapterVersions)
                .LoadAsync();

            return chapter;
        }

        public async Task<bool> DeleteAsync(int chapterId)
        {
            var chapter = await _context.Chapters.FindAsync(chapterId);
            if (chapter == null)
                return false;

            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int projectId, int chapterNo)
        {
            return await _context.Chapters
                .AnyAsync(c => c.ProjectId == projectId && c.ChapterNo == chapterNo);
        }

        public async Task<bool> IsOwnerAsync(int chapterId, int userId)
        {
            return await _context.Chapters
                .Include(c => c.Project)
                .AnyAsync(c => c.ChapterId == chapterId && c.Project.AuthorId == userId);
        }

        public async Task<int> GetMaxChapterNoAsync(int projectId)
        {
            var maxChapterNo = await _context.Chapters
                .Where(c => c.ProjectId == projectId)
                .MaxAsync(c => (int?)c.ChapterNo);

            return maxChapterNo ?? 0;
        }
    }
}