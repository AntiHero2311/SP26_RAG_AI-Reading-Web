using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository.Repositories
{
    public class StaffAuthorMessageRepository
    {
        private readonly StoryAI_DBContext _context;

        public StaffAuthorMessageRepository()
        {
            _context = new StoryAI_DBContext();
        }

        public StaffAuthorMessageRepository(StoryAI_DBContext context)
        {
            _context = context;
        }

        public async Task<StaffAuthorMessage?> GetByIdAsync(int messageId)
        {
            return await _context.StaffAuthorMessages
                .AsNoTracking()
                .Include(m => m.Contact)
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.MessageId == messageId);
        }

        public async Task<List<StaffAuthorMessage>> GetByContactIdAsync(int contactId)
        {
            return await _context.StaffAuthorMessages
                .AsNoTracking()
                .Include(m => m.Sender)
                .Where(m => m.ContactId == contactId)
                .OrderBy(m => m.SendAt)
                .ToListAsync();
        }

        public async Task<StaffAuthorMessage> CreateAsync(StaffAuthorMessage message)
        {
            _context.StaffAuthorMessages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<bool> DeleteAsync(int messageId)
        {
            var message = await _context.StaffAuthorMessages
                .FirstOrDefaultAsync(m => m.MessageId == messageId);

            if (message == null) return false;

            _context.StaffAuthorMessages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAsReadAsync(int contactId, string readerType)
        {
            // Mark messages as read for the reader (opposite sender type)
            string senderType = readerType == "Staff" ? "Author" : "Staff";

            var messages = await _context.StaffAuthorMessages
                .Where(m => m.ContactId == contactId && m.SenderType == senderType && m.IsRead == false)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return messages.Count;
        }

        public async Task<int> GetUnreadCountByContactAsync(int contactId, string readerType)
        {
            string senderType = readerType == "Staff" ? "Author" : "Staff";

            return await _context.StaffAuthorMessages
                .Where(m => m.ContactId == contactId && m.SenderType == senderType && m.IsRead == false)
                .CountAsync();
        }

        public async Task<List<StaffAuthorMessage>> GetLatestMessagesAsync(int contactId, int count)
        {
            return await _context.StaffAuthorMessages
                .AsNoTracking()
                .Include(m => m.Sender)
                .Where(m => m.ContactId == contactId)
                .OrderByDescending(m => m.SendAt)
                .Take(count)
                .OrderBy(m => m.SendAt)
                .ToListAsync();
        }
    }
}
