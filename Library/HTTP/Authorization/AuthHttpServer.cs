using InjectorGames.NetworkLibrary.HTTP.Authorization.Requests;
using InjectorGames.NetworkLibrary.HTTP.Authorization.Responses;
using InjectorGames.SharedLibrary.Credentials;
using InjectorGames.SharedLibrary.Credentials.Accounts;
using InjectorGames.SharedLibrary.Logs;
using System;
using System.Collections.Specialized;
using System.Net;

namespace InjectorGames.NetworkLibrary.HTTP.Authorization
{
    /// <summary>
    /// Authorization HTTP server class
    /// </summary>
    public class AuthHttpServer<TAccount, TAccountFactory> : HttpServer, IAuthHttpServer<TAccount, TAccountFactory>
        where TAccount : IAccount
        where TAccountFactory : IAccountFactory<TAccount>
    {
        /// <summary>
        /// Server version
        /// </summary>
        protected readonly Version version;
        /// <summary>
        /// Account factory
        /// </summary>
        protected readonly TAccountFactory accountFactory;
        /// <summary>
        /// Account database
        /// </summary>
        protected readonly IAccountDatabase<TAccount, TAccountFactory> accountDatabase;

        /// <summary>
        /// Server version
        /// </summary>
        public Version Version => version;
        /// <summary>
        /// Account factory
        /// </summary>
        public TAccountFactory AccountFactory => accountFactory;
        /// <summary>
        /// Account database
        /// </summary>
        public IAccountDatabase<TAccount, TAccountFactory> AccountDatabase => accountDatabase;

        /// <summary>
        /// Creates a new authorization HTTP server class instance
        /// </summary>
        public AuthHttpServer(Version version, TAccountFactory accountFactory, IAccountDatabase<TAccount, TAccountFactory> accountDatabase, ILogger logger, string address) : base(logger, address)
        {
            this.version = version ?? throw new ArgumentNullException();
            this.accountFactory = accountFactory ?? throw new ArgumentNullException();
            this.accountDatabase = accountDatabase ?? throw new ArgumentNullException();
            listener.Prefixes.Add($"{address}{SignUpHttpRequest.Type}/");
            listener.Prefixes.Add($"{address}{SignInHttpRequest.Type}/");
        }

        /// <summary>
        /// On HTTP server listener request receive
        /// </summary>
        protected override void OnListenerRequestReceive(HttpListenerContext context)
        {
            var httpRequest = context.Request;
            var httpResponse = context.Response;
            var urlPair = httpRequest.RawUrl.Split('?');
            var queryString = httpRequest.QueryString;

            try
            {
                switch (urlPair[0])
                {
                    default:
                        SendResponse(httpResponse, new BadRequestHttpResponse("Unknown request type"));
                        break;
                    case SignUpHttpRequest.Type:
                        OnSignUpRequest(new SignUpHttpRequest(queryString), httpRequest, httpResponse);
                        break;
                    case SignInHttpRequest.Type:
                        OnSignInRequest(new SignInHttpRequest(queryString), httpRequest, httpResponse);
                        break;
                }
            }
            catch(Exception exception)
            {
                SendResponse(httpResponse, new BadRequestHttpResponse("Bad request"));

                if (logger.Log(LogType.Warning))
                    logger.Warning($"Bad authorization server HTTP request. (type: {urlPair[0]}, remoteEndPoint: {httpRequest.RemoteEndPoint}) {exception}");
            }
        }
        protected void OnSignUpRequest(SignUpHttpRequest request, HttpListenerRequest httpRequest, HttpListenerResponse httpResponse)
        {
            if (accountDatabase.ContainsKey(request.name))
            {
                SendResponse(httpResponse, new SignUpHttpResponse(SignUpHttpResponse.ResultType.UsernameBusy));

                if (logger.Log(LogType.Trace))
                    logger.Trace($"Sended HTTP server sign up response, username busy. (username: {request.name}, remoteEndPoint: {httpRequest.RemoteEndPoint})");

                return;
            }

            var accountData = accountFactory.Create(accountDatabase.Count, false, 0, request.name, request.passhash, request.emailAddress, Token.Create(), httpRequest.RemoteEndPoint.Address);

            if (!accountDatabase.TryAdd(accountData))
            {
                SendResponse(httpResponse, new SignUpHttpResponse(SignUpHttpResponse.ResultType.FailedToWrite));

                if (logger.Log(LogType.Error))
                    logger.Error($"Failed to add account to the database on sign up request. (username: {request.name}, remoteEndPoint: {httpRequest.RemoteEndPoint})");

                return;
            }

            SendResponse(httpResponse, new SignUpHttpResponse(SignUpHttpResponse.ResultType.Success));

            if (logger.Log(LogType.Info))
                logger.Info($"Signed up a new account. (username: {request.name}, email: {request.emailAddress}, remoteEndPoint: {httpRequest.RemoteEndPoint})");
        }
        protected void OnSignInRequest(SignInHttpRequest request, HttpListenerRequest httpRequest, HttpListenerResponse httpResponse)
        {
            if (!accountDatabase.TryGetValue(request.name, accountFactory, out TAccount account))
            {
                SendResponse(httpResponse, new SignInHttpResponse(SignInHttpResponse.ResultType.IncorrectUsername));

                if (logger.Log(LogType.Trace))
                    logger.Trace($"Sended HTTP server sign in response, incorrect username. (username: {request.name}, remoteEndPoint: {httpRequest.RemoteEndPoint})");

                return;
            }

            if (request.passhash != account.Passhash)
            {
                SendResponse(httpResponse, new SignInHttpResponse(SignInHttpResponse.ResultType.IncorrectPassword));

                if (logger.Log(LogType.Trace))
                    logger.Trace($"Sended HTTP server sign in response, incorrect password. (username: {request.name}, remoteEndPoint: {httpRequest.RemoteEndPoint})");

                return;
            }

            if (account.IsBlocked)
            {
                SendResponse(httpResponse, new SignInHttpResponse(SignInHttpResponse.ResultType.AccountIsBlocked));

                if (logger.Log(LogType.Trace))
                    logger.Trace($"Sended HTTP server sign in response, account is blocked. (username: {request.name}, remoteEndPoint: {httpRequest.RemoteEndPoint})");

                return;
            }

            var accessToken = Token.Create();
            account.AccessToken = accessToken;
            account.LastUseIpAddress = httpRequest.RemoteEndPoint.Address;

            if (!accountDatabase.TryUpdate(account))
            {
                SendResponse(httpResponse, new SignInHttpResponse(SignInHttpResponse.ResultType.FailedToWrite));

                if (logger.Log(LogType.Error))
                    logger.Error($"Failed to update account in the database on sign in request. (username: {request.name}, remoteEndPoint: {httpRequest.RemoteEndPoint})");

                return;
            }

            SendResponse(httpResponse, new SignInHttpResponse(SignInHttpResponse.ResultType.Success, version, accessToken));

            if (logger.Log(LogType.Info))
                logger.Info($"Account signed in. (username: {request.name}, remoreEndPoint: {httpRequest.RemoteEndPoint})");
        }

        // TODO: change password
        // TODO: forgot password
        // TODO: change email address
        // TODO: validate email address on sign up
    }
}
