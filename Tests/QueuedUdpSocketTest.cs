using InjectorGames.NetworkLibrary.UDP;
using InjectorGames.SharedLibrary.Logs;
using InjectorGames.SharedLibrary.Times;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace Tests
{
    [TestClass]
    public class QueuedUdpSocketTest
    {
        [TestMethod]
        public void SendMessage()
        {
            var clock = new Clock();
            var logger = new ConsoleLogger(clock);
            var firstClient = new QueuedUdpSocket(logger);
            var secondClient = new QueuedUdpSocket(logger);

            firstClient.Start();
            secondClient.Start();

            var data = new byte[] { 1, 234, };
            var remoteEndPoint = new IPEndPoint(IPAddress.Loopback, secondClient.LocalEndPoint.Port);
            firstClient.Send(data, remoteEndPoint);

            Datagram datagram;

            while (!secondClient.TryDequeueNext(out datagram)) { }
            Assert.AreEqual(data.Length, datagram.Length);
            Assert.AreEqual(data[0], datagram.Data[0]);
            Assert.AreEqual(data[1], datagram.Data[1]);

            firstClient.Close();
            secondClient.Close();
        }

        [TestMethod]
        public void SendMessage_ToClosed()
        {
            var clock = new Clock();
            var logger = new ConsoleLogger(clock);
            var firstClient = new QueuedUdpSocket(logger);
            var secondClient = new QueuedUdpSocket(logger);

            firstClient.Start();
            secondClient.Start();

            var data = new byte[] { 1, };
            var remoteEndPoint = new IPEndPoint(IPAddress.Loopback, secondClient.LocalEndPoint.Port);
            secondClient.Close();

            firstClient.Send(data, remoteEndPoint);
            firstClient.Close();
        }

        [TestMethod]
        public void SendMessage_Callback()
        {
            var clock = new Clock();
            var logger = new ConsoleLogger(clock);
            var firstClient = new QueuedUdpSocket(logger);
            var secondClient = new QueuedUdpSocket(logger);

            firstClient.Start();
            secondClient.Start();

            var data = new byte[] { 1, };
            var remoteEndPoint = new IPEndPoint(IPAddress.Loopback, secondClient.LocalEndPoint.Port);
            firstClient.Send(data, remoteEndPoint);

            Datagram datagram;

            while (!secondClient.TryDequeueNext(out datagram)) { }
            Assert.AreEqual(1, datagram.Data[0]);
            data = new byte[] { 2, };
            secondClient.Send(data, datagram.IpEndPoint);

            while (!firstClient.TryDequeueNext(out datagram)) { }
            Assert.AreEqual(2, datagram.Data[0]);

            firstClient.Close();
            secondClient.Close();
        }
    }
}
