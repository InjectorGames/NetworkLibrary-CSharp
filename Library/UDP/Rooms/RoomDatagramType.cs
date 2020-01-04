namespace InjectorGames.NetworkLibrary.UDP.Rooms
{
    /// <summary>
    /// Room datagram type
    /// </summary>
    public enum RoomDatagramType : byte
    {
        Ping,
        Connect,
        Disconnect,
        Count,
    }
}
