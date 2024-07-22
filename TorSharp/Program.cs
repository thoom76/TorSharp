using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TorSharp.TorrentFile.Bencoding.Parser;
using TorSharp.TorrentFile.Lexer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using TorSharp.BackgroundServices;
using TorSharp.PubSubService;
using TorSharp.PubSubService.InMemoryPubSub;
using TorSharp.TorrentFile.Models;
using TorSharp.TorrentFile.Bencoding.Lexer;

public abstract record BaseEvent<T>{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public abstract T Message { get; init; }
}

public record TorrentMetadataDownloadRequested(string Message) : BaseEvent<string>;
public record TorrentDownloadRequested(ITorrentMetadata Message) : BaseEvent<ITorrentMetadata>;

public static class Program
{
    public static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        ConfigureLogging(builder);
        ConfigureHostedServices(builder.Services);
        ConfigureDI(builder.Services);

        IHost host = builder.Build();

        Task backgroundServices = host.RunAsync();

        var pubSubService = host.Services.GetService<IPubSubService<TorrentMetadataDownloadRequested>>() 
            ?? throw new InvalidOperationException("PubSubService is null");
        
        await pubSubService.PublishAsync(new TorrentMetadataDownloadRequested("<some url>"));

        await backgroundServices;
    }

    public static void ConfigureLogging(HostApplicationBuilder hostApplicationBuilder)
    {
        hostApplicationBuilder.Logging.SetMinimumLevel(LogLevel.Trace);
    }

    public static void ConfigureHostedServices(IServiceCollection services)
    {
        services.AddHostedService<TorrentMetadataDownloaderService>();
        services.AddHostedService<TorrentDownloaderService>();
    }

    public static void ConfigureDI(IServiceCollection services)
    {
        services.AddSingleton(typeof(IPubSubService<>), typeof(InMemoryPubSubService<>));
        services.AddSingleton<ILexer, Lexer>();
        services.AddSingleton<IParser, Parser>();
    }
}
