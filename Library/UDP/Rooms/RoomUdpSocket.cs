using InjectorGames.NetworkLibrary.Games.Players;
using InjectorGames.NetworkLibrary.Games.Rooms;
using InjectorGames.NetworkLibrary.UDP.Rooms.Requests;
using InjectorGames.NetworkLibrary.UDP.Rooms.Responses;
using InjectorGames.SharedLibrary.Credentials;
using InjectorGames.SharedLibrary.Credentials.Accounts;
using InjectorGames.SharedLibrary.Logs;
using InjectorGames.SharedLibrary.Times;
using System;
using System.Net;
using System.Threading;

namespace InjectorGames.NetworkLibrary.UDP.Rooms
{
    /// <summary>
    /// Room UDP socket class
    /// </summary>
    public class RoomUdpSocket<TPlayer, TPlayerFactory> : TaskedUdpSocket, IRoomUdpSocket<TPlayer, TPlayerFactory>
        where TPlayer : IPlayer
        where TPlayerFactory : IPlayerFactory<TPlayer>
    {
        /// <summary>
        /// Request time out delay in milliseconds
        /// </summary>
        public const int RequestTimeOut = 5000;

        /// <summary>
        /// Room identifier
        /// </summary>
        protected readonly long id;
        /// <summary>
        /// Room name
        /// </summary>
        protected string name;
        /// <summary>
        /// Room maximum player count
        /// </summary>
        protected int maxPlayerCount;

        /// <summary>
        /// Room clock
        /// </summary>
        protected readonly IClock clock;
        /// <summary>
        /// Player factory
        /// </summary>
        protected readonly TPlayerFactory playerFactory;
        /// <summary>
        /// Player concurrent database
        /// </summary>
        protected readonly IPlayerDatabase<TPlayer, TPlayerFactory> playerDatabase;

        /// <summary>
        /// Player concurrent dictionary
        /// </summary>
        protected readonly PlayerDictionary<TPlayer> players;
        /// <summary>
        /// Room update thread
        /// </summary>
        protected readonly Thread updateThread;

        /// <summary>
        /// Room identifier
        /// </summary>
        public long ID
        {
            get { return id; }
            set { throw new InvalidOperationException(); }
        }
        /// <summary>
        /// Room name
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = !string.IsNullOrEmpty(value) ? value : throw new ArgumentException(); }
        }
        /// <summary>
        /// Maximum room player count
        /// </summary>
        public int MaxPlayerCount
        {
            get { return maxPlayerCount; }
            set { maxPlayerCount = value > 0 ? value : throw new ArgumentException(); }
        }

        /// <summary>
        /// Current room player count
        /// </summary>
        public int PlayerCount => players.Count;

        /// <summary>
        /// Room clock
        /// </summary>
        public IClock Clock => clock;
        /// <summary>
        /// Player factory
        /// </summary>
        public TPlayerFactory PlayerFactory => playerFactory;
        /// <summary>
        /// Player database
        /// </summary>
        public IPlayerDatabase<TPlayer, TPlayerFactory> PlayerDatabase => playerDatabase;
        /// <summary>
        /// Player concurrent dictionary
        /// </summary>
        public PlayerDictionary<TPlayer> Players => players;

        /// <summary>
        /// Creates a new room UDP socket class instance
        /// </summary>
        public RoomUdpSocket(long id, string name, IClock clock, TPlayerFactory playerFactory, IPlayerDatabase<TPlayer, TPlayerFactory> playerDatabase, ILogger logger, int maxPlayerCount = 256, int maxTaskCount = 256) : base(logger, maxTaskCount)
        {
            this.id = id;

            Name = name;
            MaxPlayerCount = maxPlayerCount;

            this.clock = clock ?? throw new ArgumentNullException();
            this.playerFactory = playerFactory ?? throw new ArgumentNullException();
            this.playerDatabase = playerDatabase ?? throw new ArgumentNullException();

            players = new PlayerDictionary<TPlayer>();
            updateThread = new Thread(UpdateThreadLogic);
        }

        /// <summary>
        /// Returns true if room socket is equal to the object
        /// </summary>
        public override bool Equals(object obj)
        {
            return id.Equals(((IRoom)obj).ID);
        }
        /// <summary>
        /// Returns room socket hash code 
        /// </summary>
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
        /// <summary>
        /// Returns room socket string
        /// </summary>
        public override string ToString()
        {
            return id.ToString();
        }

        /// <summary>
        /// Compares room socket to the object
        /// </summary>
        public int CompareTo(object obj)
        {
            return id.CompareTo(((IRoom)obj).ID);
        }
        /// <summary>
        /// Compares two room sockets
        /// </summary>
        public int CompareTo(IRoom other)
        {
            return id.CompareTo(other.ID);
        }
        /// <summary>
        /// Returns true if room sockets is equal
        /// </summary>
        public bool Equals(IRoom other)
        {
            return id.Equals(other.ID);
        }

        /// <summary>
        /// Starts UDP socket room
        /// </summary>
        public new void Start(IPEndPoint localEndPoint)
        {
            base.Start(localEndPoint);
            updateThread.Start();
        }
        /// <summary>
        /// Starts UDP socket room
        /// </summary>
        public new void Start()
        {
            base.Start();
            updateThread.Start();
        }

        /// <summary>
        /// Returns true if player has added to the room
        /// </summary>
        public bool AddPlayer(IAccount account, out Token connectToken)
        {
            connectToken = Token.Create();

            if (!playerDatabase.TryGetValue(account.ID, playerFactory, out TPlayer player))
                player = playerFactory.Create(account.ID, 0, account.Name, connectToken, null);

            player.LastPingMS = clock.MS;
            return players.TryAdd(player.ID, player);
        }
        /// <summary>
        /// Returns true if player has disconnected from the room
        /// </summary>
        public bool DisconnectPlayer(long id, int reason)
        {
            if (!players.TryRemove(id, out TPlayer player))
            {
                if (logger.Log(LogType.Debug))
                    logger.Debug($"Failed to remove player from the array on disconnect. (id: {player.ID}, remoteEndPoint: {player.RemoteEndPoint}, reason: {reason}, roomID: {id})");

                return false;
            }

            if (player.RemoteEndPoint != null)
            {
                Send(new DisconnectUdpResponse(reason, player.RemoteEndPoint));

                if (!playerDatabase.TryUpdate(player))
                {
                    if (logger.Log(LogType.Error))
                        logger.Error($"Failed to update player in the database on disconnect. (id: {player.ID}, remoteEndPoint: {player.RemoteEndPoint}, reason: {reason}, rommID: {id})");
                }

                if (logger.Log(LogType.Info))
                    logger.Info($"Disconnected server player. (id: {player.ID}, remoteEndPoint: {player.RemoteEndPoint}, reason: {reason}, rommID: {id})");
            }
            else
            {
                if (logger.Log(LogType.Info))
                    logger.Info($"Disconnected server player. (id: {player.ID}, reason: {reason}, rommID: {id})");
            }

            return true;
        }
        /// <summary>
        /// Returns true if player has disconnected from the room
        /// </summary>
        public bool DisconnectPlayer(long id, DisconnectUdpResponse.ReasonType reason)
        {
            return DisconnectPlayer(id, (int)reason);
        }

        /// <summary>
        /// Room update thread logic method
        /// </summary>
        protected virtual void UpdateThreadLogic()
        {
            if (logger.Log(LogType.Debug))
                logger.Debug("Started room UDP socket update thread.");

            while (isRunning)
            {
                try
                {
                    var ms = clock.MS;

                    foreach (var player in players.Values)
                    {
                        if (ms - player.LastPingMS > RequestTimeOut)
                            DisconnectPlayer(player.ID, DisconnectUdpResponse.ReasonType.RequestTimeOut);
                    }
                }
                catch (Exception exception)
                {
                    OnUpdateThreadException(exception);
                }
            }

            if (logger.Log(LogType.Debug))
                logger.Debug("Stopped room UDP socket update thread.");
        }

        /// <summary>
        /// On UDP socket tasked datagram receive
        /// </summary>
        protected override void OnTaskedDatagramReceive(Datagram datagram)
        {
            if (!players.TryGetValue(datagram.IpEndPoint, out TPlayer player))
            {
                if (datagram.Type != (byte)RoomDatagramType.Connect)
                    return;

                try
                {
                    OnConnectRequest(new ConnectUdpRequest(datagram));
                }
                catch
                {
                    if (logger.Log(LogType.Debug))
                        logger.Debug($"Bad room connect UDP request. (remoteEndPoint: {datagram.IpEndPoint}, roomID: {id})");
                }
            }
            else
            {
                try
                {
                    switch (datagram.Type)
                    {
                        default:
                            DisconnectPlayer(player.ID, DisconnectUdpResponse.ReasonType.UnknownDatagram);
                            break;
                        case (byte)RoomDatagramType.Ping:
                            OnPingRequest(player);
                            break;
                        case (byte)RoomDatagramType.Connect:
                            if (logger.Log(LogType.Debug))
                                logger.Debug($"Received second room connect UDP request. (id:{player.ID}, remoteEndPoint: {datagram.IpEndPoint}, roomId: {id})");
                            break;
                        case (byte)RoomDatagramType.Disconnect:
                            DisconnectPlayer(player.ID, DisconnectUdpResponse.ReasonType.Requested);
                            break;
                    }
                }
                catch(Exception exception)
                {
                    DisconnectPlayer(player.ID, DisconnectUdpResponse.ReasonType.BadDatagram);

                    if (logger.Log(LogType.Warning))
                        logger.Warning($"Bad room UDP request. (type: {datagram.Type}, accountID:{player.ID}, remoteEndPoint: {datagram.IpEndPoint}, roomID: {id}) {exception}");
                }
            }
        }
        protected void OnPingRequest(IPlayer player)
        {
            if (clock.MS - player.LastPingMS < PingUdpRequest.MinRequestDelay)
                return;

            player.LastPingMS = clock.MS;
            Send(new PingUdpResponse(player.RemoteEndPoint));

            if (logger.Log(LogType.Debug))
                logger.Debug($"Received room ping UDP request. (accountID:{player.ID}, remoteEndPoint: {player.RemoteEndPoint}, roomID: {id})");
        }
        protected void OnConnectRequest(ConnectUdpRequest request)
        {
            if (!players.TryGetValue(request.accountId, out TPlayer player))
            {
                Send(new ConnectUdpResponse(ConnectUdpResponse.ResultType.IncorrectId, request.IpEndPoint));

                if (logger.Log(LogType.Debug))
                    logger.Debug($"Failed to connect room UDP socket player, incorrect account ID. (accountID: {request.accountId}, remoteEndPoint: {request.IpEndPoint}, roomID: {id})");

                return;
            }

            if (player.ConnecToken != request.connectToken)
            {
                Send(new ConnectUdpResponse(ConnectUdpResponse.ResultType.IncorrectToken, request.IpEndPoint));

                if (logger.Log(LogType.Debug))
                    logger.Debug($"Failed to connect room UDP socket player, incorrect token. (accountID: {request.accountId}, remoteEndPoint: {request.IpEndPoint}, roomID: {id})");

                return;
            }

            player.RemoteEndPoint = request.IpEndPoint;
            Send(new ConnectUdpResponse(ConnectUdpResponse.ResultType.Success, request.IpEndPoint));

            if (logger.Log(LogType.Info))
                logger.Info($"Connected a new room UDP socket player. (accountID: {request.accountId}, remoteEndPoint: {request.IpEndPoint}, roomID: {id})");
        }

        /// <summary>
        /// On room UDP socket update thread exception
        /// </summary>
        protected virtual void OnUpdateThreadException(Exception exception)
        {
            if (logger.Log(LogType.Fatal))
                logger.Fatal($"Room UDP socket update thread exception. {exception}");

            Close();
        }
    }
}
