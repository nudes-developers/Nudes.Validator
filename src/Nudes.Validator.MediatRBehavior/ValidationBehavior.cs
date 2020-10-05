using FluentValidation;
using MediatR;
using Nudes.Retornator.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nudes.Validator.MediatRBehavior
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TResponse : BaseResult<TResponse>, new()
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            this.validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var failedValidations = validators
                .Select(v => v.Validate(request))
                .Where(v => !v.IsValid)
                .ToList();

            if (failedValidations.Any())
            {
                var errors = failedValidations
                    .SelectMany(f => f.Errors)
                    .GroupBy(d => d.PropertyName)
                    .ToDictionary(d => d.Key, d => d.Select(f => f.ErrorMessage)
                    .ToList());

                return BaseResult<TResponse>.Throw(new Error()
                {
                    FieldErrors = errors
                });
            }

            return await next();
        }
    }
}
