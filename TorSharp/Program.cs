using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TorSharp.TorrentFile.Bencoding.Parser;
using TorSharp.TorrentFile.Lexer;
using TorSharp.BackgroundServices;
using TorSharp.PubSubService;
using TorSharp.PubSubService.InMemoryPubSub;
using TorSharp.TorrentFile.Bencoding.Lexer;
using Serilog;
using TorSharp.BackgroundServices.UIService;
using Microsoft.Extensions.Configuration;

public abstract record BaseEvent<T>
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public abstract T Message { get; init; }
}

public record TorrentMetadataDownloadRequested(string Message) : BaseEvent<string>;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder();

        ConfigureConfiguration(builder);
        ConfigureLogging(builder);
        ConfigureHostedServices(builder);
        ConfigureDI(builder);
        
        var host = builder.Build(); 
        await host.RunAsync();
    }

    public static void ConfigureConfiguration(IHostBuilder hostApplicationBuilder)
    {
        hostApplicationBuilder.ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        });
    }

    public static void ConfigureLogging(IHostBuilder hostBuilder)
    {
        var tmpLogFile = Path.Join(Path.GetTempPath(), "TorSharp.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(tmpLogFile, outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        hostBuilder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog();
        });
    }

    public static void ConfigureHostedServices(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<TorrentMetadataDownloaderService>();
            services.AddHostedService<TorrentDownloaderService>();
            services.AddHostedService<UIService>();
        });
    }

    public static void ConfigureDI(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((hostContext, services) =>
        {
            services.AddSingleton(typeof(IPubSubService<>), typeof(InMemoryPubSubService<>));
            services.AddSingleton<ILexer, Lexer>();
            services.AddSingleton<IParser, Parser>();
        });
    }
}
