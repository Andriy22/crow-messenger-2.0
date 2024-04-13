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

namespace BLL.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IRepository<Message> _messageRepository;
        private readonly IRepository<Chat> _chatRepository;
        private readonly IRepository<MessageAttachment> _messageAttachmentRepository;

        private readonly IFileService _fileService;
        private readonly UserManager<AppUser> _userManager;

        private readonly IMapper _mapper;

        public MessageService(IRepository<Message> messageRepository,
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

            return chat.Chats.Select(x => MessageConverter.MessageToResult(x, x.CreatedBy, chat.Users.FirstOrDefault(c => c.UserId == x.CreatedById)?.Role ?? UserChatRoleType.None)).ToList();
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

            return chat.Chats.Select(x => MessageConverter.MessageToResult(x, x.CreatedBy, chat.Users.FirstOrDefault(c => c.UserId == x.CreatedById)?.Role ?? UserChatRoleType.None)).ToList();
        }

        public async Task<MessageResult> SaveChatMessageAsync(MessageDto message)
        {
            var chat = await _chatRepository.GetQueryable(x => x.Id == message.ChatId).Include(x => x.Users).FirstOrDefaultAsync();

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

            var sender = await _userManager.FindByIdAsync(dbMessage.CreatedById!);

            _messageRepository.Add(dbMessage);

            return MessageConverter.MessageToResult(dbMessage, sender, chat.Users.FirstOrDefault(c => c.UserId == dbMessage.CreatedById)?.Role ?? UserChatRoleType.None);
        }

        public async Task<MessageResult> SavePrivateChatMessageAsync(MessageDto message)
        {
            var users = new List<string>() { message.SenderId!, message.ReceiverId! };
            var chat = await _chatRepository.GetQueryable(x => users.All(s => x.Users.Any(u => u.UserId == s)) && !x.IsDeleted && x.ChatType == ChatType.Private)
                .Include(x => x.Users)
                .FirstOrDefaultAsync();

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

            var sender = await _userManager.FindByIdAsync(dbMessage.CreatedById!);

            if (chat == null)
            {
                chat = new Chat
                {
                    ImagePath = "",
                    Chats = new List<Message> { dbMessage },
                    Users = new List<UserChat> { new UserChat { UserId = message.ReceiverId!, Role = UserChatRoleType.Owner }, 
                                                 new UserChat { UserId = message.SenderId!, Role = UserChatRoleType.Owner } },
                    ChatType = ChatType.Private,
                    LastUpdatedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _chatRepository.Add(chat);

                return MessageConverter.MessageToResult(dbMessage, sender, chat.Users.FirstOrDefault(c => c.UserId == dbMessage.CreatedById)?.Role ?? UserChatRoleType.None);
            }

            dbMessage.ChatId = chat.Id;

            _messageRepository.Add(dbMessage);

            return MessageConverter.MessageToResult(dbMessage, sender, chat.Users.FirstOrDefault(c => c.UserId == dbMessage.CreatedById)?.Role ?? UserChatRoleType.None);
        }
    }
}
