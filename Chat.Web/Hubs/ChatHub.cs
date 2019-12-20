using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Chat.Web.Models.ViewModels;
using Chat.Web.Models;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Chat.Web.TCModel;
using Chat.Web.ChatModel;
using Chat.Web.Helpers;

namespace Chat.Web.Hubs
{

    [Authorize]
    public class ChatHub : Hub
    {
        #region Properties
        List<string> SuperAdminList = new List<string>() { "aangell_closeline-com", "mbell_closeline-com", "eliss_closeline-com" };
        /// <summary>
        /// List of online users
        /// </summary>
        /// 
        public readonly static List<UserViewModel> _Connections = new List<UserViewModel>();

        /// <summary>
        /// List of available chat rooms
        /// </summary>
        private readonly static List<RoomViewModel> _Rooms = new List<RoomViewModel>();

        /// <summary>
        /// Mapping SignalR connections to application users.
        /// (We don't want to share connectionId)
        /// </summary>
        private readonly static Dictionary<string, string> _ConnectionsMap = new Dictionary<string, string>();
        #endregion

        public void Send(string roomName, string message)
        {
            if (TitleConnectHelper.CheckUser(roomName.DecodeUserName()))
                SendPrivate(roomName, message);
            else
                SendToRoom((roomName != string.Empty) ? Convert.ToInt32(roomName) : 0, message);
        }

        public void SendPrivate(string toUser, string message)
        {
            try
            {
                string receiver = toUser;

                var user = TitleConnectHelper.GetUserbyCompanyEmail(IdentityName.DecodeUserName());
                var touser = TitleConnectHelper.GetUserbyCompanyEmail(receiver.DecodeUserName());
                //var locationinfo = TitleConnectHelper.GetUserLocation((string.IsNullOrEmpty(user.LocationId)?0:Convert.ToInt32(user.LocationId)));

                message = Regex.Replace(message, @"\/private\(.*?\)", string.Empty).Trim();

                // Build the message
                MessageViewModel messageViewModel = new MessageViewModel()
                {
                    From = user.DisplayName,
                    FromId = user.CompanyEmail.EncodeUserName(),
                    Avatar = StaticResources.Avatars[0],
                    To = touser.DisplayName,
                    ToId = touser.CompanyEmail.EncodeUserName(),
                    Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty).ParseEmojis(),
                    Timestamp = DateTime.UtcNow.ToString()
                };

                using (var context = new ChatWebEntities())
                {
                    PrivateMessage msg = new PrivateMessage()
                    {
                        Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty),
                        Timestamp = DateTime.UtcNow.Ticks.ToString(),
                        FromId = user.CompanyEmail.EncodeUserName(),
                        ToId = touser.CompanyEmail.EncodeUserName(),
                        FromName = user.DisplayName,
                        ToName = touser.DisplayName,
                        ReadStatus = false,
                        ReadTimestamp = string.Empty 

                    };

                    context.PrivateMessages.Add(msg);
                    context.SaveChanges();
                }
                messageViewModel.IsMine = 0;
                Clients.User(receiver).privateMessage(messageViewModel);

