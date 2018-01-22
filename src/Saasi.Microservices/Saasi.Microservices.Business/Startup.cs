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
using Saasi.Shared.Queue;

namespace Saasi.Microservices.Business
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddPrometheus();
            services.AddSingleton<IMetricsContainer, MetricsContainer>();
            services.AddSingleton<IThrottleQueue>(new ThrottleQueue(50));
            services.AddTransient<IApiCaller, ApiCaller>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UsePrometheus();
            app.UseMvc();
        }
    }
}
