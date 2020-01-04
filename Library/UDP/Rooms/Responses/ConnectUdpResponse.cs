using System;
using System.IO;
using System.Net;

namespace InjectorGames.NetworkLibrary.UDP.Rooms.Responses
{
    /// <summary>
    /// Connect UDP response class
    /// </summary>
    public class ConnectUdpResponse : DatagramBase
    {
        /// <summary>
        /// Response byte size of the datagram data array
        /// </summary>
        public const int ByteSize = Datagram.HeaderByteSize + sizeof(byte);

        /// <summary>
        /// Response datagram data byte array length
        /// </summary>
        public override int Length => ByteSize;

        /// <summary>
        /// Response datagram first data array byte value
        /// </summary>
        public override byte Type
        {
            get { return (byte)RoomDatagramType.Connect; }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Connect request result
        /// </summary>
        public byte result;

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
                    binaryWriter.Write(result);
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
                        result = binaryReader.ReadByte();
                }
            }
        }

        /// <summary>
        /// Creates a new connect UDP response class instance
        /// </summary>
        public ConnectUdpResponse() { }
        /// <summary>
        /// Creates a new connect UDP response class instance
        /// </summary>
        public ConnectUdpResponse(Datagram datagram)
        {
            Data = datagram.Data;
            IpEndPoint = datagram.IpEndPoint;
        }
        /// <summary>
        /// Creates a new connect UDP response class instance
        /// </summary>
        public ConnectUdpResponse(byte result, IPEndPoint ipEndPoint)
        {
            this.result = result;
            IpEndPoint = ipEndPoint;
        }
        /// <summary>
        /// Creates a new connect UDP response class instance
        /// </summary>
        public ConnectUdpResponse(ResultType result, IPEndPoint ipEndPoint)
        {
            this.result = (byte)result;
            IpEndPoint = ipEndPoint;
        }

        /// <summary>
        /// Result type
        /// </summary>
        public enum ResultType
        {
            Success,
            IncorrectId,
            IncorrectToken,
            Count,
        }
    }
}
