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
using LoyaltyApi.Models;

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

        private void SetupContext(LoyaltyContext context, Action<LoyaltyContext> setupAction)
        {
            setupAction(context);
            context.SaveChanges();
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
                SetupContext(context, ctx =>
                {
                    ctx.MembershipBenefits.Add(new MembershipBenefit
                    {
                        Id = 1,
                        PlanId = 1,
                        Clave = "BEN1",
                        Valor = 10.5m,
                        DiasAplicables = "Lunes",
                        Observacion = "Obs1"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);

                // Act
                var result = await controller.GetBenefits();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, okResult.StatusCode);

                var response = okResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var benefits = Assert.IsAssignableFrom<IEnumerable<MembershipBenefitDto>>(dataProperty.GetValue(response));
                var benefitList = benefits.ToList();
                Assert.Single(benefitList);

                var benefit = benefitList.First();
                Assert.Equal(1, benefit.Id);
                Assert.Equal(1, benefit.PlanId);
                Assert.Equal("BEN1", benefit.Clave);
                Assert.Equal(10.5m, benefit.Valor);
                Assert.Equal("Lunes", benefit.DiasAplicables);
                Assert.Equal("Obs1", benefit.Observacion);
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
                SetupContext(context, ctx =>
                {
                    ctx.MembershipBenefits.Add(new MembershipBenefit
                    {
                        Id = 2,
                        PlanId = 2,
                        Clave = "BEN2",
                        Valor = 20.0m,
                        DiasAplicables = "Martes",
                        Observacion = "Obs2"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);

                // Act
                var result = await controller.GetBenefit(2);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, okResult.StatusCode);

                var response = okResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var benefit = Assert.IsType<MembershipBenefitDto>(dataProperty.GetValue(response));
                Assert.Equal(2, benefit.Id);
                Assert.Equal(2, benefit.PlanId);
                Assert.Equal("BEN2", benefit.Clave);
                Assert.Equal(20.0m, benefit.Valor);
                Assert.Equal("Martes", benefit.DiasAplicables);
                Assert.Equal("Obs2", benefit.Observacion);
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
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);

                // Act
                var result = await controller.GetBenefit(999);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El beneficio no existe.", errorProperty.GetValue(response));
            }
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
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 3,
                        Nombre = "Plan Test",
                        PrecioMensual = 100.0m,
                        EntradasMensuales = 1,
                        Nivel = 1,
                        Activo = true
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);
                var dto = new CreateMembershipBenefitDto
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
                var createdResult = Assert.IsType<CreatedAtActionResult>(result);
                Assert.Equal(201, createdResult.StatusCode);

                var response = createdResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var benefit = Assert.IsType<MembershipBenefitDto>(dataProperty.GetValue(response));
                Assert.Equal(3, benefit.PlanId);
                Assert.Equal("BEN3", benefit.Clave);
                Assert.Equal(30.0m, benefit.Valor);
                Assert.Equal("Miércoles", benefit.DiasAplicables);
                Assert.Equal("Obs3", benefit.Observacion);

                var dbBenefit = await context.MembershipBenefits.FindAsync(benefit.Id);
                Assert.NotNull(dbBenefit);
                Assert.Equal(3, dbBenefit.PlanId);
                Assert.Equal("BEN3", dbBenefit.Clave);
                Assert.Equal(30.0m, dbBenefit.Valor);
                Assert.Equal("Miércoles", dbBenefit.DiasAplicables);
                Assert.Equal("Obs3", dbBenefit.Observacion);
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
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);
                var dto = new CreateMembershipBenefitDto
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
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El plan especificado no existe o está inactivo.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreateBenefit retorne Conflict si la clave ya existe para el plan.
        /// </summary>
        [Fact]
        public async Task CreateBenefit_ReturnsConflict_WhenClaveIsDuplicated()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 3,
                        Nombre = "Plan Test",
                        PrecioMensual = 100.0m,
                        EntradasMensuales = 1,
                        Nivel = 1,
                        Activo = true
                    });
                    ctx.MembershipBenefits.Add(new MembershipBenefit
                    {
                        Id = 4,
                        PlanId = 3,
                        Clave = "BEN3",
                        Valor = 30.0m,
                        DiasAplicables = "Miércoles",
                        Observacion = "Obs3"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);
                var dto = new CreateMembershipBenefitDto
                {
                    PlanId = 3,
                    Clave = "BEN3",
                    Valor = 40.0m,
                    DiasAplicables = "Jueves",
                    Observacion = "Obs4"
                };

                // Act
                var result = await controller.CreateBenefit(dto);

                // Assert
                var conflictResult = Assert.IsType<ConflictObjectResult>(result);
                Assert.Equal(409, conflictResult.StatusCode);

                var response = conflictResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Ya existe un beneficio con esta clave para el plan especificado.", errorProperty.GetValue(response));
            }
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
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 5,
                        Nombre = "Plan Test",
                        PrecioMensual = 100.0m,
                        EntradasMensuales = 1,
                        Nivel = 1,
                        Activo = true
                    });
                    ctx.MembershipBenefits.Add(new MembershipBenefit
                    {
                        Id = 5,
                        PlanId = 5,
                        Clave = "BEN5",
                        Valor = 50.0m,
                        DiasAplicables = "Viernes",
                        Observacion = "Obs5"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);
                var dto = new CreateMembershipBenefitDto
                {
                    PlanId = 5,
                    Clave = "BEN5-EDIT",
                    Valor = 55.0m,
                    DiasAplicables = "Sábado",
                    Observacion = "Obs5-EDIT"
                };

                // Act
                var result = await controller.UpdateBenefit(5, dto);

                // Assert
                var noContentResult = Assert.IsType<NoContentResult>(result);
                Assert.Equal(204, noContentResult.StatusCode);

                var updatedBenefit = await context.MembershipBenefits.FindAsync(5);
                Assert.Equal(5, updatedBenefit.PlanId);
                Assert.Equal("BEN5-EDIT", updatedBenefit.Clave);
                Assert.Equal(55.0m, updatedBenefit.Valor);
                Assert.Equal("Sábado", updatedBenefit.DiasAplicables);
                Assert.Equal("Obs5-EDIT", updatedBenefit.Observacion);
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
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 7,
                        Nombre = "Plan Test",
                        PrecioMensual = 100.0m,
                        EntradasMensuales = 1,
                        Nivel = 1,
                        Activo = true
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);
                var dto = new CreateMembershipBenefitDto
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
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El beneficio no existe.", errorProperty.GetValue(response));
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
                SetupContext(context, ctx =>
                {
                    ctx.MembershipBenefits.Add(new MembershipBenefit
                    {
                        Id = 8,
                        PlanId = 8,
                        Clave = "BEN8",
                        Valor = 80.0m,
                        DiasAplicables = "Lunes",
                        Observacion = "Obs8"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);
                var dto = new CreateMembershipBenefitDto
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
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El plan especificado no existe o está inactivo.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdateBenefit retorne Conflict si la clave está duplicada.
        /// </summary>
        [Fact]
        public async Task UpdateBenefit_ReturnsConflict_WhenClaveIsDuplicated()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 5,
                        Nombre = "Plan Test",
                        PrecioMensual = 100.0m,
                        EntradasMensuales = 1,
                        Nivel = 1,
                        Activo = true
                    });
                    ctx.MembershipBenefits.Add(new MembershipBenefit
                    {
                        Id = 5,
                        PlanId = 5,
                        Clave = "BEN5",
                        Valor = 50.0m,
                        DiasAplicables = "Viernes",
                        Observacion = "Obs5"
                    });
                    ctx.MembershipBenefits.Add(new MembershipBenefit
                    {
                        Id = 6,
                        PlanId = 5,
                        Clave = "BEN6",
                        Valor = 60.0m,
                        DiasAplicables = "Sábado",
                        Observacion = "Obs6"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);
                var dto = new CreateMembershipBenefitDto
                {
                    PlanId = 5,
                    Clave = "BEN6",
                    Valor = 55.0m,
                    DiasAplicables = "Sábado",
                    Observacion = "Obs5-EDIT"
                };

                // Act
                var result = await controller.UpdateBenefit(5, dto);

                // Assert
                var conflictResult = Assert.IsType<ConflictObjectResult>(result);
                Assert.Equal(409, conflictResult.StatusCode);

                var response = conflictResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Ya existe un beneficio con esta clave para el plan especificado.", errorProperty.GetValue(response));
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
                SetupContext(context, ctx =>
                {
                    ctx.MembershipBenefits.Add(new MembershipBenefit
                    {
                        Id = 9,
                        PlanId = 9,
                        Clave = "BEN9",
                        Valor = 90.0m,
                        DiasAplicables = "Miércoles",
                        Observacion = "Obs9"
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);

                // Act
                var result = await controller.DeleteBenefit(9);

                // Assert
                var noContentResult = Assert.IsType<NoContentResult>(result);
                Assert.Equal(204, noContentResult.StatusCode);

                var dbBenefit = await context.MembershipBenefits.FindAsync(9);
                Assert.Null(dbBenefit);
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
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipBenefitsController(context);

                // Act
                var result = await controller.DeleteBenefit(999);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El beneficio no existe.", errorProperty.GetValue(response));
            }
        }
    }
}
