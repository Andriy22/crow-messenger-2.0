using AutoMapper;
using BLL.Common.Constants;
using BLL.Common.Dtos.Chat;
using BLL.Services.Abstractions;
using DAL.Entities;
using DAL.Enums;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace BLL.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IRepository<Message> _messageRepository;
        private readonly IRepository<Chat> _chatRepository;
        private readonly IRepository<MessageAttachment> _messageAttachmentRepository;

        private readonly IFileService _fileService;
        private readonly UserManager<AppUser> _userManager;

        private readonly IMapper _mapper;

        public ChatService(IRepository<Message> messageRepository, 
                                  IRepository<Chat> chatRepository, 
                                  IRepository<MessageAttachment> messageAttachmentRepository,
                                  IFileService fileService,
                                  UserManager<AppUser> userManager,
                                  IMapper mapper)
        {
            _messageRepository = messageRepository;
            _chatRepository = chatRepository;
            _messageAttachmentRepository = messageAttachmentRepository;

            _fileService = fileService;
            _userManager = userManager;

            _mapper = mapper;
        }

        public async Task DeleteMessageAsync(long messageId, string userId)
        {
            var message = await _messageRepository.GetQueryable(x => x.Id == messageId && x.CreatedById == userId)
                .FirstOrDefaultAsync();

            if (message != null)
            {
                _messageRepository.ExplicitDelete(message);
            }
        }

        public async Task<List<ChatResult>> GetAllChatsAsync(string userId)
        {
            var chats = await _chatRepository.GetQueryable(x => x.Users.Any(u => u.UserId == userId) && !x.IsDeleted)
                .Include(x => x.Users)
                .ThenInclude(x => x.User)
                .ToListAsync();

            return chats.Select(x => ChatToResult(x, userId)).ToList();
        }

        public async Task<ChatResult> GetChatByIdAsync(long chatId, string userId)
        {
            var chat = await _chatRepository.GetQueryable(x => x.Id == chatId && !x.IsDeleted)
                .Include(x => x.Users)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

            return ChatToResult(chat, userId);
        }

        public async Task<List<MessageResult>> GetChatMessagesAsync(long chatId, string ownerId)
        {
            var chat = await _chatRepository.GetQueryable(x => x.Users.Any(x => x.UserId == ownerId) && x.Id == chatId)
                .Include(x => x.Users)
                .Include(x => x.Chats.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.CreatedBy)
                .Include(x => x.Chats.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.Attachments)
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                return new List<MessageResult>();
            }

            return chat.Chats.Select(x => MessageToResult(x, x.CreatedBy)).ToList();
        }

        public async Task<List<MessageResult>> GetMessagesWithUserAsync(string userId, string ownerId)
        {
            var users = new List<string>() { userId, ownerId };
            var chat = await _chatRepository.GetQueryable(x => x.Users.All(u => users.Contains(u.UserId)) && !x.IsDeleted && x.ChatType == ChatType.Private)
                .Include(x => x.Users)
                .Include(x => x.Chats.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.CreatedBy)
                .Include(x => x.Chats.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.Attachments)
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                return new List<MessageResult>();
            }

            return chat.Chats.Select(x => MessageToResult(x, x.CreatedBy)).ToList();
        }

        public async Task<MessageResult> SaveChatMessageAsync(MessageDto message)
        {
            var chat = await _chatRepository.GetQueryable(x => x.Id == message.ChatId).FirstOrDefaultAsync();

            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

            var dbMessage = _mapper.Map<Message>(message);

            var files = await _fileService.SaveFilesAsync(message.Attachments ?? new List<IFormFile>(), FileConstants.UsersFiles);

            dbMessage.CreatedAt = DateTime.UtcNow;
            dbMessage.LastUpdatedAt = DateTime.UtcNow;

            dbMessage.Attachments = files.Select(x => new MessageAttachment
            {
                AttachmentPath = x,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                CreatedById = message.SenderId!
            }).ToList();

            var sender = await _userManager.FindByIdAsync(dbMessage.CreatedById);

            _messageRepository.Add(dbMessage);

            return MessageToResult(dbMessage, sender);
        }

        public async Task<MessageResult> SavePrivateChatMessageAsync(MessageDto message)
        {
            var users = new List<string>() { message.SenderId!, message.ReceiverId! };
            var chat = await _chatRepository.GetQueryable(x => x.Users.All(u => users.Contains(u.UserId)) && !x.IsDeleted && x.ChatType == ChatType.Private)
                .Include(x => x.Users)
                .FirstOrDefaultAsync();

            var dbMessage = _mapper.Map<Message>(message);

            var files = await _fileService.SaveFilesAsync(message.Attachments ?? new List<IFormFile>(), FileConstants.UsersFiles);

            dbMessage.CreatedAt = DateTime.UtcNow;
            dbMessage.LastUpdatedAt = DateTime.UtcNow;

            dbMessage.Attachments = files.Select(x => new MessageAttachment { AttachmentPath = x, 
                CreatedAt = DateTime.UtcNow, 
                LastUpdatedAt = DateTime.UtcNow, 
                CreatedById = message.SenderId }).ToList();

            var sender = await _userManager.FindByIdAsync(dbMessage.CreatedById);

            if (chat == null)
            {
                chat = new Chat
                {
                    ImagePath = "",
                    Chats = new List<Message> { dbMessage },
                    Users = new List<UserChat> { new UserChat { UserId = message.ReceiverId! }, new UserChat { UserId = message.SenderId! } },
                    ChatType = ChatType.Private,
                    LastUpdatedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _chatRepository.Add(chat);

                return MessageToResult(dbMessage, sender);
            }

            dbMessage.ChatId = chat.Id;

            _messageRepository.Add(dbMessage);

            return MessageToResult(dbMessage, sender);
        }

        private MessageResult MessageToResult(Message dbMessage, AppUser? sender)
        {
            return new MessageResult
            {
                Attachments = dbMessage.Attachments.Select(a => new MessageAttachmentResult { AttachemntPath = a.AttachmentPath, Id = a.Id }).ToList(),
                Message = dbMessage.Text,
                MessageType = dbMessage.MessageType,
                Sender = new MessageUserResult
                {
                    Id = sender?.Id ?? "",
                    NickName = sender?.NickName ?? "",
                    ProfileImg = sender?.ProfileImg ?? ""
                },
                Id = dbMessage.Id,
                CreatedAt = dbMessage.CreatedAt,
                SeenAt = dbMessage.SeenAt,
                ChatId = dbMessage.ChatId,
            };
        }

        private ChatResult ChatToResult(Chat chat, string userId)
        {
            return new ChatResult
            {
                Id = chat.Id,
                ChatType = chat.ChatType,
                Users = chat.Users.Select(u => new MessageUserResult
                {
                    Id = u.UserId,
                    NickName = u?.User?.NickName ?? "",
                    ProfileImg = u?.User?.ProfileImg ?? ""
                }).ToList(),
                ProfileImg = chat.ChatType != ChatType.Private ? chat.ImagePath : chat.Users.FirstOrDefault(x => x.UserId != userId)!.User!.ProfileImg!,
                Title = chat.ChatType != ChatType.Private ? chat.Title : chat.Users.FirstOrDefault(x => x.UserId != userId)!.User!.NickName!,
            };
        }
    }
}
