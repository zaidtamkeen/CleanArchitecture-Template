using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace CleanTemplate.Api.Auth
{
    /// <summary>
    ///












    /// </summary>
        /// <summary>
        /// متطلب تفويض قائم على الصلاحية Permission.
        /// يفحص مطالبات المستخدم (permissions/permission أو scope) للتأكد من احتواء الصلاحية المطلوبة.
        /// </summary>

    public sealed class HasPermissionRequirement : IAuthorizationRequirement
    {
        public HasPermissionRequirement(string permission, IConfiguration configuration)
        {
            Permission = permission;
            Configuration = configuration;
        }
        public string Permission { get; }
        public IConfiguration Configuration { get; }
    }

    public sealed class HasPermissionHandler : AuthorizationHandler<HasPermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasPermissionRequirement requirement)
        {
            var serviceName = requirement.Configuration["Auth:ServiceName"]; // optional override
            if (string.IsNullOrWhiteSpace(serviceName))
                serviceName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Service";

            var prefixed = $"{serviceName}.{requirement.Permission}";
            var prefixedLower = prefixed.ToLowerInvariant();

            var has = context.User?.Claims?.Any(c =>
                         (c.Type.Equals("permissions", StringComparison.OrdinalIgnoreCase) &&
                          (c.Value.Equals(requirement.Permission, StringComparison.OrdinalIgnoreCase) ||
                           c.Value.Equals(prefixed, StringComparison.OrdinalIgnoreCase)))
                       || (c.Type.Equals("scope", StringComparison.OrdinalIgnoreCase) &&
                           (c.Value.Split(' ').Contains(requirement.Permission.ToLowerInvariant()) ||
                            c.Value.Split(' ').Contains(prefixedLower)))) ?? false;

            if (has) context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}

