using InjectorGames.NetworkLibrary.Games.Players;
using InjectorGames.NetworkLibrary.UDP;
using InjectorGames.NetworkLibrary.UDP.Rooms;
using InjectorGames.NetworkLibrary.UDP.Rooms.Requests;
using InjectorGames.NetworkLibrary.UDP.Rooms.Responses;
using InjectorGames.SharedLibrary.Credentials;
using InjectorGames.SharedLibrary.Credentials.Accounts;
using InjectorGames.SharedLibrary.Logs;
using InjectorGames.SharedLibrary.Times;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Net;

namespace Tests
{
    [TestClass]
    public class RoomUdpSocketTest
    {
        public const string testDatabasePath = "test-database/";

        public void Connect(IAccount addAccount, IAccount connectAccount)
        {
            var clock = new Clock();
            var playerFactory = new PlayerFactory();
            var playerDatabase = new PlayerDiskDatabase<IPlayer, IPlayerFactory<IPlayer>>(testDatabasePath, false);
            var logger = new ConsoleLogger(clock);

            var room = new RoomUdpSocket<IPlayer, IPlayerFactory<IPlayer>>(0, "Test Room #0", clock, playerFactory, playerDatabase, logger);
            var client = new QueuedUdpSocket(logger);

            room.Start();
            client.Start();

            room.AddPlayer(addAccount, out Token connectToken);

            var remoteEndPoint = new IPEndPoint(IPAddress.Loopback, room.LocalEndPoint.Port);
            client.Send(new ConnectUdpRequest(connectAccount.ID, connectToken, remoteEndPoint));

            Datagram datagram;

            while (!client.TryDequeueNext(out datagram)) { }
            Assert.AreEqual((byte)RoomDatagramType.Connect, datagram.Type);

            var response = new ConnectUdpResponse(datagram);
            Assert.AreEqual((byte)ConnectUdpResponse.ResultType.Success, response.result);

            room.Close();
            client.Close();
        }

        [TestMethod]
        public void Connect_Correct()
        {
            var account = new Account(0, false, 0, "test_user1", Passhash.PasswordToPasshash("Str0ngPassw0rd123"), new EmailAddress("info@mail.com"), Token.Create(), IPAddress.Any);
            Connect(account, account);
        }

        [TestMethod]
        public void Connect_Incorrect_ID()
        {
            var addAccount = new Account(0, false, 0, "test_user1", Passhash.PasswordToPasshash("Str0ngPassw0rd123"), new EmailAddress("info@mail.com"), Token.Create(), IPAddress.Any);
            var connectAccount = new Account(1, false, 0, "test_user1", Passhash.PasswordToPasshash("Str0ngPassw0rd123"), new EmailAddress("info@mail.com"), Token.Create(), IPAddress.Any);
            var isThrowed = false;
            try { Connect(addAccount, connectAccount); }
            catch { isThrowed = true; }
            Assert.IsTrue(isThrowed);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(testDatabasePath))
                Directory.Delete(testDatabasePath, true);
        }
    }
}
