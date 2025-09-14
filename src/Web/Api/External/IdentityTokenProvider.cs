using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CleanTemplate.Api.External
{
    /// <summary>
    /// موفّر توكين موحّد للحصول على access_token من Tamkeen.IdentityService.
    /// مدعوم حاليًا عبر REST API (OAuth2 token endpoint). يمكن لاحقًا إضافة تنفيذ gRPC.
    /// </summary>
    public interface IIdentityTokenProvider
    {
        Task<string> GetAccessTokenAsync(string clientId, string clientSecret, string scope, CancellationToken ct = default);
    }

    internal sealed class ApiIdentityTokenProvider : IIdentityTokenProvider
    {
        private readonly IConfiguration _cfg;
        public ApiIdentityTokenProvider(IConfiguration cfg) => _cfg = cfg;

        public async Task<string> GetAccessTokenAsync(string clientId, string clientSecret, string scope, CancellationToken ct = default)
        {
            var authority = _cfg["Identity:Authority"]?.TrimEnd('/') ?? throw new InvalidOperationException("Identity:Authority missing");
            using var http = new HttpClient { BaseAddress = new Uri(authority) };
            var body = new StringContent($"grant_type=client_credentials&client_id={Uri.EscapeDataString(clientId)}&client_secret={Uri.EscapeDataString(clientSecret)}&scope={Uri.EscapeDataString(scope ?? string.Empty)}", Encoding.UTF8, "application/x-www-form-urlencoded");
            var resp = await http.PostAsync("/connect/token", body, ct);
            resp.EnsureSuccessStatusCode();
            using var s = await resp.Content.ReadAsStreamAsync(ct);
            var doc = await JsonDocument.ParseAsync(s, cancellationToken: ct);
            var token = doc.RootElement.GetProperty("access_token").GetString();
            if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("No access_token in response");
            return token!;
        }
    }

    /// <summary>
    /// هيكل جاهز لتبنّي gRPC لاحقًا، مفعّل عند ضبط Identity:Transport = "Grpc".
    /// يتطلب إضافة ملفات proto ومكتبات client الخاصة بـ Tamkeen.IdentityService.
    /// </summary>
    internal sealed class GrpcIdentityTokenProvider : IIdentityTokenProvider
    {
        private readonly IConfiguration _cfg;
        public GrpcIdentityTokenProvider(IConfiguration cfg) => _cfg = cfg;

        public async Task<string> GetAccessTokenAsync(string clientId, string clientSecret, string scope, CancellationToken ct = default)
        {
            // ملاحظة: بروتوكول gRPC في Tamkeen.IdentityService يوفر Authenticate (للمستخدمين)
            // وليس تدفق OAuth2 Client Credentials القياسي. لاستخراج access_token عبر gRPC
            // سنعتمد على حساب خدمة (service account) يُحدد في الإعدادات.
            var address = _cfg["Identity:Grpc:Address"] ?? throw new InvalidOperationException("Identity:Grpc:Address missing");
            var allowUntrusted = bool.TryParse(_cfg["Identity:Grpc:AllowUntrusted"], out var a) && a;

            var tenantId = _cfg["Identity:ClientUser:TenantId"] ?? "default";
            var username = _cfg["Identity:ClientUser:Username"] ?? throw new InvalidOperationException("Identity:ClientUser:Username missing");
            var password = _cfg["Identity:ClientUser:Password"] ?? throw new InvalidOperationException("Identity:ClientUser:Password missing");

            // إعداد القناة مع خيار قبول الشهادات غير الموثوقة في بيئة التطوير
            System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler();
            if (allowUntrusted)
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            var httpClient = new System.Net.Http.HttpClient(handler)
            {
                BaseAddress = new Uri(address)
            };

            var channel = Grpc.Net.Client.GrpcChannel.ForAddress(address, new Grpc.Net.Client.GrpcChannelOptions
            {
                HttpClient = httpClient
            });

            // استدعاء gRPC Authenticate
            var client = new Tamkeen.Identity.Grpc.Identity.IdentityClient(channel);
            var req = new Tamkeen.Identity.Grpc.LoginRequest
            {
                TenantId = tenantId,
                Username = username,
                Password = password
            };

            var reply = await client.AuthenticateAsync(req, cancellationToken: ct);
            if (!reply.Success || string.IsNullOrWhiteSpace(reply.AccessToken))
            {
                throw new InvalidOperationException($"gRPC Authenticate failed: {reply.ErrorCode} {reply.ErrorMessage}");
            }

            return reply.AccessToken;
        }
    }
}

