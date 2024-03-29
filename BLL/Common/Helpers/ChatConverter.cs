﻿using BLL.Common.Dtos.Chat;
using DAL.Entities;
using DAL.Enums;

namespace BLL.Common.Helpers
{
    public static class ChatConverter
    {
        public static ChatResult ChatToResult(Chat chat, string userId)
        {
            return new ChatResult
            {
                Id = chat.Id,
                ChatType = chat.ChatType,
                Users = chat.Users.Select(u => new MessageUserResult
                {
                    Id = u.UserId,
                    NickName = u?.User?.NickName ?? "",
                    ProfileImg = u?.User?.ProfileImg ?? "",
                    Role = u?.Role ?? UserChatRoleType.None
                    
                }).ToList(),
                ProfileImg = chat.ChatType != ChatType.Private ? chat.ImagePath : chat.Users.FirstOrDefault(x => x.UserId != userId)!.User!.ProfileImg!,
                Title = chat.ChatType != ChatType.Private ? chat.Title : chat.Users.FirstOrDefault(x => x.UserId != userId)!.User!.NickName!,
            };
        }
    }
}
