using System;
using System.IO;
using System.Net;

namespace InjectorGames.NetworkLibrary.UDP.Rooms.Requests
{
    /// <summary>
    /// Disconnect UDP request class
    /// </summary>
    public class DisconnectUdpRequest : DatagramBase
    {
        /// <summary>
        /// Request byte size of the datagram data array
        /// </summary>
        public const int ByteSize = Datagram.HeaderByteSize + sizeof(int);

        /// <summary>
        /// Request datagram data byte array length
        /// </summary>
        public override int Length => ByteSize;

        /// <summary>
        /// Request datagram first data array byte value
        /// </summary>
        public override byte Type
        {
            get { return (byte)RoomDatagramType.Connect; }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Disconnect reason
        /// </summary>
        public int reason;

        /// <summary>
        /// Request datagram data byte array
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
                    using (var binaryReader = new BinaryReader(new MemoryStream()))
                        reason = binaryReader.ReadInt32();
                }
            }
        }

        /// <summary>
        /// Creates a new disconnect UDP request class instance
        /// </summary>
        public DisconnectUdpRequest() { }
        /// <summary>
        /// Creates a new disconnect UDP request class instance
        /// </summary>
        public DisconnectUdpRequest(Datagram datagram)
        {
            Data = datagram.Data;
            IpEndPoint = datagram.IpEndPoint;
        }
        /// <summary>
        /// Creates a new disconnect UDP request class instance
        /// </summary>
        public DisconnectUdpRequest(int reason, IPEndPoint ipEndPoint)
        {
            this.reason = reason;
            IpEndPoint = ipEndPoint;
        }

        // TODO: add reason enumerator
    }
}
