
       using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoyaltyApi.Controllers;
using LoyaltyApi.Data;
using LoyaltyApi.Entities;
using LoyaltyApi.Models;

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

        private void SetupContext(LoyaltyContext context, Action<LoyaltyContext> setupAction)
        {
            setupAction(context);
            context.SaveChanges();
        }

        /// <summary>
        /// Verifica que GetPayments retorne la lista de pagos existentes.
        /// </summary>
        [Fact]
        public async Task GetPayments_ReturnsListOfPayments()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaPago = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 1,
                        CustomerMembershipId = 1,
                        FechaPago = fechaPago,
                        Monto = 100.0m,
                        Estado = "Aprobado",
                        ReferenciaExterna = "REF1",
                        Periodo = 202406,
                        Observaciones = "Pago inicial"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);

                // Act
                var result = await controller.GetPayments();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, okResult.StatusCode);

                var response = okResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var payments = Assert.IsAssignableFrom<IEnumerable<MembershipPaymentDto>>(dataProperty.GetValue(response));
                var paymentList = payments.ToList();
                Assert.Single(paymentList);

                var payment = paymentList.First();
                Assert.Equal(1, payment.Id);
                Assert.Equal(1, payment.CustomerMembershipId);
                Assert.True(Math.Abs((fechaPago - payment.FechaPago).TotalSeconds) <= 1);
                Assert.Equal(100.0m, payment.Monto);
                Assert.Equal("Aprobado", payment.Estado);
                Assert.Equal("REF1", payment.ReferenciaExterna);
                Assert.Equal(202406, payment.Periodo);
                Assert.Equal("Pago inicial", payment.Observaciones);
            }
        }

        /// <summary>
        /// Verifica que GetPayments retorne una lista vacía si no hay pagos.
        /// </summary>
        [Fact]
        public async Task GetPayments_ReturnsEmptyList_WhenNoPaymentsExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);

                // Act
                var result = await controller.GetPayments();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, okResult.StatusCode);

                var response = okResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                Assert.NotNull(dataProperty);

                var payments = Assert.IsAssignableFrom<IEnumerable<MembershipPaymentDto>>(dataProperty.GetValue(response));
                Assert.Empty(payments);
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
            var fechaPago = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 2,
                        CodCliente = 2,
                        PlanId = 2,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 2,
                        CustomerMembershipId = 2,
                        FechaPago = fechaPago,
                        Monto = 200.0m,
                        Estado = "Pendiente",
                        ReferenciaExterna = "REF2",
                        Periodo = 202407,
                        Observaciones = "Pago pendiente"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);

                // Act
                var result = await controller.GetPayment(2);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, okResult.StatusCode);

                var response = okResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var payment = Assert.IsType<MembershipPaymentDto>(dataProperty.GetValue(response));
                Assert.Equal(2, payment.Id);
                Assert.Equal(2, payment.CustomerMembershipId);
                Assert.True(Math.Abs((fechaPago - payment.FechaPago).TotalSeconds) <= 1);
                Assert.Equal(200.0m, payment.Monto);
                Assert.Equal("Pendiente", payment.Estado);
                Assert.Equal("REF2", payment.ReferenciaExterna);
                Assert.Equal(202407, payment.Periodo);
                Assert.Equal("Pago pendiente", payment.Observaciones);
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
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);

                // Act
                var result = await controller.GetPayment(999);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El pago no existe.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreatePayment cree un nuevo pago.
        /// </summary>
        [Fact]
        public async Task CreatePayment_CreatesPayment()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaPago = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    FechaPago = fechaPago,
                    Monto = 150.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF3",
                    Periodo = 202408,
                    Observaciones = "Nuevo pago"
                };

                // Act
                var result = await controller.CreatePayment(dto);

                // Assert
                var createdResult = Assert.IsType<CreatedAtActionResult>(result);
                Assert.Equal(201, createdResult.StatusCode);

                var response = createdResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var payment = Assert.IsType<MembershipPaymentDto>(dataProperty.GetValue(response));
                Assert.Equal(1, payment.CustomerMembershipId);
                Assert.True(Math.Abs((fechaPago - payment.FechaPago).TotalSeconds) <= 1);
                Assert.Equal(150.0m, payment.Monto);
                Assert.Equal("Aprobado", payment.Estado);
                Assert.Equal("REF3", payment.ReferenciaExterna);
                Assert.Equal(202408, payment.Periodo);
                Assert.Equal("Nuevo pago", payment.Observaciones);

                var dbPayment = await context.MembershipPayments.FindAsync(payment.Id);
                Assert.NotNull(dbPayment);
                Assert.Equal(1, dbPayment.CustomerMembershipId);
                Assert.Equal(150.0m, dbPayment.Monto);
                Assert.Equal("Aprobado", dbPayment.Estado);
                Assert.Equal("REF3", dbPayment.ReferenciaExterna);
                Assert.Equal(202408, dbPayment.Periodo);
                Assert.Equal("Nuevo pago", dbPayment.Observaciones);
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
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 999,
                    FechaPago = DateTime.UtcNow,
                    Monto = 150.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF4",
                    Periodo = 202409,
                    Observaciones = "Pago fallido"
                };

                // Act
                var result = await controller.CreatePayment(dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("La membresía especificada no existe o está inactiva.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreatePayment retorne BadRequest si la membresía está inactiva.
        /// </summary>
        [Fact]
        public async Task CreatePayment_ReturnsBadRequest_WhenMembershipIsInactive()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "INACTIVO"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    FechaPago = DateTime.UtcNow,
                    Monto = 150.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF4",
                    Periodo = 202409,
                    Observaciones = "Pago fallido"
                };

                // Act
                var result = await controller.CreatePayment(dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("La membresía especificada no existe o está inactiva.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreatePayment retorne BadRequest si el periodo es inválido.
        /// </summary>
        [Fact]
        public async Task CreatePayment_ReturnsBadRequest_WhenPeriodoIsInvalid()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    FechaPago = DateTime.UtcNow,
                    Monto = 150.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF5",
                    Periodo = 202513, // Periodo inválido (mes > 12)
                    Observaciones = "Pago fallido"
                };

                // Act
                var result = await controller.CreatePayment(dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El Periodo debe estar en formato yyyyMM (ej. 202506).", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreatePayment retorne BadRequest si la fecha de pago es futura.
        /// </summary>
        [Fact]
        public async Task CreatePayment_ReturnsBadRequest_WhenFechaPagoIsFuture()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    FechaPago = DateTime.UtcNow.AddDays(1),
                    Monto = 150.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF5",
                    Periodo = 202408,
                    Observaciones = "Pago fallido"
                };

                // Act
                var result = await controller.CreatePayment(dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("La FechaPago no puede ser futura.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreatePayment retorne Conflict si el periodo está duplicado.
        /// </summary>
        [Fact]
        public async Task CreatePayment_ReturnsConflict_WhenPeriodoIsDuplicated()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 1,
                        CustomerMembershipId = 1,
                        FechaPago = DateTime.UtcNow,
                        Monto = 100.0m,
                        Estado = "Aprobado",
                        ReferenciaExterna = "REF1",
                        Periodo = 202408,
                        Observaciones = "Pago inicial"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    FechaPago = DateTime.UtcNow,
                    Monto = 150.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF5",
                    Periodo = 202408,
                    Observaciones = "Pago duplicado"
                };

                // Act
                var result = await controller.CreatePayment(dto);

                // Assert
                var conflictResult = Assert.IsType<ConflictObjectResult>(result);
                Assert.Equal(409, conflictResult.StatusCode);

                var response = conflictResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Ya existe un pago para este período y membresía.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreatePayment retorne Conflict si la referencia externa está duplicada.
        /// </summary>
        [Fact]
        public async Task CreatePayment_ReturnsConflict_WhenReferenciaExternaIsDuplicated()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 1,
                        CustomerMembershipId = 1,
                        FechaPago = DateTime.UtcNow,
                        Monto = 100.0m,
                        Estado = "Aprobado",
                        ReferenciaExterna = "REF5",
                        Periodo = 202408,
                        Observaciones = "Pago inicial"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    FechaPago = DateTime.UtcNow,
                    Monto = 150.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF5",
                    Periodo = 202409,
                    Observaciones = "Pago duplicado"
                };

                // Act
                var result = await controller.CreatePayment(dto);

                // Assert
                var conflictResult = Assert.IsType<ConflictObjectResult>(result);
                Assert.Equal(409, conflictResult.StatusCode);

                var response = conflictResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Ya existe un pago con esta referencia externa.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdatePayment actualice un pago existente.
        /// </summary>
        [Fact]
        public async Task UpdatePayment_UpdatesPayment()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaPago = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 5,
                        CustomerMembershipId = 1,
                        FechaPago = fechaPago,
                        Monto = 100.0m,
                        Estado = "Pendiente",
                        ReferenciaExterna = "REF5",
                        Periodo = 202410,
                        Observaciones = "Pago en revisión"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    FechaPago = fechaPago,
                    Monto = 120.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF5-EDIT",
                    Periodo = 202411,
                    Observaciones = "Pago actualizado"
                };

                // Act
                var result = await controller.UpdatePayment(5, dto);

                // Assert
                var noContentResult = Assert.IsType<NoContentResult>(result);
                Assert.Equal(204, noContentResult.StatusCode);

                var payment = await context.MembershipPayments.FindAsync(5);
                Assert.Equal(1, payment.CustomerMembershipId);
                Assert.True(Math.Abs((dto.FechaPago - payment.FechaPago).TotalSeconds) <= 1);
                Assert.Equal(120.0m, payment.Monto);
                Assert.Equal("Aprobado", payment.Estado);
                Assert.Equal("REF5-EDIT", payment.ReferenciaExterna);
                Assert.Equal(202411, payment.Periodo);
                Assert.Equal("Pago actualizado", payment.Observaciones);
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
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 2,
                        CodCliente = 2,
                        PlanId = 2,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 2,
                    FechaPago = DateTime.UtcNow,
                    Monto = 200.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF6",
                    Periodo = 202412,
                    Observaciones = "Nuevo pago"
                };

                // Act
                var result = await controller.UpdatePayment(999, dto);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El pago no existe.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdatePayment retorne BadRequest si el periodo es inválido.
        /// </summary>
        [Fact]
        public async Task UpdatePayment_ReturnsBadRequest_WhenPeriodoIsInvalid()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 5,
                        CustomerMembershipId = 1,
                        FechaPago = DateTime.UtcNow,
                        Monto = 100.0m,
                        Estado = "Pendiente",
                        ReferenciaExterna = "REF5",
                        Periodo = 202410,
                        Observaciones = "Pago en revisión"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    FechaPago = DateTime.UtcNow,
                    Monto = 120.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF5-EDIT",
                    Periodo = 202513, // Periodo inválido
                    Observaciones = "Pago actualizado"
                };

                // Act
                var result = await controller.UpdatePayment(5, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El Periodo debe estar en formato yyyyMM (ej. 202506).", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdatePayment retorne BadRequest si la fecha de pago es futura.
        /// </summary>
        [Fact]
        public async Task UpdatePayment_ReturnsBadRequest_WhenFechaPagoIsFuture()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 5,
                        CustomerMembershipId = 1,
                        FechaPago = DateTime.UtcNow,
                        Monto = 100.0m,
                        Estado = "Pendiente",
                        ReferenciaExterna = "REF5",
                        Periodo = 202410,
                        Observaciones = "Pago en revisión"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    FechaPago = DateTime.UtcNow.AddDays(1),
                    Monto = 120.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF5-EDIT",
                    Periodo = 202411,
                    Observaciones = "Pago actualizado"
                };

                // Act
                var result = await controller.UpdatePayment(5, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("La FechaPago no puede ser futura.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdatePayment retorne Conflict si el periodo está duplicado.
        /// </summary>
        [Fact]
        public async Task UpdatePayment_ReturnsConflict_WhenPeriodoIsDuplicated()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 5,
                        CustomerMembershipId = 1,
                        FechaPago = DateTime.UtcNow,
                        Monto = 100.0m,
                        Estado = "Pendiente",
                        ReferenciaExterna = "REF5",
                        Periodo = 202410,
                        Observaciones = "Pago en revisión"
                    });
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 6,
                        CustomerMembershipId = 1,
                        FechaPago = DateTime.UtcNow,
                        Monto = 150.0m,
                        Estado = "Aprobado",
                        ReferenciaExterna = "REF6",
                        Periodo = 202411,
                        Observaciones = "Otro pago"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    FechaPago = DateTime.UtcNow,
                    Monto = 120.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF5-EDIT",
                    Periodo = 202411,
                    Observaciones = "Pago actualizado"
                };

                // Act
                var result = await controller.UpdatePayment(5, dto);

                // Assert
                var conflictResult = Assert.IsType<ConflictObjectResult>(result);
                Assert.Equal(409, conflictResult.StatusCode);

                var response = conflictResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Ya existe un pago para este período y membresía.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdatePayment retorne Conflict si la referencia externa está duplicada.
        /// </summary>
        [Fact]
        public async Task UpdatePayment_ReturnsConflict_WhenReferenciaExternaIsDuplicated()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 1,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "ACTIVO"
                    });
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 5,
                        CustomerMembershipId = 1,
                        FechaPago = DateTime.UtcNow,
                        Monto = 100.0m,
                        Estado = "Pendiente",
                        ReferenciaExterna = "REF5",
                        Periodo = 202410,
                        Observaciones = "Pago en revisión"
                    });
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 6,
                        CustomerMembershipId = 1,
                        FechaPago = DateTime.UtcNow,
                        Monto = 150.0m,
                        Estado = "Aprobado",
                        ReferenciaExterna = "REF6",
                        Periodo = 202411,
                        Observaciones = "Otro pago"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);
                var dto = new CreateMembershipPaymentDto
                {
                    CustomerMembershipId = 1,
                    FechaPago = DateTime.UtcNow,
                    Monto = 120.0m,
                    Estado = "Aprobado",
                    ReferenciaExterna = "REF6",
                    Periodo = 202412,
                    Observaciones = "Pago actualizado"
                };

                // Act
                var result = await controller.UpdatePayment(5, dto);

                // Assert
                var conflictResult = Assert.IsType<ConflictObjectResult>(result);
                Assert.Equal(409, conflictResult.StatusCode);

                var response = conflictResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Ya existe un pago con esta nueva referencia externa.", errorProperty.GetValue(response));
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
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPayments.Add(new MembershipPayment
                    {
                        Id = 9,
                        CustomerMembershipId = 1,
                        FechaPago = DateTime.UtcNow,
                        Monto = 90.0m,
                        Estado = "Aprobado",
                        ReferenciaExterna = "REF9",
                        Periodo = 202413,
                        Observaciones = "Pago final"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);

                // Act
                var result = await controller.DeletePayment(9);

                // Assert
                var noContentResult = Assert.IsType<NoContentResult>(result);
                Assert.Equal(204, noContentResult.StatusCode);

                var payment = await context.MembershipPayments.FindAsync(9);
                Assert.Null(payment);
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
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPaymentsController(context);

                // Act
                var result = await controller.DeletePayment(999);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El pago no existe.", errorProperty.GetValue(response));
            }
        }
    }
}
