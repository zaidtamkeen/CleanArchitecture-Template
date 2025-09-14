using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanTemplate.Application.Abstractions;
using CleanTemplate.Application.Permissions;
using MediatR;

namespace CleanTemplate.Application.Behaviors
{
    /// <summary>
    /// سلوك بايبلاين للتأكد من امتلاك المستخدم للصلاحية المطلوبة قبل تنفيذ أي طلب
    /// يعتمد على وجود RequirePermissionAttribute على صنف الطلب (IRequest)
    /// </summary>
    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ICurrentUser _currentUser;

        public AuthorizationBehavior(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var attrs = request?.GetType().GetCustomAttributes(typeof(RequirePermissionAttribute), true)
                         ?.Cast<RequirePermissionAttribute>()?.ToArray() ?? Array.Empty<RequirePermissionAttribute>();

            if (attrs.Length > 0)
            {
                foreach (var attr in attrs)
                {
                    if (!_currentUser.IsAuthenticated || !_currentUser.HasPermission(attr.Permission))
                        throw new UnauthorizedAccessException($"Missing permission: {attr.Permission}");
                }
            }

            return await next();
        }
    }
}

