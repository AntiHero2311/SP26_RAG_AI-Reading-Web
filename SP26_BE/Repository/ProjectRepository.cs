using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository
{
    public class ProjectRepository
    {
        private readonly StoryAI_DBContext _context;

        public ProjectRepository()
        {
            _context = new StoryAI_DBContext();
        }

        public ProjectRepository(StoryAI_DBContext context)
        {
            _context = context;
        }

        public async Task<Project> CreateAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            
            // ✅ Load Author relationship sau khi save
            await _context.Entry(project)
                .Reference(p => p.Author)
                .LoadAsync();
            
            return project;
        }

        public async Task<Project?> GetByIdAsync(int projectId, bool includeDeleted = false)
        {
            var query = _context.Projects
                .Include(p => p.Author)
                .Include(p => p.Chapters)
                .Include(p => p.ChatSessions)
                .Include(p => p.Genres)
                .AsTracking();

            if (!includeDeleted)
            {
                query = query.Where(p => p.IsDeleted != true);
            }

            return await query.FirstOrDefaultAsync(p => p.ProjectId == projectId);
        }

        public async Task<List<Project>> GetByAuthorIdAsync(int authorId, bool includeDraft = true)
        {
            var query = _context.Projects
                .Include(p => p.Author)
                .Include(p => p.Chapters)
                .Include(p => p.ChatSessions)
                .Include(p => p.Genres)
                .Where(p => p.AuthorId == authorId && p.IsDeleted != true)
                .AsNoTracking();

            if (!includeDraft)
            {
                query = query.Where(p => p.Status != "Draft");
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project> UpdateAsync(Project project)
        {
            var existingEntry = _context.ChangeTracker.Entries<Project>()
                .FirstOrDefault(e => e.Entity.ProjectId == project.ProjectId);
            
            if (existingEntry != null)
            {
                existingEntry.State = EntityState.Detached;
            }

            project.UpdatedAt = DateTime.UtcNow;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            
            // ✅ Load relationships sau khi update
            await _context.Entry(project)
                .Reference(p => p.Author)
                .LoadAsync();
            await _context.Entry(project)
                .Collection(p => p.Chapters)
                .LoadAsync();
            await _context.Entry(project)
                .Collection(p => p.ChatSessions)
                .LoadAsync();
            await _context.Entry(project)
                .Collection(p => p.Genres)
                .LoadAsync();
            
            return project;
        }

        public async Task<bool> SoftDeleteAsync(int projectId)
        {
            var project = await GetByIdAsync(projectId);
            if (project == null)
                return false;

            project.IsDeleted = true;
            project.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsOwnerAsync(int projectId, int userId)
        {
            return await _context.Projects
                .AnyAsync(p => p.ProjectId == projectId && p.AuthorId == userId);
        }
    }
}