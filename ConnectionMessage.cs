﻿using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json;
namespace Phi_MGUS
{
    public static class ConnectionMessage
    {
        /// <summary>
        /// client and server message | 客户端与服务器消息
        /// </summary>
        public class Message
        {
            public string action = "";
            //public string token = "";
            /// <summary>
            /// Deserialize message | 反序列化消息
            /// </summary>
            /// <returns>Json字符串</returns>
            public string Serialize()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        /// <summary>
        /// Server to client message | 服务器到客户端消息
        /// </summary>
        public static class Server
        {
            /// <summary>
            /// Get client metadata message | 获取客户端元数据消息
            /// </summary>
            public class GetData : Message
            {
                public new readonly string action = "getData";
                public bool needPassword = false;
            }
            
            /// <summary>
            /// Join server failed message | 加入服务器失败消息
            /// </summary>
            public class JoinServerFailed : Message
            {
                public new readonly string action = "joinServerFailed";
                public ReasonType reason = ReasonType.Unknown;

                public enum ReasonType
                {
                    AuthFailedByPwdIncorrect,// Will be kicked out | 会被踢出服务器
                    AuthFailedByPwdNull, // Will be kicked out | 会被踢出服务器
                    JoinFailedByIllegalClient,
                    JoinFailedByInvalidParameter,
                    Unknown
                }
            }

            /// <summary>
            /// Join server success message | 加入服务器成功消息
            /// </summary>
            public class JoinServerSuccess : Message
            {
                public new readonly string action = "joinServerSuccess";
            }

            /// <summary>
            /// Room new failed message | 房间新建失败消息
            /// </summary>
            public class NewRoomFailed : Message
            {
                public new readonly string action = "newRoomFailed";
                public ReasonType reason = ReasonType.Unknown;
                public enum ReasonType
                {
                    RoomAlreadyExists,
                    RoomIdentifierInvalid,
                    AlreadyInRoom,
                    Unknown
                }
            }

            /// <summary>
            /// Room new success message | 房间新建成功消息
            /// </summary>
            public class NewRoomSuccess : Message
            {
                public new readonly string action = "newRoomSuccess";
            }
            /// <summary>
            /// Room join failed message | 房间加入成功消息
            /// </summary>
            public class JoinRoomFailed : Message
            {
                public new readonly string action = "joinRoomFailed";
                public ReasonType reason = ReasonType.Unknown;
                public enum ReasonType
                {
                    RoomNotFound,
                    RoomIsFull,
                    AlreadyInRoom,
                    Unknown
                }
            }
            /// <summary>
            /// Room join success message | 房间加入成功消息
            /// </summary>
            public class JoinRoomSuccess : Message
            {
                public new readonly string action = "joinRoomSuccess";
                public Data data = new Data();
                public class Data
                {
                    public string roomID = "";
                    public string roomOwner = "";
                    public string chartMD5 = "";// If no chart is selected, it is empty | 如果没有选择谱面，则为空
                    public string chartUrl = "";// If no chart is selected, it is empty | 如果没有选择谱面，则为空
                    public List<string> userList = new List<string>();
                }
            }
            /// <summary>
            /// Leave room failed message | 离开房间失败消息
            /// </summary>
            public class LeaveRoomFailed : Message
            {
                public new readonly string action = "leaveRoomFailed";
                public ReasonType reason = ReasonType.Unknown;
                public enum ReasonType
                {
                    NotInRoom,
                    Unknown
                }
            }
            /// <summary>
            /// Leave room success message | 离开房间成功消息
            /// </summary>
            public class LeaveRoomSuccess : Message
            {
                public new readonly string action = "leaveRoomSuccess";
            }
            /// <summary>
            /// User join room message | 新用户加入房间消息
            /// </summary>
            public class UserJoinRoom : Message
            {
                public readonly string action = "newUserJoinRoom";
                public Data data = new Data();
                public class Data
                {
                    public string userName = null;
                    public bool isSpectator = false;
                    public string avatarUrl = "";
                }
                public UserJoinRoom(string userName, bool isSpectator, string avatarUrl)
                {
                    data.userName = userName;
                    data.isSpectator = isSpectator;
                    data.avatarUrl = avatarUrl;
                }
            }
            
            /// <summary>
            /// User leave room message | 用户离开房间消息
            /// </summary>
            public class UserLeaveRoom : Message
            {
                public new readonly string action = "userLeaveRoom";
                public Data data = new Data();
                public class Data
                {
                    public string userName = "";
                    public string roomID = "";
                }
                public UserLeaveRoom(string userName, string roomID)
                {
                    data.userName = userName;
                    data.roomID = roomID;
                }
            }
            
