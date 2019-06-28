using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ReverseProxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ReverseProxyTest
{
    public class ReverseMiddlewareTests
    {
        [Fact]
        public async Task TestGenericService()
        {
            // Arrange
            var middleware = new ReverseMiddleware(null, CreateLoadBalancerFromConfig());

            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethod.Get.Method;
            context.Request.Host = new HostString("generic.com");
            context.Response.Body = new MemoryStream();

            //Act
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();

            Assert.True(context.Response.StatusCode == 200);
            Assert.False(String.IsNullOrEmpty(streamText));

            //if wrong the main page is return, so checking is not the standard page
            Assert.False(streamText.Contains("reverse", StringComparison.CurrentCultureIgnoreCase));

        }

        [Fact]
        public async Task TestEchoService()
        {
            // Arrange
            var middleware = new ReverseMiddleware(null, CreateLoadBalancerFromConfig());

            var context = new DefaultHttpContext();
            context.Request.Method = HttpMethod.Get.Method;
            context.Request.Host = new HostString("echo.com");
            context.Response.Body = new MemoryStream();

            //Act
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();

            Assert.True(context.Response.StatusCode == 200);
            Assert.False(String.IsNullOrEmpty(streamText));
            Assert.True(streamText.Contains("foo", StringComparison.CurrentCultureIgnoreCase));
        }


        public ILoadBalancer CreateLoadBalancerFromConfig()
        {
            var confServices = new List<Service>();
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddYamlFile("appsettings.yml");
            var conf = configurationBuilder.Build();
            conf.GetSection("proxy:services").Bind(confServices);

            LoadBalancer loadBalancer = new LoadBalancer();
            foreach (var s in confServices)
                loadBalancer.Services.Add(s.Domain, s);

            return loadBalancer;
        }
    }

}
