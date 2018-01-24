using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexogen.Libraries.Metrics;
using Nexogen.Libraries.Metrics.Prometheus;
using Nexogen.Libraries.Metrics.Prometheus.AspCore;
using System.Net.Http;
using System.Net.Http.Headers;
using Polly;
using System.Net;

namespace Saasi.Microservices.Business
{
    public class ApiCaller : IApiCaller
    {
        private static readonly string URL_IO = "http://io/api/io?read=";
        private static readonly string URL_CPU = "http://cpu/api/cpu?round=";
        private static readonly string URL_MEMORY = "http://memory/api/memory?round=";

        private static HttpClient client = new HttpClient();

        private static readonly HttpStatusCode[] httpStatusCodesWorthRetrying = {
            HttpStatusCode.RequestTimeout, // 408
            HttpStatusCode.InternalServerError, // 500
            HttpStatusCode.BadGateway, // 502
            HttpStatusCode.ServiceUnavailable, // 503
            HttpStatusCode.GatewayTimeout // 504
        }; 
        public async Task<object> CallCpu(int round) {
            HttpResponseMessage result = await Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .RetryAsync(3)
                .ExecuteAsync( () => client.GetAsync(URL_CPU+round.ToString()));
            return await result.Content.ReadAsStringAsync();
        }

        public async Task<object> CallMemory(int round) {
            HttpResponseMessage result = await Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .RetryAsync(3)
                .ExecuteAsync( () => client.GetAsync(URL_MEMORY+round.ToString()));
            return await result.Content.ReadAsStringAsync();
        }

        public async Task<object> CallIo(int read) {
            HttpResponseMessage result = await Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                .RetryAsync(3)
                .ExecuteAsync( () => client.GetAsync(URL_IO+read.ToString()));
            return await result.Content.ReadAsStringAsync();
        }
    }

    public interface IApiCaller {
        Task<object> CallCpu(int round);
        Task<object> CallMemory(int round);
        Task<object> CallIo(int read);

    }
}
