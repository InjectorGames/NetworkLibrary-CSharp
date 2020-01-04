namespace InjectorGames.NetworkLibrary.HTTP
{
    /// <summary>
    /// Base HTTP request abstract class
    /// </summary>
    public abstract class HttpRequestBase
    {
        /// <summary>
        /// Returns HTTP request URL
        /// </summary>
        public abstract string ToURL(string address);
    }
}
