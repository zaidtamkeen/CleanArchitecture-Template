using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CleanTemplate.Api.External
{
    /// <summary>
    /// عميل PayMobile كمثال تكامل خارجي يستخدم تدفق Client Credentials ضد Tamkeen.IdentityService.
    /// - يستدعي /connect/token للحصول على access_token.
    /// - يضع التوكين في ترويسة Authorization ويستدعي API الخاصة بـ PayMobile.
    /// استخدم كمرجع في خدمات مثل Tamkeen.FundTransferLimits.
    /// </summary>
    public interface IPayMobileClient
    {
        Task<string> CheckBalanceAsync(string accountId, CancellationToken ct = default);
    }

    internal sealed class PayMobileClient : IPayMobileClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;
        private readonly IIdentityTokenProvider _tokenProvider;

        public PayMobileClient(HttpClient httpClient, IConfiguration configuration, IIdentityTokenProvider tokenProvider)
        {
            _http = httpClient;
            _cfg = configuration;
            _tokenProvider = tokenProvider;
            _http.BaseAddress = new Uri(_cfg["PayMobile:BaseUrl"]!);
        }

        public async Task<string> CheckBalanceAsync(string accountId, CancellationToken ct = default)
        {
            // Acquire token using Client Credentials from Tamkeen.IdentityService
            var clientId = _cfg["PayMobile:ClientId"] ?? throw new InvalidOperationException("PayMobile:ClientId missing");
            var clientSecret = _cfg["PayMobile:ClientSecret"] ?? throw new InvalidOperationException("PayMobile:ClientSecret missing");
            var scope = _cfg["PayMobile:Scope"] ?? string.Empty;
            var token = await _tokenProvider.GetAccessTokenAsync(clientId, clientSecret, scope, ct);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _http.GetAsync($"v1/accounts/{accountId}/balance", ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync(ct);
        }


    }
}

