using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using LoyaltyApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace LoyaltyApi.IntegrationTests
{
    public class MembershipPlansIntegrationTests : IClassFixture<WebApplicationFactory<LoyaltyApi.Program>>
    {
        private readonly HttpClient _client;

        public MembershipPlansIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Add("X-API-Key", "YourSecureApiKey123");
        }

        /// <summary>
        /// Verifica que GET /api/plans retorne 200 y una lista de planes.
        /// </summary>
        [Fact]
        public async Task GetPlans_ReturnsOkAndList()
        {
            var response = await _client.GetAsync("/api/plans");
            response.EnsureSuccessStatusCode();
            var plans = await response.Content.ReadFromJsonAsync<MembershipPlanDto[]>();
            Assert.NotNull(plans);
        }

        /// <summary>
        /// Verifica que POST /api/plans cree un plan y lo retorne.
        /// </summary>
        [Fact]
        public async Task CreatePlan_ReturnsCreated()
        {
            var dto = new CreateMembershipPlanDto
            {
                Nombre = "Plan Integral",
                PrecioMensual = 99,
                EntradasMensuales = 10,
                MesesAcumulacionMax = 12,
                Nivel = 1,
                Activo = true
            };

            var response = await _client.PostAsJsonAsync("/api/plans", dto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<MembershipPlanDto>();
            Assert.NotNull(created);
            Assert.Equal("Plan Integral", created.Nombre);
        }

        /// <summary>
        /// Verifica que GET /api/plans/{id} retorne 404 si el plan no existe.
        /// </summary>
        [Fact]
        public async Task GetPlanById_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/plans/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
