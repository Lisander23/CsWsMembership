using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoyaltyApi.Controllers;
using LoyaltyApi.Entities;
using LoyaltyApi.Data;
using System.Linq;
using LoyaltyApi.Models;

namespace LoyaltyApi.Tests
{
    public class EntryUsagesControllerTests
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
        /// Verifica que GetEntryUsages retorne los usos asociados a un balance específico.
        /// </summary>
        [Fact]
        public async Task GetEntryUsages_ReturnsUsagesForBalance()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaUso = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership { Id = 1, Estado = "ACTIVO" });
                    ctx.EntryBalances.Add(new EntryBalance
                    {
                        Id = 100,
                        CustomerMembershipId = 1,
                        Periodo = 202406,
                        EntradasAsignadas = 5,
                        EntradasUsadas = 2,
                        FechaVencimiento = DateTime.UtcNow.AddMonths(1)
                    });
                    ctx.EntryUsages.Add(new EntryUsage
                    {
                        Id = 1,
                        EntryBalanceId = 100,
                        FechaUso = fechaUso,
                        CodComplejo = 1001.50m,
                        CodFuncion = 5001,
                        IdEntrada = 12345
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryUsagesController(context);

                // Act
                var result = await controller.GetEntryUsages(100);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, okResult.StatusCode);

                var response = okResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var usages = Assert.IsAssignableFrom<IEnumerable<EntryUsageDto>>(dataProperty.GetValue(response));
                var usageList = usages.ToList();
                Assert.Single(usageList);

                var usage = usageList.First();
                Assert.Equal(1, usage.Id);
                Assert.Equal(100, usage.EntryBalanceId);
                Assert.True(Math.Abs((fechaUso - usage.FechaUso).TotalSeconds) <= 1);
                Assert.Equal(1001.50m, usage.CodComplejo);
                Assert.Equal(5001, usage.CodFuncion);
                Assert.Equal(12345, usage.IdEntrada);
            }
        }

        /// <summary>
        /// Verifica que GetEntryUsages retorne NotFound si el balance no existe.
        /// </summary>
        [Fact]
        public async Task GetEntryUsages_ReturnsNotFound_WhenBalanceDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryUsagesController(context);

                // Act
                var result = await controller.GetEntryUsages(999);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El saldo no existe.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que GetEntryUsages retorne BadRequest si la membresía está inactiva.
        /// </summary>
        [Fact]
        public async Task GetEntryUsages_ReturnsBadRequest_WhenMembershipIsInactive()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership { Id = 1, Estado = "INACTIVO" });
                    ctx.EntryBalances.Add(new EntryBalance
                    {
                        Id = 100,
                        CustomerMembershipId = 1,
                        Periodo = 202406,
                        EntradasAsignadas = 5,
                        EntradasUsadas = 2,
                        FechaVencimiento = DateTime.UtcNow.AddMonths(1)
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryUsagesController(context);

                // Act
                var result = await controller.GetEntryUsages(100);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("La membresía asociada no existe o está inactiva.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreateEntryUsage cree un nuevo uso y actualice el contador de entradas usadas.
        /// </summary>
        [Fact]
        public async Task CreateEntryUsage_CreatesUsageAndUpdatesBalance()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaUso = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership { Id = 1, Estado = "ACTIVO" });
                    ctx.EntryBalances.Add(new EntryBalance
                    {
                        Id = 200,
                        CustomerMembershipId = 1,
                        Periodo = 202406,
                        EntradasAsignadas = 5,
                        EntradasUsadas = 2,
                        FechaVencimiento = DateTime.UtcNow.AddMonths(1)
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryUsagesController(context);
                var dto = new CreateEntryUsageDto
                {
                    FechaUso = fechaUso,
                    CodComplejo = 1002.75m,
                    CodFuncion = 5002,
                    IdEntrada = 12346
                };

                // Act
                var result = await controller.CreateEntryUsage(200, dto);

                // Assert
                var createdResult = Assert.IsType<CreatedAtActionResult>(result);
                Assert.Equal(201, createdResult.StatusCode);

                var response = createdResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var usage = Assert.IsType<EntryUsageDto>(dataProperty.GetValue(response));
                Assert.Equal(200, usage.EntryBalanceId);
                Assert.True(Math.Abs((dto.FechaUso - usage.FechaUso).TotalSeconds) <= 1);
                Assert.Equal(1002.75m, usage.CodComplejo);
                Assert.Equal(5002, usage.CodFuncion);
                Assert.Equal(12346, usage.IdEntrada);

                // Verifica que el contador de entradas usadas se haya incrementado
                var balance = await context.EntryBalances.FindAsync(200);
                Assert.Equal(3, balance.EntradasUsadas);
            }
        }

        /// <summary>
        /// Verifica que CreateEntryUsage retorne NotFound si el balance no existe.
        /// </summary>
        [Fact]
        public async Task CreateEntryUsage_ReturnsNotFound_WhenBalanceDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryUsagesController(context);
                var dto = new CreateEntryUsageDto
                {
                    FechaUso = DateTime.UtcNow,
                    CodComplejo = 1003.25m,
                    CodFuncion = 5003,
                    IdEntrada = 12347
                };

                // Act
                var result = await controller.CreateEntryUsage(999, dto);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El saldo no existe.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreateEntryUsage retorne BadRequest si la membresía está inactiva.
        /// </summary>
        [Fact]
        public async Task CreateEntryUsage_ReturnsBadRequest_WhenMembershipIsInactive()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership { Id = 1, Estado = "INACTIVO" });
                    ctx.EntryBalances.Add(new EntryBalance
                    {
                        Id = 200,
                        CustomerMembershipId = 1,
                        Periodo = 202406,
                        EntradasAsignadas = 5,
                        EntradasUsadas = 2,
                        FechaVencimiento = DateTime.UtcNow.AddMonths(1)
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryUsagesController(context);
                var dto = new CreateEntryUsageDto
                {
                    FechaUso = DateTime.UtcNow,
                    CodComplejo = 1003.25m,
                    CodFuncion = 5003,
                    IdEntrada = 12347
                };

                // Act
                var result = await controller.CreateEntryUsage(200, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("La membresía asociada no existe o está inactiva.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreateEntryUsage retorne BadRequest si el saldo está vencido.
        /// </summary>
        [Fact]
        public async Task CreateEntryUsage_ReturnsBadRequest_WhenBalanceIsExpired()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership { Id = 1, Estado = "ACTIVO" });
                    ctx.EntryBalances.Add(new EntryBalance
                    {
                        Id = 200,
                        CustomerMembershipId = 1,
                        Periodo = 202406,
                        EntradasAsignadas = 5,
                        EntradasUsadas = 2,
                        FechaVencimiento = DateTime.UtcNow.AddDays(-1)
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryUsagesController(context);
                var dto = new CreateEntryUsageDto
                {
                    FechaUso = DateTime.UtcNow,
                    CodComplejo = 1003.25m,
                    CodFuncion = 5003,
                    IdEntrada = 12347
                };

                // Act
                var result = await controller.CreateEntryUsage(200, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El saldo está vencido.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreateEntryUsage retorne BadRequest si no hay entradas disponibles.
        /// </summary>
        [Fact]
        public async Task CreateEntryUsage_ReturnsBadRequest_WhenNoEntriesAvailable()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership { Id = 1, Estado = "ACTIVO" });
                    ctx.EntryBalances.Add(new EntryBalance
                    {
                        Id = 200,
                        CustomerMembershipId = 1,
                        Periodo = 202406,
                        EntradasAsignadas = 5,
                        EntradasUsadas = 5,
                        FechaVencimiento = DateTime.UtcNow.AddMonths(1)
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryUsagesController(context);
                var dto = new CreateEntryUsageDto
                {
                    FechaUso = DateTime.UtcNow,
                    CodComplejo = 1003.25m,
                    CodFuncion = 5003,
                    IdEntrada = 12347
                };

                // Act
                var result = await controller.CreateEntryUsage(200, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("No hay entradas disponibles en este saldo.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreateEntryUsage retorne BadRequest si FechaUso es futura.
        /// </summary>
        [Fact]
        public async Task CreateEntryUsage_ReturnsBadRequest_WhenFechaUsoIsFuture()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership { Id = 1, Estado = "ACTIVO" });
                    ctx.EntryBalances.Add(new EntryBalance
                    {
                        Id = 200,
                        CustomerMembershipId = 1,
                        Periodo = 202406,
                        EntradasAsignadas = 5,
                        EntradasUsadas = 2,
                        FechaVencimiento = DateTime.UtcNow.AddMonths(1)
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryUsagesController(context);
                var dto = new CreateEntryUsageDto
                {
                    FechaUso = DateTime.UtcNow.AddDays(1),
                    CodComplejo = 1003.25m,
                    CodFuncion = 5003,
                    IdEntrada = 12347
                };

                // Act
                var result = await controller.CreateEntryUsage(200, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("La FechaUso no puede ser futura.", errorProperty.GetValue(response));
            }
        }
    }
}
