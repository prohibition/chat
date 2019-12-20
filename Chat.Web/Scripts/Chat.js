$(function () {

    var chatHub = $.connection.chatHub;

    // Client Operations
    chatHub.client.privateMessage = function (messageView) {

        var isMine = messageView.FromId === model.myUserName();

        var message = new ChatMessage(messageView.Content,
            messageView.Timestamp,
            messageView.From,
            isMine,
            messageView.Avatar);

        if (messageView.ToId == model.joinedRoom || messageView.FromId == model.joinedRoom || messageView.FromId == model.myUserName())
        {
            model.chatMessages.push(message);
            model.updateUnRead();
            $(".message-history").animate({ scrollTop: $(".message-history")[0].scrollHeight }, 1000);
        }
        else {
            var content = "New Message From " + messageView.From;
            Shownotification(content);
        }

        model.unReadMessage();
        

    };

    chatHub.client.groupMessage = function (messageView) {

        var isMine = messageView.FromId === model.myUserName();

        var message = new ChatMessage(messageView.Content,
            messageView.Timestamp,
            messageView.From,
            isMine,
            messageView.Avatar);

        
        if (messageView.ToId == model.joinedRoom || messageView.FromId == model.joinedRoom || messageView.FromId == model.myUserName())
        {
            model.chatMessages.push(message);
            model.updateUnRead();
            $(".message-history").animate({ scrollTop: $(".message-history")[0].scrollHeight }, 1000);

        }
        else
        {
            var content = "New Message From " + messageView.From;
            Shownotification(content);
        }


        model.unReadMessage();

        

    };

    chatHub.client.getProfileInfo = function (displayName, avatar, status, userName) {

        var src = '/Content/icons/avatar_2x.png';

        if (avatar != "" && avatar != null) {
            src = "https://titleconnectgroup.com/ProfilePictures/" + avatar;
        }

        model.myName(displayName);
        model.myUserName(userName);

        model.myAvatar(src);
        model.myStatus(status);

        if (status == "Online" || status=="Away")
            model.myStatusCss(status);
        else
            model.myStatusCss("Offline");

    };

    chatHub.client.updateUserStatus = function (status, userName) {
       
        if (status == "Online" || status == "Away")
        {
            $('#' + userName + '_status').removeClass("Online").removeClass("Away").removeClass('Offline');
            $('#' + userName + '_status').addClass(status).attr("data-original-title", status)
            $('#li_' + userName).attr("data-original-title", status);
        }
        else 
        {
            $('#' + userName + '_status').removeClass("Online").removeClass("Away").removeClass('Offline');
            $('#' + userName + '_status').addClass("Offline").attr("data-original-title", status)
            $('#li_' + userName).attr("data-original-title", status);
        }

    };

    chatHub.client.addUser = function (user) {
        model.userAdded(new ChatUser(user.Username,
            user.DisplayName,
            user.Avatar,
            user.CurrentRoom,
            user.Device));
    };

    chatHub.client.removeUser = function (user) {
        model.userRemoved(user.Username);
    };

    deleteMember = function (obj)
    {
        var id = $(obj).attr("id");

        if (confirm("Are you sure? You want to remove this user?"))
        {
            model.deleteMember(id);
        }
    }
    
    loadUser = function (obj) {
        

        var id = $(obj).attr("id");

        var isme = $('#' + id).attr('isme');

        if (isme){
            return false;
        }

        var spanunread = id + '_unread';

        $('#' + spanunread).html("").removeClass("unread");

        var compId = '#' + $('#' + spanunread).attr('compId');

        $(compId).html("").removeClass("unread");

        var username = $(obj).attr("uname");

        var li = '#li_' + id

        var companyName = $(obj).attr("comp");

        var roomName = username + " (" + companyName + ")";

        $('#chat-message').attr("placeholder", "Message " + roomName)

        $(".channel-menu_prefix").hide();
        model.joinedRoom = $(obj).attr("id");
        model.joinRoom();
        model.chatMessages.removeAll();
        $("input#iRoom").val(roomName);
        $("#joinedRoom").text(roomName);
        $('#users-list li').removeClass('active');
        $('#room-list li').removeClass('active');
        $(li).addClass('active');
        //$('#spactions').addClass('hidden');
        $('#chat-message').focus();
    }

    loadRoom = function (obj) {

        var id = $(obj).attr("id");

        var spanNew = id + '_new';

        $('#' + spanNew).remove();

        var roomName = $(obj).attr("cname");

        var li = '#li_' + id;

        $('#chat-message').attr("placeholder", "Message " + roomName)

        $(".channel-menu_prefix").show();
        model.joinedRoom = $(obj).attr("id");
        model.joinRoom();
        model.chatMessages.removeAll();
        $("#joinedRoom").text(roomName);
        $('#room-list li').removeClass('active');
        $('#users-list li').removeClass('active');
        //$('#spactions').removeClass('hidden');

        $(li).addClass('active');

        $('#roomtitle').html(roomName);

        $('#chat-message').focus();
    }

    chatHub.client.addChatRoom = function (room) {
        model.roomAdded(new ChatRoom(room.Id, room.Name, room.IsPrivate));
        showSuccessToast('New Room Added', room.Name);
    };

    chatHub.client.getcurrentMembers = function (room) {
        model.currentgroupMembers();
        showSuccessToast(room.Name,'Member List Updated');
    };


    chatHub.client.removeChatRoom = function (room) {
        model.roomDeleted(room);
    };

    chatHub.client.onError = function (message) {
        
        showDangerToast('Error', message);

    };

    chatHub.client.groupInvitation = function (message) {
        
        var content = "New Group Invitation:" + message;
        Shownotification(content);
    };

    chatHub.client.onRoomDeleted = function (message)
    {
        $("ul#room-list li a")[0].click();
        showSuccessToast('Success', message);
    };



    $.connection.hub.start().done(function () {
        
        model.roomList();
        model.companyList();
        model.joinedRoom = "";
        model.joinRoom();

    });


    $.connection.hub.error(function (error) {
        console.warn(error);
    });

    $.connection.hub.connectionSlow(function () {
        // Your function to notify user.
        console.log('connection slow');
    });

    $.connection.hub.reconnecting(function () {
        // Your function to notify user.
        console.log('reconnecting');
    });


    $.connection.hub.disconnected(function () {
        setTimeout(function () {
            $.connection.hub.start();
        }, 5000); // Restart connection after 5 seconds.

        console.log('restarted connection');
    });
    $.connection.hub.stateChanged(function (change) {
        var oldState = null,
            newState = null;

        for (var state in $.signalR.connectionState) {
            if ($.signalR.connectionState[state] === change.oldState) {
                oldState = state;
            } else if ($.signalR.connectionState[state] === change.newState) {
                newState = state;
            }
        }

        //
    });


    var Model = function () {
        var self = this;
        self.message = ko.observable("");
        self.chatRooms = ko.observableArray([]);
        self.chatUsers = ko.observableArray([]);
        self.chatAllUsers = ko.observableArray([]);
        self.groupMembers = ko.observableArray([]);
        self.chatCompanies = ko.observableArray([]);
        self.chatMessages = ko.observableArray([]).distinct('date');
        self.chatUnMessages = ko.observableArray([]);
        self.chatDates = ko.observableArray([]);
        self.joinedRoom = ko.observable("");
        self.joineduser = ko.observable("");
        self.serverInfoMessage = ko.observable("");
        self.myName = ko.observable("");
        self.myAvatar = ko.observable("");
        self.myStatus = ko.observable("");
        self.myStatusCss = ko.observable("");
        self.myUserName = ko.observable("");
        
        self.onEnter = function (d, e) {
            if (e.keyCode === 13) {

                if (self.message().trim() != "")
                {
                    self.sendNewMessage();
                }
            }
            return true;
        }
        self.filter = ko.observable("");
        self.filteredChatUsers = ko.computed(function () {
            if (!self.filter()) {
                return self.chatUsers();
            } else {
                return ko.utils.arrayFilter(self.chatUsers(), function (user) {
                    var displayName = user.displayName().toLowerCase();
                    return displayName.includes(self.filter().toLowerCase());
                });
            }
        });

    };

    Model.prototype = {

        // Server Operations
        sendNewMessage: function () {
            var self = this;
            chatHub.server.send(self.joinedRoom, self.message());
            self.message("");
        },

        joinRoom: function (root)
        {
            var self = this;
            if (root)
            {
                self = root;
            }
            
            chatHub.server.join(self.joinedRoom).done(function ()
            {
                self.groupMembers.removeAll();
                self.groupDetails();
                self.messageHistory();
            });
        },

        updateUnRead: function () {

            var self = this;
            chatHub.server.updateUnRead(self.joinedRoom).done(function () {
               
            });

        },

        roomList: function () {
            var self = this;
            chatHub.server.getRooms().done(function (result) {
                self.chatRooms.removeAll();
                for (var i = 0; i < result.length; i++) {
                    self.chatRooms.push(new ChatRoom(result[i].Id, result[i].Name, result[i].IsPrivate));
                }
            });
        },

        userList: function () {
            var self = this;
            chatHub.server.getUsers(self.joinedRoom).done(function (result) {
                self.chatUsers.removeAll();
                for (var i = 0; i < result.length; i++) {
                    self.chatUsers.push(new ChatUser(result[i].Username,
                        result[i].DisplayName,
                        result[i].Avatar,
                        result[i].CurrentRoom,
                        result[i].Device))
                }
            });

        },

        usersList: function () {
            var self = this;
            chatHub.server.getAllUsers().done(function (result) {
                self.chatAllUsers.removeAll();

                for (var i = 0; i < result.length; i++) {
                    self.chatAllUsers.push(new User(result[i].Username,
                        result[i].DisplayName,
                        result[i].Avatar))
                }
            });

        },

        companyList: function () {
            var self = this;
            chatHub.server.getCompanyies().done(function (result) {
                self.chatCompanies.removeAll();
                
                for (var i = 0; i < result.length; i++) {
                    self.chatCompanies.push(new Company(result[i].CompanyId, result[i].CompanyName, result[i].Employees, result[i].EmployeesCount,result[i].OnlineEmployeesCount))
                }
            });

        },

        createRoom: function () {
            var name = $("#roomName").val();
            var isPrivate = $("#chkPrivate").is(":checked");
            chatHub.server.createRoom(name, isPrivate);
        },

        inviteUsers: function () {
            var self = this;
            var users = $("#users").val();
            chatHub.server.inviteUsers(self.joinedRoom, users);
        },

        changeStatus: function () {

            var status = $("#customstatus").val();
            chatHub.server.changeStatus(status);
        },

        

        deleteRoom: function () {
            var self = this;
            chatHub.server.deleteRoom(self.joinedRoom);
        },

        deleteMember: function (user) {
            var self = this;
            chatHub.server.deleteMember(self.joinedRoom, user);
        },

        

        groupDetails: function () {
            var self = this;
            chatHub.server.getGroupDetails(self.joinedRoom).done(function (result) {
               
                if (result != undefined) {
                    if (result.IsOwner == true) {
                        $('#spactions').removeClass('hidden');
                    }
                    else {
                        $('#spactions').addClass('hidden');
                    }

                    if (result.IsPrivate == true) {
                        $('.channel-menu_prefix').html('<i class="mdi mdi-lock mdi-18px"></i>');
                        $('#addmember').removeClass('hidden');
                        $('#members').removeClass('hidden');
                        $('#membercount').html(result.Members);

                        self.currentgroupMembers();
                    }
                    else {
                        $('.channel-menu_prefix').html('#');
                        $('#addmember').addClass('hidden');
                        $('#members').addClass('hidden');
                        $('#membercount').html('');
                    }
                }
                else
                {
                    $('#spactions').addClass('hidden');
                    $('.channel-menu_prefix').html('');
                }

            });
        },

        currentgroupMembers: function () {
            var self = this;
            chatHub.server.groupMembers(self.joinedRoom).done(function (result) {

                self.groupMembers.removeAll();

                for (var i = 0; i < result.length; i++) {
                    self.groupMembers.push(new User(result[i].Username,
                        result[i].DisplayName,
                        result[i].Avatar))
                }

            });
        },

        messageHistory: function () {
            var self = this;

            self.chatDates.removeAll();
            self.chatMessages.removeAll();

            chatHub.server.getMessageHistory(self.joinedRoom).done(function (result) {
                
                for (var i = 0; i < result.length; i++) {
                    var isMine = result[i].From == self.myName();
                    self.chatMessages.push(new ChatMessage(result[i].Content,
                                                     result[i].Timestamp,
                                                     result[i].From,
                                                     isMine,
                                                     result[i].Avatar))
                }

                
                self.chatMessages = self.chatMessages.distinct('date');

                

                for (var i = 0; i < result.length; i++) 
                {
                    var date=converDate(new Date(result[i].Timestamp));

                    if (self.chatDates().indexOf(date) == -1)
                    {
                        self.chatDates.push(date);
                    }
                }

                $(".message-history").animate({ scrollTop: $(".message-history")[0].scrollHeight }, 1000);

            });
        },
        
        unReadMessage: function () {
            var self = this;
            $('[data-toggle="tooltip"]').tooltip();
            chatHub.server.getUnReadMessages().done(function (result) {
                for (var i = 0; i < result.length; i++) 
                {
                    var id = '#' + result[i].FromId + '_unread';

                    var compId = '#'+$(id).attr('compId');

                    
                    if (result[i].Count > 0 && self.joinedRoom != result[i].FromId) {
                        
                        $(id).html(result[i].Count).addClass("unread");

                        $(compId).removeClass("unread");
                        $(compId).html('new').addClass("unread");

                    } else {

                        $(id).html("").removeClass("unread");
                    }
                }
            });
        },

        roomAdded: function (room) {
            var self = this;
            self.chatRooms.push(room);
        },

        roomDeleted: function(id){
            var self = this;
            var temp;
            ko.utils.arrayForEach(self.chatRooms(), function (room) {
                if (room.Id == id)
                    temp = room;
            });
            self.chatRooms.remove(temp);
        },

        userAdded: function (user) {
            var self = this;
            self.chatUsers.push(user)
        },

        userRemoved: function (id) {
            var self = this;
            var temp;
            ko.utils.arrayForEach(self.chatUsers(), function (user) {
                if (user.userName() == id)
                    temp = user;
            });
            self.chatUsers.remove(temp);
        },
        
    };

    // Represent server data
    function ChatRoom(roomId, name,isprivate) {
        var self = this;
        var isPublic = !isprivate;

        self.isPublic = ko.observable(isPublic);
        self.isPrivate = ko.observable(isprivate);
        self.roomId = ko.observable(roomId);
        self.name = ko.observable(name);
        
    }

    function ChatUser(userName, displayName, avatar, currentRoom, device) {
        var self = this;
        self.userName = ko.observable(userName);
        self.displayName = ko.observable(displayName);
        self.avatar = ko.observable(avatar);
        self.currentRoom = ko.observable(currentRoom);
        self.device = ko.observable(device);
    }

    function User(userName, displayName, avatar, companyName, status, companyId) {
        var self = this;

        var css = "Offline";

        if (status == "Online" || status == "Away") {
            css = status;
        }
        var isMe = userName === model.myUserName();
        
        self.isMe = ko.observable(isMe);
        self.userName = ko.observable(userName);

        self.displayName = ko.observable(displayName);
        self.avatar = ko.observable(avatar);
        self.companyName = ko.observable(companyName);
        self.status = ko.observable(status);
        self.statuscss = ko.observable(css);
        self.companyId = ko.observable(companyId);

    }

    function Company(companyId, companyName, employeesdata, employeeCount, onlineEmployeeCount) {
        var self = this; var href = '#' + companyId;
        self.companyId = ko.observable(companyId);
        self.companyName = ko.observable(companyName);
        self.href = ko.observable(href);
        self.employeeCount = ko.observable(employeeCount);
        self.onlineEmployeeCount = ko.observable(onlineEmployeeCount);
        self.employees = ko.observableArray();
         

        for (var i = 0; i < employeesdata.length; i++) {
            self.employees.push(new User(employeesdata[i].Username,
                employeesdata[i].DisplayName,
                employeesdata[i].Avatar, employeesdata[i].CompanyName, employeesdata[i].Status, employeesdata[i].CompanyId));
        }
    }

    function ChatMessage(content, timestamp, from, isMine, avatar) {
        var self = this;
        self.content = ko.observable(content);
        self.timestamp = ko.observable(converTime(new Date(timestamp)));
        self.date = ko.observable(converDate(new Date(timestamp)));
        self.from = ko.observable(from);
        self.isMine = ko.observable(isMine);
        self.avatar = ko.observable(avatar);
        self.initials = ko.observable(from.getInitials());
        self.avatarCss = ko.observable(getColorHash(from));
    }

    function converDate(date)
    {
        var date = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds()));
        
        if (date.toString("MM/dd/yyyy") == Date.parse('today').toString("MM/dd/yyyy"))
        {
            return "Today";
        }
        else if (date.toString("MM/dd/yyyy") == Date.parse('yesterday').toString("MM/dd/yyyy")) {
            return "Yesterday";
        }
        else {

            return date.toString("dddd, MMMM dd yyyy");
        }
        
    }

    function converTime(date) {
        var date = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds()));
        //console.log(date.toString("hh:mm:ss tt"));

        return date.toString("hh:mm tt");
    }

    ko.observableArray.fn.distinct = function (prop) {
        var target = this;
        target.index = target.index || {};
        target.index[prop] = target.index[prop] || ko.observable({});

        ko.computed(function () {
            //rebuild index
            var propIndex = {};

            ko.utils.arrayForEach(target(), function (item) {
                var key = ko.utils.unwrapObservable(item[prop]);
                if (key) {
                    propIndex[key] = propIndex[key] || [];
                    propIndex[key].push(item);
                }
            });

            target.index[prop](propIndex);

        });

        return target;
    };


    var model = new Model();
    ko.applyBindings(model);


});