            /// <summary>
            /// User selected chart message | 用户选择谱面消息
            /// </summary>
            public class UserSelectedChart : Message
            {
                public new readonly string action = "userSelectedChart";
                public Data data = new Data();
                public class Data
                {
                    public string chartMD5 = "";
                    public string chartUrl = "";
                }
                public UserSelectedChart(string chartMD5, string chartUrl)
                {
                    data.chartMD5 = chartMD5;
                    data.chartUrl = chartUrl;
                }
            }
            /// <summary>
            /// Select chart failed message | 选择谱面失败消息
            /// </summary>
            public class SelectChartFailed : Message
            {
                public new readonly string action = "selectChartFailed";
                public ReasonType reason = ReasonType.Unknown;
                public enum ReasonType
                {
                    InsufficientPermissions,
                    NotInRoom,
                    Unknown
                }
            }
            
            /// <summary>
            /// Game start message | 游戏开始消息
            /// </summary>
            public class GameStart : Message
            {
                public new readonly string action = "gameStart";
                public Data data = new Data();
                public class Data
                {
                    public string chartMD5 = "";
                    public string chartUrl = "";
                    private DateTime _startDate;
                    public double startDate
                    {
                        get => (_startDate - new DateTime(1970, 1, 1)).TotalMilliseconds;
                        set => _startDate = new DateTime(1970, 1, 1).AddMilliseconds(value);
                    }
                }
                public GameStart(string chartMD5, string chartUrl, DateTime startDate)
                {
                    data.chartMD5 = chartMD5;
                    data.chartUrl = chartUrl;
                    data.startDate = (startDate - new DateTime(1970, 1, 1)).TotalMilliseconds;
                }
            }

        }

        /// <summary>
        /// Client to server message | 客户端到服务器消息
        /// </summary>
        public static class Client
        {
            /// <summary>
            /// Client meta data | 客户端元数据
            /// </summary>
            public class ClientMetaData : Message
            {
                public new readonly string action = "clientMetaData";

                public Data data = new Data();

                public class Data
                {
                    public FeatureSupport features = new FeatureSupport();
                    public string clientName = "anonymous";
                    public int clientVersion = -1;
                    public string userName = null; // If it is an anonymous user, this value is null
                    public string password = null; // if server is private，this value is not null
                    public bool isDebugger = false;
                    public bool isSpectator = false;
                }
            }

            /// <summary>
            /// Feature support | 功能支持
            /// </summary>
            public class FeatureSupport
            {
                public bool RealTimeUpload = false;
                public bool VotingSelection = false;
                public bool RealTimeLeaderboard = false;
                public bool RealTimeChat = false;
            }
            /// <summary>
            /// New room | 新建房间
            /// </summary>
            public class NewRoom : Message
            {
                public readonly string action = "newRoom";
                public Data data = new Data
                {
                    //RoomID is a random string, length is 16 | 房间ID是随机字符串，长度为16
                    roomID = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16)
                };
                
                public class Data
                {
                    public int maxUser = 8;
                    private string _roomID;
                    public string roomID
                    {
                        set
                        {
                            if (value.Length > 32)
                            {
                                throw new ArgumentException("RoomIdentifier cannot exceed 32 digits.");// 不能超过32位
                            }
                            if (!Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"))
                            {
                                throw new ArgumentException("RoomIdentifier can only use English or numbers.");// 只能使用英文或数字
                            }
                            _roomID = value;
                        }
                        get
                        {
                            return _roomID;
                        }
                    }// Only English or numbers can be used, and cannot exceed 32 digits | 只能使用英文或数字，且不超过32位
                    
                }
                public NewRoom(int? maxUser = 8, string roomID = null)
                {
                    if (roomID == null)
                    {
                        roomID = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
                    }
                    else
                    {
                        data.roomID = roomID;
                    }
                    if (maxUser == null)
                    {
                        maxUser = 8;
                    }
                    else
                    {
                        data.maxUser = maxUser.Value;
                    }
                }
            }
            
            /// <summary>
            /// Join room | 加入房间
            /// </summary>
            public class JoinRoom : Message
            {
                public readonly string action = "joinRoom";
                public Data data = new Data();
                
                public class Data
                {
                    public string roomID = "";
                }

                public JoinRoom(string roomID)
                {
                    data.roomID = roomID;
                }
            }
            /// <summary>
            /// Leave room | 离开房间
            /// </summary>
            public class LeaveRoom : Message
            {
                public readonly string action = "leaveRoom";
            }
            
            public class SelectChart : Message
            {
                public readonly string action = "selectChart";
                public Data data = new Data();
                public class Data
                {
                    public string chartMD5 = "";
                    public string chartUrl = "";
                }
            }
            
            public class GameStart : Message
            {
                public readonly string action = "gameStart";
            }
        }
    }
}