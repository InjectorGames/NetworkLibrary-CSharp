namespace InjectorGames.NetworkLibrary.UDP
{
    /// <summary>
    /// Queued UDP socket interface (thread-safe)
    /// </summary>
    public interface IQueuedUdpSocket : IUdpSocket
    {
        /// <summary>
        /// Returns true if datagram has dequeued from the queue
        /// </summary>
        bool TryDequeueNext(out Datagram datagram);
        /// <summary>
        /// Returns true if datagrams has dequeued from the queue
        /// </summary>
        bool TryDequeueAll(out Datagram[] datagramArray);
    }
}
