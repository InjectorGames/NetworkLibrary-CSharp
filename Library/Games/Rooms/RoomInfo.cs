using System;
using System.Text;

namespace InjectorGames.NetworkLibrary.Games.Rooms
{
    /// <summary>
    /// Room information container structure
    /// </summary>
    public struct RoomInfo
    {
        /// <summary>
        /// Unique room identifier
        /// </summary>
        public long id;
        /// <summary>
        /// Room name
        /// </summary>
        public string name;

        /// <summary>
        /// Creates a new room container instance
        /// </summary>
        public RoomInfo(long id, string name)
        {
            this.id = id;
            this.name = name;
        }
        /// <summary>
        /// Creates a new room container instance
        /// </summary>
        public RoomInfo(string serialized)
        {
            var values = serialized.Split(':');

            if (values.Length != 2)
                throw new ArgumentException();

            id = long.Parse(values[0]);
            name = Encoding.Unicode.GetString(Convert.FromBase64String(values[1]));
        }

        /// <summary>
        /// Returns room information string
        /// </summary>
        public override string ToString()
        {
            return $"{id}:{Convert.ToBase64String(Encoding.Unicode.GetBytes(name))}";
        }
    }
}
