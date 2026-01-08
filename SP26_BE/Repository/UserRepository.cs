using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository
{
    public class UserRepository
    {
        private readonly StoryAI_DBContext _context;

            public UserRepository()
        {
            _context = new StoryAI_DBContext();
        }

        public UserRepository(StoryAI_DBContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive == true);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive == true);
        }

        public async Task<User?> GetProfileByIdAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == userId && u.IsActive == true)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetAllUsersAsync(bool includeInactive = false)
        {
            var query = _context.Users
                .Include(u => u.Projects)
                .Include(u => u.Comments)
                .Include(u => u.FollowFollowers)
                .Include(u => u.FollowAuthors)
                .AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(u => u.IsActive == true);
            }

            return await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<(List<User> Users, int TotalCount)> GetAllUsersPaginatedAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null,
            string? role = null,
            bool includeInactive = false)
        {
            var query = _context.Users
                .Include(u => u.Projects)
                .Include(u => u.Comments)
                .Include(u => u.FollowFollowers)
                .Include(u => u.FollowAuthors)
                .AsNoTracking();

            // Filter by active status
            if (!includeInactive)
            {
                query = query.Where(u => u.IsActive == true);
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => 
                    u.FullName.ToLower().Contains(searchTerm) || 
                    u.Email.ToLower().Contains(searchTerm));
            }

            // Role filter
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<User> UpdateAsync(User user)
        {
            var existingEntry = _context.ChangeTracker.Entries<User>()
                .FirstOrDefault(e => e.Entity.UserId == user.UserId);
            
            if (existingEntry != null)
            {
                existingEntry.State = EntityState.Detached;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UpdateProfileAsync(int userId, string fullName, string? avatarUrl)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return false;

            user.FullName = fullName;
            user.AvatarUrl = avatarUrl;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.IsActive == true);
        }

        public async Task<User?> GetByPasswordResetTokenAsync(string resetToken)
        {
            return await _context.Users
                .AsTracking()
                .FirstOrDefaultAsync(u => u.PasswordResetToken == resetToken 
                    && u.PasswordResetTokenExpiryTime > DateTime.UtcNow 
                    && u.IsActive == true);
        }
    }
}
