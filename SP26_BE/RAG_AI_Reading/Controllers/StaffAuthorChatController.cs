using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RAG_AI_Reading.DTOs;
using Service.Services;
using System.Security.Claims;

namespace RAG_AI_Reading.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StaffAuthorChatController : ControllerBase
    {
        private readonly StaffAuthorChatService _chatService;

        public StaffAuthorChatController(StaffAuthorChatService chatService)
        {
            _chatService = chatService;
        }

        // Contact endpoints

        /// <summary>
        /// Get all contacts for the current user (Staff or Author)
        /// </summary>
        [HttpGet("contacts")]
        public async Task<IActionResult> GetMyContacts()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            var (success, message, contacts) = userRole == "Staff" || userRole == "Admin"
                ? await _chatService.GetContactsByStaffIdAsync(userId)
                : await _chatService.GetContactsByAuthorIdAsync(userId);

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = contacts?.Select(c => new StaffAuthorContactResponseDto
            {
                ContactId = c.ContactId,
                StaffId = c.StaffId,
                AuthorId = c.AuthorId,
                StaffName = c.Staff?.FullName,
                AuthorName = c.Author?.FullName,
                StaffAvatar = c.Staff?.AvatarUrl,
                AuthorAvatar = c.Author?.AvatarUrl,
                ContactDate = c.ContactDate,
                Status = c.Status,
                LastMessage = c.StaffAuthorMessages?.OrderByDescending(m => m.SendAt).FirstOrDefault() != null
                    ? new StaffAuthorMessageResponseDto
                    {
                        MessageId = c.StaffAuthorMessages.OrderByDescending(m => m.SendAt).First().MessageId,
                        MessageText = c.StaffAuthorMessages.OrderByDescending(m => m.SendAt).First().MessageText,
                        SendAt = c.StaffAuthorMessages.OrderByDescending(m => m.SendAt).First().SendAt,
                        SenderType = c.StaffAuthorMessages.OrderByDescending(m => m.SendAt).First().SenderType,
                        IsRead = c.StaffAuthorMessages.OrderByDescending(m => m.SendAt).First().IsRead
                    }
                    : null
            }).ToList();

            return Ok(new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Get a specific contact with all messages
        /// </summary>
        [HttpGet("contacts/{contactId}")]
        public async Task<IActionResult> GetContactById(int contactId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            string userType = (userRole == "Staff" || userRole == "Admin") ? "Staff" : "Author";

            // Validate access
            var (accessSuccess, accessMessage) = await _chatService.ValidateContactAccessAsync(contactId, userId, userType);
            if (!accessSuccess)
            {
                return Forbid(accessMessage);
            }

            var (success, message, contact) = await _chatService.GetContactByIdAsync(contactId);

            if (!success)
            {
                return NotFound(new { message });
            }

            var response = new StaffAuthorContactResponseDto
            {
                ContactId = contact!.ContactId,
                StaffId = contact.StaffId,
                AuthorId = contact.AuthorId,
                StaffName = contact.Staff?.FullName,
                AuthorName = contact.Author?.FullName,
                StaffAvatar = contact.Staff?.AvatarUrl,
                AuthorAvatar = contact.Author?.AvatarUrl,
                ContactDate = contact.ContactDate,
                Status = contact.Status,
                Messages = contact.StaffAuthorMessages?.Select(m => new StaffAuthorMessageResponseDto
                {
                    MessageId = m.MessageId,
                    ContactId = m.ContactId,
                    SenderType = m.SenderType,
                    SenderId = m.SenderId,
                    SenderName = m.Sender?.FullName,
                    SenderAvatar = m.Sender?.AvatarUrl,
                    MessageText = m.MessageText,
                    SendAt = m.SendAt,
                    IsRead = m.IsRead
                }).ToList()
            };

            return Ok(new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Create a new contact between Staff and Author
        /// </summary>
        [HttpPost("contacts")]
        public async Task<IActionResult> CreateContact([FromBody] CreateStaffAuthorContactRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            // Validate that the requesting user is either the staff or has permission
            if ((userRole == "Staff" || userRole == "Admin") && request.StaffId != userId)
            {
                return Forbid("You can only create contacts for yourself.");
            }

            var (success, message, contact) = await _chatService.CreateOrGetContactAsync(
                request.StaffId,
                request.AuthorId
            );

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = new StaffAuthorContactResponseDto
            {
                ContactId = contact!.ContactId,
                StaffId = contact.StaffId,
                AuthorId = contact.AuthorId,
                ContactDate = contact.ContactDate,
                Status = contact.Status
            };

            return Ok(new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Update contact status (Active, Closed, Pending)
        /// </summary>
        [HttpPut("contacts/{contactId}/status")]
        public async Task<IActionResult> UpdateContactStatus(int contactId, [FromBody] UpdateContactStatusRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            string userType = (userRole == "Staff" || userRole == "Admin") ? "Staff" : "Author";

            // Validate access
            var (accessSuccess, accessMessage) = await _chatService.ValidateContactAccessAsync(contactId, userId, userType);
            if (!accessSuccess)
            {
                return Forbid(accessMessage);
            }

            var (success, message, contact) = await _chatService.UpdateContactStatusAsync(contactId, request.Status);

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = new StaffAuthorContactResponseDto
            {
                ContactId = contact!.ContactId,
                StaffId = contact.StaffId,
                AuthorId = contact.AuthorId,
                ContactDate = contact.ContactDate,
                Status = contact.Status
            };

            return Ok(new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Delete a contact (Staff only)
        /// </summary>
        [HttpDelete("contacts/{contactId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> DeleteContact(int contactId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            // Validate access
            var (accessSuccess, accessMessage) = await _chatService.ValidateContactAccessAsync(contactId, userId, "Staff");
            if (!accessSuccess)
            {
                return Forbid(accessMessage);
            }

            var (success, message) = await _chatService.DeleteContactAsync(contactId);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        // Message endpoints

        /// <summary>
        /// Get all messages for a contact
        /// </summary>
        [HttpGet("contacts/{contactId}/messages")]
        public async Task<IActionResult> GetMessagesByContact(int contactId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            string userType = (userRole == "Staff" || userRole == "Admin") ? "Staff" : "Author";

            // Validate access
            var (accessSuccess, accessMessage) = await _chatService.ValidateContactAccessAsync(contactId, userId, userType);
            if (!accessSuccess)
            {
                return Forbid(accessMessage);
            }

            var (success, message, messages) = await _chatService.GetMessagesByContactIdAsync(contactId);

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = messages?.Select(m => new StaffAuthorMessageResponseDto
            {
                MessageId = m.MessageId,
                ContactId = m.ContactId,
                SenderType = m.SenderType,
                SenderId = m.SenderId,
                SenderName = m.Sender?.FullName,
                SenderAvatar = m.Sender?.AvatarUrl,
                MessageText = m.MessageText,
                SendAt = m.SendAt,
                IsRead = m.IsRead
            }).ToList();

            return Ok(new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Create a new message (handled via HTTP, but SignalR is recommended for real-time)
        /// </summary>
        [HttpPost("messages")]
        public async Task<IActionResult> CreateMessage([FromBody] CreateStaffAuthorMessageRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int senderId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            string senderType = (userRole == "Staff" || userRole == "Admin") ? "Staff" : "Author";

            // Validate access
            var (accessSuccess, accessMessage) = await _chatService.ValidateContactAccessAsync(request.ContactId, senderId, senderType);
            if (!accessSuccess)
            {
                return Forbid(accessMessage);
            }

            var (success, message, chatMessage) = await _chatService.CreateMessageAsync(
                request.ContactId,
                senderType,
                senderId,
                request.MessageText
            );

            if (!success)
            {
                return BadRequest(new { message });
            }

            var response = new StaffAuthorMessageResponseDto
            {
                MessageId = chatMessage!.MessageId,
                ContactId = chatMessage.ContactId,
                SenderType = chatMessage.SenderType,
                SenderId = chatMessage.SenderId,
                SenderName = chatMessage.Sender?.FullName,
                SenderAvatar = chatMessage.Sender?.AvatarUrl,
                MessageText = chatMessage.MessageText,
                SendAt = chatMessage.SendAt,
                IsRead = chatMessage.IsRead
            };

            return CreatedAtAction(nameof(GetContactById), new { contactId = request.ContactId }, new
            {
                message,
                data = response
            });
        }

        /// <summary>
        /// Mark messages as read in a contact
        /// </summary>
        [HttpPost("contacts/{contactId}/mark-read")]
        public async Task<IActionResult> MarkMessagesAsRead(int contactId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            string readerType = (userRole == "Staff" || userRole == "Admin") ? "Staff" : "Author";

            // Validate access
            var (accessSuccess, accessMessage) = await _chatService.ValidateContactAccessAsync(contactId, userId, readerType);
            if (!accessSuccess)
            {
                return Forbid(accessMessage);
            }

            var (success, message) = await _chatService.MarkMessagesAsReadAsync(contactId, readerType);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message });
        }

        /// <summary>
        /// Get unread message count for the current user
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token không hợp lệ" });
            }

            string userType = (userRole == "Staff" || userRole == "Admin") ? "Staff" : "Author";

            var (success, message, unreadCount) = await _chatService.GetUnreadCountAsync(userId, userType);

            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new
            {
                message,
                data = new { unreadCount }
            });
        }
    }
}
