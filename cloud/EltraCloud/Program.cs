using System.IO;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

#pragma warning disable CS1591

namespace EltraCloud
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                CreateLinuxWebHostBuilder(args).Build().Run();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateWindowsWebHostBuilder(args).Build().Run();
            }
        }

        public static IWebHostBuilder CreateWindowsWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", true)
                .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                .AddCommandLine(args)
                .Build();

           return WebHost.CreateDefaultBuilder(args)
               .UseUrls("http://0.0.0.0:5000")
               .UseConfiguration(config)
               .UseIISIntegration()
               .UseStartup<Startup>();
        }

        public static IWebHostBuilder CreateLinuxWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", true)
                .AddCommandLine(args)
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:5000")
                .UseConfiguration(config)
                .UseStartup<Startup>();
        }
    }
}
