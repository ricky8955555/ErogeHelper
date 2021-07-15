using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ErogeHelper.Common.Function
{
    [DebuggerStepThrough]
    public class HttpClientDiagnosticsHandler : DelegatingHandler
    {
        public HttpClientDiagnosticsHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        public HttpClientDiagnosticsHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var totalElapsedTime = Stopwatch.StartNew();

            Log.Debug($"Request: {request}");
            if (request.Content is not null)
            {
                var content = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                Log.Debug($"Request Content: {content}");
            }

            var responseElapsedTime = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken);

            Log.Debug($"Response: {response}");
            var respContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            Log.Debug($"Response Content: {respContent}");

            responseElapsedTime.Stop();
            Log.Debug($"Response elapsed time: {responseElapsedTime.ElapsedMilliseconds} ms");

            totalElapsedTime.Stop();
            Log.Debug($"Total elapsed time: {totalElapsedTime.ElapsedMilliseconds} ms");

            return response;
        }
    }
}