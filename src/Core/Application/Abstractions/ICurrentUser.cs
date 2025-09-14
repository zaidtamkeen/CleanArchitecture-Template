using System.Collections.Generic;

namespace CleanTemplate.Application.Abstractions
{
    /// <summary>
    /// واجهة عامة لقراءة سياق المستخدم/الحساب الحالي من طبقة التطبيق دون الإلمام بتفاصيل الهوية (JWT/Claims).
    /// تُطبق في طبقة الويب وتُحقن هنا كواجهة فقط للمحافظة على نظافة الطبقات.
    /// </summary>
    public interface ICurrentUser
    {
        string? UserId { get; }
        string? DisplayName { get; }
        string? Email { get; }
        string? TenantId { get; }
        bool IsAuthenticated { get; }
        bool IsServiceAccount { get; }
        IReadOnlyCollection<string> Permissions { get; }
        bool HasPermission(string permission);
    }
}

