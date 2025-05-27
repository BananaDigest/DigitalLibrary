using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace API.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _log;
        public ExceptionFilter(ILogger<ExceptionFilter> log) => _log = log;

        public void OnException(ExceptionContext context)
        {
            _log.LogError(context.Exception, context.Exception.Message);

            context.Result = new ObjectResult(new
            {
                error = context.Exception.Message
            })
            { StatusCode = 500 };

            context.ExceptionHandled = true;
        }
    }
}
