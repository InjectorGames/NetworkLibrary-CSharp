using InjectorGames.SharedLibrary.Logs;
using System.Collections.Generic;

namespace InjectorGames.NetworkLibrary.UDP
{
    /// <summary>
    /// Queued UDP socket class (thread-safe)
    /// </summary>
    public class QueuedUdpSocket : UdpSocket, IQueuedUdpSocket
    {
        /// <summary>
        /// Maximum datagram count in the queue
        /// </summary>
        protected int maxDatagramCount;

        /// <summary>
        /// Datagram queue locker
        /// </summary>
        protected readonly object locker;
        /// <summary>
        /// Received datagram queue
        /// </summary>
        protected readonly Queue<Datagram> datagrams;

        /// <summary>
        /// Creates a new queued UDP socket abstract class instance
        /// </summary>
        public QueuedUdpSocket(ILogger logger, int maxDatagramCount = 256) : base(logger)
        {
            this.maxDatagramCount = maxDatagramCount;

            locker = new object();
            datagrams = new Queue<Datagram>();
        }

        /// <summary>
        /// Returns true if datagram has dequeued from the queue
        /// </summary>
        public bool TryDequeueNext(out Datagram datagram)
        {
            lock (locker)
            {
                if (datagrams.Count == 0)
                {
                    datagram = default;
                    return false;
                }

                datagram = datagrams.Dequeue();

                if (logger.Log(LogType.Trace))
                    logger.Trace($"Dequeued UDP socket datagram. (remoteEndPoint: {datagram.IpEndPoint}, length: {datagram.Length}, type: {datagram.Type}))");

                return true;
            }
        }

        /// <summary>
        /// Returns true if datagrams has dequeued from the queue
        /// </summary>
        public bool TryDequeueAll(out Datagram[] datagramArray)
        {
            lock (locker)
            {
                if (datagrams.Count == 0)
                {
                    datagramArray = null;
                    return false;
                }

                datagramArray = datagrams.ToArray();
                datagrams.Clear();

                if (logger.Log(LogType.Trace))
                    logger.Trace($"Dequeued all UDP socket datagrams. (count: {datagramArray.Length}))");

                return true;
            }
        }

        /// <summary>
        /// On UDP socket datagram receive
        /// </summary>
        protected override void OnDatagramReceive(Datagram datagram)
        {
            lock (locker)
            {
                if (datagrams.Count < maxDatagramCount)
                {
                    datagrams.Enqueue(datagram);

                    if (logger.Log(LogType.Trace))
                        logger.Trace($"Enqueued UDP socket datagram. (remoteEndPoint: {datagram.IpEndPoint}, length: {datagram.Length}, type: {datagram.Type}))");
                }
                else
                {
                    if (logger.Log(LogType.Trace))
                        logger.Trace($"Failed to enqueued UDP socket datagram, queue is full. (remoteEndPoint: {datagram.IpEndPoint}, length: {datagram.Length}, type: {datagram.Type}))");
                }
            }
        }
    }
}
