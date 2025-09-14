using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CleanTemplate.Api.Auth
{
    /// <summary>
    ///








    /// </summary>
        /// <summary>
        /// متطلب تفويض قائم على scope (قائمة داخل مطالبة "scope").
        /// ينجح إن وُجد الscope المطلوب ضمن القائمة.
        /// </summary>

    public sealed class HasScopeRequirement : IAuthorizationRequirement
    {
        public HasScopeRequirement(string scope)
        {
            Scope = scope;
        }
        public string Scope { get; }
    }

    public sealed class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            var scopes = context.User?.FindAll("scope")?.SelectMany(c => c.Value.Split(' ')) ?? Array.Empty<string>();
            if (scopes.Contains(requirement.Scope, StringComparer.OrdinalIgnoreCase))
                context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}

