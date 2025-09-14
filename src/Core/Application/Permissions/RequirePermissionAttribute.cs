using System;

namespace CleanTemplate.Application.Permissions
{
    /// <summary>
    ///


///














    ///











    /// </summary>
        /// <summary>
        /// Attribute 444444 444444 444 444 4 44 444 Request/Handler.
        /// 44444 44 AuthorizationBehavior 44444 444444 444444 444444.
        /// </summary>

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class RequirePermissionAttribute : Attribute
    {
        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }
    }
}

