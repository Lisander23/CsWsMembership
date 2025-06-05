using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LoyaltyApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LoyaltyApi.IntegrationTests
{
    public class MembershipPaymentsIntegrationTests : IClassFixture<WebApplicationFactory<LoyaltyApi.Program>>
    {
        private readonly HttpClient _client;

        public MembershipPaymentsIntegrationTests(WebApplicationFactory<LoyaltyApi.Program> factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Add("X-API-Key", "YourSecureApiKey123");
        }

        /// <summary>
        /// Verifica que GET /api/payments retorne 200 y una lista de pagos.
        /// </summary>
        [Fact]
        public async Task GetPayments_ReturnsOkAndList()
        {
            // Arrange
            var membershipId = await CreateCustomerMembershipAsync();
            var paymentDto = new CreateMembershipPaymentDto
            {
                CustomerMembershipId = membershipId,
                FechaPago = DateTime.UtcNow,
                Monto = 100,
                Estado = "Pagado",
                ReferenciaExterna = "REF123-" + Guid.NewGuid(),
                Periodo = 6,
                Observaciones = "Pago puntual"
            };
            await _client.PostAsJsonAsync("/api/payments", paymentDto);

            // Act
            var response = await _client.GetAsync("/api/payments");

            // Assert
            response.EnsureSuccessStatusCode();
            var payments = await response.Content.ReadFromJsonAsync<MembershipPaymentDto[]>();
            Assert.NotNull(payments);
            Assert.Contains(payments, p => p.Monto == 100 && p.ReferenciaExterna.StartsWith("REF123-"));
        }

        /// <summary>
        /// Verifica que POST /api/payments cree un pago y lo retorne.
        /// </summary>
        [Fact]
        public async Task CreatePayment_ReturnsCreated()
        {
            // Arrange
            var membershipId = await CreateCustomerMembershipAsync();
            var paymentDto = new CreateMembershipPaymentDto
            {
                CustomerMembershipId = membershipId,
                FechaPago = DateTime.UtcNow,
                Monto = 100,
                Estado = "Pagado",
                ReferenciaExterna = "REF123-" + Guid.NewGuid(),
                Periodo = 6,
                Observaciones = "Pago puntual"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/payments", paymentDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<MembershipPaymentDto>();
            Assert.NotNull(created);
            Assert.Equal(100, created.Monto);
            Assert.Equal(paymentDto.ReferenciaExterna, created.ReferenciaExterna);
        }

        /// <summary>
        /// Verifica que GET /api/payments/{id} retorne 404 si el pago no existe.
        /// </summary>
        [Fact]
        public async Task GetPaymentById_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/payments/9999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Verifica que GET /api/payments/{id} retorne 200 y el pago correcto.
        /// </summary>
        [Fact]
        public async Task GetPaymentById_ReturnsOkAndPayment()
        {
            // Arrange
            var membershipId = await CreateCustomerMembershipAsync();
            var paymentDto = new CreateMembershipPaymentDto
            {
                CustomerMembershipId = membershipId,
                FechaPago = DateTime.UtcNow,
                Monto = 100,
                Estado = "Pagado",
                ReferenciaExterna = "REF123-" + Guid.NewGuid(),
                Periodo = 6,
                Observaciones = "Pago puntual"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/payments", paymentDto);
            var created = await createResponse.Content.ReadFromJsonAsync<MembershipPaymentDto>();

            // Act
            var response = await _client.GetAsync($"/api/payments/{created.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var payment = await response.Content.ReadFromJsonAsync<MembershipPaymentDto>();
            Assert.NotNull(payment);
            Assert.Equal(created.Id, payment.Id);
            Assert.Equal(100, payment.Monto);
        }

        /// <summary>
        /// Verifica que POST /api/payments retorne 400 si la membresía no existe.
        /// </summary>
        [Fact]
        public async Task CreatePayment_ReturnsBadRequest_WhenMembershipDoesNotExist()
        {
            // Arrange
            var paymentDto = new CreateMembershipPaymentDto
            {
                CustomerMembershipId = 9999,
                FechaPago = DateTime.UtcNow,
                Monto = 90,
                Estado = "Pendiente",
                ReferenciaExterna = "INVALID",
                Periodo = 3,
                Observaciones = "Membership inexistente"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/payments", paymentDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Verifica que PUT /api/payments/{id} actualice un pago existente.
        /// </summary>
        [Fact]
        public async Task UpdatePayment_ReturnsNoContent()
        {
            // Arrange
            var membershipId = await CreateCustomerMembershipAsync();
            var paymentDto = new CreateMembershipPaymentDto
            {
                CustomerMembershipId = membershipId,
                FechaPago = DateTime.UtcNow,
                Monto = 100,
                Estado = "Pagado",
                ReferenciaExterna = "REF123-" + Guid.NewGuid(),
                Periodo = 6,
                Observaciones = "Pago puntual"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/payments", paymentDto);
            var created = await createResponse.Content.ReadFromJsonAsync<MembershipPaymentDto>();

            var updateDto = new CreateMembershipPaymentDto
            {
                CustomerMembershipId = membershipId,
                FechaPago = DateTime.UtcNow.AddDays(1),
                Monto = 120,
                Estado = "Pendiente",
                ReferenciaExterna = "REF-UPDATED",
                Periodo = 7,
                Observaciones = "Pago actualizado"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/payments/{created.Id}", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify update
            var getResponse = await _client.GetAsync($"/api/payments/{created.Id}");
            var updated = await getResponse.Content.ReadFromJsonAsync<MembershipPaymentDto>();
            Assert.Equal(120, updated.Monto);
            Assert.Equal("REF-UPDATED", updated.ReferenciaExterna);
        }

        /// <summary>
        /// Verifica que PUT /api/payments/{id} retorne 404 si el pago no existe.
        /// </summary>
        [Fact]
        public async Task UpdatePayment_ReturnsNotFound_WhenPaymentDoesNotExist()
        {
            // Arrange
            var updateDto = new CreateMembershipPaymentDto
            {
                CustomerMembershipId = 1,
                FechaPago = DateTime.UtcNow,
                Monto = 120,
                Estado = "Rechazado",
                ReferenciaExterna = "REF-UPDATE",
                Periodo = 2,
                Observaciones = "Actualización inválida"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/payments/9999", updateDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Verifica que DELETE /api/payments/{id} elimine un pago existente.
        /// </summary>
        [Fact]
        public async Task DeletePayment_ReturnsNoContent()
        {
            // Arrange
            var membershipId = await CreateCustomerMembershipAsync();
            var paymentDto = new CreateMembershipPaymentDto
            {
                CustomerMembershipId = membershipId,
                FechaPago = DateTime.UtcNow,
                Monto = 100,
                Estado = "Pagado",
                ReferenciaExterna = "REF123-" + Guid.NewGuid(),
                Periodo = 6,
                Observaciones = "Pago puntual"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/payments", paymentDto);
            var created = await createResponse.Content.ReadFromJsonAsync<MembershipPaymentDto>();

            // Act
            var response = await _client.DeleteAsync($"/api/payments/{created.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify deletion
            var getResponse = await _client.GetAsync($"/api/payments/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        /// <summary>
        /// Verifica que DELETE /api/payments/{id} retorne 404 si el pago no existe.
        /// </summary>
        [Fact]
        public async Task DeletePayment_ReturnsNotFound_WhenPaymentDoesNotExist()
        {
            // Act
            var response = await _client.DeleteAsync("/api/payments/9999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<int> CreateCustomerMembershipAsync()
        {
            var planId = await CreateMembershipPlanAsync();

            var dto = new CreateCustomerMembershipDto
            {
                CodCliente = 1,
                PlanId = planId,
                FechaInicio = DateTime.UtcNow,
                IdSuscripcionMP = "SusMP-" + Guid.NewGuid(),
                IdClienteMP = "CliMP-" + Guid.NewGuid(),
                MesesAcumulacionPersonalizado = 6
            };

            var response = await _client.PostAsJsonAsync("/api/memberships", dto);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<CustomerMembershipDto>();
            return created.Id;
        }

        private async Task<int> CreateMembershipPlanAsync()
        {
            var dto = new CreateMembershipPlanDto
            {
                Nombre = "Plan Test " + Guid.NewGuid(),
                PrecioMensual = 59.99m,
                EntradasMensuales = 6,
                MesesAcumulacionMax = 12,
                Nivel = 1,
                Activo = true
            };

            var response = await _client.PostAsJsonAsync("/api/plans", dto);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<MembershipPlanDto>();
            return created.Id;
        }
    }
}