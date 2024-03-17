using AutoMapper;
using BLL.Common.Constants;
using BLL.Common.Dtos.Chat;
using BLL.Common.Dtos.PrivateChat;
using BLL.Services.Abstractions;
using DAL.Entities;
using DAL.Enums;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            return chats.Select(x => new ChatResult
            {
                ChatType = x.ChatType,
                ProfileImg = x.ImagePath,
                Id = x.Id,
                Users = x.Users.Select(u => new MessageUserResult
                {
                    Id = u.UserId,
                    NickName = u.User?.NickName ?? "",
                    ProfileImg = u.User?.ProfileImg ?? ""
                }).ToList()
            }).ToList();
        }

        public async Task<ChatResult> GetChatByIdAsync(long chatId)
        {
            var chat = await _chatRepository.GetQueryable(x => x.Id == chatId && !x.IsDeleted)
                .Include(x => x.Users)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

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
                ProfileImg = chat.ImagePath,
            };
        }

        public async Task<List<PrivateMessageResult>> GetMessagesWithUserAsync(string userId, string ownerId)
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
                return new List<PrivateMessageResult>();
            }

            return chat.Chats.Select(x => new PrivateMessageResult
            {
                Attachments = x.Attachments.Select(a => new MessageAttachmentResult { AttachemntPath = a.AttachmentPath, Id = a.Id }).ToList(),
                Message = x.Text,
                MessageType = x.MessageType,
                Sender = new MessageUserResult
                {
                    Id = x.CreatedById,
                    NickName = x.CreatedBy?.NickName ?? "",
                    ProfileImg = x.CreatedBy?.ProfileImg ?? ""
                },
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                SeenAt = x.SeenAt,
                ChatId = x.ChatId,
            }).ToList();
        }

        public async Task<PrivateMessageResult> SavePrivateChatMessageAsync(PrivateMessageDto message)
        {
            var users = new List<string>() { message.SenderId, message.ReceiverId };
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
                    Users = new List<UserChat> { new UserChat { UserId = message.ReceiverId }, new UserChat { UserId = message.SenderId } },
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

        private PrivateMessageResult MessageToResult(Message dbMessage, AppUser? sender)
        {
            return new PrivateMessageResult
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
    }
}
