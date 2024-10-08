﻿using System.Text.RegularExpressions;
using Fleck;
using Newtonsoft.Json;

namespace Phi_MGUS;

public static class GameManager
{
    /// <summary>
    /// Room Manager | 房间管理器
    /// </summary>
    public static class RoomManager
    {
        public static readonly List<Room> RoomList = new();

        public static void AddRoom(User owner, string roomID, int maxUser)
        {
            RoomList.Add(new Room(owner, roomID, maxUser));
            owner.status = User.Status.InRoom;
        }

        /// <summary>
        /// Dissolve the room | 解散房间
        /// </summary>
        /// <param name="room">房间</param>
        public static void RemoveRoom(Room room)
        {
            for (var i = RoomList.Count - 1; i >= 0; i--)
            {
                if (RoomList[i] == room)
                {
                    RoomList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Remove room by roomID | 根据房间ID删除房间
        /// </summary>
        /// <param name="roomID">房间ID</param>
        public static void RemoveRoom(string roomID)
        {
            for (var i = RoomList.Count - 1; i >= 0; i--)
            {
                if (RoomList[i].roomID == roomID)
                {
                    RoomList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Get room by roomID | 根据房间ID获取房间
        /// </summary>
        /// <param name="roomID">房间ID</param>
        /// <returns>Room | 房间</returns>
        public static Room? GetRoom(string roomID)
        {
            return RoomList.FirstOrDefault(x => x.roomID == roomID);
        }
    }

    /// <summary>
    /// User Manager | 用户管理器
    /// </summary>
    public static class UserManager
    {
        public static readonly List<User> UserList = new();

        public static void AddUser(string name, IWebSocketConnection socket, User.UserConfig config)
        {
            UserList.Add(new User(name, socket, config));
        }

        /// <summary>
        /// Remove user | 移除用户
        /// </summary>
        /// <param name="socket">用户对应Socket</param>
        public static void RemoveUser(IWebSocketConnection socket)
        {
            var user = GetUser(socket);
            if (user.room != null)
            {
                user.room!.Leave(user);
            }

            UserList.Remove(user);
            socket.Close();
        }

        public static bool Contains(IWebSocketConnection socket)
        {
            return UserList.Any(x => x.userSocket == socket);
        }

        public static User GetUser(IWebSocketConnection socket)
        {
            return UserList.First(x => x.userSocket == socket);
        }
    }

    /// <summary>
    /// Room | 房间
    /// </summary>
    public class Room
    {
        public List<User> userList;
        public User owner;
        public int maxUser;
        public string roomID;

        public class ChartInfo
        {
            public string? MD5;
            public string? url;

            public ChartInfo(string MD5, string url)
            {
                this.MD5 = MD5;
                this.url = url;
            }
        }

        private ChartInfo? _chartInfo;

        public ChartInfo? chartInfo
        {
            get => _chartInfo;
            set
            {
                Broadcast(new ConnectionMessage.Server.UserSelectedChart(value!.MD5!, value.url!).Serialize());
                _chartInfo = value;
            }
        }

        /// <summary>
        /// Create room | 创建房间
        /// </summary>
        /// <param name="owner">房间的所有者</param>
        /// <param name="roomID">房间ID</param>
        /// <param name="maxUser">最大用户数量</param>
        /// <exception cref="ArgumentException">Illegal room ID | 非法房间ID</exception>
        public Room(User owner, string roomID, int maxUser)
        {
            if (roomID.Length > 32)
            {
                throw new ArgumentException("RoomIdentifier cannot exceed 32 digits."); // 房间ID长度不能超过32位
            }

            if (!Regex.IsMatch(roomID, @"^[a-zA-Z0-9]+$"))
            {
                throw new ArgumentException("RoomIdentifier can only use English or numbers."); // 房间ID只能使用英文或数字
            }

            this.owner = owner;
            owner.room = this;
            this.roomID = roomID;
            userList = new();
            userList.Add(owner);
            this.maxUser = maxUser;
        }

        /// <summary>
        /// User join room | 用户加入房间
        /// </summary>
        /// <param name="user">用户</param>
        public void Join(User user)
        {
            userList.Add(user);
            user.room = this;
            user.status = User.Status.InRoom;
            Broadcast(
                new ConnectionMessage.Server.UserJoinRoom(
                    user.name,
                    user.userConfig!.isSpectator,
                    user.avatarUrl
                ).Serialize(), user);
        }

        /// <summary>
        /// User leave room | 用户离开房间
        /// </summary>
        /// <param name="user">用户</param>
        public void Leave(User user)
        {
            userList.Remove(user);
            if (userList.Count == 0)
            {
                RoomManager.RemoveRoom(this);
                LogManager.WriteLog($"{roomID} user all left, room removed.");
            }
            else
            {
                Broadcast(
                    new ConnectionMessage.Server.UserLeaveRoom(
                        user.name,
                        roomID
                    ).Serialize(), user);
            }
        }

        /// <summary>
        /// Broadcast message to room | 广播消息到房间
        /// </summary>
        /// <param name="message">被广播信息</param>
        private void Broadcast(string message, User? exceptUser = null)
        {
            foreach (var user in userList)
            {
                if (user == exceptUser)
                {
                    continue;
                }

                user.userSocket.Send(message);
            }
        }

        /// <summary>
        /// room user count | 房间用户数量
        /// </summary>
        public int UserCount => userList.Count;

        /// <summary>
        /// user index | 用户索引
        /// </summary>
        /// <param name="index">索引</param>
        public User this[int index]
        {
            get => userList[index];
            set => userList[index] = value;
        }

        public void GameStart()
        {
            if (_chartInfo != null)
            {
                Broadcast(new ConnectionMessage.Server.GameStart(_chartInfo.MD5, _chartInfo.url,
                    DateTime.Now.AddSeconds(5)).Serialize());
            }
        }
    }

    /// <summary>
    /// User(Player) | 用户（玩家）
    /// </summary>
    public class User
    {
        public enum Status
        {
            AFK,
            InRoom
        }

        /// <summary>
        /// User Config | 用户配置
        /// </summary>
        public class UserConfig
        {
            public bool isSpectator;
            public bool isDebugger;
            public bool isAnonymous;
            public ConnectionMessage.Client.FeatureSupport featureSupport = new();
        }

        public Status status = Status.AFK;
        public UserConfig? userConfig;
        public string name;
        public IWebSocketConnection userSocket;
        public Room? room;
        public DateTime joinTime = DateTime.Now;
        public string avatarUrl = Program.config.userDefauletAvatarUrl;


        public User(string name, IWebSocketConnection socket, UserConfig config)
        {
            this.name = string.IsNullOrEmpty(name) ? "anonymous" : this.name = name;
            userSocket = socket;
            userConfig = config;
        }

        /// <summary>
        /// User disconnect | 用户断开连接
        /// </summary>
        public void Disconnect()
        {
            UserManager.RemoveUser(userSocket);
            if (room != null)
            {
                if (room.owner == this)
                {
                    if (room.UserCount == 0)
                    {
                        RoomManager.RemoveRoom(room); // Remove room | 移除房间
                    }
                    else
                    {
                        room.owner = room[0];
                    }
                }
            }
        }
    }
}