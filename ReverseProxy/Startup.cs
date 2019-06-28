using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ReverseProxy
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            ILoadBalancer loadBalancer = CreateLoadBalancerFromConfig();

            // read the rule from environment
            //if (Enum.TryParse(typeof(LoadBalancingRule), rule, out object readRule))
            //    loadBalancer.Rule = (LoadBalancingRule)readRule;


            app.UseMiddleware<ReverseMiddleware>(loadBalancer);


            app.Run(async (context) =>
            {
                StringBuilder sb = new StringBuilder("<h1>Reverse Proxy Running</h1><br/>");
                foreach (var s in loadBalancer.Services)
                {
                    sb.Append($"<h2>{s.Value.Name}</h2>");
                    foreach (var host in s.Value.Hosts)
                        sb.Append($"<h5>{host.Address}:{host.Port}</h5>");
                }

                await context.Response.WriteAsync(sb.ToString());
            });
        }

        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public ILoadBalancer CreateLoadBalancerFromConfig()
        {
            var confServices = new List<Service>();
            Configuration.GetSection("proxy:services").Bind(confServices);

            LoadBalancer loadBalancer = new LoadBalancer();
            foreach (var s in confServices)
                loadBalancer.Services.Add(s.Domain, s);

            return loadBalancer;

        }


    }
}
