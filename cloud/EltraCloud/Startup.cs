using System.Runtime.InteropServices;
using EltraCommon.Logger;
using EltraCloud.DataSource;
using EltraCloud.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Threading.Tasks;
using EltraCloud.Channels;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace EltraCloud
{
    /// <summary>
    /// Sturtup
    /// </summary>
    public class Startup
    {
        #region Private fields
                
        private SessionService _sessionService;

        #endregion

        #region Constructors

        /// <summary>
        /// Startup constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Configuration interface
        /// </summary>
        public IConfiguration Configuration { get; }

        #endregion

        #region Methods

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies 
                // is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            MsgLogger.LogPath = Configuration.GetValue<string>(WebHostDefaults.ContentRootKey);
            MsgLogger.LogLevels = Configuration.GetValue<string>("Logging:LogLevels");

            Console.WriteLine($"Log levels: {MsgLogger.LogLevels}");

            services.AddHttpContextAccessor();

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            var storage = new Storage(Configuration);

            CreateSessionService(storage);

            services.AddSingleton<IAuthService>(CreateAuthService(storage));
            services.AddSingleton<IIp2LocationService>(CreateIp2LocationService());

            services.AddSingleton<ISessionService>(_sessionService);

            services.AddControllers()
                        .AddNewtonsoftJson();

            services.AddRazorPages();

            // Configure Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ELTRA IoT API",
                    Version = "v1"
                    // You can also set Description, Contact, License, TOS...
                });

                // Configure Swagger to use the xml documentation file
                var xmlFile = Path.ChangeExtension(typeof(Startup).Assembly.Location, ".xml");
                
                c.IncludeXmlComments(xmlFile);
                c.EnableAnnotations();
            });
        }
        
        private AuthService CreateAuthService(Storage storage)
        {
            return new AuthService(storage);
        }

        private void CreateSessionService(Storage storage)
        {
            _sessionService = new SessionService(storage);

            _sessionService.Start();
        }

        private Ip2LocationService CreateIp2LocationService()
        {
            string filePath = Configuration.GetValue<string>("Ip2Location:CsvFilePath");

            var ip2LocationService = new Ip2LocationService { Ip2LocationFile = filePath };

            ip2LocationService.Start();

            return ip2LocationService;
        }

        ///<summary>This method gets called by the runtime. Use this method to configure the HTTP request pipeline.</summary> 
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

                app.UseAuthentication();
            }

            app.UseHttpsRedirection();
            app.UseCookiePolicy();
            app.UseStaticFiles();

            UseWebSockets(app);

            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ELTRA IoT API");
                c.RoutePrefix = "docs";
            });

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                    endpoints.MapControllers();
                });
        }

        private void UseWebSockets(IApplicationBuilder app)
        {
            var webSocketOptions = new WebSocketOptions()
            {
                ReceiveBufferSize = 4096,
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            };

            app.UseWebSockets(webSocketOptions);

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        await HandleWebSocketRequest(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
        }

        private async Task HandleWebSocketRequest(HttpContext context)
        {
            var source = context.Connection.RemoteIpAddress;

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            var socketFinishedTcs = new TaskCompletionSource<object>();
            var webSocketChannelProcessor = new ChannelProcessor(_sessionService);

            if (webSocketChannelProcessor != null)
            {
                MsgLogger.WriteDebug($"{GetType().Name} - HandleWebSocketRequest", $"Create channel request from {source?.ToString()}");

                await webSocketChannelProcessor.CreateChannel(source, webSocket, socketFinishedTcs);
            }
            else
            {
                MsgLogger.WriteError($"{GetType().Name} - HandleWebSocketRequest", "Channel processor not available!");

                socketFinishedTcs.TrySetResult(null);
            }

            await socketFinishedTcs.Task;

            MsgLogger.WriteDebug($"{GetType().Name} - HandleWebSocketRequest", "Web Socket task finished");
        }

        #endregion
    }
}
