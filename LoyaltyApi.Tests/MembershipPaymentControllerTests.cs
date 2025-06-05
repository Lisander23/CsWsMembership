using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoyaltyApi.Controllers;
using LoyaltyApi.Data;
using LoyaltyApi.Entities;
using System.Linq;

namespace LoyaltyApi.Tests
{
    public class MembershipPaymentsControllerTests
    {
        private DbContextOptions<LoyaltyContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<LoyaltyContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        /// <summary>
        /// Verifica que GetPayments retorne la lista de pagos existentes.
        /// </summary>
        [Fact]
        public async Task GetPayments_ReturnsListOfPayments()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPayments.Add(new MembershipPayment
                {
                    Id = 1,
                    CustomerMembershipId = 1,
                    Monto = 100.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF1"
                });
                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);

                // Act
                var result = await controller.GetPayments();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var payments = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
                Assert.Single(payments);
            }
        }

        /// <summary>
        /// Verifica que GetPayment retorne un pago existente por ID.
        /// </summary>
        [Fact]
        public async Task GetPayment_ReturnsPaymentById()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPayments.Add(new MembershipPayment
                {
                    Id = 2,
                    CustomerMembershipId = 2,
                    Monto = 200.0m,
                    Estado = "Pendiente",
                    ReferenciaExterna = "REF2"
                });
                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);

                // Act
                var result = await controller.GetPayment(2);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                Assert.NotNull(okResult.Value);
            }
        }

        /// <summary>
        /// Verifica que GetPayment retorne NotFound si el pago no existe.
        /// </summary>
        [Fact]
        public async Task GetPayment_ReturnsNotFound_WhenPaymentDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new LoyaltyContext(options);
            var controller = new MembershipPaymentsController(context);

            // Act
            var result = await controller.GetPayment(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifica que CreatePayment cree un nuevo pago.
        /// </summary>
        [Fact]
        public async Task CreatePayment_CreatesPayment()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.CustomerMemberships.Add(new CustomerMembership
                {
                    Id = 1,
                    CodCliente = 1,
                    PlanId = 1,
                    FechaInicio = DateTime.Now,
                    Estado = "Activo"
                });
                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new LoyaltyApi.Models.CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    Monto = 150.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF3"
                };

                // Act
                var result = await controller.CreatePayment(dto);

                // Assert
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                Assert.NotNull(createdResult.Value);
            }
        }

        /// <summary>
        /// Verifica que CreatePayment retorne BadRequest si la membresía no existe.
        /// </summary>
        [Fact]
        public async Task CreatePayment_ReturnsBadRequest_WhenMembershipDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new LoyaltyContext(options);
            var controller = new MembershipPaymentsController(context);
            var dto = new LoyaltyApi.Models.CreateMembershipPaymentDto
            {
                CustomerMembershipId = 999,
                Monto = 150.0m,
                Estado = "Aprobado",
                ReferenciaExterna = "REF4"
            };

            // Act
            var result = await controller.CreatePayment(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Verifica que UpdatePayment actualice un pago existente.
        /// </summary>
        [Fact]
        public async Task UpdatePayment_UpdatesPayment()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPayments.Add(new MembershipPayment
                {
                    Id = 5,
                    CustomerMembershipId = 1,
                    Monto = 100.0m,
                    Estado = "Pendiente",
                    ReferenciaExterna = "REF5"
                });
                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new LoyaltyApi.Models.CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    Monto = 120.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF5-EDIT"
                };

                // Act
                var result = await controller.UpdatePayment(5, dto);

                // Assert
                Assert.IsType<NoContentResult>(result);
            }
        }

        /// <summary>
        /// Verifica que UpdatePayment retorne NotFound si el pago no existe.
        /// </summary>
        [Fact]
        public async Task UpdatePayment_ReturnsNotFound_WhenPaymentDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.CustomerMemberships.Add(new CustomerMembership
                {
                    Id = 2,
                    CodCliente = 2,
                    PlanId = 2,
                    FechaInicio = DateTime.Now,
                    Estado = "Activo"
                });
                context.SaveChanges();
            }
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new LoyaltyApi.Models.CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 2,
                    Monto = 200.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF6"
                };

                // Act
                var result = await controller.UpdatePayment(999, dto);

                // Assert
                Assert.IsType<NotFoundObjectResult>(result);
            }
        }

        /// <summary>
        /// Verifica que DeletePayment elimine un pago existente.
        /// </summary>
        [Fact]
        public async Task DeletePayment_DeletesPayment()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPayments.Add(new MembershipPayment
                {
                    Id = 9,
                    CustomerMembershipId = 1,
                    Monto = 90.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF9"
                });
                context.SaveChanges();
            }
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);

                // Act
                var result = await controller.DeletePayment(9);

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.Null(context.MembershipPayments.Find(9));
            }
        }

        /// <summary>
        /// Verifica que DeletePayment retorne NotFound si el pago no existe.
        /// </summary>
        [Fact]
        public async Task DeletePayment_ReturnsNotFound_WhenPaymentDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new LoyaltyContext(options);
            var controller = new MembershipPaymentsController(context);

            // Act
            var result = await controller.DeletePayment(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
