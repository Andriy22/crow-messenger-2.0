using AutoMapper;
using BLL.Common.Dtos.Chat;
using BLL.Common.Dtos.Profile;
using DAL.Entities;

namespace BLL.Common.Mapper
{
    public class AutoMapperProfileConfiguration : Profile
    {
        public AutoMapperProfileConfiguration() 
        {
            CreateMap<MessageDto, Message>()
                .ForMember(x => x.ReplyMessageId, opt => opt.MapFrom(x => x.ReplyMessageId))
                .ForMember(x => x.MessageType, opt => opt.MapFrom(x => x.MessageType))
                .ForMember(x => x.Text, opt => opt.MapFrom(x => x.Message))
                .ForMember(x => x.CreatedById, opt => opt.MapFrom(x => x.SenderId));

            CreateMap<AppUser, MessageUserResult>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id))
                .ForMember(x => x.NickName, opt => opt.MapFrom(x => x.NickName))
                .ForMember(x => x.ProfileImg, opt => opt.MapFrom(x => x.ProfileImg));

            CreateMap<AppUser, AppUserResult>();
        }
    }
}
