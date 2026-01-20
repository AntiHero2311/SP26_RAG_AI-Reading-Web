using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Repository.Models;
using Service.Services;
using System.Security.Claims;

namespace RAG_AI_Reading.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly StaffAuthorChatService _chatService;

        public ChatHub(StaffAuthorChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their own group for private messaging
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                
                // Add to role group (Staff or Author)
                if (userRole == "Staff" || userRole == "Admin")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "StaffGroup");
                }
                else if (userRole == "User" || userRole == "Author")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "AuthorGroup");
                }

                await Clients.Caller.SendAsync("Connected", new { userId, role = userRole, connectionId = Context.ConnectionId });
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Send a message in a contact/conversation
        /// </summary>
        public async Task SendMessage(int contactId, string messageText)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int senderId))
            {
                await Clients.Caller.SendAsync("Error", "Invalid user authentication");
                return;
            }

            // Determine sender type based on role
            string senderType = (userRole == "Staff" || userRole == "Admin") ? "Staff" : "Author";

            // Create message
            var (success, message, createdMessage) = await _chatService.CreateMessageAsync(
                contactId, 
                senderType, 
                senderId, 
                messageText
            );

            if (!success)
            {
                await Clients.Caller.SendAsync("Error", message);
                return;
            }

            // Get contact to find recipient
            var (contactSuccess, contactMessage, contact) = await _chatService.GetContactByIdAsync(contactId);
            
            if (!contactSuccess || contact == null)
            {
                await Clients.Caller.SendAsync("Error", "Contact not found");
                return;
            }

            // Determine recipient ID
            int recipientId = senderType == "Staff" ? contact.AuthorId.GetValueOrDefault() : contact.StaffId.GetValueOrDefault();

            // Prepare message response
            var messageResponse = new
            {
                messageId = createdMessage!.MessageId,
                contactId = createdMessage.ContactId,
                senderType = createdMessage.SenderType,
                senderId = createdMessage.SenderId,
                messageText = createdMessage.MessageText,
                sendAt = createdMessage.SendAt,
                isRead = createdMessage.IsRead,
                senderName = createdMessage.Sender?.FullName
            };

            // Send to sender (confirmation)
            await Clients.Caller.SendAsync("ReceiveMessage", messageResponse);

            // Send to recipient if they're online
            await Clients.Group($"User_{recipientId}").SendAsync("ReceiveMessage", messageResponse);
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        public async Task MarkAsRead(int contactId)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                await Clients.Caller.SendAsync("Error", "Invalid user authentication");
                return;
            }

            string readerType = (userRole == "Staff" || userRole == "Admin") ? "Staff" : "Author";

            var (success, message) = await _chatService.MarkMessagesAsReadAsync(contactId, readerType);

            if (success)
            {
                await Clients.Caller.SendAsync("MessagesMarkedAsRead", new { contactId });
                
                // Notify the other party
                var (contactSuccess, contactMessage, contact) = await _chatService.GetContactByIdAsync(contactId);
                if (contactSuccess && contact != null)
                {
                    int otherUserId = readerType == "Staff" ? contact.AuthorId.GetValueOrDefault() : contact.StaffId.GetValueOrDefault();
                    await Clients.Group($"User_{otherUserId}").SendAsync("MessagesMarkedAsRead", new { contactId });
                }
            }
            else
            {
                await Clients.Caller.SendAsync("Error", message);
            }
        }

        /// <summary>
        /// User is typing notification
        /// </summary>
        public async Task UserTyping(int contactId)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return;
            }

            var (success, message, contact) = await _chatService.GetContactByIdAsync(contactId);
            
            if (!success || contact == null) return;

            string senderType = (userRole == "Staff" || userRole == "Admin") ? "Staff" : "Author";
            int recipientId = senderType == "Staff" ? contact.AuthorId.GetValueOrDefault() : contact.StaffId.GetValueOrDefault();

            await Clients.Group($"User_{recipientId}").SendAsync("UserTyping", new { contactId, userId, senderType });
        }

        /// <summary>
        /// Join a specific contact room for real-time updates
        /// </summary>
        public async Task JoinContact(int contactId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Contact_{contactId}");
            await Clients.Caller.SendAsync("JoinedContact", new { contactId });
        }

        /// <summary>
        /// Leave a contact room
        /// </summary>
        public async Task LeaveContact(int contactId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Contact_{contactId}");
            await Clients.Caller.SendAsync("LeftContact", new { contactId });
        }
    }
}
