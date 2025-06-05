using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using LoyaltyApi.Models; // Asegúrate de que tus DTOs estén aquí (MembershipBenefitDto, CreateMembershipBenefitDto)
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using LoyaltyApi.Data; // Para LoyaltyContext
using LoyaltyApi.Entities; // Para tus entidades (MembershipPlan, MembershipBenefit)
using Microsoft.Extensions.DependencyInjection; // Para CreateScope

namespace LoyaltyApi.IntegrationTests
{
    public class MembershipBenefitsIntegrationTests : IClassFixture<WebApplicationFactory<LoyaltyApi.Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<LoyaltyApi.Program> _factory;

        public MembershipBenefitsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Add("X-API-Key", "YourSecureApiKey123");
        }

        // Método para limpiar y seedear la base de datos específicamente para las pruebas de beneficios
        private async Task InitializeDatabaseForBenefitsTestsAsync()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LoyaltyContext>();
                await context.Database.EnsureDeletedAsync(); // Elimina la base de datos
                await context.Database.EnsureCreatedAsync(); // La crea de nuevo

                // **IMPORTANTE:** Seedear un plan activo, ya que los beneficios dependen de un plan existente y activo.
                var activePlan = new MembershipPlan
                {
                    Nombre = "Plan Activo para Beneficios",
                    PrecioMensual = 50,
                    EntradasMensuales = 5,
                    MesesAcumulacionMax = 6,
                    Nivel = 1,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };
                context.MembershipPlans.Add(activePlan);
                await context.SaveChangesAsync(); // Guarda el plan para obtener su ID

                // Seedear algunos beneficios para las pruebas de GET y DELETE
                context.MembershipBenefits.AddRange(
                    new MembershipBenefit { PlanId = activePlan.Id, Clave = "DESC10_BEN", Valor = 10, DiasAplicables = "L-V", Observacion = "10% de descuento" },
                    new MembershipBenefit { PlanId = activePlan.Id, Clave = "FREE_POP_BEN", Valor = 0, DiasAplicables = "S-D", Observacion = "Popcorn gratis" }
                );
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Verifica que GET /api/benefits retorne 200 y una lista de beneficios.
        /// </summary>
        [Fact]
        public async Task GetBenefits_ReturnsOkAndList()
        {
            await InitializeDatabaseForBenefitsTestsAsync(); // Llama a la inicialización
            var response = await _client.GetAsync("/api/benefits");
            response.EnsureSuccessStatusCode(); // Status Code 200-299

            var benefits = await response.Content.ReadFromJsonAsync<MembershipBenefitDto[]>();
            Assert.NotNull(benefits);
            Assert.True(benefits.Length >= 2); // Esperamos al menos los 2 beneficios que se sembraron
        }

        /// <summary>
        /// Verifica que GET /api/benefits/{id} retorne 200 y un beneficio específico.
        /// </summary>
        [Fact]
        public async Task GetBenefitById_ReturnsOkAndBenefit()
        {
            await InitializeDatabaseForBenefitsTestsAsync(); // Llama a la inicialización
            // Obtener un ID de un beneficio existente de los datos sembrados
            int existingBenefitId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LoyaltyContext>();
                existingBenefitId = (await context.MembershipBenefits.FirstAsync()).Id;
            }

            var response = await _client.GetAsync($"/api/benefits/{existingBenefitId}");
            response.EnsureSuccessStatusCode();

            var benefit = await response.Content.ReadFromJsonAsync<MembershipBenefitDto>();
            Assert.NotNull(benefit);
            Assert.Equal(existingBenefitId, benefit.Id);
        }

        /// <summary>
        /// Verifica que GET /api/benefits/{id} retorne 404 si el beneficio no existe.
        /// </summary>
        [Fact]
        public async Task GetBenefitById_ReturnsNotFound()
        {
            await InitializeDatabaseForBenefitsTestsAsync(); // Llama a la inicialización
            var response = await _client.GetAsync("/api/benefits/9999"); // Usar un ID que no exista
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Verifica que POST /api/benefits cree un beneficio y retorne 201 Created.
        /// </summary>
        [Fact]
        public async Task CreateBenefit_ReturnsCreated()
        {
            await InitializeDatabaseForBenefitsTestsAsync(); // Llama a la inicialización para asegurar un plan activo
            int activePlanId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LoyaltyContext>();
                activePlanId = (await context.MembershipPlans.Where(p => p.Activo).FirstAsync()).Id;
            }

            var createDto = new CreateMembershipBenefitDto
            {
                PlanId = activePlanId,
                Clave = "BONUS_PUNTOS_NUEVO",
                Valor = 50,
                DiasAplicables = "Todos",
                Observacion = "Puntos extra por compra"
            };

            var response = await _client.PostAsJsonAsync("/api/benefits", createDto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdBenefit = await response.Content.ReadFromJsonAsync<MembershipBenefitDto>();
            Assert.NotNull(createdBenefit);
            Assert.Equal(createDto.Clave, createdBenefit.Clave);
            Assert.True(createdBenefit.Id > 0); // Verifica que se asignó un ID
        }

        /// <summary>
        /// Verifica que POST /api/benefits retorne 400 Bad Request si el PlanId no existe o está inactivo.
        /// </summary>
        [Fact]
        public async Task CreateBenefit_WithInvalidPlanId_ReturnsBadRequest()
        {
            await InitializeDatabaseForBenefitsTestsAsync(); // Llama a la inicialización
            var createDto = new CreateMembershipBenefitDto
            {
                PlanId = 9999, // ID de plan no existente
                Clave = "INVALID_PLAN_REF",
                Valor = 10,
                DiasAplicables = "L-V",
                Observacion = "Beneficio con plan inválido"
            };

            var response = await _client.PostAsJsonAsync("/api/benefits", createDto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorMessage = await response.Content.ReadAsStringAsync();
            Assert.Contains("El plan especificado no existe o está inactivo.", errorMessage);
        }

        /// <summary>
        /// Verifica que PUT /api/benefits/{id} actualice un beneficio y retorne 204 No Content.
        /// </summary>
        [Fact]
        public async Task UpdateBenefit_ReturnsNoContent()
        {
            await InitializeDatabaseForBenefitsTestsAsync(); // Llama a la inicialización
            int existingBenefitId;
            int activePlanId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LoyaltyContext>();
                var existingBenefit = await context.MembershipBenefits.FirstAsync();
                existingBenefitId = existingBenefit.Id;
                activePlanId = (await context.MembershipPlans.Where(p => p.Activo).FirstAsync()).Id;
            }

            var updateDto = new CreateMembershipBenefitDto
            {
                PlanId = activePlanId,
                Clave = "DESC20_UPDATED_BEN",
                Valor = 20,
                DiasAplicables = "L-D",
                Observacion = "20% de descuento actualizado"
            };

            var response = await _client.PutAsJsonAsync($"/api/benefits/{existingBenefitId}", updateDto);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verificar la actualización obteniendo el beneficio de nuevo
            var getResponse = await _client.GetAsync($"/api/benefits/{existingBenefitId}");
            getResponse.EnsureSuccessStatusCode();
            var updatedBenefit = await getResponse.Content.ReadFromJsonAsync<MembershipBenefitDto>();
            Assert.NotNull(updatedBenefit);
            Assert.Equal("DESC20_UPDATED_BEN", updatedBenefit.Clave);
            Assert.Equal(20, updatedBenefit.Valor);
        }

        /// <summary>
        /// Verifica que PUT /api/benefits/{id} retorne 404 si el beneficio a actualizar no existe.
        /// </summary>
        [Fact]
        public async Task UpdateBenefit_ReturnsNotFound()
        {
            await InitializeDatabaseForBenefitsTestsAsync(); // Llama a la inicialización
            int activePlanId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LoyaltyContext>();
                activePlanId = (await context.MembershipPlans.Where(p => p.Activo).FirstAsync()).Id;
            }

            var updateDto = new CreateMembershipBenefitDto
            {
                PlanId = activePlanId,
                Clave = "NON_EXISTENT_BEN",
                Valor = 10,
                DiasAplicables = "",
                Observacion = ""
            };

            var response = await _client.PutAsJsonAsync("/api/benefits/9999", updateDto); // ID no existente
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Verifica que PUT /api/benefits/{id} retorne 400 Bad Request si el PlanId para la actualización no existe o está inactivo.
        /// </summary>
        [Fact]
        public async Task UpdateBenefit_WithInvalidPlanId_ReturnsBadRequest()
        {
            await InitializeDatabaseForBenefitsTestsAsync(); // Llama a la inicialización
            int existingBenefitId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LoyaltyContext>();
                var existingBenefit = await context.MembershipBenefits.FirstAsync();
                existingBenefitId = existingBenefit.Id;
            }

            var updateDto = new CreateMembershipBenefitDto
            {
                PlanId = 9999, // ID de plan no existente
                Clave = "INVALID_PLAN_UPDATE_BEN",
                Valor = 15,
                DiasAplicables = "",
                Observacion = ""
            };

            var response = await _client.PutAsJsonAsync($"/api/benefits/{existingBenefitId}", updateDto);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var errorMessage = await response.Content.ReadAsStringAsync();
            Assert.Contains("El plan especificado no existe o está inactivo.", errorMessage);
        }

        /// <summary>
        /// Verifica que DELETE /api/benefits/{id} elimine un beneficio y retorne 204 No Content.
        /// </summary>
        [Fact]
        public async Task DeleteBenefit_ReturnsNoContent()
        {
            await InitializeDatabaseForBenefitsTestsAsync(); // Llama a la inicialización
            int benefitToDeleteId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<LoyaltyContext>();
                benefitToDeleteId = (await context.MembershipBenefits.FirstAsync()).Id;
            }

            var response = await _client.DeleteAsync($"/api/benefits/{benefitToDeleteId}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verificar la eliminación intentando obtener el beneficio
            var getResponse = await _client.GetAsync($"/api/benefits/{benefitToDeleteId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        /// <summary>
        /// Verifica que DELETE /api/benefits/{id} retorne 404 si el beneficio a eliminar no existe.
        /// </summary>
        [Fact]
        public async Task DeleteBenefit_ReturnsNotFound()
        {
            await InitializeDatabaseForBenefitsTestsAsync(); // Llama a la inicialización
            var response = await _client.DeleteAsync("/api/benefits/9999"); // ID no existente
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}