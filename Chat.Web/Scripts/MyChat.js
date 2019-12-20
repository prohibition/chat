$(function () {

    var chatHub = $.connection.chatHub;

    $.connection.hub.start().done(function () {
        console.log("chat started");
        model.roomList();
        model.userList();
        model.companyList();
        model.usersList();
        model.joinedRoom = "";
        model.joinRoom();
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

    // Client Operations
    chatHub.client.privateMessage = function (messageView) {

        var isMine = messageView.From === model.myName();
        var message = new ChatMessage(messageView.Content,
            messageView.Timestamp,
            messageView.From,
            isMine,
            messageView.Avatar);


        if (messageView.ToId == model.joinedRoom || messageView.FromId == model.joinedRoom) {
            model.chatMessages.push(message);
            $(".chat-body").animate({ scrollTop: $(".chat-body")[0].scrollHeight }, 1000);

        }
        else
        {
            var notification = new Notification("New Message From "+messageView.From + ": " + messageView.Content);
        }
    };


    chatHub.client.groupMessage = function (messageView) {

        var isMine = messageView.From === model.myName();
        var message = new ChatMessage(messageView.Content,
            messageView.Timestamp,
            messageView.From,
            isMine,
            messageView.Avatar);


        
        if (messageView.ToId == model.joinedRoom)
        {
            model.chatMessages.push(message);
            $(".chat-body").animate({ scrollTop: $(".chat-body")[0].scrollHeight }, 1000);

        }
        else
        {
            var notification = new Notification("New Message From " + messageView.To + ": " + messageView.Content);
        }

    };

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

    chatHub.client.getProfileInfo = function (displayName, avatar) {
        model.myName(displayName);
        model.myAvatar(avatar);
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

    $('ul#room-list').on('click', 'a', function () {
        var roomName = $(this).text();
        model.joinedRoom = $(this).attr("id");
        model.joinRoom();
        model.chatMessages.removeAll();
        $("input#iRoom").val(roomName);
        $("#joinedRoom").text(roomName);
        $('#room-list a').removeClass('active');
        $('#users-list a').removeClass('active');
        $(this).addClass('active');
    });

   

    loadUser=function(obj){
        
        var username = $(obj).text();
        var companyName = $(obj).attr("comp");

        var roomName = username + " (" + companyName + ")";

        model.joinedRoom = $(obj).attr("id");
        model.joinRoom();
        model.chatMessages.removeAll();
        $("input#iRoom").val(roomName);
        $("#joinedRoom").text(roomName);
        $('#users-list a').removeClass('active');
        $('#room-list a').removeClass('active');
        $(obj).addClass('active');
    }

    chatHub.client.addChatRoom = function (room) {
        model.roomAdded(new ChatRoom(room.Id, room.Name));
    };

    chatHub.client.removeChatRoom = function (room) {
        model.roomDeleted(room.Id);
    };

    chatHub.client.onError = function (message) {
        model.serverInfoMessage(message);

        $("#errorAlert").removeClass("hidden").show().delay(5000).fadeOut(500);
    };

    chatHub.client.onRoomDeleted = function (message) {
        model.serverInfoMessage(message);
        $("#errorAlert").removeClass("hidden").show().delay(5000).fadeOut(500);

        // Join to the first room in list
        $("ul#room-list li a")[0].click();
    };

    var Model = function () {
        var self = this;
        self.message = ko.observable("");
        self.chatRooms = ko.observableArray([]);
        self.chatUsers = ko.observableArray([]);
        self.chatAllUsers = ko.observableArray([]);
        self.chatCompanies = ko.observableArray([]);
        self.chatMessages = ko.observableArray([]);
        self.joinedRoom = ko.observable("");
        self.joineduser = ko.observable("");
        self.serverInfoMessage = ko.observable("");
        self.myName = ko.observable("");
        self.myAvatar = ko.observable("");
        self.onEnter = function (d, e) {
            if (e.keyCode === 13) {
                self.sendNewMessage();
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
                console.log(root);
            }
            
            chatHub.server.join(self.joinedRoom).done(function () {
                self.userList();
                self.messageHistory();
            });
        },

        roomList: function () {
            var self = this;
            chatHub.server.getRooms().done(function (result) {
                self.chatRooms.removeAll();
                for (var i = 0; i < result.length; i++) {
                    self.chatRooms.push(new ChatRoom(result[i].Id, result[i].Name));
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
                    self.chatCompanies.push(new Company(result[i].CompanyId, result[i].CompanyName, result[i].Employees, result[i].EmployeesCount))
                }
            });

        },

        createRoom: function () {
            var name = $("#roomName").val();
            var isPrivate = $("#chkPrivate").is(":checkhed");
            chatHub.server.createRoom(name,isPrivate);
        },

        deleteRoom: function () {
            var self = this;
            chatHub.server.deleteRoom(self.joinedRoom);
        },

        messageHistory: function () {
            var self = this;
            chatHub.server.getMessageHistory(self.joinedRoom).done(function (result) {
                self.chatMessages.removeAll();
                for (var i = 0; i < result.length; i++) {
                    var isMine = result[i].From == self.myName();
                    self.chatMessages.push(new ChatMessage(result[i].Content,
                                                     result[i].Timestamp,
                                                     result[i].From,
                                                     isMine,
                                                     result[i].Avatar))
                }

                $(".chat-body").animate({ scrollTop: $(".chat-body")[0].scrollHeight }, 1000);

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
                if (room.roomId() == id)
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
    function ChatRoom(roomId, name) {
        var self = this;
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

    function User(userName, displayName, avatar, companyName) {
        var self = this;
        self.userName = ko.observable(userName);
        self.displayName = ko.observable(displayName);
        self.avatar = ko.observable(avatar);
        self.companyName = ko.observable(companyName);
    }

    function Company(companyId, companyName,employeesdata,employeeCount) {
        var self = this; var href = '#' + companyId;
        self.companyId = ko.observable(companyId);
        self.companyName = ko.observable(companyName);
        self.href = ko.observable(href);
        self.employeeCount = ko.observable(employeeCount);
        self.employees = ko.observableArray();

        for (var i = 0; i < employeesdata.length; i++) {
            self.employees.push(new User(employeesdata[i].Username,
                employeesdata[i].DisplayName,
                employeesdata[i].Avatar, employeesdata[i].CompanyName))
        }

        
    }

    function ChatMessage(content, timestamp, from, isMine, avatar) {
        var self = this;
        self.content = ko.observable(content);
        self.timestamp = ko.observable(timestamp);
        self.from = ko.observable(from);
        self.isMine = ko.observable(isMine);
        self.avatar = ko.observable(avatar);
    }

    var model = new Model();
    ko.applyBindings(model);

});