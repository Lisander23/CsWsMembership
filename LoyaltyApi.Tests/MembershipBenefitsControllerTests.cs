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
    public class MembershipBenefitsControllerTests
    {
        private DbContextOptions<LoyaltyContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<LoyaltyContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        /// <summary>
        /// Verifica que GetBenefits retorne la lista de beneficios existentes.
        /// </summary>
        [Fact]
        public async Task GetBenefits_ReturnsListOfBenefits()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipBenefits.Add(new MembershipBenefit
                {
                    Id = 1,
                    PlanId = 1,
                    Clave = "BEN1",
                    Valor = 10.5m,
                    DiasAplicables = "Lunes",
                    Observacion = "Obs1"
                });
                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);

                // Act
                var result = await controller.GetBenefits();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result.ExecuteResultAsync);
                var benefits = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
                Assert.Single(benefits);
            }
        }

        /// <summary>
        /// Verifica que GetBenefit retorne un beneficio existente por ID.
        /// </summary>
        [Fact]
        public async Task GetBenefit_ReturnsBenefitById()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipBenefits.Add(new MembershipBenefit
                {
                    Id = 2,
                    PlanId = 2,
                    Clave = "BEN2",
                    Valor = 20.0m,
                    DiasAplicables = "Martes",
                    Observacion = "Obs2"
                });
                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);

                // Act
                var result = await controller.GetBenefit(2);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result.ExecuteResultAsync);
                Assert.NotNull(okResult.Value);
            }
        }

        /// <summary>
        /// Verifica que GetBenefit retorne NotFound si el beneficio no existe.
        /// </summary>
        [Fact]
        public async Task GetBenefit_ReturnsNotFound_WhenBenefitDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new LoyaltyContext(options);
            var controller = new MembershipBenefitsController(context);

            // Act
            var result = await controller.GetBenefit(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.ExecuteResultAsync);
        }

        /// <summary>
        /// Verifica que CreateBenefit cree un nuevo beneficio si el plan existe y está activo.
        /// </summary>
        [Fact]
        public async Task CreateBenefit_CreatesBenefit_WhenPlanIsActive()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 3,
                    Nombre = "Plan Test",
                    PrecioMensual = 100.0m,
                    EntradasMensuales = 1,
                    Nivel = 1,
                    Activo = true
                });


                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);
                var dto = new LoyaltyApi.Models.CreateMembershipBenefitDto
                {
                    PlanId = 3,
                    Clave = "BEN3",
                    Valor = 30.0m,
                    DiasAplicables = "Miércoles",
                    Observacion = "Obs3"
                };

                // Act
                var result = await controller.CreateBenefit(dto);

                // Assert
                var createdResult = Assert.IsType<CreatedAtActionResult>(result.ExecuteResultAsync);
                Assert.NotNull(createdResult.Value);
            }
        }

        /// <summary>
        /// Verifica que CreateBenefit retorne BadRequest si el plan no existe o está inactivo.
        /// </summary>
        [Fact]
        public async Task CreateBenefit_ReturnsBadRequest_WhenPlanIsInactiveOrNotFound()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new LoyaltyContext(options);
            var controller = new MembershipBenefitsController(context);
            var dto = new LoyaltyApi.Models.CreateMembershipBenefitDto
            {
                PlanId = 999,
                Clave = "BEN4",
                Valor = 40.0m,
                DiasAplicables = "Jueves",
                Observacion = "Obs4"
            };

            // Act
            var result = await controller.CreateBenefit(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.ExecuteResultAsync);
        }

        /// <summary>
        /// Verifica que UpdateBenefit actualice un beneficio existente si el plan es válido.
        /// </summary>
        [Fact]
        public async Task UpdateBenefit_UpdatesBenefit_WhenPlanIsActive()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipBenefits.Add(new MembershipBenefit
                {
                    Id = 5,
                    PlanId = 5,
                    Clave = "BEN5",
                    Valor = 50.0m,
                    DiasAplicables = "Viernes",
                    Observacion = "Obs5"
                });
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 6,
                    Nombre = "Plan Test",
                    PrecioMensual = 100.0m,
                    EntradasMensuales = 1,
                    Nivel = 1,
                    Activo = true
                });

                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);
                var dto = new LoyaltyApi.Models.CreateMembershipBenefitDto
                {
                    PlanId = 6,
                    Clave = "BEN5-EDIT",
                    Valor = 55.0m,
                    DiasAplicables = "Sábado",
                    Observacion = "Obs5-EDIT"
                };

                // Act
                var result = await controller.UpdateBenefit(5, dto);

                // Assert
                Assert.IsType<NoContentResult>(result);
            }
        }

        /// <summary>
        /// Verifica que UpdateBenefit retorne NotFound si el beneficio no existe.
        /// </summary>
        [Fact]
        public async Task UpdateBenefit_ReturnsNotFound_WhenBenefitDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 7,
                    Nombre = "Plan Test",
                    PrecioMensual = 100.0m,
                    EntradasMensuales = 1,
                    Nivel = 1,
                    Activo = true
                });

                context.SaveChanges();
            }
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);
                var dto = new LoyaltyApi.Models.CreateMembershipBenefitDto
                {
                    PlanId = 7,
                    Clave = "BEN6",
                    Valor = 60.0m,
                    DiasAplicables = "Domingo",
                    Observacion = "Obs6"
                };

                // Act
                var result = await controller.UpdateBenefit(999, dto);

                // Assert
                Assert.IsType<NotFoundObjectResult>(result);
            }
        }

        /// <summary>
        /// Verifica que UpdateBenefit retorne BadRequest si el plan es inválido o inactivo.
        /// </summary>
        [Fact]
        public async Task UpdateBenefit_ReturnsBadRequest_WhenPlanIsInactiveOrNotFound()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipBenefits.Add(new MembershipBenefit
                {
                    Id = 8,
                    PlanId = 8,
                    Clave = "BEN8",
                    Valor = 80.0m,
                    DiasAplicables = "Lunes",
                    Observacion = "Obs8"
                });
                context.SaveChanges();
            }
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);
                var dto = new LoyaltyApi.Models.CreateMembershipBenefitDto
                {
                    PlanId = 999,
                    Clave = "BEN8-EDIT",
                    Valor = 88.0m,
                    DiasAplicables = "Martes",
                    Observacion = "Obs8-EDIT"
                };

                // Act
                var result = await controller.UpdateBenefit(8, dto);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        /// <summary>
        /// Verifica que DeleteBenefit elimine un beneficio existente.
        /// </summary>
        [Fact]
        public async Task DeleteBenefit_DeletesBenefit()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipBenefits.Add(new MembershipBenefit
                {
                    Id = 9,
                    PlanId = 9,
                    Clave = "BEN9",
                    Valor = 90.0m,
                    DiasAplicables = "Miércoles",
                    Observacion = "Obs9"
                });
                context.SaveChanges();
            }
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);

                // Act
                var result = await controller.DeleteBenefit(9);

                // Assert
                Assert.IsType<NoContentResult>(result);
                Assert.Null(context.MembershipBenefits.Find(9));
            }
        }

        /// <summary>
        /// Verifica que DeleteBenefit retorne NotFound si el beneficio no existe.
        /// </summary>
        [Fact]
        public async Task DeleteBenefit_ReturnsNotFound_WhenBenefitDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new LoyaltyContext(options);
            var controller = new MembershipBenefitsController(context);

            // Act
            var result = await controller.DeleteBenefit(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
