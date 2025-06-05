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
    public class EntryUsagesControllerTests
    {
        private DbContextOptions<LoyaltyContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<LoyaltyContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        /// <summary>
        /// Verifica que GetEntryUsages retorne los usos asociados a un balance específico.
        /// </summary>
        [Fact]
        public async Task GetEntryUsages_ReturnsUsagesForBalance()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.EntryUsages.Add(new EntryUsage
                {
                    Id = 1,
                    EntryBalanceId = 100,
                    FechaUso = DateTime.UtcNow,
                    CodComplejo = 1,
                    CodFuncion = 10,
                    IdEntrada = 1000
                });
                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryUsagesController(context);

                // Act
                var result = await controller.GetEntryUsages(100);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var usages = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
                Assert.Single(usages);
            }
        }

        /// <summary>
        /// Verifica que CreateEntryUsage cree un nuevo uso de entrada y actualice el contador de entradas usadas.
        /// </summary>
        [Fact]
        public async Task CreateEntryUsage_CreatesUsageAndUpdatesBalance()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.EntryBalances.Add(new EntryBalance
                {
                    Id = 200,
                    CustomerMembershipId = 1,
                    Periodo = 202406,
                    EntradasAsignadas = 5,
                    EntradasUsadas = 2,
                    FechaVencimiento = DateTime.UtcNow.AddMonths(1)
                });
                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new EntryUsagesController(context);
                var dto = new EntryUsagesController.EntryUsageDto
                {
                    FechaUso = DateTime.UtcNow,
                    CodComplejo = 2,
                    CodFuncion = 20,
                    IdEntrada = 2000
                };

                // Act
                var result = await controller.CreateEntryUsage(200, dto);

                // Assert
                var createdResult = Assert.IsType<CreatedAtActionResult>(result);
                var usage = Assert.IsType<EntryUsage>(createdResult.Value);
                Assert.Equal(200, usage.EntryBalanceId);

                // Verifica que el contador de entradas usadas se haya incrementado
                var balance = context.EntryBalances.Find(200);
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
            using var context = new LoyaltyContext(options);
            var controller = new EntryUsagesController(context);
            var dto = new EntryUsagesController.EntryUsageDto
            {
                FechaUso = DateTime.UtcNow,
                CodComplejo = 3,
                CodFuncion = 30,
                IdEntrada = 3000
            };

            // Act
            var result = await controller.CreateEntryUsage(999, dto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
