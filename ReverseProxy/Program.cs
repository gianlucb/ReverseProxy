using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NetEscapades.Configuration.Yaml;

namespace ReverseProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var b = CreateWebHostBuilder(args);
            b.UseUrls("http://localhost:80"); //this should be read from YAML file. But this is a current limitation of .NET core (cannot read conf before to build it)
            b.Build().Run();
        }


        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
           .ConfigureAppConfiguration(builder =>
           {
               //adding YAML support
               builder.AddYamlFile("appsettings.yml", optional: false);
           })
           .UseStartup<Startup>();

    }
}
