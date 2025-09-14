using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CleanTemplate.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CleanTemplate.Api.Auth
{
    /// <summary>
    ///













    /// </summary>
        /// <summary>
        /// تنفيذ لـ ICurrentUser يقرأ من JWT Claims عبر HttpContextAccessor.
        /// - يوفر خصائص عامة (UserId, Email, TenantId, Permissions) معزولة عن طبقة الويب.
        /// - يجمع الصلاحيات من مطالبات permissions/permission وكذلك من مطالبات scope.
        /// </summary>

    public sealed class CurrentUser : ICurrentUser
    {
        private static readonly string[] PermissionClaimTypes = new[] { "permissions", "permission" };
        private static readonly string ScopeClaim = "scope";

        public CurrentUser(IHttpContextAccessor accessor)
        {
            var user = accessor.HttpContext?.User;
            IsAuthenticated = user?.Identity?.IsAuthenticated == true;

            if (user != null)
            {
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
                DisplayName = user.FindFirstValue("name") ?? user.FindFirstValue(ClaimTypes.Name);
                Email = user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email");
                TenantId = user.FindFirstValue("tenant") ?? user.FindFirstValue("tenant_id") ?? user.FindFirstValue("tid");
                IsServiceAccount = user.HasClaim(c => c.Type == "client_id") || user.HasClaim(c => c.Type == "azp");

                var perms = new List<string>();
                foreach (var t in PermissionClaimTypes)
                    perms.AddRange(user.FindAll(t).Select(c => c.Value));
                // scopes might be space-separated
                var scopes = user.FindAll(ScopeClaim).SelectMany(c => c.Value.Split(' ')).ToArray();
                perms.AddRange(scopes);
                Permissions = perms.Distinct().ToArray();
            }
            else
            {
                Permissions = new string[0];
            }
        }

        public string? UserId { get; }
        public string? DisplayName { get; }
        public string? Email { get; }
        public string? TenantId { get; }
        public bool IsAuthenticated { get; }
        public bool IsServiceAccount { get; }
        public IReadOnlyCollection<string> Permissions { get; } = new List<string>();

        public bool HasPermission(string permission)
            => Permissions.Contains(permission) || Permissions.Contains(permission.ToLowerInvariant());
    }
}

