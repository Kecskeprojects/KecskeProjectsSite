using Backend.Communication.Internal;
using Backend.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;

namespace Backend.Tools.Extensions;

//Used to override the default logging configuration and add the custom FileLoggerProvider to the logging system
public static class FileLoggerExtensions
{
    //Custom extension method to add the FileLoggerProvider to the logging system and register the necessary configuration options
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<FileLoggerConfiguration, FileLoggerProvider>(builder.Services);

        return builder;
    }

    //Overloaded extension method to allow configuring the FileLoggerConfiguration options directly when adding the FileLoggerProvider to the logging system
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, Action<FileLoggerConfiguration> configure)
    {
        _ = builder.AddFileLogger();
        _ = builder.Services.Configure(configure);

        return builder;
    }
}