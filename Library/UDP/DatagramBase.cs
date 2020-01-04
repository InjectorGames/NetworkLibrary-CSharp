using System.Net;

namespace InjectorGames.NetworkLibrary.UDP
{
    /// <summary>
    /// Datagram base abstract class
    /// </summary>
    public abstract class DatagramBase : IDatagram
    {
        /// <summary>
        /// Datagram data byte array length
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Datagram first data array byte value
        /// </summary>
        public abstract byte Type { get; set; }

        /// <summary>
        /// Datagram remote/local ip end point
        /// </summary>
        public virtual IPEndPoint IpEndPoint { get; set; }

        /// <summary>
        /// Datagram data byte array
        /// </summary>
        public abstract byte[] Data { get; set; }
    }
}
