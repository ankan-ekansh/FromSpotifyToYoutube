using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FromSpotifyToYoutube.Services.Youtube
{
    public class CodeReceiver : ICodeReceiver
    {
        public string RedirectUri => "http://localhost:8081";

        public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
        {
            var authorizationUrl = url.Build().AbsoluteUri;
            string redirectUrl = "http://localhost:8081/";

            using (HttpListener httpListener = new HttpListener())
            {
                httpListener.Prefixes.Add(redirectUrl);
                httpListener.Start();

                Console.WriteLine($"Please click the link to authorize the application {authorizationUrl}");

                var ret = await GetResponseFromListener(httpListener, taskCancellationToken).ConfigureAwait(false);

                return ret;
            }
        }

        private async Task<AuthorizationCodeResponseUrl> GetResponseFromListener(HttpListener listener, CancellationToken ct)
        {
            HttpListenerContext context;
            // Set up cancellation. HttpListener.GetContextAsync() doesn't accept a cancellation token,
            // the HttpListener needs to be stopped which immediately aborts the GetContextAsync() call.
            using (ct.Register(listener.Stop))
            {
                // Wait to get the authorization code response.
                try
                {
                    context = await listener.GetContextAsync().ConfigureAwait(false);
                }
                catch (Exception) when (ct.IsCancellationRequested)
                {
                    ct.ThrowIfCancellationRequested();
                    // Next line will never be reached because cancellation will always have been requested in this catch block.
                    // But it's required to satisfy compiler.
                    throw new InvalidOperationException("Cancellation requested, exiting.");
                }
                catch
                {
                    throw new Exception("Some error occurred while getting response from listener.");
                }

                NameValueCollection coll = context.Request.QueryString;
                // Write a "close" response.
                var bytes = Encoding.UTF8.GetBytes("<html><body>Authentication successful! You may close the browser window now.</body></html>");
                context.Response.ContentLength64 = bytes.Length;
                context.Response.SendChunked = false;
                context.Response.KeepAlive = false;
                var output = context.Response.OutputStream;
                await output.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                await output.FlushAsync().ConfigureAwait(false);
                output.Close();
                context.Response.Close();

                // Create a new response URL with a dictionary that contains all the response query parameters.
                return new AuthorizationCodeResponseUrl(coll.AllKeys.ToDictionary(k => k, k => coll[k]));
            }
        }
    }
}
