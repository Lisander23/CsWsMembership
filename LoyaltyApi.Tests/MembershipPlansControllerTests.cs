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
        public class MembershipPlansControllerTests
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
        /// Verifica que GetPlans retorne la lista de planes activos.
        /// </summary>
        [Fact]
        public async Task GetPlans_ReturnsListOfActivePlans()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaCreacion = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 1,
                        Nombre = "Plan Básico",
                        PrecioMensual = 10.0m,
                        EntradasMensuales = 2,
                        MesesAcumulacionMax = 6,
                        Nivel = 1,
                        Activo = true,
                        FechaCreacion = fechaCreacion
                    });
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 2,
                        Nombre = "Plan Inactivo",
                        PrecioMensual = 20.0m,
                        EntradasMensuales = 4,
                        MesesAcumulacionMax = 12,
                        Nivel = 2,
                        Activo = false,
                        FechaCreacion = fechaCreacion
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.GetPlans();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, okResult.StatusCode);

                var response = okResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var plans = Assert.IsAssignableFrom<IEnumerable<MembershipPlanDto>>(dataProperty.GetValue(response));
                var planList = plans.ToList();
                Assert.Single(planList);

                var plan = planList.First();
                Assert.Equal(1, plan.Id);
                Assert.Equal("Plan Básico", plan.Nombre);
                Assert.Equal(10.0m, plan.PrecioMensual);
                Assert.Equal(2, plan.EntradasMensuales);
                Assert.Equal(6, plan.MesesAcumulacionMax);
                Assert.Equal(1, plan.Nivel);
                Assert.True(plan.Activo);
                Assert.True(Math.Abs((fechaCreacion - plan.FechaCreacion).TotalSeconds) <= 1);
            }
        }

        /// <summary>
        /// Verifica que GetPlan retorne un plan activo por ID.
        /// </summary>
        [Fact]
        public async Task GetPlan_ReturnsPlanById()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaCreacion = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 1,
                        Nombre = "Plan Test",
                        PrecioMensual = 15.0m,
                        EntradasMensuales = 3,
                        MesesAcumulacionMax = 6,
                        Nivel = 1,
                        Activo = true,
                        FechaCreacion = fechaCreacion
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.GetPlan(1);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, okResult.StatusCode);

                var response = okResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var plan = Assert.IsType<MembershipPlanDto>(dataProperty.GetValue(response));
                Assert.Equal(1, plan.Id);
                Assert.Equal("Plan Test", plan.Nombre);
                Assert.Equal(15.0m, plan.PrecioMensual);
                Assert.Equal(3, plan.EntradasMensuales);
                Assert.Equal(6, plan.MesesAcumulacionMax);
                Assert.Equal(1, plan.Nivel);
                Assert.True(plan.Activo);
                Assert.True(Math.Abs((fechaCreacion - plan.FechaCreacion).TotalSeconds) <= 1);
            }
        }

        /// <summary>
        /// Verifica que GetPlan retorne NotFound si el plan no existe.
        /// </summary>
        [Fact]
        public async Task GetPlan_ReturnsNotFound_WhenPlanDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.GetPlan(999);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El plan no existe o está inactivo.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que GetPlan retorne NotFound si el plan está inactivo.
        /// </summary>
        [Fact]
        public async Task GetPlan_ReturnsNotFound_WhenPlanIsInactive()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaCreacion = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 1,
                        Nombre = "Plan Inactivo",
                        PrecioMensual = 20.0m,
                        EntradasMensuales = 4,
                        MesesAcumulacionMax = 12,
                        Nivel = 2,
                        Activo = false,
                        FechaCreacion = fechaCreacion
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.GetPlan(1);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El plan no existe o está inactivo.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreatePlan cree un nuevo plan si el nombre no existe.
        /// </summary>
        [Fact]
        public async Task CreatePlan_CreatesPlan_WhenNameIsUnique()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);
                var dto = new CreateMembershipPlanDto
                {
                    Nombre = "Nuevo Plan",
                    PrecioMensual = 30.0m,
                    EntradasMensuales = 5,
                    MesesAcumulacionMax = 12,
                    Nivel = 2,
                    Activo = true
                };

                // Act
                var result = await controller.CreatePlan(dto);

                // Assert
                var createdResult = Assert.IsType<CreatedAtActionResult>(result);
                Assert.Equal(201, createdResult.StatusCode);

                var response = createdResult.Value;
                var dataProperty = response.GetType().GetProperty("data");
                var timestampProperty = response.GetType().GetProperty("timestamp");
                Assert.NotNull(dataProperty);
                Assert.NotNull(timestampProperty);

                var plan = Assert.IsType<MembershipPlanDto>(dataProperty.GetValue(response));
                Assert.Equal("Nuevo Plan", plan.Nombre);
                Assert.Equal(30.0m, plan.PrecioMensual);
                Assert.Equal(5, plan.EntradasMensuales);
                Assert.Equal(12, plan.MesesAcumulacionMax);
                Assert.Equal(2, plan.Nivel);
                Assert.True(plan.Activo);
                Assert.True(plan.FechaCreacion <= DateTime.UtcNow);

                var dbPlan = await context.MembershipPlans.FindAsync(plan.Id);
                Assert.NotNull(dbPlan);
                Assert.Equal("Nuevo Plan", dbPlan.Nombre);
                Assert.Equal(30.0m, dbPlan.PrecioMensual);
                Assert.Equal(5, dbPlan.EntradasMensuales);
                Assert.Equal(12, dbPlan.MesesAcumulacionMax);
                Assert.Equal(2, dbPlan.Nivel);
                Assert.True(dbPlan.Activo);
            }
        }

        /// <summary>
        /// Verifica que CreatePlan retorne Conflict si ya existe un plan activo con el mismo nombre.
        /// </summary>
        [Fact]
        public async Task CreatePlan_ReturnsConflict_WhenNameExists()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaCreacion = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 1,
                        Nombre = "Plan Duplicado",
                        PrecioMensual = 40.0m,
                        EntradasMensuales = 6,
                        MesesAcumulacionMax = 12,
                        Nivel = 2,
                        Activo = true,
                        FechaCreacion = fechaCreacion
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);
                var dto = new CreateMembershipPlanDto
                {
                    Nombre = "Plan Duplicado",
                    PrecioMensual = 50.0m,
                    EntradasMensuales = 7,
                    MesesAcumulacionMax = 12,
                    Nivel = 3,
                    Activo = true
                };

                // Act
                var result = await controller.CreatePlan(dto);

                // Assert
                var conflictResult = Assert.IsType<ConflictObjectResult>(result);
                Assert.Equal(409, conflictResult.StatusCode);

                var response = conflictResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Ya existe un plan activo con ese nombre.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que CreatePlan retorne BadRequest si MesesAcumulacionMax es inválido.
        /// </summary>
        [Fact]
        public async Task CreatePlan_ReturnsBadRequest_WhenMesesAcumulacionMaxIsInvalid()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);
                var dto = new CreateMembershipPlanDto
                {
                    Nombre = "Nuevo Plan",
                    PrecioMensual = 30.0m,
                    EntradasMensuales = 5,
                    MesesAcumulacionMax = 13, // Inválido (> 12)
                    Nivel = 2,
                    Activo = true
                };

                // Act
                var result = await controller.CreatePlan(dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Los meses de acumulación máxima deben estar entre 1 y 12.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdatePlan actualice un plan existente.
        /// </summary>
        [Fact]
        public async Task UpdatePlan_UpdatesPlan()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaCreacion = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 1,
                        Nombre = "Plan Original",
                        PrecioMensual = 20.0m,
                        EntradasMensuales = 4,
                        MesesAcumulacionMax = 12,
                        Nivel = 2,
                        Activo = true,
                        FechaCreacion = fechaCreacion
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);
                var dto = new CreateMembershipPlanDto
                {
                    Nombre = "Plan Editado",
                    PrecioMensual = 25.0m,
                    EntradasMensuales = 5,
                    MesesAcumulacionMax = 12,
                    Nivel = 3,
                    Activo = true
                };

                // Act
                var result = await controller.UpdatePlan(1, dto);

                // Assert
                var noContentResult = Assert.IsType<NoContentResult>(result);
                Assert.Equal(204, noContentResult.StatusCode);

                var plan = await context.MembershipPlans.FindAsync(1);
                Assert.Equal("Plan Editado", plan.Nombre);
                Assert.Equal(25.0m, plan.PrecioMensual);
                Assert.Equal(5, plan.EntradasMensuales);
                Assert.Equal(12, plan.MesesAcumulacionMax);
                Assert.Equal(3, plan.Nivel);
                Assert.True(plan.Activo);
                Assert.True(Math.Abs((fechaCreacion - plan.FechaCreacion).TotalSeconds) <= 1);
            }
        }

        /// <summary>
        /// Verifica que UpdatePlan retorne NotFound si el plan no existe.
        /// </summary>
        [Fact]
        public async Task UpdatePlan_ReturnsNotFound_WhenPlanDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);
                var dto = new CreateMembershipPlanDto
                {
                    Nombre = "Plan Inexistente",
                    PrecioMensual = 10.0m,
                    EntradasMensuales = 2,
                    MesesAcumulacionMax = 6,
                    Nivel = 1,
                    Activo = true
                };

                // Act
                var result = await controller.UpdatePlan(999, dto);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El plan no existe.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdatePlan retorne Conflict si el nombre ya existe en otro plan activo.
        /// </summary>
        [Fact]
        public async Task UpdatePlan_ReturnsConflict_WhenNameExistsInOtherPlan()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaCreacion = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 1,
                        Nombre = "Plan Uno",
                        PrecioMensual = 10.0m,
                        EntradasMensuales = 2,
                        MesesAcumulacionMax = 6,
                        Nivel = 1,
                        Activo = true,
                        FechaCreacion = fechaCreacion
                    });
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 2,
                        Nombre = "Plan Dos",
                        PrecioMensual = 20.0m,
                        EntradasMensuales = 4,
                        MesesAcumulacionMax = 12,
                        Nivel = 2,
                        Activo = true,
                        FechaCreacion = fechaCreacion
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);
                var dto = new CreateMembershipPlanDto
                {
                    Nombre = "Plan Dos",
                    PrecioMensual = 15.0m,
                    EntradasMensuales = 3,
                    MesesAcumulacionMax = 6,
                    Nivel = 1,
                    Activo = true
                };

                // Act
                var result = await controller.UpdatePlan(1, dto);

                // Assert
                var conflictResult = Assert.IsType<ConflictObjectResult>(result);
                Assert.Equal(409, conflictResult.StatusCode);

                var response = conflictResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Ya existe otro plan activo con ese nombre.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que UpdatePlan retorne BadRequest si MesesAcumulacionMax es inválido.
        /// </summary>
        [Fact]
        public async Task UpdatePlan_ReturnsBadRequest_WhenMesesAcumulacionMaxIsInvalid()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaCreacion = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 1,
                        Nombre = "Plan Original",
                        PrecioMensual = 20.0m,
                        EntradasMensuales = 4,
                        MesesAcumulacionMax = 12,
                        Nivel = 2,
                        Activo = true,
                        FechaCreacion = fechaCreacion
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);
                var dto = new CreateMembershipPlanDto
                {
                    Nombre = "Plan Editado",
                    PrecioMensual = 25.0m,
                    EntradasMensuales = 5,
                    MesesAcumulacionMax = 0, // Inválido (< 1)
                    Nivel = 3,
                    Activo = true
                };

                // Act
                var result = await controller.UpdatePlan(1, dto);

                // Assert
                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);

                var response = badRequestResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("Los meses de acumulación máxima deben estar entre 1 y 12.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que DeletePlan elimine (soft delete) un plan activo.
        /// </summary>
        [Fact]
        public async Task DeletePlan_DeletesPlan()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaCreacion = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 1,
                        Nombre = "Plan a Eliminar",
                        PrecioMensual = 10.0m,
                        EntradasMensuales = 2,
                        MesesAcumulacionMax = 6,
                        Nivel = 1,
                        Activo = true,
                        FechaCreacion = fechaCreacion
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.DeletePlan(1);

                // Assert
                var noContentResult = Assert.IsType<NoContentResult>(result);
                Assert.Equal(204, noContentResult.StatusCode);

                var plan = await context.MembershipPlans.FindAsync(1);
                Assert.False(plan.Activo);
            }
        }

        /// <summary>
        /// Verifica que DeletePlan retorne NotFound si el plan no existe.
        /// </summary>
        [Fact]
        public async Task DeletePlan_ReturnsNotFound_WhenPlanDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.DeletePlan(999);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El plan no existe o ya está inactivo.", errorProperty.GetValue(response));
            }
        }

        /// <summary>
        /// Verifica que DeletePlan retorne NotFound si el plan ya está inactivo.
        /// </summary>
        [Fact]
        public async Task DeletePlan_ReturnsNotFound_WhenPlanIsInactive()
        {
            // Arrange
            var options = GetInMemoryOptions();
            var fechaCreacion = DateTime.UtcNow;
            using (var context = new LoyaltyContext(options))
            {
                SetupContext(context, ctx =>
                {
                    ctx.MembershipPlans.Add(new MembershipPlan
                    {
                        Id = 1,
                        Nombre = "Plan Inactivo",
                        PrecioMensual = 10.0m,
                        EntradasMensuales = 2,
                        MesesAcumulacionMax = 6,
                        Nivel = 1,
                        Activo = false,
                        FechaCreacion = fechaCreacion
                    });
                });
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.DeletePlan(1);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal(404, notFoundResult.StatusCode);

                var response = notFoundResult.Value;
                var errorProperty = response.GetType().GetProperty("error");
                Assert.NotNull(errorProperty);
                Assert.Equal("El plan no existe o ya está inactivo.", errorProperty.GetValue(response));
            }
        }
    }
}
