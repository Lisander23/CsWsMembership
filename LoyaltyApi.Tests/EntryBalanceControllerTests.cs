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

        /// <summary>
        /// Verifica que GetEntryBalances retorne los balances asociados a un ID de membresía específico.
        /// </summary>
        [Fact]
        public async Task GetEntryBalances_ReturnsBalancesForMembership()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.EntryBalances.Add(new EntryBalance
                {
                    Id = 1,
                    CustomerMembershipId = 10,
                    Periodo = 202401,
                    EntradasAsignadas = 5,
                    EntradasUsadas = 2,
                    FechaVencimiento = DateTime.UtcNow.AddMonths(1)
                });
                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryBalancesController(context);

                // Act
                var result = await controller.GetEntryBalances(10);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var balances = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
                Assert.Single(balances);
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
            using var context = new LoyaltyContext(options);
            var controller = new EntryBalancesController(context);

            var dto = new EntryBalancesController.EntryBalanceDto
            {
                Periodo = 202405,
                EntradasAsignadas = 10,
                FechaVencimiento = DateTime.UtcNow.AddMonths(2)
            };

            // Act
            var result = await controller.CreateEntryBalance(20, dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var entry = Assert.IsType<EntryBalance>(createdResult.Value);
            Assert.Equal(20, entry.CustomerMembershipId);
            Assert.Equal(10, entry.EntradasAsignadas);
            Assert.Equal(0, entry.EntradasUsadas);
        }

        /// <summary>
        /// Verifica que UpdateBalance actualice los campos de un balance existente y retorne NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateBalance_UpdatesFieldsAndReturnsNoContent()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.EntryBalances.Add(new EntryBalance
                {
                    Id = 2,
                    CustomerMembershipId = 30,
                    Periodo = 202406,
                    EntradasAsignadas = 3,
                    EntradasUsadas = 1,
                    FechaVencimiento = DateTime.UtcNow.AddMonths(1)
                });
                context.SaveChanges();
            }

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
                var result = await controller.UpdateBalance(2, dto);

                // Assert
                Assert.IsType<NoContentResult>(result);
                var updated = context.EntryBalances.Find(2);
                Assert.Equal(7, updated.EntradasAsignadas);
                Assert.Equal(2, updated.EntradasUsadas);
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
            using var context = new LoyaltyContext(options);
            var controller = new EntryBalancesController(context);
            var dto = new EntryBalancesController.UpdateBalanceDto();

            // Act
            var result = await controller.UpdateBalance(999, dto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
