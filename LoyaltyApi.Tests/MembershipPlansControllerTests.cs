using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoyaltyApi.Controllers;
using LoyaltyApi.Data;
using LoyaltyApi.Entities;
using LoyaltyApi.Models;
using System.Linq;

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

        /// <summary>
        /// Verifica que GetPlans retorne la lista de planes activos.
        /// </summary>
        [Fact]
        public async Task GetPlans_ReturnsListOfActivePlans()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 1,
                    Nombre = "Plan Básico",
                    PrecioMensual = 10,
                    EntradasMensuales = 2,
                    MesesAcumulacionMax = 6,
                    Nivel = 1,
                    Activo = true
                });
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 2,
                    Nombre = "Plan Inactivo",
                    PrecioMensual = 20,
                    EntradasMensuales = 4,
                    MesesAcumulacionMax = 12,
                    Nivel = 2,
                    Activo = false
                });
                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.GetPlans();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var plans = Assert.IsAssignableFrom<IEnumerable<MembershipPlanDto>>(okResult.Value);
                Assert.Single(plans);
                Assert.Contains(plans, p => p.Nombre == "Plan Básico");
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
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 1,
                    Nombre = "Plan Test",
                    PrecioMensual = 15,
                    EntradasMensuales = 3,
                    MesesAcumulacionMax = 6,
                    Nivel = 1,
                    Activo = true
                });
                context.SaveChanges();
            }

            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.GetPlan(1);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var plan = Assert.IsType<MembershipPlanDto>(okResult.Value);
                Assert.Equal("Plan Test", plan.Nombre);
            }
        }

        /// <summary>
        /// Verifica que GetPlan retorne NotFound si el plan no existe o está inactivo.
        /// </summary>
        [Fact]
        public async Task GetPlan_ReturnsNotFound_WhenPlanDoesNotExistOrInactive()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 1,
                    Nombre = "Plan Inactivo",
                    PrecioMensual = 20,
                    EntradasMensuales = 4,
                    MesesAcumulacionMax = 12,
                    Nivel = 2,
                    Activo = false
                });
                context.SaveChanges();
            }
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.GetPlan(1);

                // Assert
                var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
                Assert.Equal("Plan no encontrado o inactivo.", notFound.Value);
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
            using var context = new LoyaltyContext(options);
            var controller = new MembershipPlansController(context);
            var dto = new CreateMembershipPlanDto
            {
                Nombre = "Nuevo Plan",
                PrecioMensual = 30,
                EntradasMensuales = 5,
                MesesAcumulacionMax = 12,
                Nivel = 2,
                Activo = true
            };

            // Act
            var result = await controller.CreatePlan(dto);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var plan = Assert.IsType<MembershipPlanDto>(created.Value);
            Assert.Equal("Nuevo Plan", plan.Nombre);
        }

        /// <summary>
        /// Verifica que CreatePlan retorne Conflict si ya existe un plan activo con el mismo nombre.
        /// </summary>
        [Fact]
        public async Task CreatePlan_ReturnsConflict_WhenNameExists()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Nombre = "Plan Duplicado",
                    PrecioMensual = 40,
                    EntradasMensuales = 6,
                    MesesAcumulacionMax = 12,
                    Nivel = 2,
                    Activo = true
                });
                context.SaveChanges();
            }
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);
                var dto = new CreateMembershipPlanDto
                {
                    Nombre = "Plan Duplicado",
                    PrecioMensual = 50,
                    EntradasMensuales = 7,
                    MesesAcumulacionMax = 12,
                    Nivel = 3,
                    Activo = true
                };

                // Act
                var result = await controller.CreatePlan(dto);

                // Assert
                var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
                Assert.Equal("Ya existe un plan activo con ese nombre.", conflict.Value);
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
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 1,
                    Nombre = "Plan Original",
                    PrecioMensual = 20,
                    EntradasMensuales = 4,
                    MesesAcumulacionMax = 12,
                    Nivel = 2,
                    Activo = true
                });
                context.SaveChanges();
            }
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);
                var dto = new CreateMembershipPlanDto
                {
                    Nombre = "Plan Editado",
                    PrecioMensual = 25,
                    EntradasMensuales = 5,
                    MesesAcumulacionMax = 12,
                    Nivel = 3,
                    Activo = true
                };

                // Act
                var result = await controller.UpdatePlan(1, dto);

                // Assert
                Assert.IsType<NoContentResult>(result);
                var plan = context.MembershipPlans.Find(1);
                Assert.Equal("Plan Editado", plan.Nombre);
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
            using var context = new LoyaltyContext(options);
            var controller = new MembershipPlansController(context);
            var dto = new CreateMembershipPlanDto
            {
                Nombre = "Plan Inexistente",
                PrecioMensual = 10,
                EntradasMensuales = 2,
                MesesAcumulacionMax = 6,
                Nivel = 1,
                Activo = true
            };

            // Act
            var result = await controller.UpdatePlan(999, dto);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Plan no encontrado o inactivo.", notFound.Value);
        }

        /// <summary>
        /// Verifica que UpdatePlan retorne Conflict si el nombre ya existe en otro plan activo.
        /// </summary>
        [Fact]
        public async Task UpdatePlan_ReturnsConflict_WhenNameExistsInOtherPlan()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 1,
                    Nombre = "Plan Uno",
                    PrecioMensual = 10,
                    EntradasMensuales = 2,
                    MesesAcumulacionMax = 6,
                    Nivel = 1,
                    Activo = true
                });
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 2,
                    Nombre = "Plan Dos",
                    PrecioMensual = 20,
                    EntradasMensuales = 4,
                    MesesAcumulacionMax = 12,
                    Nivel = 2,
                    Activo = true
                });
                context.SaveChanges();
            }
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);
                var dto = new CreateMembershipPlanDto
                {
                    Nombre = "Plan Dos",
                    PrecioMensual = 15,
                    EntradasMensuales = 3,
                    MesesAcumulacionMax = 6,
                    Nivel = 1,
                    Activo = true
                };

                // Act
                var result = await controller.UpdatePlan(1, dto);

                // Assert
                var conflict = Assert.IsType<ConflictObjectResult>(result);
                Assert.Equal("Ya existe otro plan activo con ese nombre.", conflict.Value);
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
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 1,
                    Nombre = "Plan a Eliminar",
                    PrecioMensual = 10,
                    EntradasMensuales = 2,
                    MesesAcumulacionMax = 6,
                    Nivel = 1,
                    Activo = true
                });
                context.SaveChanges();
            }
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.DeletePlan(1);

                // Assert
                Assert.IsType<NoContentResult>(result);
                var plan = context.MembershipPlans.Find(1);
                Assert.False(plan.Activo);
            }
        }

        /// <summary>
        /// Verifica que DeletePlan retorne NotFound si el plan no existe o ya está inactivo.
        /// </summary>
        [Fact]
        public async Task DeletePlan_ReturnsNotFound_WhenPlanDoesNotExistOrInactive()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using (var context = new LoyaltyContext(options))
            {
                context.MembershipPlans.Add(new MembershipPlan
                {
                    Id = 1,
                    Nombre = "Plan Inactivo",
                    PrecioMensual = 10,
                    EntradasMensuales = 2,
                    MesesAcumulacionMax = 6,
                    Nivel = 1,
                    Activo = false
                });
                context.SaveChanges();
            }
            using (var context = new LoyaltyContext(options))
            {
                var controller = new MembershipPlansController(context);

                // Act
                var result = await controller.DeletePlan(1);

                // Assert
                var notFound = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("Plan not found or already inactive.", notFound.Value);
            }
        }
    }
}
