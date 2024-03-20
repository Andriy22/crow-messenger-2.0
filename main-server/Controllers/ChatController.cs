using API.Hubs;
using BLL.Common.Dtos.Chat;
using BLL.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace API.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _chatHub;

        public ChatController(IChatService chatService, IHubContext<ChatHub> chatHub)
        {
            _chatService = chatService;
            _chatHub = chatHub;
        }

        [HttpPost("send-private-message")]
        public async Task<ActionResult<MessageResult>> SendPrivateMessageAsync([FromForm] MessageDto data)
        {

            data.SenderId = User?.Identity?.Name ?? throw new Exception("Forbidden");

            var result = await _chatService.SavePrivateChatMessageAsync(data);

            var chat = await _chatService.GetChatByIdAsync(result.ChatId, data.SenderId);

            foreach (var user in chat.Users)
            {
                await _chatHub.Clients.User(user.Id).SendAsync("ReceivedNewMessage", result);
            }

            return Ok(result);
        }

        [HttpPost("send-message-to-chat")]
        public async Task<ActionResult<MessageResult>> SendChatMessageAsync([FromForm] MessageDto data)
        {
            data.SenderId = User?.Identity?.Name ?? throw new Exception("Forbidden");

            var result = await _chatService.SaveChatMessageAsync(data);

            var chat = await _chatService.GetChatByIdAsync(result.ChatId, data.SenderId);

            foreach (var user in chat.Users)
            {
                await _chatHub.Clients.User(user.Id).SendAsync("ReceivedNewMessage", result);
            }

            return Ok(result);
        }
    }
}
