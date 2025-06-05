using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoyaltyApi.Controllers;
using LoyaltyApi.Data;
using LoyaltyApi.Models;
using LoyaltyApi.Entities;

public class CustomerMembershipsControllerTests
{
    private LoyaltyContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LoyaltyContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new LoyaltyContext(options);
    }

    // --- GetMemberships: Lista de membresías activas ---
    // Verifica que GetMemberships devuelva un HTTP 200 con solo las membresías activas.
    [Fact]
    public async Task GetMemberships_ReturnsOkResult_WithListOfActiveMemberships()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.CustomerMemberships.Add(new CustomerMembership { Id = 1, CodCliente = 100, PlanId = 1, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddYears(1), Estado = "Activo", IdSuscripcionMP = "sub1", IdClienteMP = "client1", MesesAcumulacionPersonalizado = 12 });
        context.CustomerMemberships.Add(new CustomerMembership { Id = 2, CodCliente = 101, PlanId = 2, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddYears(1), Estado = "Activo", IdSuscripcionMP = "sub2", IdClienteMP = "client2", MesesAcumulacionPersonalizado = null });
        context.CustomerMemberships.Add(new CustomerMembership { Id = 3, CodCliente = 102, PlanId = 1, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddMonths(6), Estado = "Inactivo", IdSuscripcionMP = "sub3", IdClienteMP = "client3", MesesAcumulacionPersonalizado = 6 });
        await context.SaveChangesAsync();
        var controller = new CustomerMembershipsController(context);

        // Act
        var result = await controller.GetMemberships();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var memberships = Assert.IsAssignableFrom<IEnumerable<CustomerMembershipDto>>(okResult.Value);

        Assert.Equal(2, memberships.Count());
        Assert.Contains(memberships, m => m.Id == 1);
        Assert.Contains(memberships, m => m.Id == 2);
        Assert.DoesNotContain(memberships, m => m.Id == 3);

        var firstMembership = memberships.FirstOrDefault(m => m.Id == 1);
        Assert.NotNull(firstMembership);
        Assert.Equal(100, firstMembership.CodCliente);
        Assert.Equal(1, firstMembership.PlanId);
        Assert.Equal("Activo", firstMembership.Estado);
        Assert.Equal("sub1", firstMembership.IdSuscripcionMP);
        Assert.Equal("client1", firstMembership.IdClienteMP);
        Assert.Equal(12, firstMembership.MesesAcumulacionPersonalizado);

        context.Dispose();
    }

    // --- GetMemberships: Sin membresías activas ---
    // Verifica que GetMemberships devuelva un HTTP 200 con una lista vacía si no hay membresías activas.
    [Fact]
    public async Task GetMemberships_ReturnsOkResult_WhenNoActiveMembershipsExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var controller = new CustomerMembershipsController(context);

        // Act
        var result = await controller.GetMemberships();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var memberships = Assert.IsAssignableFrom<IEnumerable<CustomerMembershipDto>>(okResult.Value);
        Assert.Empty(memberships);

        context.Dispose();
    }

    // --- GetMembership(int id): Membresía activa encontrada ---
    // Verifica que GetMembership devuelva un HTTP 200 con una membresía activa por ID.
    [Fact]
    public async Task GetMembershipById_ReturnsOkResult_WithActiveMembership()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var activeMembership = new CustomerMembership { Id = 1, CodCliente = 100, PlanId = 1, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddYears(1), Estado = "Activo" };
        var inactiveMembership = new CustomerMembership { Id = 2, CodCliente = 101, PlanId = 2, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddYears(1), Estado = "Inactivo" };
        context.CustomerMemberships.AddRange(activeMembership, inactiveMembership);
        await context.SaveChangesAsync();
        var controller = new CustomerMembershipsController(context);
        var expectedId = 1;

        // Act
        var result = await controller.GetMembership(expectedId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedMembership = Assert.IsType<CustomerMembershipDto>(okResult.Value);

        Assert.Equal(expectedId, returnedMembership.Id);
        Assert.Equal(activeMembership.CodCliente, returnedMembership.CodCliente);
        Assert.Equal("Activo", returnedMembership.Estado);

        context.Dispose();
    }

    // --- GetMembership(int id): Membresía no encontrada ---
    // Verifica que GetMembership devuelva un HTTP 404 si la membresía no existe.
    [Fact]
    public async Task GetMembershipById_ReturnsNotFound_WhenMembershipDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        await context.SaveChangesAsync();
        var controller = new CustomerMembershipsController(context);
        var nonExistentId = 99;

        // Act
        var result = await controller.GetMembership(nonExistentId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
        context.Dispose();
    }

    // --- GetMembership(int id): Membresía inactiva ---
    // Verifica que GetMembership devuelva un HTTP 404 si la membresía está inactiva.
    [Fact]
    public async Task GetMembershipById_ReturnsNotFound_WhenMembershipIsInactive()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var inactiveMembership = new CustomerMembership { Id = 3, CodCliente = 102, PlanId = 1, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddMonths(6), Estado = "Inactivo" };
        context.CustomerMemberships.Add(inactiveMembership);
        await context.SaveChangesAsync();
        var controller = new CustomerMembershipsController(context);
        var inactiveId = 3;

        // Act
        var result = await controller.GetMembership(inactiveId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
        context.Dispose();
    }

    // --- GetMembershipStatus: Membresía activa con entradas disponibles ---
    // Verifica que GetMembershipStatus devuelva un HTTP 200 con el estado y entradas disponibles de una membresía activa.
    [Fact]
    public async Task GetMembershipStatus_ReturnsOkResult_WithActiveMembershipStatusAndAvailableEntries()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var codCliente = "100";
        var planId = 1;
        var entradasMensuales = 5;
        var entradasUsadas = 2;
        var periodoActual = int.Parse(DateTime.Now.ToString("yyyyMM"));

        var plan = new MembershipPlan
        {
            Id = planId,
            Nombre = "Plan Premium",
            PrecioMensual = 50.0m,
            EntradasMensuales = entradasMensuales,
            Nivel = 1,
            Activo = true,
            Benefits = new List<MembershipBenefit> { new MembershipBenefit { Clave = "VIP", Observacion = "Acceso VIP" }, new MembershipBenefit { Clave = "DESCUENTO", Observacion = "Descuentos exclusivos" } }
        };
        var membership = new CustomerMembership
        {
            Id = 1,
            CodCliente = decimal.Parse(codCliente),
            PlanId = planId,
            Plan = plan,
            FechaInicio = DateTime.Now.AddMonths(-1),
            FechaFin = DateTime.Now.AddYears(1),
            Estado = "ACTIVO"
        };
        var entryBalance = new EntryBalance
        {
            Id = 1,
            CustomerMembershipId = membership.Id,
            Periodo = periodoActual,
            EntradasUsadas = entradasUsadas
        };

        context.MembershipPlans.Add(plan);
        context.CustomerMemberships.Add(membership);
        context.EntryBalances.Add(entryBalance);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);

        // Act
        var result = await controller.GetMembershipStatus(codCliente);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedData = Assert.IsType<MembershipStatusDto>(okResult.Value);

        Assert.Equal("ACTIVO", returnedData.Estado);
        Assert.Equal(planId, returnedData.PlanId);
        Assert.Equal("Plan Premium", returnedData.NombrePlan);
        Assert.Equal(50.0m, returnedData.PrecioMensual);
        Assert.Equal(entradasMensuales, returnedData.EntradasMensuales);
        Assert.Equal(entradasMensuales - entradasUsadas, returnedData.EntradasDisponibles);
        Assert.Equal(1, returnedData.Nivel);
        Assert.Contains("Acceso VIP", (IEnumerable<string>)returnedData.Beneficios);
        Assert.Contains("Descuentos exclusivos", (IEnumerable<string>)returnedData.Beneficios);

        context.Dispose();
    }

    // --- GetMembershipStatus: Código de cliente inválido ---
    // Verifica que GetMembershipStatus devuelva un HTTP 400 si el código de cliente no es válido.
    [Fact]
    public async Task GetMembershipStatus_ReturnsBadRequest_WhenCodClienteIsInvalid()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var controller = new CustomerMembershipsController(context);
        var invalidCodCliente = "ABC";

        // Act
        var result = await controller.GetMembershipStatus(invalidCodCliente);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("CodCliente inválido.", badRequestResult.Value);

        context.Dispose();
    }

    // --- GetMembershipStatus: Sin membresía activa ---
    // Verifica que GetMembershipStatus devuelva un HTTP 404 si no hay membresía activa para el cliente.
    [Fact]
    public async Task GetMembershipStatus_ReturnsNotFound_WhenNoActiveMembershipFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Clientes.Add(new Cliente { CodCliente = 200 });
        await context.SaveChangesAsync();
        var controller = new CustomerMembershipsController(context);
        var codCliente = "200";

        // Act
        var result = await controller.GetMembershipStatus(codCliente);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Cliente sin membresía activa.", notFoundResult.Value);

        context.Dispose();
    }

    // --- GetMembershipStatus: Sin balances de entradas para el periodo ---
    // Verifica que GetMembershipStatus calcule correctamente las entradas disponibles si no hay balances para el periodo actual.
    [Fact]
    public async Task GetMembershipStatus_ReturnsCorrectEntries_WhenNoEntryBalancesForPeriod()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var codCliente = "300";
        var planId = 3;
        var entradasMensuales = 10;
        var periodoActual = int.Parse(DateTime.Now.ToString("yyyyMM"));

        var plan = new MembershipPlan
        {
            Id = planId,
            Nombre = "Plan Base",
            PrecioMensual = 30.0m,
            EntradasMensuales = entradasMensuales,
            Nivel = 2,
            Activo = true
        };
        var membership = new CustomerMembership
        {
            Id = 2,
            CodCliente = decimal.Parse(codCliente),
            PlanId = planId,
            Plan = plan,
            FechaInicio = DateTime.Now.AddMonths(-1),
            FechaFin = DateTime.Now.AddYears(1),
            Estado = "ACTIVO"
        };

        context.EntryBalances.Add(new EntryBalance { Id = 2, CustomerMembershipId = membership.Id, Periodo = periodoActual - 1, EntradasUsadas = 5 });
        context.MembershipPlans.Add(plan);
        context.CustomerMemberships.Add(membership);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);

        // Act
        var result = await controller.GetMembershipStatus(codCliente);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedData = Assert.IsType<MembershipStatusDto>(okResult.Value);

        Assert.Equal("ACTIVO", returnedData.Estado);
        Assert.Equal(entradasMensuales, returnedData.EntradasMensuales);
        Assert.Equal(entradasMensuales, returnedData.EntradasDisponibles);
        Assert.Equal(0, returnedData.EntradasMensuales - returnedData.EntradasDisponibles);

        context.Dispose();
    }

    // --- GetCustomerMembershipStatus: Redirección a GetMembershipStatus ---
    // Verifica que GetCustomerMembershipStatus llame a GetMembershipStatus y devuelva el resultado esperado.
    [Fact]
    public async Task GetCustomerMembershipStatus_CallsGetMembershipStatus_AndReturnsResult()
    {
        // Arrange
        var codClienteDecimal = 123m;
        var codCliente = codClienteDecimal.ToString();

        var plan = new MembershipPlan
        {
            Id = 10,
            Nombre = "Plan Oro",
            PrecioMensual = 29.99m,
            EntradasMensuales = 5,
            Nivel = 2,
            Benefits = new List<MembershipBenefit>
            {
                new MembershipBenefit { Id = 1, Clave = "PISCINA", Observacion = "Acceso a piscina", PlanId = 10, Valor = 0 },
                new MembershipBenefit { Id = 2, Clave = "YOGA", Observacion = "Clases de yoga", PlanId = 10, Valor = 0 }
            }
        };

        var customerMembership = new CustomerMembership
        {
            Id = 1,
            CodCliente = codClienteDecimal,
            Estado = "ACTIVO",
            PlanId = 10,
            Plan = plan
        };

        var entryBalances = new List<EntryBalance>
        {
            new EntryBalance
            {
                CustomerMembershipId = 1,
                Periodo = int.Parse(DateTime.Now.ToString("yyyyMM")),
                EntradasUsadas = 2
            }
        };

        var context = GetInMemoryDbContext();
        await context.MembershipPlans.AddAsync(plan); // Agrega el plan explícitamente
        await context.CustomerMemberships.AddAsync(customerMembership);
        await context.EntryBalances.AddRangeAsync(entryBalances);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);

        // Act
        var result = await controller.GetCustomerMembershipStatus(codCliente);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<MembershipStatusDto>(okResult.Value);

        Assert.Equal("ACTIVO", dto.Estado);
        Assert.Equal(10, dto.PlanId);
        Assert.Equal("Plan Oro", dto.NombrePlan);
        Assert.Equal(29.99m, dto.PrecioMensual);
        Assert.Equal(5, dto.EntradasMensuales);
        Assert.Equal(3, dto.EntradasDisponibles); // 5 - 2
        Assert.Equal(2, dto.Nivel);
        Assert.Contains("Acceso a piscina", dto.Beneficios);
        Assert.Contains("Clases de yoga", dto.Beneficios);
    }


    // --- CreateMembership: Crea una membresía válida ---
    // Verifica que CreateMembership devuelva un HTTP 201 cuando los datos son válidos.
    [Fact]
    public async Task CreateMembership_ReturnsCreated_WhenValid()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var cliente = new Cliente { CodCliente = 1, NomCliente = "Test" };
        var plan = new MembershipPlan { Id = 1, Nombre = "Plan", PrecioMensual = 10, EntradasMensuales = 2, Nivel = 1, Activo = true };
        context.Clientes.Add(cliente);
        context.MembershipPlans.Add(plan);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);
        var dto = new CreateCustomerMembershipDto
        {
            CodCliente = 1,
            PlanId = 1,
            FechaInicio = DateTime.Now,
            FechaFin = DateTime.Now.AddMonths(1)
        };

        // Act
        var result = await controller.CreateMembership(dto);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var membership = Assert.IsType<CustomerMembershipDto>(created.Value);
        Assert.Equal(1, membership.CodCliente);
        Assert.Equal(1, membership.PlanId);
        context.Dispose();
    }


    // --- UpdateMembership: Actualiza una membresía válida ---
    // Verifica que UpdateMembership devuelva un HTTP 204 cuando la actualización es exitosa.
    [Fact]
    public async Task UpdateMembership_ReturnsNoContent_WhenValid()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var cliente = new Cliente { CodCliente = 1, NomCliente = "Test" };
        var plan = new MembershipPlan { Id = 1, Nombre = "Plan", PrecioMensual = 10, EntradasMensuales = 2, Nivel = 1, Activo = true };
        var membership = new CustomerMembership { Id = 1, CodCliente = 1, PlanId = 1, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddMonths(1), Estado = "Activo" };
        context.Clientes.Add(cliente);
        context.MembershipPlans.Add(plan);
        context.CustomerMemberships.Add(membership);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);
        var dto = new CreateCustomerMembershipDto
        {
            CodCliente = 1,
            PlanId = 1,
            FechaInicio = DateTime.Now,
            FechaFin = DateTime.Now.AddMonths(2)
        };

        // Act
        var result = await controller.UpdateMembership(1, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        context.Dispose();
    }


    // --- DeleteMembership: Elimina una membresía activa ---
    // Verifica que DeleteMembership devuelva un HTTP 204 cuando la eliminación es exitosa.
    [Fact]
    public async Task DeleteMembership_ReturnsNoContent_WhenValid()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var membership = new CustomerMembership { Id = 1, CodCliente = 1, PlanId = 1, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddMonths(1), Estado = "Activo" };
        context.CustomerMemberships.Add(membership);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);

        // Act
        var result = await controller.DeleteMembership(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        context.Dispose();
    }

    // --- CreateMembership: Cliente no existe ---
    // Verifica que CreateMembership devuelva un HTTP 400 si el cliente no existe.
    [Fact]
    public async Task CreateMembership_ReturnsBadRequest_WhenClienteDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var plan = new MembershipPlan { Id = 1, Nombre = "Plan", PrecioMensual = 10, EntradasMensuales = 2, Nivel = 1, Activo = true };
        context.MembershipPlans.Add(plan);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);
        var dto = new CreateCustomerMembershipDto
        {
            CodCliente = 99, // Cliente inexistente
            PlanId = 1,
            FechaInicio = DateTime.Now,
            FechaFin = DateTime.Now.AddMonths(1)
        };

        // Act
        var result = await controller.CreateMembership(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("El cliente especificado no existe.", badRequest.Value);
        context.Dispose();
    }

    // --- CreateMembership: Plan no existe ---
    // Verifica que CreateMembership devuelva un HTTP 400 si el plan no existe.
    [Fact]
    public async Task CreateMembership_ReturnsBadRequest_WhenPlanDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var cliente = new Cliente { CodCliente = 1, NomCliente = "Test" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);
        var dto = new CreateCustomerMembershipDto
        {
            CodCliente = 1,
            PlanId = 99, // Plan inexistente
            FechaInicio = DateTime.Now,
            FechaFin = DateTime.Now.AddMonths(1)
        };

        // Act
        var result = await controller.CreateMembership(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("El plan especificado no existe.", badRequest.Value);
        context.Dispose();
    }

    // --- CreateMembership: Ya existe membresía activa ---
    // Verifica que CreateMembership devuelva un HTTP 409 si ya existe una membresía activa para el cliente y plan.
    [Fact]
    public async Task CreateMembership_ReturnsConflict_WhenActiveMembershipExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var cliente = new Cliente { CodCliente = 1, NomCliente = "Test" };
        var plan = new MembershipPlan { Id = 1, Nombre = "Plan", PrecioMensual = 10, EntradasMensuales = 2, Nivel = 1, Activo = true };
        var membership = new CustomerMembership { CodCliente = 1, PlanId = 1, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddMonths(1), Estado = "Activo" };
        context.Clientes.Add(cliente);
        context.MembershipPlans.Add(plan);
        context.CustomerMemberships.Add(membership);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);
        var dto = new CreateCustomerMembershipDto
        {
            CodCliente = 1,
            PlanId = 1,
            FechaInicio = DateTime.Now,
            FechaFin = DateTime.Now.AddMonths(1)
        };

        // Act
        var result = await controller.CreateMembership(dto);

        // Assert
        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal("Ya existe una membresía activa para este cliente y plan.", conflict.Value);
        context.Dispose();
    }

    // --- UpdateMembership: Membresía no encontrada ---
    // Verifica que UpdateMembership devuelva un HTTP 404 si la membresía no existe.
    [Fact]
    public async Task UpdateMembership_ReturnsNotFound_WhenMembershipDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var controller = new CustomerMembershipsController(context);
        var dto = new CreateCustomerMembershipDto
        {
            CodCliente = 1,
            PlanId = 1,
            FechaInicio = DateTime.Now,
            FechaFin = DateTime.Now.AddMonths(1)
        };

        // Act
        var result = await controller.UpdateMembership(99, dto);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Membresía no encontrada o inactiva.", notFound.Value);
        context.Dispose();
    }

    // --- UpdateMembership: Cliente no existe ---
    // Verifica que UpdateMembership devuelva un HTTP 400 si el cliente no existe.
    [Fact]
    public async Task UpdateMembership_ReturnsBadRequest_WhenClienteDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var plan = new MembershipPlan { Id = 1, Nombre = "Plan", PrecioMensual = 10, EntradasMensuales = 2, Nivel = 1, Activo = true };
        var membership = new CustomerMembership { Id = 1, CodCliente = 1, PlanId = 1, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddMonths(1), Estado = "Activo" };
        context.MembershipPlans.Add(plan);
        context.CustomerMemberships.Add(membership);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);
        var dto = new CreateCustomerMembershipDto
        {
            CodCliente = 99, // Cliente inexistente
            PlanId = 1,
            FechaInicio = DateTime.Now,
            FechaFin = DateTime.Now.AddMonths(1)
        };

        // Act
        var result = await controller.UpdateMembership(1, dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("El cliente especificado no existe.", badRequest.Value);
        context.Dispose();
    }

    // --- UpdateMembership: Plan no existe ---
    // Verifica que UpdateMembership devuelva un HTTP 400 si el plan no existe.
    [Fact]
    public async Task UpdateMembership_ReturnsBadRequest_WhenPlanDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var cliente = new Cliente { CodCliente = 1, NomCliente = "Test" };
        var membership = new CustomerMembership { Id = 1, CodCliente = 1, PlanId = 1, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddMonths(1), Estado = "Activo" };
        context.Clientes.Add(cliente);
        context.CustomerMemberships.Add(membership);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);
        var dto = new CreateCustomerMembershipDto
        {
            CodCliente = 1,
            PlanId = 99, // Plan inexistente
            FechaInicio = DateTime.Now,
            FechaFin = DateTime.Now.AddMonths(1)
        };

        // Act
        var result = await controller.UpdateMembership(1, dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("El plan especificado no existe.", badRequest.Value);
        context.Dispose();
    }


    // --- UpdateMembership: Ya existe otra membresía activa ---
    // Verifica que UpdateMembership devuelva un HTTP 409 si ya existe otra membresía activa para el cliente y plan.
    [Fact]
    public async Task UpdateMembership_ReturnsConflict_WhenOtherActiveMembershipExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var cliente = new Cliente { CodCliente = 1, NomCliente = "Test" };
        var plan = new MembershipPlan { Id = 1, Nombre = "Plan", PrecioMensual = 10, EntradasMensuales = 2, Nivel = 1, Activo = true };
        var membership1 = new CustomerMembership { Id = 1, CodCliente = 1, PlanId = 1, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddMonths(1), Estado = "Activo" };
        var membership2 = new CustomerMembership { Id = 2, CodCliente = 1, PlanId = 1, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddMonths(2), Estado = "Activo" };
        context.Clientes.Add(cliente);
        context.MembershipPlans.Add(plan);
        context.CustomerMemberships.AddRange(membership1, membership2);
        await context.SaveChangesAsync();

        var controller = new CustomerMembershipsController(context);
        var dto = new CreateCustomerMembershipDto
        {
            CodCliente = 1,
            PlanId = 1,
            FechaInicio = DateTime.Now,
            FechaFin = DateTime.Now.AddMonths(3)
        };

        // Act
        var result = await controller.UpdateMembership(1, dto);

        // Assert
        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("Ya existe otra membresía activa para este cliente y plan.", conflict.Value);
        context.Dispose();
    }


    // --- DeleteMembership: Membresía no encontrada o inactiva ---
    // Verifica que DeleteMembership devuelva un HTTP 404 si la membresía no existe o ya está inactiva.
    [Fact]
    public async Task DeleteMembership_ReturnsNotFound_WhenMembershipDoesNotExistOrInactive()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        // No agregamos membresía
        var controller = new CustomerMembershipsController(context);

        // Act
        var result = await controller.DeleteMembership(99);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Membresía no encontrada o ya inactiva.", notFound.Value);
        context.Dispose();
    }


}