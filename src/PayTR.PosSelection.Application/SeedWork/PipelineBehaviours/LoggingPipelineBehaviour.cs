using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PayTR.PosSelection.Application.SeedWork.PipelineBehaviours
{
    public class LoggingPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> _logger;

        public LoggingPipelineBehaviour(ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            
            _logger.LogInformation("Handling {Class} : {@Request}", typeof(TRequest).Name, JsonSerializer.Serialize(request));
            var response = await next();
            _logger.LogInformation("Handling {Class} : {@Response}", typeof(TResponse).Name, JsonSerializer.Serialize(response));
            return response;
        }
    }
}