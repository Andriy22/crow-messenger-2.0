using BLL.Services.Abstractions;
using DAL.Entities;
using DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using System;

namespace API.Hubs
{
    [EnableCors("signalr")]
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _privateChatService;
        private readonly IMessageService _messageService;
        private readonly IUsersService _usersService;
        private readonly IOnlineStatusService _onlineStatusService;

        private readonly static Dictionary<string, string> _activeUsers = new Dictionary<string, string>();

        public ChatHub(IChatService privateChatService, IUsersService usersService, IMessageService messageService, IOnlineStatusService onlineStatusService)
        {
            _privateChatService = privateChatService;
            _usersService = usersService;
            _messageService = messageService;
            _onlineStatusService = onlineStatusService;
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
            await Clients.Caller.SendAsync("ReceivedChatMessages", await _messageService.GetMessagesWithUserAsync(userId, ownerId));
        }

        [HubMethodName("delete-message")]
        public async Task DeleteMessageAsync(long messageId)
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");
            await _messageService.DeleteMessageAsync(messageId, ownerId);

            await Clients.Caller.SendAsync("MessageDeleted");
        }

        [HubMethodName("create-new-chat")]
        public async Task CreateChatAsync(string title, ChatType type)
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");
            var chat = await _privateChatService.CreateChatAsync(title, type, ownerId);

            await Clients.Caller.SendAsync("ChatCreated", chat);
        }

        [HubMethodName("add-users-to-chat")]
        public async Task AddUsersToChatAsync(long chatId, List<string> accountsIds)
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");
            var chat = await _privateChatService.AddUsersToChatAsync(chatId, accountsIds, ownerId);

            foreach (var connection in _activeUsers.Where(x => accountsIds.Contains(x.Value)))
            {
                await Groups.AddToGroupAsync(connection.Key, chat.Id.ToString());
            }

            await Clients.Group(chat.Id.ToString()).SendAsync("ChatUpdated", chat);
        }

        [HubMethodName("remove-users-from-chat")]
        public async Task RemoveUsersFromChatAsync(long chatId, List<string> accountsIds)
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");
            var chat = await _privateChatService.RemoveUsersFromChatAsync(chatId, accountsIds, ownerId);

            foreach (var connection in _activeUsers.Where(x => accountsIds.Contains(x.Value)))
            {
                await Groups.RemoveFromGroupAsync(connection.Key, chat.Id.ToString());
            }

            await Clients.Group(chat.Id.ToString()).SendAsync("ChatUpdated", chat);
        }

        [HubMethodName("delete-chat")]
        public async Task DeleteChatAsync(long chatId)
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");
            await _privateChatService.DeleteChatByIdAsync(chatId, ownerId);

            await Clients.Group(chatId.ToString()).SendAsync("ChatDeleted", chatId);
        }

        [HubMethodName("leave-chat")]
        public async Task LeaveChatAsync(long chatId)
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");
            var chat = await _privateChatService.LeaveChatAsync(chatId, ownerId);

            foreach (var connection in _activeUsers.Where(x => x.Value == ownerId))
            {
                await Groups.RemoveFromGroupAsync(connection.Key, chat.Id.ToString());
            }

            await Clients.Group(chatId.ToString()).SendAsync("ChatUpdated", chat);
        }

        [HubMethodName("get-users-by-nickname")]
        public async Task GetUsersByNicknameAsync(string nickName)
        {
            await Clients.Caller.SendAsync("UsersReceived", await _usersService.GetUsersByNicknameAsync(nickName));
        }

        [HubMethodName("get-chat-messages")]
        public async Task GetUsersByNicknameAsync(long chatId)
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");
            await Clients.Caller.SendAsync("ReceivedChatMessages", await _messageService.GetChatMessagesAsync(chatId, ownerId));
        }

        [HubMethodName("get-contacts-online")]
        public async Task GetContactsOnlineAsync()
        {
            var ownerId = Context?.User?.Identity?.Name ?? throw new Exception("Not authorized");

            var broadcastUsers = await _onlineStatusService.GetUsersIdsToBroadcastAsync(ownerId);

            await Clients.Caller.SendAsync("UsersOnline", broadcastUsers.Where(x => _activeUsers.ContainsValue(x)).ToList());
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

            await _onlineStatusService.SetUserLastOnline(ownerId, DateTime.UtcNow);
            var broadcastUsers = await _onlineStatusService.GetUsersIdsToBroadcastAsync(ownerId);

            foreach (var user in broadcastUsers)
            {
                if (_activeUsers.ContainsValue(user))
                {
                    await Clients.User(user).SendAsync("UserGoesOnline", ownerId);
                }
            }

            await Clients.Caller.SendAsync("UsersOnline", broadcastUsers.Where(x => _activeUsers.ContainsValue(x)).ToList());

            await Clients.Caller.SendAsync("Connected");

            await base.OnConnectedAsync();
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

                await _onlineStatusService.SetUserLastOnline(ownerId, DateTime.UtcNow);
                var broadcastUsers = await _onlineStatusService.GetUsersIdsToBroadcastAsync(ownerId);

                _activeUsers.Remove(Context.ConnectionId);

                if (!_activeUsers.ContainsValue(ownerId))
                {
                    foreach (var user in broadcastUsers)
                    {
                        if (_activeUsers.ContainsValue(user))
                        {
                            await Clients.User(user).SendAsync("UserGoesOffline", ownerId);
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
