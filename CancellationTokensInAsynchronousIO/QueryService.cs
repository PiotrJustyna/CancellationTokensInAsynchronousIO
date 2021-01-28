using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CancellationTokensInAsynchronousIO
{
    public interface IQueryService
    {
        Task<(bool, bool)> GetCommentsAsync(CancellationToken cancellationToken);
    }

    public class QueryService : IQueryService
    {
        private readonly IHttpClientFactory _clientFactory;

        public QueryService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<(bool, bool)> GetCommentsAsync(CancellationToken cancellationToken)
        {
            (bool, bool) result = (false, false);
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://jsonplaceholder.typicode.com/comments");

            var client = _clientFactory.CreateClient();
            
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (response.IsSuccessStatusCode &&
                !cancellationToken.IsCancellationRequested)
            {
                // second operation started
                result.Item1 = true;
                
                // net 5.0
                // _ = await response.Content.ReadAsStringAsync(cancellationToken);
                
                // net core 3.1
                _ = await response.Content.ReadAsStringAsync();
                
                // second operation fully executed
                result.Item2 = true;
            }

            return result;
        }
    }
}