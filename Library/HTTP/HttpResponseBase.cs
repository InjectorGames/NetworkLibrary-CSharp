namespace InjectorGames.NetworkLibrary.HTTP
{
    /// <summary>
    /// Base HTTP response abstract class
    /// </summary>
    public abstract class HttpResponseBase
    {
        /// <summary>
        /// Returns HTTP response body
        /// </summary>
        public abstract string ToBody();
    }
}
