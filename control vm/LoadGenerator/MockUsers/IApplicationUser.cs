using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoadGenerator.MockUsers {
    public interface IApplicationUser {
        
        Task Run(string baseURL);
    }
}