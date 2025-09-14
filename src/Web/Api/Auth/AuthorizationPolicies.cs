using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace CleanTemplate.Api.Auth
{
    /// <summary>
    /// سياسات تفويض إرشادية تُسجَّل عند بدء التشغيل.
    /// ملاحظة: داخل Attributes استخدم نصوص ثابتة مثل:
    ///   [Authorize(Policy = "Permission:Products.Read")]
    ///   [Authorize(Policy = "Scope:fundtransferlimits.read")]
    /// </summary>

    public static class AuthorizationPolicies
    {
        /// <summary>
        /// تسجيل سياسات توضيحية مبنية على اسم الخدمة:
        /// - Scope: {servicename}.read / {servicename}.write
        /// - Permission: {ServiceName}.Read / {ServiceName}.Manage
        /// يمكن للمطور تعديل الأسماء حسب الخدمة الجديدة.
        /// </summary>
        public static void Register(AuthorizationOptions options, IConfiguration configuration)
        {
            // هذا القالب يستخدم مزود سياسات ديناميكي؛ هذه الأمثلة ثابتة لأغراض التوضيح فقط ويمكن تعديلها/إزالتها.
            var serviceName = configuration["Auth:ServiceName"] ?? System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Service";
            var readScope = serviceName.ToLowerInvariant() + ".read";
            var writeScope = serviceName.ToLowerInvariant() + ".write";
            // Examples (optional):
            options.AddPolicy($"Scope:{readScope}", p => p.Requirements.Add(new HasScopeRequirement(readScope)));
            options.AddPolicy($"Scope:{writeScope}", p => p.Requirements.Add(new HasScopeRequirement(writeScope)));
            options.AddPolicy($"Permission:{serviceName}.Read", p => p.Requirements.Add(new HasPermissionRequirement("Read", configuration)));
            options.AddPolicy($"Permission:{serviceName}.Manage", p => p.Requirements.Add(new HasPermissionRequirement("Manage", configuration)));
        }

        /// <summary>
        /// دالة مساعدة لبناء نص سياسة Permission للاستخدام البرمجي (ليس داخل Attribute).
        /// مثال: var policy = AuthorizationPolicies.Permission("Products.Read");
        /// </summary>
        public static string Permission(string permission) => $"{DynamicAuthorizationPolicyProvider.PermissionPrefix}{permission}";

        /// <summary>
        /// دالة مساعدة لبناء نص سياسة Scope للاستخدام البرمجي (ليس داخل Attribute).
        /// مثال: var policy = AuthorizationPolicies.Scope("fundtransferlimits.read");
        /// </summary>
        public static string Scope(string scope) => $"{DynamicAuthorizationPolicyProvider.ScopePrefix}{scope}";
    }
}

