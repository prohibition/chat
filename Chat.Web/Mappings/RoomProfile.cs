using AutoMapper;
using Chat.Web.ChatModel;
using Chat.Web.Models;
using Chat.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web.Mappings
{
    public class RoomProfile : Profile
    {
        public RoomProfile()
        {
            CreateMap<Room, RoomViewModel>().ForMember(dst => dst.OwnerId, opt => opt.MapFrom(x => x.UserAccount_Id));

            CreateMap<RoomViewModel, Room>().ForMember(dst => dst.UserAccount_Id, opt => opt.MapFrom(x => x.OwnerId));
        }
    }
}