                messageViewModel.IsMine = 1;
                Clients.Caller.privateMessage(messageViewModel);



            }
            catch (Exception ex)
            {
                Clients.Caller.onError(ex.Message.ToString());
            }
        }

        public void SendToRoom(int id, string message)
        {
            try
            {
                using (var db = new ChatWebEntities())
                {
                    var user = TitleConnectHelper.GetUserbyCompanyEmail(IdentityName.DecodeUserName());
                    var room = db.Rooms.Where(r => r.Id == id).FirstOrDefault();

                    // Create and save message in database
                    Message msg = new Message()
                    {
                        Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty),
                        Timestamp = DateTime.UtcNow.Ticks.ToString(),
                        FromId = IdentityName.EncodeUserName(),
                        FromName = user.DisplayName,
                        ToRoomId = id
                    };
                    db.Messages.Add(msg);
                    db.SaveChanges();

                    var touser = db.Rooms.FirstOrDefault(x => x.Id == id);

                    MessageViewModel messageViewModel = new MessageViewModel()
                    {
                        From = user.DisplayName,
                        FromId = user.CompanyEmail.EncodeUserName(),
                        Avatar = user.Avatar,
                        To = touser.Name,
                        ToId = touser.Id.ToString(),
                        Content = Regex.Replace(message, @"(?i)<(?!img|a|/a|/img).*?>", String.Empty).ParseEmojis(),
                        Timestamp = DateTime.UtcNow.ToString()
                    };


                    Clients.Group(room.Id.ToString()).groupMessage(messageViewModel);
                }
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("Message not send!");
            }
        }

        public void InviteUsers(string roomId,string[] users)
        {
            try
            {
                if (roomId == null)
                {
                    Clients.Caller.onError("Please select room");
                }
                if (users == null)
                {
                    Clients.Caller.onError("Please select user(s)");
                }

                if (users!=null & users.Length==0)
                {
                    Clients.Caller.onError("Please select user(s)");
                }

                using (var context=new ChatWebEntities())
                {
                    var Id = Convert.ToInt32(roomId);

                    var room = context.Rooms.FirstOrDefault(x => x.Id == Id);

                    if (room != null && room.IsPrivate)
                    {
                        foreach (var user in users)
                        {
                            context.GroupMembers.Add(new GroupMember { UserId = user, GroupId = Id, AddedBy = IdentityName, AddedDate = DateTime.Now });
                        }

                        context.SaveChanges();

                        var roomViewModel = Mapper.Map<Room, RoomViewModel>(room);


                        Clients.Users(users.ToList()).addChatRoom(roomViewModel);
                        Clients.Users(users.ToList()).groupInvitation($"You have invitation to join <b>{room.Name}</b>.");

                        Clients.Caller.getcurrentMembers(roomId);
                    }
                   
                }


            }
            catch (Exception ex)
            {
                Clients.Caller.onError("You failed to invite user(s)" + ex.Message);
            }
        }
        public void Join(string roomName)
        {
            try
            {
                //var user = _Connections.Where(u => u.Username == IdentityName).FirstOrDefault();
                //if (user.CurrentRoom != roomName)
                //{
                //    // Remove user from others list
                //    //if (!string.IsNullOrEmpty(user.CurrentRoom))
                //      //  Clients.OthersInGroup(user.CurrentRoom).removeUser(user);

                //    // Join to new chat room
                //    //Leave(user.CurrentRoom);

                //    Groups.Add(Context.ConnectionId, roomName);
                //    user.CurrentRoom = roomName;

                //    // Tell others to update their list of users
                //    //Clients.OthersInGroup(roomName).addUser(user);
                //}

                Groups.Add(Context.ConnectionId, roomName);

            }
            catch (Exception ex)
            {
                Clients.Caller.onError("You failed to join the chat room!" + ex.Message);
            }
        }

        private void Leave(string roomName)
        {
            Groups.Remove(Context.ConnectionId, roomName);
        }

        public RoomViewModel GetGroupDetails(string roomId)
        {
            var Id = 0;

            if (!string.IsNullOrEmpty(roomId))
            {
                try
                {
                    Id = Convert.ToInt32(roomId);
                }
                catch 
                {

                }
            }

            using (var context=new ChatWebEntities())
            {
                var room = context.Rooms.FirstOrDefault(x => x.Id == Id);

                var members= context.GroupMembers.Where(x => x.Id == Id).ToList();
                var roomViewModel = Mapper.Map<Room, RoomViewModel>(room);

                if (roomViewModel != null)
                {
                    roomViewModel.IsOwner = (roomViewModel.OwnerId == IdentityName) || (SuperAdminList.Contains(IdentityName));
                    roomViewModel.Members = members.Count();
                }


                


                return roomViewModel;
            }
        }

        public void ChangeStatus(string Status)
        {
            try
            {
                using (var db = new TitleConnectGroupEntities())
                {
                    var companyEmail = IdentityName.DecodeUserName();
                    var user = db.Users.Where(x => x.CompanyEmail == companyEmail).FirstOrDefault();

                    if (user != null)
                        user.UserActiveStatus = Status;

                    db.SaveChanges();
                    
                    var userViewModel = new UserViewModel();
                    userViewModel.DisplayName = user.DisplayName;
                    userViewModel.Avatar = user.ProfilePicture;
                    userViewModel.Status = (string.IsNullOrEmpty(user.UserActiveStatus) ? "Offline" : user.UserActiveStatus);


                    if (_Connections.Any(x => x.Username == IdentityName))
                    {
                        var conn = _Connections.FirstOrDefault(x => x.Username == IdentityName);

                        conn.Status = userViewModel.Status;
                    }
                    

                    var userName = IdentityName;

                    Clients.Caller.getProfileInfo(userViewModel.DisplayName, userViewModel.Avatar, userViewModel.Status, userName);
                    Clients.All.updateUserStatus(userViewModel.Status, userName);

                }
            }
            catch (Exception ex)
            {

                Clients.Caller.onError("Couldn't update status : " + ex.Message);
            }
        }
        public void CreateRoom(string roomName,bool isPrivate)
        {
            try
            {
                using (var db = new ChatWebEntities())
                {
                    // Accept: Letters, numbers and one space between words.
                    Match match = Regex.Match(roomName, @"^\w+( \w+)*$");
                    if (!match.Success)
                    {
                        Clients.Caller.onError("Invalid room name!\nRoom name must contain only letters and numbers.");
                    }
                    else if (roomName.Length < 5 || roomName.Length > 20)
                    {
                        Clients.Caller.onError("Room name must be between 5-20 characters!");
                    }
                    else if (db.Rooms.Any(r => r.Name == roomName))
                    {
                        Clients.Caller.onError("Another chat room with this name exists");
                    }
                    else
                    {

                        var room = new Room()
                        {
                            Name = roomName,
                            UserAccount_Id = IdentityName.EncodeUserName(),
                            CreatedDate = DateTime.UtcNow,
                            IsPrivate = isPrivate
                        };

                        db.Rooms.Add(room);
                        
                        db.SaveChanges();

                        if (room.IsPrivate)
                        {
                            db.GroupMembers.Add(new GroupMember { UserId = IdentityName, GroupId = room.Id, AddedBy = IdentityName, AddedDate = DateTime.Now });
                        }


                        db.SaveChanges();

                        if (room != null)
                        {
                            // Update room list
                            var roomViewModel = Mapper.Map<Room, RoomViewModel>(room);
                            _Rooms.Add(roomViewModel);

                            if(!room.IsPrivate)
                                Clients.All.addChatRoom(roomViewModel);
                            else
                                Clients.Caller.addChatRoom(roomViewModel);

                        }
                    }
                }//using
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("Couldn't create chat room: " + ex.Message);
            }
        }

        public void DeleteRoom(string roomId)
        {
            try
            {
                var Id = 0;

                if (!string.IsNullOrEmpty(roomId))
                {
                    try
                    {
                        Id = Convert.ToInt32(roomId);
                    }
                    catch
                    {

                    }
                }

                using (var db = new ChatWebEntities())
                {
                    // Delete from database
                    var room = db.Rooms.Where(r => r.Id == Id && (r.UserAccount_Id == IdentityName || SuperAdminList.Contains(IdentityName) )).FirstOrDefault();
                    if (room != null)
                    {
                        room.IsDeleted = true;
                        room.DeletedBy = IdentityName;
                        room.DeletedDate=DateTime.Now;

                        db.SaveChanges();
                        
                        var roomViewModel = _Rooms.First<RoomViewModel>(r => r.Id == Id);
                        _Rooms.Remove(roomViewModel);

                        // Move users back to Lobby

                        Clients.Group(roomId).onRoomDeleted(string.Format("Room {0} has been deleted.\nYou are now moved to the Lobby!", room.Name));

                        // Tell all users to update their room list
                        Clients.All.removeChatRoom(roomViewModel);
                    }
                }
            }
            catch (Exception)
            {
                Clients.Caller.onError("Can't delete this chat room.");
            }
        }

        public void DeleteMember(string roomId, string userId)
        {
            try
            {
                var Id = 0;

                try
                {
                    Id = Convert.ToInt32(roomId);
                }
                catch
                {

                }

                if (!string.IsNullOrEmpty(userId))
                {
                    using (var db = new ChatWebEntities())
                    {
                        // Delete from database
                        var member = db.GroupMembers.Where(r => r.UserId == userId && r.GroupId == Id).FirstOrDefault();
                        if (member != null)
                        {
                            db.GroupMembers.Remove(member);
                            db.SaveChanges();
                        }
                    }

                }

                Clients.Caller.getcurrentMembers(roomId);
                Clients.User(userId).removeChatRoom(roomId);

            }
            catch (Exception)
            {
                Clients.Caller.onError("Can't delete this user.");
            }
        }

        public IEnumerable<MessageViewModel> GetMessageHistory(string chatuniqueId)
        {
            using (var context = new ChatWebEntities())
            {
                List<MessageViewModel> messages = new List<MessageViewModel>();

                if (TitleConnectHelper.CheckUser(chatuniqueId.DecodeUserName()))
                {

                    var messageHistory = context.PrivateMessages.Where(m => (m.FromId == IdentityName && m.ToId == chatuniqueId) || (m.FromId == chatuniqueId && m.ToId == IdentityName))
                    .OrderByDescending(m => m.Timestamp)
                    .Take(20)
                    .AsEnumerable()
                    .Reverse()
                    .ToList();


                    foreach (var msg in messageHistory)
                    {
                        MessageViewModel messageViewModel = new MessageViewModel()
                        {
                            From = msg.FromName,
                            FromId = msg.FromId,
                            Avatar = "",// StaticResources.Avatars[0],
                            To = msg.ToName,
                            ToId = msg.ToId,
                            Content = msg.Content.ParseEmojis(),
                            Timestamp = new DateTime(long.Parse(msg.Timestamp)).ToString()
                        };

                        messages.Add(messageViewModel);
                    }

                    var unreadMessage = context.PrivateMessages.Where(x => x.ReadStatus == false && x.FromId== chatuniqueId).ToList();

                    unreadMessage.ForEach(x => { x.ReadStatus = true; x.ReadTimestamp = DateTime.UtcNow.ToString(); });
                    context.SaveChanges();

                    return messages.AsEnumerable();
                }

                else
                {
                    var Id = 0;

                    try
                    {
                        Id = Convert.ToInt32(chatuniqueId);
                    }
                    catch
                    {

                    }


                    var messageHistory = context.Messages.Where(m => m.ToRoomId == Id)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(100)
                    .AsEnumerable()
                    .Reverse()
                    .ToList();

                    foreach (var msg in messageHistory)
                    {
                        MessageViewModel messageViewModel = new MessageViewModel()
                        {
                            From = msg.FromName,
                            FromId = msg.FromId,
                            Avatar = StaticResources.Avatars[0],
                            To = msg.Room.Name,
                            ToId = msg.ToRoomId.ToString(),
                            Content = msg.Content,
                            Timestamp = new DateTime(long.Parse(msg.Timestamp)).ToString()
                        };

                        messages.Add(messageViewModel);
                    }

                    return messages.AsEnumerable();

                }
            }
        }

        public void UpdateUnRead(string chatuniqueId)
        {
            using (var context = new ChatWebEntities())
            {
                    var unreadMessage = context.PrivateMessages.Where(x => x.ReadStatus == false && x.FromId == chatuniqueId).ToList();

                    unreadMessage.ForEach(x => { x.ReadStatus = true; x.ReadTimestamp = DateTime.UtcNow.ToString(); });
                    context.SaveChanges();

                 
            }
        }

        public IEnumerable<UnReadMessageViewModel> GetUnReadMessages()
        {
            var messages = new List<MessageViewModel>();

            using (var context = new ChatWebEntities())
            {
                return context.PrivateMessages.Where(m => m.ToId == IdentityName && !m.ReadStatus)
                   .GroupBy(p => p.FromId)
                   .Select(g => new  UnReadMessageViewModel { FromId = g.Key, ToId= IdentityName, Count = g.Count() }).ToList();
            }
        }

        public IEnumerable<RoomViewModel> GetRooms()

        {
            using (var db = new ChatWebEntities())
            {
                // First run?
                _Rooms.Clear();

                if (_Rooms.Count == 0)
                {
                    foreach (var room in db.Rooms.Where(x=>!x.IsPrivate  && !x.IsDeleted))
                    {
                        var roomViewModel = Mapper.Map<Room, RoomViewModel>(room);
                        _Rooms.Add(roomViewModel);
                    }

                    foreach (var room in db.Rooms.Where(x => x.IsPrivate && !x.IsDeleted))
                    {
                        var IsUserPrivateMember = db.GroupMembers.Any(x => x.GroupId == room.Id && x.UserId == IdentityName);
                        if(IsUserPrivateMember)
                        {
                            var roomViewModel = Mapper.Map<Room, RoomViewModel>(room);
                            _Rooms.Add(roomViewModel);
                        }
                    }
                }
            }

            return _Rooms.ToList();
        }

        public IEnumerable<UserViewModel> GetUsers(string roomName)
        {
            return _Connections.Where(u => u.CurrentRoom == roomName).ToList();
        }

        public IEnumerable<UserViewModel> GroupMembers(string roomId)
        {

            var userList = new List<UserViewModel>();

            if (!string.IsNullOrEmpty(roomId))
            {
                var Id = 0;

                if (!string.IsNullOrEmpty(roomId))
                {
                    try
                    {
                        Id = Convert.ToInt32(roomId);
                    }
                    catch
                    {

                    }
                }

                if (Id > 0)
                {
                    var emails = new List<string>();

                    using (var context = new ChatWebEntities())
                    {
                        var users = context.GroupMembers.Where(x => x.GroupId == Id).ToList();

                        users.ForEach(x =>
                        {
                            emails.Add(x.UserId.DecodeUserName());
                        });

                    }

                    var userlist = TitleConnectHelper.GetUserbyCompanyEmails(emails.ToArray());

                    foreach (var user in userlist)
                    {
                        userList.Add(new UserViewModel
                        {
                            Username = user.CompanyEmail.EncodeUserName(),
                            DisplayName = user.DisplayName,
                            Avatar = user.Avatar,
                            CompanyName = user.CompanyName,
                            CompanyId = user.CompanyId.ToString(),
                            UserId = user.UserId.ToString(),
                            Status = (string.IsNullOrEmpty(user.UserActiveStatus) ? "Offline" : user.UserActiveStatus.ToString())
                        });
                    }

                    userList = userList.Where(x => x.Username != IdentityName).OrderBy(x => x.DisplayName).ToList();

                    return userList.AsEnumerable();
                }
               
            }


            return userList;


        }
        public IEnumerable<UserViewModel> GetAllUsers()
        {
            var userList = new List<UserViewModel>();

            var userlist = TitleConnectHelper.GetAllUsers();

            foreach (var user in userlist)
            {
                userList.Add(new UserViewModel
                {
                    Username = user.CompanyEmail.EncodeUserName(),
                    DisplayName = user.DisplayName,
                    Avatar = user.Avatar,
                    CompanyName = user.CompanyName,
                    CompanyId = user.CompanyId.ToString(),
                    UserId = user.UserId.ToString(),
                    Status = (string.IsNullOrEmpty(user.UserActiveStatus) ? "Offline" : user.UserActiveStatus.ToString())
                });
            }

            userList = userList.Where(x => x.Username != IdentityName).OrderBy(x=>x.Status).ThenBy(x=>x.DisplayName).ToList();

            return userList.AsEnumerable();
        }
        
        public IEnumerable<UserViewModel> GetCompanyUsers(int companyId)
        {
            var userList = new List<UserViewModel>();

            var userlist = TitleConnectHelper.GetCompanyUsers(companyId);


            foreach (var user in userlist)
            {
                userList.Add(new UserViewModel
                {
                    Username = user.CompanyEmail.EncodeUserName(),
                    DisplayName = user.DisplayName,
                    Avatar = user.Avatar,
                    CompanyName = user.CompanyName,
                    CompanyId = user.CompanyId.ToString(),
                    UserId = user.UserId.ToString(),
                    Status = (string.IsNullOrEmpty(user.UserActiveStatus) ? "Offline" : user.UserActiveStatus.ToString())
                });
            }

            
            //userList = userList.Where(x => x.Username != IdentityName).ToList();

            var onlinelist = userList.Where(x=>x.Status=="Online").OrderBy(x => x.DisplayName).ToList();
            var awaylist = userList.Where(x => x.Status == "Away").OrderBy(x => x.DisplayName).ToList();
            var otherlist = userList.Where(x => x.Status != "Away" && x.Status != "Online").OrderBy(x => x.DisplayName).ToList();

            var finallist = new List<UserViewModel>();


            finallist.AddRange(onlinelist);
            finallist.AddRange(awaylist);
            finallist.AddRange(otherlist);

            return finallist.AsEnumerable();
        }

        public IEnumerable<CompanyViewModel> GetCompanyies()
        {
            var companies = TitleConnectHelper.GetCompanies();

            foreach (var company in companies)
            {
                company.Employees = GetCompanyUsers(company.CompanyId);
                company.EmployeesCount = company.Employees.Count();
                company.OnlineEmployeesCount = company.Employees.Where(x => x.Status == "Online").Count();
            }


            return companies.Where(x => x.EmployeesCount > 0).ToList().AsEnumerable();
        }

        #region OnConnected/OnDisconnected

        public override Task OnConnected()
        {
            try
            {
                var user = TitleConnectHelper.GetUserbyCompanyEmail(IdentityName.DecodeUserName());

                var userViewModel = Mapper.Map<User, UserViewModel>(user);
                userViewModel.Device = GetDevice();
                userViewModel.CurrentRoom = "";
                userViewModel.Avatar = user.ProfilePicture;
                userViewModel.Status = (string.IsNullOrEmpty(user.UserActiveStatus) ? "Offline" : user.UserActiveStatus);
                
                if (!_Connections.Any(x => x.Username == IdentityName))
                {
                    _Connections.Add(userViewModel);
                }
                if (!_ConnectionsMap.Any(x => x.Key == IdentityName))
                {
                    _ConnectionsMap.Add(IdentityName, Context.ConnectionId);
                }

                var rooms = GetRooms();

                foreach (var room in rooms)
                {
                    Groups.Add(Context.ConnectionId, room.Id.ToString());
                }

                var userName = IdentityName;
                
                Clients.Caller.getProfileInfo(userViewModel.DisplayName, userViewModel.Avatar, userViewModel.Status, userName);
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("OnConnected:" + ex.Message);
            }


            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                var user = _Connections.Where(u => u.Username == IdentityName).First();
                _Connections.Remove(user);

                // Remove mapping
                _ConnectionsMap.Remove(user.Username);
            }
            catch (Exception ex)
            {
                Clients.Caller.onError("OnDisconnected: " + ex.Message);
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var user = _Connections.Where(u => u.Username == IdentityName).FirstOrDefault();
            if (user != null)
            {
                var userName = IdentityName;
                Clients.Caller.getProfileInfo(user.DisplayName, user.Avatar, user.Status, userName);
            }

            return base.OnReconnected();
        }
        #endregion

        private string IdentityName
        {
            get { return Context.User.Identity.Name; }
        }

        private string GetDevice()
        {
            string device = Context.Headers.Get("Device");

            if (device != null && (device.Equals("Desktop") || device.Equals("Mobile")))
                return device;

            return "Web";
        }
    }
}