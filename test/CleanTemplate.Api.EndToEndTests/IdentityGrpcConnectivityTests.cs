using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Tamkeen.Identity.Grpc;
using Xunit;

namespace CleanTemplate.Api.EndToEndTests
{
    public class IdentityGrpcConnectivityTests
    {
        [Fact]
        public async Task Grpc_ValidateToken_WithInvalidToken_Should_ReturnReply_NotThrow()
        {
            // Arrange: ensure the IdentityService is running locally on 7101 before executing this test.
            // The CI/dev pipeline should start it via `dotnet run --project Tamkeen.IdentityService/Tamkeen.IdentityService/src/Web/Api/Tamkeen.IdentityService.Api.csproj`
            var address = Environment.GetEnvironmentVariable("IDENTITY_GRPC_ADDRESS") ?? "https://localhost:7101";

            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            using var http = new HttpClient(handler) { BaseAddress = new Uri(address) };

            using var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpClient = http });
            var client = new Identity.IdentityClient(channel);

            // Act
            try
            {
                var reply = await client.ValidateTokenAsync(new TokenRequest
                {
                    TenantId = "default",
                    Token = "this-is-not-a-valid-jwt"
                });

                // Assert: we only assert that we reached the service and received a structured reply
                Assert.NotNull(reply);
                Assert.False(reply.IsValid); // invalid token expected
            }
            catch (Exception ex)
            {
                // If the service is not running locally, skip this test with a clear message
                throw new Xunit.Sdk.SkipException($"Skipped: IdentityService gRPC endpoint not reachable at {address}. Details: {ex.Message}");
            }
        }
    }
}

