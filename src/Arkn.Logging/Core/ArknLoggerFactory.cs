using Arkn.Logging.Abstractions;
using Arkn.Logging.Models;
using Microsoft.Extensions.Logging;

namespace Arkn.Logging.Core;

/// <summary>
/// Bridges <see cref="IArknLogger"/> with Microsoft.Extensions.Logging.
/// When an <see cref="ILoggerFactory"/> is provided, also forwards entries to MEL.
/// </summary>
public sealed class ArknLoggerFactory
{
    private readonly ILoggerFactory? _melFactory;

    public ArknLoggerFactory(ILoggerFactory? melFactory = null)
    {
        _melFactory = melFactory;
    }

    /// <summary>
    /// Creates an <see cref="IArknLogger"/> that writes to the given sinks
    /// and optionally bridges to MEL under the given category name.
    /// </summary>
    public IArknLogger Create(
        IEnumerable<IArknLogSink> sinks,
        ArknLogLevel minimumLevel = ArknLogLevel.Trace,
        string? melCategory = null)
    {
        var sinkList = sinks.ToList();

        if (_melFactory is not null)
        {
            var category = melCategory ?? "Arkn";
            var melLogger = _melFactory.CreateLogger(category);
            sinkList.Add(new MelBridgeSink(melLogger));
        }

        return new ArknLogger(sinkList, minimumLevel);
    }

    // ── MEL bridge sink (internal) ─────────────────────────────────────────────

    private sealed class MelBridgeSink : IArknLogSink
    {
        private readonly ILogger _logger;

        public MelBridgeSink(ILogger logger) => _logger = logger;

        public void Write(LogEntry entry)
        {
            var melLevel = entry.Level switch
            {
                ArknLogLevel.Trace   => LogLevel.Trace,
                ArknLogLevel.Debug   => LogLevel.Debug,
                ArknLogLevel.Info    => LogLevel.Information,
                ArknLogLevel.Warning => LogLevel.Warning,
                ArknLogLevel.Error   => LogLevel.Error,
                ArknLogLevel.Fatal   => LogLevel.Critical,
                _                    => LogLevel.Information,
            };

            _logger.Log(melLevel, entry.Exception, "{Message}", entry.Message);
        }
    }
}
