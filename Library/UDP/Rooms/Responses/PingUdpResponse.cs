using System;
using System.Net;

namespace InjectorGames.NetworkLibrary.UDP.Rooms.Responses
{
    /// <summary>
    /// Ping UDP response class
    /// </summary>
    public class PingUdpResponse : DatagramBase
    {
        /// <summary>
        /// Response byte size of the datagram data array
        /// </summary>
        public const int ByteSize = Datagram.HeaderByteSize;

        /// <summary>
        /// Response datagram data byte array length
        /// </summary>
        public override int Length => ByteSize;

        /// <summary>
        /// Response datagram first data array byte value
        /// </summary>
        public override byte Type
        {
            get { return (byte)RoomDatagramType.Ping; }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Response datagram data byte array
        /// </summary>
        public override byte[] Data
        {
            get { return new byte[ByteSize] { Type, }; }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Creates a new ping UDP response class instance
        /// </summary>
        public PingUdpResponse() { }
        /// <summary>
        /// Creates a new ping UDP response class instance
        /// </summary>
        public PingUdpResponse(Datagram datagram)
        {
            IpEndPoint = datagram.IpEndPoint;
        }
        /// <summary>
        /// Creates a new ping UDP response class instance
        /// </summary>
        public PingUdpResponse(IPEndPoint ipEndPoint)
        {
            IpEndPoint = ipEndPoint;
        }
    }
}
