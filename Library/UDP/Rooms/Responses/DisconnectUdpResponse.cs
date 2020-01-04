using System;
using System.IO;
using System.Net;

namespace InjectorGames.NetworkLibrary.UDP.Rooms.Responses
{
    /// <summary>
    /// Disconnect UDP response class
    /// </summary>
    public class DisconnectUdpResponse : DatagramBase
    {
        /// <summary>
        /// Request datagram data byte array length
        /// </summary>
        public const int ByteSize = Datagram.HeaderByteSize + sizeof(int);

        /// <summary>
        /// Response datagram data byte array length
        /// </summary>
        public override int Length => ByteSize;

        /// <summary>
        /// Response datagram first data array byte value
        /// </summary>
        public override byte Type
        {
            get { return (byte)RoomDatagramType.Disconnect; }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Disconnect response reason type
        /// </summary>
        public int reason;

        /// <summary>
        /// Response datagram data byte array
        /// </summary>
        public override byte[] Data
        {
            get
            {
                var data = new byte[ByteSize];
                using (var binaryWriter = new BinaryWriter(new MemoryStream(data)))
                {
                    binaryWriter.Write(Type);
                    binaryWriter.Write(reason);
                    return data;
                }
            }
            set
            {
                if (value.Length != ByteSize)
                    throw new ArgumentException();

                using (var memoryStream = new MemoryStream(value))
                {
                    memoryStream.Seek(Datagram.HeaderByteSize, SeekOrigin.Begin);
                    using (var binaryReader = new BinaryReader(memoryStream))
                        reason = binaryReader.ReadInt32();
                }
            }
        }

        /// <summary>
        /// Creates a new disconnect UDP response class instance
        /// </summary>
        public DisconnectUdpResponse() { }
        /// <summary>
        /// Creates a new disconnect UDP response class instance
        /// </summary>
        public DisconnectUdpResponse(Datagram datagram)
        {
            Data = datagram.Data;
            IpEndPoint = datagram.IpEndPoint;
        }
        /// <summary>
        /// Creates a new disconnect UDP response class instance
        /// </summary>
        public DisconnectUdpResponse(int reason, IPEndPoint ipEndPoint)
        {
            this.reason = reason;
            IpEndPoint = ipEndPoint;
        }

        /// <summary>
        /// Creates a new disconnect UDP response class instance
        /// </summary>
        public DisconnectUdpResponse(ReasonType reason, IPEndPoint ipEndPoint)
        {
            this.reason = (int)reason;
            IpEndPoint = ipEndPoint;
        }

        /// <summary>
        /// Reason type
        /// </summary>
        public enum ReasonType : int
        {
            UnknownDatagram,
            BadDatagram,
            Requested,
            RoomHasClosed,
            RequestTimeOut,
            Count,
        }
    }
}
