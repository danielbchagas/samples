using Samples.EventBus.Domain.Events;
using System.Text.Json;

namespace Samples.EventBus.Application.Handler;

internal class SubmittedHandler
{
    private readonly ILogger<SubmittedHandler> _logger;

    public SubmittedHandler(ILogger<SubmittedHandler> logger)
    {
        _logger = logger;
    }

    public void OnSubmitted(object sender, Submitted e)
    {
        _logger.LogInformation("SubmittedHandler: {0}", 
            JsonSerializer.Serialize(e, new JsonSerializerOptions
            {
                WriteIndented = true
            })
        );
    }
}
