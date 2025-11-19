using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PayTR.PosSelection.Shared.SeedWork;

namespace PayTR.PosSelection.Application.SeedWork.PipelineBehaviours
{
    public class ExceptionPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<ExceptionPipelineBehaviour<TRequest, TResponse>> _logger;

        public ExceptionPipelineBehaviour(ILogger<ExceptionPipelineBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (DomainException)
            {
                throw;
            }
            catch (ApplicationGeneralException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UndefinedApplicationException(ex.Message, ex);
            }
        }
    }

}
