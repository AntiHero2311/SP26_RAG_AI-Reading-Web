using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository.Repositories
{
    public class StaffAuthorContactRepository
    {
        private readonly StoryAI_DBContext _context;

        public StaffAuthorContactRepository()
        {
            _context = new StoryAI_DBContext();
        }

        public StaffAuthorContactRepository(StoryAI_DBContext context)
        {
            _context = context;
        }

        public async Task<StaffAuthorContact?> GetByIdAsync(int contactId)
        {
            return await _context.StaffAuthorContacts
                .AsNoTracking()
                .Include(c => c.Staff)
                .Include(c => c.Author)
                .Include(c => c.StaffAuthorMessages.OrderBy(m => m.SendAt))
                .FirstOrDefaultAsync(c => c.ContactId == contactId);
        }

        public async Task<List<StaffAuthorContact>> GetByStaffIdAsync(int staffId)
        {
            return await _context.StaffAuthorContacts
                .AsNoTracking()
                .Include(c => c.Author)
                .Include(c => c.StaffAuthorMessages.OrderByDescending(m => m.SendAt).Take(1))
                .Where(c => c.StaffId == staffId)
                .OrderByDescending(c => c.ContactDate)
                .ToListAsync();
        }

        public async Task<List<StaffAuthorContact>> GetByAuthorIdAsync(int authorId)
        {
            return await _context.StaffAuthorContacts
                .AsNoTracking()
                .Include(c => c.Staff)
                .Include(c => c.StaffAuthorMessages.OrderByDescending(m => m.SendAt).Take(1))
                .Where(c => c.AuthorId == authorId)
                .OrderByDescending(c => c.ContactDate)
                .ToListAsync();
        }

        public async Task<StaffAuthorContact?> GetExistingContactAsync(int staffId, int authorId)
        {
            return await _context.StaffAuthorContacts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.StaffId == staffId && c.AuthorId == authorId);
        }

        public async Task<StaffAuthorContact> CreateAsync(StaffAuthorContact contact)
        {
            _context.StaffAuthorContacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task<StaffAuthorContact?> UpdateAsync(StaffAuthorContact contact)
        {
            var existingContact = await _context.StaffAuthorContacts
                .FirstOrDefaultAsync(c => c.ContactId == contact.ContactId);

            if (existingContact == null) return null;

            existingContact.Status = contact.Status;

            await _context.SaveChangesAsync();
            return existingContact;
        }

        public async Task<bool> DeleteAsync(int contactId)
        {
            var contact = await _context.StaffAuthorContacts
                .Include(c => c.StaffAuthorMessages)
                .FirstOrDefaultAsync(c => c.ContactId == contactId);

            if (contact == null) return false;

            _context.StaffAuthorContacts.Remove(contact);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int contactId)
        {
            return await _context.StaffAuthorContacts.AnyAsync(c => c.ContactId == contactId);
        }

        public async Task<int> GetUnreadCountByUserAsync(int userId, string userType)
        {
            if (userType == "Staff")
            {
                return await _context.StaffAuthorMessages
                    .Where(m => m.Contact.StaffId == userId && m.SenderType == "Author" && m.IsRead == false)
                    .CountAsync();
            }
            else // Author
            {
                return await _context.StaffAuthorMessages
                    .Where(m => m.Contact.AuthorId == userId && m.SenderType == "Staff" && m.IsRead == false)
                    .CountAsync();
            }
        }
    }
}
