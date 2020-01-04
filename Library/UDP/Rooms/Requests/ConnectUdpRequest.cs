using InjectorGames.SharedLibrary.Credentials;
using System;
using System.IO;
using System.Net;

namespace InjectorGames.NetworkLibrary.UDP.Rooms.Requests
{
    /// <summary>
    /// Connect UDP request class
    /// </summary>
    public class ConnectUdpRequest : DatagramBase
    {
        /// <summary>
        /// Request datagram data byte array length
        /// </summary>
        public const int ByteSize = Datagram.HeaderByteSize + sizeof(long) + Token.ByteSize;

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
        /// Player identifier
        /// </summary>
        public long accountId;
        /// <summary>
        /// Connect token
        /// </summary>
        public Token connectToken;

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
                    binaryWriter.Write(accountId);
                    connectToken.ToBytes(binaryWriter);
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
                    {
                        accountId = binaryReader.ReadInt64();
                        connectToken = new Token(binaryReader);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new connect UDP request class instance
        /// </summary>
        public ConnectUdpRequest() { }
        /// <summary>
        /// Creates a new connect UDP request class instance
        /// </summary>
        public ConnectUdpRequest(Datagram datagram)
        {
            Data = datagram.Data;
            IpEndPoint = datagram.IpEndPoint;
        }
        /// <summary>
        /// Creates a new connect UDP request class instance
        /// </summary>
        public ConnectUdpRequest(long accountId, Token connectToken, IPEndPoint ipEndPoint)
        {
            this.accountId = accountId;
            this.connectToken = connectToken;
            IpEndPoint = ipEndPoint;
        }
    }
}
