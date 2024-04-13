using AutoMapper;
using BLL.Common.Constants;
using BLL.Common.Dtos.Chat;
using BLL.Common.Helpers;
using BLL.Services.Abstractions;
using DAL.Entities;
using DAL.Enums;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Net.Mail;

namespace BLL.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IRepository<Chat> _chatRepository;
        private readonly IRepository<UserChat> _userChatRepository;

        private readonly IFileService _fileService;
        private readonly UserManager<AppUser> _userManager;

        private readonly IMapper _mapper;

        public ChatService(IRepository<Chat> chatRepository,
                           IRepository<MessageAttachment> messageAttachmentRepository,
                           IFileService fileService,
                           UserManager<AppUser> userManager,
                           IMapper mapper,
                           IRepository<UserChat> userChatRepository)
        {
            _chatRepository = chatRepository;
            _userChatRepository = userChatRepository;

            _fileService = fileService;
            _userManager = userManager;

            _mapper = mapper;
        }

        public async Task<ChatResult> AddUsersToChatAsync(long chatId, List<string> accountIds, string userId)
        {
            var chat = await _chatRepository.GetQueryable(x => x.Id == chatId && !x.IsDeleted)
               .Include(x => x.Users)
               .Include(x => x.Chats)
               .FirstOrDefaultAsync();

            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

            if (chat.Users.FirstOrDefault(x => x.UserId == userId)?.Role != UserChatRoleType.Owner)
            {
                throw new Exception("You don't have permissions to add users to this chat");
            }

            var accountsToAdd = accountIds.Where(x => !chat.Users.Any(u => u.UserId == x)).ToList();

            await _userChatRepository.AddRangeAsync(accountsToAdd.Select(x => new UserChat { UserId = x, Role = UserChatRoleType.None }).ToList());

            chat.Chats.Add(MessageHelper.CreateSystemMessage(SystemMessagesConstants.UserAdded));

            chat.LastUpdatedAt = DateTime.UtcNow;

            _chatRepository.Edit(chat);

            return ChatConverter.ChatToResult(chat, userId);
        }

        public async Task<ChatResult> CreateChatAsync(string title, ChatType chatType, string userId)
        {
            var newChat = new Chat
            {
                Title = title,
                Chats = new List<Message> { MessageHelper.CreateSystemMessage(SystemMessagesConstants.ChatCreated) },
                ChatType = chatType,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now,
                ImagePath = "",
                Users = new List<UserChat> { new UserChat { UserId = userId, Role = UserChatRoleType.Owner } }
            };

            await _chatRepository.AddAsync(newChat);

            return ChatConverter.ChatToResult(newChat, userId);
        }

        public async Task DeleteChatByIdAsync(long chatId, string userId)
        {
            var chat = await _chatRepository.GetQueryable(x => x.Id == chatId && !x.IsDeleted)
                .Include(x => x.Users)
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

            if (chat.Users.FirstOrDefault(x => x.UserId == userId)?.Role != UserChatRoleType.Owner)
            {
                throw new Exception("You don't have permissions to delete this chat");
            }
            
            _chatRepository.ExplicitDelete(chat);
        }

        public async Task<List<ChatResult>> GetAllChatsAsync(string userId)
        {
            var chats = await _chatRepository.GetQueryable(x => x.Users.Any(u => u.UserId == userId) && !x.IsDeleted)
                .Include(x => x.Users)
                .ThenInclude(x => x.User)
                .Include(x => x.Chats.OrderByDescending(x => x.CreatedAt).Take(1))
                .ThenInclude(x => x.CreatedBy)
                .Include(x => x.Chats.OrderByDescending(x => x.CreatedAt).Take(1))
                .ThenInclude(x => x.Attachments)
                .ToListAsync();

            return chats.Select(x => ChatConverter.ChatToResult(x, userId)).ToList();
        }

        public async Task<ChatResult> GetChatByIdAsync(long chatId, string userId)
        {
            var chat = await _chatRepository.GetQueryable(x => x.Id == chatId && !x.IsDeleted)
                .Include(x => x.Users)
                .ThenInclude(x => x.User)
                .Include(x => x.Chats.OrderByDescending(x => x.CreatedAt).Take(1))
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

            return ChatConverter.ChatToResult(chat, userId);
        }

        public async Task<ChatResult> LeaveChatAsync(long chatId, string userId)
        {
            var chat = await _chatRepository.GetQueryable(x => x.Id == chatId && !x.IsDeleted)
                          .Include(x => x.Users)
                          .Include(x => x.Chats)
                          .FirstOrDefaultAsync();

            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

            if (chat.Users.FirstOrDefault(x => x.UserId == userId)?.Role != UserChatRoleType.Owner)
            {
                throw new Exception("You don't have permissions to add users to this chat");
            }

            var recordsToRemove = chat.Users.Where(u => u.UserId == userId && u.Role != UserChatRoleType.Owner);

            if (!recordsToRemove.Any())
            {
                throw new Exception("You can't leave your own chat");
            }

            foreach (var record in recordsToRemove)
            {
                _userChatRepository.Delete(record);
            }

            chat.Chats.Add(MessageHelper.CreateSystemMessage(SystemMessagesConstants.UserLeft));

            chat.LastUpdatedAt = DateTime.UtcNow;

            _chatRepository.Edit(chat);

            return ChatConverter.ChatToResult(chat, userId);
        }

        public async Task<ChatResult> RemoveUsersFromChatAsync(long chatId, List<string> accountIds, string userId)
        {
            var chat = await _chatRepository.GetQueryable(x => x.Id == chatId && !x.IsDeleted)
                          .Include(x => x.Users)
                          .Include(x => x.Chats)
                          .FirstOrDefaultAsync();

            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

            if (chat.Users.FirstOrDefault(x => x.UserId == userId)?.Role != UserChatRoleType.Owner)
            {
                throw new Exception("You don't have permissions to add users to this chat");
            }

            var recordsToRemove = chat.Users.Where(u => accountIds.Contains(u.UserId) && u.Role != UserChatRoleType.Owner);

            foreach (var record in recordsToRemove)
            {
                _userChatRepository.Delete(record);
            }

            chat.Chats.Add(MessageHelper.CreateSystemMessage(SystemMessagesConstants.UserRemoved));

            chat.LastUpdatedAt = DateTime.UtcNow;

            _chatRepository.Edit(chat);

            return ChatConverter.ChatToResult(chat, userId);
        }
    }
}
