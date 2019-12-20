using AutoMapper;
using Chat.Web.ChatModel;
using Chat.Web.Helpers;
using Chat.Web.Models;
using Chat.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web.Mappings
{
    public class GroupMessageProfile : Profile
    {
        public GroupMessageProfile()
        {
            CreateMap<Message, MessageViewModel>()
                .ForMember(dst => dst.Content, opt => opt.MapFrom(x => BasicEmojis.ParseEmojis(x.Content)))
                .ForMember(dst => dst.Timestamp, opt => opt.MapFrom(x => new DateTime(long.Parse(x.Timestamp)).ToLongTimeString()));

            CreateMap<MessageViewModel, Message>();
        }
    }

    public class PrivateMessageProfile : Profile
    {
        public PrivateMessageProfile()
        {
            CreateMap<PrivateMessage, MessageViewModel>()
                .ForMember(dst => dst.Content, opt => opt.MapFrom(x => BasicEmojis.ParseEmojis(x.Content)))
                .ForMember(dst => dst.Timestamp, opt => opt.MapFrom(x => new DateTime(long.Parse(x.Timestamp)).ToLongTimeString()));

            CreateMap<MessageViewModel, PrivateMessage>();
        }
    }
}