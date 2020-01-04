using System.Net;

namespace InjectorGames.NetworkLibrary.UDP
{
    /// <summary>
    /// Datagram structure
    /// </summary>
    public struct Datagram
    {
        /// <summary>
        /// Datagram header byte size in the data array
        /// </summary>
        public const int HeaderByteSize = 1;
        /// <summary>
        /// Index of the type value in the datagram data array
        /// </summary>
        public const int HeaderTypeIndex = 0;

        /// <summary>
        /// Datagram data byte array length
        /// </summary>
        public int Length => Data.Length;

        /// <summary>
        /// Datagram first data array byte value
        /// </summary>
        public byte Type
        {
            get { return Data[HeaderTypeIndex]; }
            set { Data[HeaderTypeIndex] = value; }
        }

        /// <summary>
        /// Datagram remote/local ip end point
        /// </summary>
        public IPEndPoint IpEndPoint { get; set; }

        /// <summary>
        /// Datagram data byte array
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Creates a new datagram structure instance
        /// </summary>
        public Datagram(IPEndPoint ipEndPoint, byte[] data)
        {
            IpEndPoint = ipEndPoint;
            Data = data;
        }
    }
}
