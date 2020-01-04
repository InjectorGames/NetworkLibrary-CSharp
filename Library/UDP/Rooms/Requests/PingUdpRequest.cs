using System;
using System.Net;

namespace InjectorGames.NetworkLibrary.UDP.Rooms.Requests
{
    /// <summary>
    /// Ping UDP request class
    /// </summary>
    public class PingUdpRequest : DatagramBase
    {
        /// <summary>
        /// Request datagram data byte array length
        /// </summary>
        public const int ByteSize = Datagram.HeaderByteSize;
        /// <summary>
        /// Minimum delay between two ping requests
        /// </summary>
        public const long MinRequestDelay = 1000;

        /// <summary>
        /// Request datagram data byte array length
        /// </summary>
        public override int Length => ByteSize;

        /// <summary>
        /// Request datagram first data array byte value
        /// </summary>
        public override byte Type
        {
            get { return (byte)RoomDatagramType.Ping; }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Request datagram data byte array
        /// </summary>
        public override byte[] Data
        {
            get { return new byte[ByteSize] { Type, }; }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Creates a new ping UDP request class instance
        /// </summary>
        public PingUdpRequest() { }
        /// <summary>
        /// Creates a new ping UDP request class instance
        /// </summary>
        public PingUdpRequest(Datagram datagram)
        {
            IpEndPoint = datagram.IpEndPoint;
        }
        /// <summary>
        /// Creates a new ping UDP request class instance
        /// </summary>
        public PingUdpRequest(IPEndPoint ipEndPoint)
        {
            IpEndPoint = ipEndPoint;
        }
    }
}
