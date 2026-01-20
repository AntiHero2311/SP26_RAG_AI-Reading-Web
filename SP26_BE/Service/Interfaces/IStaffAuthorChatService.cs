using Repository.Models;

namespace Service.Interfaces
{
    public interface IStaffAuthorChatService
    {
        // Contact methods
        Task<(bool Success, string Message, StaffAuthorContact? Contact)> GetContactByIdAsync(int contactId);
        Task<(bool Success, string Message, List<StaffAuthorContact>? Contacts)> GetContactsByStaffIdAsync(int staffId);
        Task<(bool Success, string Message, List<StaffAuthorContact>? Contacts)> GetContactsByAuthorIdAsync(int authorId);
        Task<(bool Success, string Message, StaffAuthorContact? Contact)> CreateOrGetContactAsync(int staffId, int authorId);
        Task<(bool Success, string Message, StaffAuthorContact? Contact)> UpdateContactStatusAsync(int contactId, string status);
        Task<(bool Success, string Message)> DeleteContactAsync(int contactId);
        Task<(bool Success, string Message)> ValidateContactAccessAsync(int contactId, int userId, string userType);

        // Message methods
        Task<(bool Success, string Message, StaffAuthorMessage? ChatMessage)> GetMessageByIdAsync(int messageId);
        Task<(bool Success, string Message, List<StaffAuthorMessage>? Messages)> GetMessagesByContactIdAsync(int contactId);
        Task<(bool Success, string Message, StaffAuthorMessage? ChatMessage)> CreateMessageAsync(int contactId, string senderType, int senderId, string messageText);
        Task<(bool Success, string Message)> DeleteMessageAsync(int messageId);
        Task<(bool Success, string Message)> MarkMessagesAsReadAsync(int contactId, string readerType);
        Task<(bool Success, string Message, int UnreadCount)> GetUnreadCountAsync(int userId, string userType);
    }
}
