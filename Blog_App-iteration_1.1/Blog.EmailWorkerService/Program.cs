using Blog.Core.Settings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Blog.Core.Services;
using Blog.Core.Interfaces;
namespace Blog.EmailWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure EmailSettings from configuration
                    services.Configure<EmailSettings>(hostContext.Configuration.GetSection("EmailSettings"));

                    // Register EmailSender as a singleton service
                    services.AddSingleton<IEmailSender, EmailSender>();
                    
                    // Register SharedEmailQueueService as a singleton service
                    services.AddSingleton<ISharedEmailQueueService>(new SharedEmailQueueService(isServer: true));
                    
                    // Register the background worker service
                    services.AddHostedService<Worker>();
                });
    }
}