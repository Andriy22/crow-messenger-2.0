using BLL.Services.Abstractions;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    [EnableCors("signalr")]
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _privateChatService;
        private readonly UserManager<AppUser> _userManager;

        private readonly static Dictionary<string, string> _activeUsers = new Dictionary<string, string>();

        public ChatHub(IChatService privateChatService, UserManager<AppUser> userManager)
        {
            _privateChatService = privateChatService;
            _userManager = userManager;
        }

        [HubMethodName("get-my-chats")]
        public async Task GetAllChatsAsync()
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");
            await Clients.Caller.SendAsync("ReceiveMyChats", await _privateChatService.GetAllChatsAsync(ownerId));
        }

        [HubMethodName("get-all-messages-with-user")]
        public async Task GetAllChatsAsync(string userId)
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");
            await Clients.Caller.SendAsync("ReceivedAllMessagesWithUser", await _privateChatService.GetMessagesWithUserAsync(userId, ownerId));
        }

        [HubMethodName("delete-message")]
        public async Task DeleteMessageAsync(long messageId)
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");
            await _privateChatService.DeleteMessageAsync(messageId, ownerId);

            await Clients.Caller.SendAsync("MessageDeleted");
        }

        public override async Task OnConnectedAsync()
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");

            _activeUsers.Add(Context.ConnectionId, ownerId);

            var chats = await _privateChatService.GetAllChatsAsync(ownerId);

            foreach (var chat in chats)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chat.Id.ToString());
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string? ownerId = null;

            if (_activeUsers.TryGetValue(Context.ConnectionId, value: out ownerId))
            {
                var chats = await _privateChatService.GetAllChatsAsync(ownerId);

                foreach (var chat in chats)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, chat.Id.ToString());
                }
            }
        }
    }
}
