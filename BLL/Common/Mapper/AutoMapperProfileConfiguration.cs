using AutoMapper;
using BLL.Common.Dtos.PrivateChat;
using DAL.Entities;

namespace BLL.Common.Mapper
{
    public class AutoMapperProfileConfiguration : Profile
    {
        public AutoMapperProfileConfiguration() 
        {
            CreateMap<PrivateMessageDto, Message>()
                .ForMember(x => x.ReplyMessageId, opt => opt.MapFrom(x => x.ReplyMessageId))
                .ForMember(x => x.MessageType, opt => opt.MapFrom(x => x.MessageType))
                .ForMember(x => x.Text, opt => opt.MapFrom(x => x.Message))
                .ForMember(x => x.CreatedById, opt => opt.MapFrom(x => x.SenderId));
        }
    }
}
