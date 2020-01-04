using System.Net;

namespace InjectorGames.NetworkLibrary.UDP
{
    /// <summary>
    /// Datagram interface
    /// </summary>
    public interface IDatagram
    {
        /// <summary>
        /// Datagram data byte array length
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Datagram first data array byte value
        /// </summary>
        byte Type { get; set; }

        /// <summary>
        /// Datagram remote/local ip end point
        /// </summary>
        IPEndPoint IpEndPoint { get; set; }

        /// <summary>
        /// Datagram data byte array
        /// </summary>
        byte[] Data { get; set; }
    }
}
