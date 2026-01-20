using Repository;
using Repository.Models;
using Repository.Repositories;
using Service.Interfaces;

namespace Service.Services
{
    public class StaffAuthorChatService : IStaffAuthorChatService
    {
        private readonly StaffAuthorContactRepository _contactRepository;
        private readonly StaffAuthorMessageRepository _messageRepository;
        private readonly UserRepository _userRepository;

        public StaffAuthorChatService(
            StaffAuthorContactRepository contactRepository,
            StaffAuthorMessageRepository messageRepository,
            UserRepository userRepository)
        {
            _contactRepository = contactRepository;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        // Contact methods
        public async Task<(bool Success, string Message, StaffAuthorContact? Contact)> GetContactByIdAsync(int contactId)
        {
            try
            {
                var contact = await _contactRepository.GetByIdAsync(contactId);
                if (contact == null)
                {
                    return (false, "Contact not found.", null);
                }

                return (true, "Contact retrieved successfully.", contact);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving contact: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, List<StaffAuthorContact>? Contacts)> GetContactsByStaffIdAsync(int staffId)
        {
            try
            {
                var staffExists = await _userRepository.GetByIdAsync(staffId);
                if (staffExists == null)
                {
                    return (false, "Staff not found.", null);
                }

                var contacts = await _contactRepository.GetByStaffIdAsync(staffId);
                return (true, "Contacts retrieved successfully.", contacts);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving contacts: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, List<StaffAuthorContact>? Contacts)> GetContactsByAuthorIdAsync(int authorId)
        {
            try
            {
                var authorExists = await _userRepository.GetByIdAsync(authorId);
                if (authorExists == null)
                {
                    return (false, "Author not found.", null);
                }

                var contacts = await _contactRepository.GetByAuthorIdAsync(authorId);
                return (true, "Contacts retrieved successfully.", contacts);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving contacts: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, StaffAuthorContact? Contact)> CreateOrGetContactAsync(int staffId, int authorId)
        {
            try
            {
                // Validate staff
                var staff = await _userRepository.GetByIdAsync(staffId);
                if (staff == null)
                {
                    return (false, "Staff not found.", null);
                }

                // Check if staff role is Staff or Admin
                if (staff.Role != "Staff" && staff.Role != "Admin")
                {
                    return (false, "User is not a staff member.", null);
                }

                // Validate author
                var author = await _userRepository.GetByIdAsync(authorId);
                if (author == null)
                {
                    return (false, "Author not found.", null);
                }

                // Check if contact already exists
                var existingContact = await _contactRepository.GetExistingContactAsync(staffId, authorId);
                if (existingContact != null)
                {
                    return (true, "Contact already exists.", existingContact);
                }

                // Create new contact
                var contact = new StaffAuthorContact
                {
                    StaffId = staffId,
                    AuthorId = authorId,
                    ContactDate = DateTime.UtcNow,
                    Status = "Active"
                };

                var createdContact = await _contactRepository.CreateAsync(contact);
                return (true, "Contact created successfully.", createdContact);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating contact: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, StaffAuthorContact? Contact)> UpdateContactStatusAsync(int contactId, string status)
        {
            try
            {
                var existingContact = await _contactRepository.GetByIdAsync(contactId);
                if (existingContact == null)
                {
                    return (false, "Contact not found.", null);
                }

                existingContact.Status = status;
                var updatedContact = await _contactRepository.UpdateAsync(existingContact);

                return (true, "Contact status updated successfully.", updatedContact);
            }
            catch (Exception ex)
            {
                return (false, $"Error updating contact: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteContactAsync(int contactId)
        {
            try
            {
                var exists = await _contactRepository.ExistsAsync(contactId);
                if (!exists)
                {
                    return (false, "Contact not found.");
                }

                var deleted = await _contactRepository.DeleteAsync(contactId);
                if (!deleted)
                {
                    return (false, "Failed to delete contact.");
                }

                return (true, "Contact deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting contact: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ValidateContactAccessAsync(int contactId, int userId, string userType)
        {
            try
            {
                var contact = await _contactRepository.GetByIdAsync(contactId);
                if (contact == null)
                {
                    return (false, "Contact not found.");
                }

                bool hasAccess = false;
                if (userType == "Staff" || userType == "Admin")
                {
                    hasAccess = contact.StaffId == userId;
                }
                else // Author
                {
                    hasAccess = contact.AuthorId == userId;
                }

                if (!hasAccess)
                {
                    return (false, "You don't have permission to access this contact.");
                }

                return (true, "Access granted.");
            }
            catch (Exception ex)
            {
                return (false, $"Error validating access: {ex.Message}");
            }
        }

        // Message methods
        public async Task<(bool Success, string Message, StaffAuthorMessage? ChatMessage)> GetMessageByIdAsync(int messageId)
        {
            try
            {
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null)
                {
                    return (false, "Message not found.", null);
                }

                return (true, "Message retrieved successfully.", message);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving message: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, List<StaffAuthorMessage>? Messages)> GetMessagesByContactIdAsync(int contactId)
        {
            try
            {
                var contactExists = await _contactRepository.ExistsAsync(contactId);
                if (!contactExists)
                {
                    return (false, "Contact not found.", null);
                }

                var messages = await _messageRepository.GetByContactIdAsync(contactId);
                return (true, "Messages retrieved successfully.", messages);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving messages: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, StaffAuthorMessage? ChatMessage)> CreateMessageAsync(int contactId, string senderType, int senderId, string messageText)
        {
            try
            {
                var contactExists = await _contactRepository.ExistsAsync(contactId);
                if (!contactExists)
                {
                    return (false, "Contact not found.", null);
                }

                // Validate sender
                var sender = await _userRepository.GetByIdAsync(senderId);
                if (sender == null)
                {
                    return (false, "Sender not found.", null);
                }

                var message = new StaffAuthorMessage
                {
                    ContactId = contactId,
                    SenderType = senderType,
                    SenderId = senderId,
                    MessageText = messageText,
                    SendAt = DateTime.UtcNow,
                    IsRead = false
                };

                var createdMessage = await _messageRepository.CreateAsync(message);
                
                // Reload to include sender details
                var messageWithDetails = await _messageRepository.GetByIdAsync(createdMessage.MessageId);
                
                return (true, "Message sent successfully.", messageWithDetails);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating message: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteMessageAsync(int messageId)
        {
            try
            {
                var deleted = await _messageRepository.DeleteAsync(messageId);
                if (!deleted)
                {
                    return (false, "Message not found or failed to delete.");
                }

                return (true, "Message deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting message: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> MarkMessagesAsReadAsync(int contactId, string readerType)
        {
            try
            {
                var contactExists = await _contactRepository.ExistsAsync(contactId);
                if (!contactExists)
                {
                    return (false, "Contact not found.");
                }

                var count = await _messageRepository.MarkAsReadAsync(contactId, readerType);
                return (true, $"{count} message(s) marked as read.");
            }
            catch (Exception ex)
            {
                return (false, $"Error marking messages as read: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message, int UnreadCount)> GetUnreadCountAsync(int userId, string userType)
        {
            try
            {
                var userExists = await _userRepository.GetByIdAsync(userId);
                if (userExists == null)
                {
                    return (false, "User not found.", 0);
                }

                var count = await _contactRepository.GetUnreadCountByUserAsync(userId, userType);
                return (true, "Unread count retrieved successfully.", count);
            }
            catch (Exception ex)
            {
                return (false, $"Error getting unread count: {ex.Message}", 0);
            }
        }
    }
}
