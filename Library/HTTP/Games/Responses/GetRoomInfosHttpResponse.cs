﻿using InjectorGames.NetworkLibrary.Games.Rooms;
using System;
using System.Text;

namespace InjectorGames.NetworkLibrary.HTTP.Games.Responses
{
    /// <summary>
    /// Get room infos HTTP response class
    /// </summary>
    public class GetRoomInfosHttpResponse : HttpResponseBase
    {
        /// <summary>
        /// Response type string value
        /// </summary>
        public const string Type = "GetRoomInfos";

        /// <summary>
        /// Get room infos request result
        /// </summary>
        public int result;
        /// <summary>
        /// Room information array
        /// </summary>
        public RoomInfo[] roomInfos;

        /// <summary>
        /// Creates a new get room infos HTTP response class instance
        /// </summary>
        public GetRoomInfosHttpResponse(int result, RoomInfo[] roomInfos = null)
        {
            this.result = result;
            this.roomInfos = roomInfos;
        }
        /// <summary>
        /// Creates a new get room infos HTTP response class instance
        /// </summary>
        public GetRoomInfosHttpResponse(ResultType result, RoomInfo[] roomInfos = null)
        {
            this.result = (int)result;
            this.roomInfos = roomInfos;
        }
        /// <summary>
        /// Creates a new get room infos HTTP response class instance
        /// </summary>
        public GetRoomInfosHttpResponse(string data)
        {
            var values = data.Split(' ');

            if (values.Length == 1)
            {
                result = int.Parse(values[0]);
            }
            else if (values.Length == 2)
            {
                result = int.Parse(values[0]);

                var rooms = values[1].Split(',');
                roomInfos = new RoomInfo[rooms.Length];

                for (int i = 0; i < rooms.Length; i++)
                    roomInfos[i] = new RoomInfo(rooms[i]);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Returns HTTP server response body
        /// </summary>
        public override string ToBody()
        {
            if (roomInfos != null && roomInfos.Length > 0)
            {
                var rooms = new StringBuilder();

                for (int i = 0; i < roomInfos.Length; i++)
                {
                    rooms.Append(roomInfos[i].ToString());

                    if (i < roomInfos.Length - 1)
                        rooms.Append(',');
                }

                return $"{Type}\n{result} {rooms}";
            }
            else
            {
                return $"{Type}\n{result}";
            }
        }

        /// <summary>
        /// Result type
        /// </summary>
        public enum ResultType
        {
            Success,
            IncorrectUsername,
            IncorrectAccessToken,
            Count,
        }
    }
}
