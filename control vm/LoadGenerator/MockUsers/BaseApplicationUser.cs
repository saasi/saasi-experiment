using System;
using System.Net.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoadGenerator.MockUsers {
    public class BaseApplicationUser : IApplicationUser
    {
        protected HttpClient _httpClient;
        protected string _guid;

        public BaseApplicationUser()
        {
            _httpClient = new HttpClient();
            _httpClient.MaxResponseContentBufferSize = 256000;
            _guid = System.Guid.NewGuid().ToString();
        }
        public virtual async Task Run(string baseURL, int duration)
        {
            throw new NotImplementedException("");
        }

        
    }
}