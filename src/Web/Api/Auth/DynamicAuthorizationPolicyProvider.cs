using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CleanTemplate.Api.Auth
{
    /// <summary>
    ///
















    /// </summary>
        /// <summary>
        /// موفّر سياسات تفويض ديناميكي:
        /// - ينشئ سياسات وقت التشغيل وفق نمط الاسم (Permission:* أو Scope:*).
        /// - يسهّل بناء السياسات حسب الخدمة دون تعريفات ثابتة عديدة.
        /// </summary>

    public sealed class DynamicAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly AuthorizationOptions _options;
        private readonly IConfiguration _configuration;

        public const string PermissionPrefix = "Permission:";
        public const string ScopePrefix = "Scope:";

        public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IConfiguration configuration)
            : base(options)
        {
            _options = options.Value;
            _configuration = configuration;
        }

        public override Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // If the policy already exists, return it
            if (_options.GetPolicy(policyName) is { } existing)
                return Task.FromResult(existing);

            // Dynamically build from convention
            if (policyName.StartsWith(PermissionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var perm = policyName.Substring(PermissionPrefix.Length);
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new HasPermissionRequirement(perm, _configuration))
                    .Build();
                _options.AddPolicy(policyName, policy);
                return Task.FromResult(policy);
            }
            if (policyName.StartsWith(ScopePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var scope = policyName.Substring(ScopePrefix.Length);
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new HasScopeRequirement(scope))
                    .Build();
                _options.AddPolicy(policyName, policy);
                return Task.FromResult(policy);
            }

            return base.GetPolicyAsync(policyName);
        }
    }
}

