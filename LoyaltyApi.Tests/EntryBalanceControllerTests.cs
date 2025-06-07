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
    public class EntryBalanceControllerTests
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
        /// Verifica que GetEntryBalances retorne los balances asociados a un ID de membresía específico.
        /// </summary>
        [Fact]
        public async Task GetEntryBalances_ReturnsBalancesForMembership()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaVencimiento = DateTime.UtcNow.AddMonths(1);
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 10,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "Activo"
                    });
                    ctx.EntryBalances.Add(new EntryBalance
                    {
                        Id = 1,
                        CustomerMembershipId = 10,
                        Periodo = 202401,
                        EntradasAsignadas = 5,
                        EntradasUsadas = 2,
                        FechaVencimiento = fechaVencimiento
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryBalancesController(context);

                // Act
                var result = await controller.GetEntryBalances(10);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, okResult.StatusCode);

                var response = okResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var balances = Assert.IsAssignableFrom<IEnumerable<object>>(dataProperty.GetValue(response));
                var balanceList = balances.ToList();
                Assert.Single(balanceList);

                var balance = balanceList.First();
                var balanceProps = balance.GetType().GetProperties()
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(balance));
                Assert.Equal(1, balanceProps["Id"]);
                Assert.Equal(202401, balanceProps["Periodo"]);
                Assert.Equal(5, balanceProps["EntradasAsignadas"]);
                Assert.Equal(2, balanceProps["EntradasUsadas"]);
                Assert.True(Math.Abs((fechaVencimiento - (DateTime)balanceProps["FechaVencimiento"]).TotalSeconds) <= 1);
            }
        }

        /// <summary>
        /// Verifica que GetEntryBalances retorne NotFound si la membresía no existe.
        /// </summary>
        [Fact]
        public async Task GetEntryBalances_ReturnsNotFound_WhenMembershipDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryBalancesController(context);

                // Act
                var result = await controller.GetEntryBalances(999);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("La membresía especificada no existe.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreateEntryBalance cree un nuevo balance de entradas y lo retorne correctamente.
        /// </summary>
        [Fact]
        public async Task CreateEntryBalance_CreatesAndReturnsEntry()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaVencimiento = DateTime.UtcNow.AddMonths(2);
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 20,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "Activo"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryBalancesController(context);
                var dto = new EntryBalancesController.EntryBalanceDto
                {
                    Periodo = 202405,
                    EntradasAsignadas = 10,
                    FechaVencimiento = fechaVencimiento
                };

                // Act
                var result = await controller.CreateEntryBalance(20, dto);

                // Assert
                var createdResult = Assert.IsType<CreatedAtActionResult>(result);
                Assert.Equal(201, createdResult.StatusCode);

                var response = createdResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var entry = Assert.IsType<EntryBalance>(dataProperty.GetValue(response));
                Assert.Equal(20, entry.CustomerMembershipId);
                Assert.Equal(202405, entry.Periodo);
                Assert.Equal(10, entry.EntradasAsignadas);
                Assert.Equal(0, entry.EntradasUsadas);
                Assert.True(Math.Abs((fechaVencimiento - entry.FechaVencimiento).TotalSeconds) <= 1);

                var dbEntry = await context.EntryBalances.FindAsync(entry.Id);
                Assert.NotNull(dbEntry);
                Assert.Equal(20, dbEntry.CustomerMembershipId);
                Assert.Equal(202405, dbEntry.Periodo);
                Assert.Equal(10, dbEntry.EntradasAsignadas);
                Assert.Equal(0, dbEntry.EntradasUsadas);
            }
        }

        /// <summary>
        /// Verifica que CreateEntryBalance retorne BadRequest si la membresía no existe.
        /// </summary>
        [Fact]
        public async Task CreateEntryBalance_ReturnsBadRequest_WhenMembershipDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryBalancesController(context);
                var dto = new EntryBalancesController.EntryBalanceDto
                {
                    Periodo = 202405,
                    EntradasAsignadas = 10,
                    FechaVencimiento = DateTime.UtcNow.AddMonths(2)
                };

                // Act
                var result = await controller.CreateEntryBalance(999, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("La membresía especificada no existe.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreateEntryBalance retorne BadRequest si EntradasAsignadas es negativo.
        /// </summary>
        [Fact]
        public async Task CreateEntryBalance_ReturnsBadRequest_WhenEntradasAsignadasIsNegative()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 20,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "Activo"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryBalancesController(context);
                var dto = new EntryBalancesController.EntryBalanceDto
                {
                    Periodo = 202405,
                    EntradasAsignadas = -10,
                    FechaVencimiento = DateTime.UtcNow.AddMonths(2)
                };

                // Act
                var result = await controller.CreateEntryBalance(20, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El número de entradas asignadas no puede ser negativo.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreateEntryBalance retorne BadRequest si Periodo es inválido.
        /// </summary>
        [Fact]
        public async Task CreateEntryBalance_ReturnsBadRequest_WhenPeriodoIsInvalid()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 20,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "Activo"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryBalancesController(context);
                var dto = new EntryBalancesController.EntryBalanceDto
                {
                    Periodo = 0,
                    EntradasAsignadas = 10,
                    FechaVencimiento = DateTime.UtcNow.AddMonths(2)
                };

                // Act
                var result = await controller.CreateEntryBalance(20, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El período debe ser mayor a cero.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdateBalance actualice los campos de un balance existente y retorne NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateBalance_UpdatesFieldsAndReturnsNoContent()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaVencimientoOriginal = DateTime.UtcNow.AddMonths(1);
            var fechaVencimientoNueva = DateTime.UtcNow.AddMonths(3);
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.CustomerMemberships.Add(new CustomerMembership
                    {
                        Id = 30,
                        CodCliente = 1,
                        PlanId = 1,
                        FechaInicio = DateTime.UtcNow,
                        Estado = "Activo"
                    });
                    ctx.EntryBalances.Add(new EntryBalance
                    {
                        Id = 2,
                        CustomerMembershipId = 30,
                        Periodo = 202406,
                        EntradasAsignadas = 3,
                        EntradasUsadas = 1,
                        FechaVencimiento = fechaVencimientoOriginal
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryBalancesController(context);
                var dto = new EntryBalancesController.UpdateBalanceDto
                {
                    EntradasAsignadas = 7,
                    EntradasUsadas = 2,
                    FechaVencimiento = fechaVencimientoNueva
                };

                // Act
                var result = await controller.UpdateBalance(2, dto);

                // Assert
                var noContentResult = Assert.IsType<NoContentResult>(result);
                Assert.Equal(204, noContentResult.StatusCode);

                var updated = await context.EntryBalances.FindAsync(2);
                Assert.Equal(7, updated.EntradasAsignadas);
                Assert.Equal(2, updated.EntradasUsadas);
                Assert.True(Math.Abs((fechaVencimientoNueva - updated.FechaVencimiento).TotalSeconds) <= 1);
            }
        }

        /// <summary>
        /// Verifica que UpdateBalance retorne NotFound cuando el balance no existe.
        /// </summary>
        [Fact]
        public async Task UpdateBalance_ReturnsNotFound_WhenBalanceDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryBalancesController(context);
                var dto = new EntryBalancesController.UpdateBalanceDto
                {
                    EntradasAsignadas = 7,
                    EntradasUsadas = 2,
                    FechaVencimiento = DateTime.UtcNow.AddMonths(3)
                };

                // Act
                var result = await controller.UpdateBalance(999, dto);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El saldo especificado no existe.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdateBalance retorne BadRequest si EntradasAsignadas es negativo.
        /// </summary>
        [Fact]
        public async Task UpdateBalance_ReturnsBadRequest_WhenEntradasAsignadasIsNegative()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.EntryBalances.Add(new EntryBalance
                    {
                        Id = 2,
                        CustomerMembershipId = 30,
                        Periodo = 202406,
                        EntradasAsignadas = 3,
                        EntradasUsadas = 1,
                        FechaVencimiento = DateTime.UtcNow.AddMonths(1)
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryBalancesController(context);
                var dto = new EntryBalancesController.UpdateBalanceDto
                {
                    EntradasAsignadas = -7
                };

                // Act
                var result = await controller.UpdateBalance(2, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Las entradas asignadas no pueden ser negativas.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdateBalance retorne BadRequest si EntradasUsadas es negativo.
        /// </summary>
        [Fact]
        public async Task UpdateBalance_ReturnsBadRequest_WhenEntradasUsadasIsNegative()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.EntryBalances.Add(new EntryBalance
                    {
                        Id = 2,
                        CustomerMembershipId = 30,
                        Periodo = 202406,
                        EntradasAsignadas = 3,
                        EntradasUsadas = 1,
                        FechaVencimiento = DateTime.UtcNow.AddMonths(1)
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryBalancesController(context);
                var dto = new EntryBalancesController.UpdateBalanceDto
                {
                    EntradasUsadas = -2
                };

                // Act
                var result = await controller.UpdateBalance(2, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Las entradas usadas no pueden ser negativas.", errorProperty.GetValue(response));
            }
        }
    }
}
