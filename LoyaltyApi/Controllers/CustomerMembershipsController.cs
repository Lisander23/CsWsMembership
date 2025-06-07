using Microsoft.AspNetCore.Mvc;
using LoyaltyApi.Data;
using LoyaltyApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace LoyaltyApi.Controllers
{
    [Route("api/memberships")]
    [ApiController]
    [Produces("application/json")]
    public class CustomerMembershipsController : ControllerBase
    {
        private readonly LoyaltyContext _context;

        public CustomerMembershipsController(LoyaltyContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea una nueva membresía para un cliente.
        /// </summary>
        /// <param name="createDto">Datos de la membresía a crear.</param>
        /// <returns>Detalles de la membresía creada.</returns>
        /// <response code="201">Membresía creada exitosamente.</response>
        /// <response code="400">Cliente o plan no existe, o plan inactivo.</response>
        /// <response code="409">Ya existe una membresía activa para este cliente y plan.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// {
        ///   "CodCliente": 123.45,
        ///   "PlanId": 1,
        ///   "FechaInicio": "2025-06-06T00:00:00Z",
        ///   "FechaFin": "2026-06-06T00:00:00Z",
        ///   "IdSuscripcionMP": "sub_123",
        ///   "IdClienteMP": "cust_456",
        ///   "MesesAcumulacionPersonalizado": 12
        /// }
        /// Ejemplo de respuesta:
        /// {
        ///   "data": {
        ///     "Id": 1,
        ///     "CodCliente": 123.45,
        ///     "PlanId": 1,
        ///     "NombrePlan": "Premium",
        ///     "FechaInicio": "2025-06-06T00:00:00Z",
        ///     "FechaFin": "2026-06-06T00:00:00Z",
        ///     "Estado": "ACTIVO",
        ///     "IdSuscripcionMP": "sub_123",
        ///     "IdClienteMP": "cust_456",
        ///     "MesesAcumulacionPersonalizado": 12
        ///   },
        ///   "timestamp": "2025-06-06T17:21:00Z"
        /// }
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMembership(CreateCustomerMembershipDto createDto)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(createDto.CodCliente);
                if (cliente == null)
                    return BadRequest(new { error = "El cliente especificado no existe." });

                var plan = await _context.MembershipPlans.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == createDto.PlanId);
                if (plan == null)
                    return BadRequest(new { error = "El plan especificado no existe." });
                if (!plan.Activo)
                    return BadRequest(new { error = "El plan especificado está inactivo." });

                if (await _context.CustomerMemberships.AnyAsync(cm =>
                    cm.CodCliente == createDto.CodCliente &&
                    cm.PlanId == createDto.PlanId &&
                    cm.Estado == "ACTIVO"))
                {
                    return Conflict(new { error = "Ya existe una membresía activa para este cliente y plan." });
                }

                if (createDto.FechaInicio > createDto.FechaFin)
                    return BadRequest(new { error = "La fecha de inicio debe ser anterior a la fecha de fin." });

                var membership = new Entities.CustomerMembership
                {
                    CodCliente = createDto.CodCliente,
                    PlanId = createDto.PlanId,
                    FechaInicio = createDto.FechaInicio,
                    FechaFin = createDto.FechaFin,
                    Estado = "ACTIVO",
                    IdSuscripcionMP = string.IsNullOrEmpty(createDto.IdSuscripcionMP) ? null : createDto.IdSuscripcionMP,
                    IdClienteMP = string.IsNullOrEmpty(createDto.IdClienteMP) ? null : createDto.IdClienteMP,
                    MesesAcumulacionPersonalizado = createDto.MesesAcumulacionPersonalizado == 0 ? null : createDto.MesesAcumulacionPersonalizado
                };

                _context.CustomerMemberships.Add(membership);
                await _context.SaveChangesAsync();

                var result = new CustomerMembershipDto
                {
                    Id = membership.Id,
                    CodCliente = membership.CodCliente,
                    PlanId = membership.PlanId,
                    FechaInicio = membership.FechaInicio,
                    FechaFin = membership.FechaFin,
                    Estado = membership.Estado,
                    IdSuscripcionMP = membership.IdSuscripcionMP ?? "",
                    IdClienteMP = membership.IdClienteMP ?? "",
                    MesesAcumulacionPersonalizado = membership.MesesAcumulacionPersonalizado
                };

                return CreatedAtAction(nameof(GetMembership), new { id = membership.Id }, new { data = result, timestamp = DateTime.UtcNow });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor al crear la membresía." });
            }
        }

        /// <summary>
        /// Obtiene todas las membresías activas.
        /// </summary>
        /// <returns>Lista de membresías activas con el conteo.</returns>
        /// <response code="200">Membresías obtenidas exitosamente.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// {
        ///   "count": 2,
        ///   "data": [
        ///     {
        ///       "Id": 1,
        ///       "CodCliente": 123.45,
        ///       "PlanId": 1,
        ///       "NombrePlan": "Premium",
        ///       "FechaInicio": "2025-06-06T00:00:00Z",
        ///       "FechaFin": "2026-06-06T00:00:00Z",
        ///       "Estado": "ACTIVO",
        ///       "IdSuscripcionMP": "sub_123",
        ///       "IdClienteMP": "cust_456",
        ///       "MesesAcumulacionPersonalizado": 12
        ///     },
        ///     {
        ///       "Id": 2,
        ///       "CodCliente": 678.90,
        ///       "PlanId": 2,
        ///       "NombrePlan": "Basic",
        ///       "FechaInicio": "2025-06-06T00:00:00Z",
        ///       "FechaFin": "2026-06-06T00:00:00Z",
        ///       "Estado": "ACTIVO",
        ///       "IdSuscripcionMP": "",
        ///       "IdClienteMP": "",
        ///       "MesesAcumulacionPersonalizado": null
        ///     }
        ///   ],
        ///   "timestamp": "2025-06-06T17:21:00Z"
        /// }
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMemberships()
        {
            try
            {
                var memberships = await _context.CustomerMemberships
                    .Where(cm => cm.Estado == "ACTIVO")
                    .Include(cm => cm.Plan)
                    .Select(cm => new CustomerMembershipDto
                    {
                        Id = cm.Id,
                        CodCliente = cm.CodCliente,
                        PlanId = cm.PlanId,
                        FechaInicio = cm.FechaInicio,
                        FechaFin = cm.FechaFin,
                        Estado = cm.Estado,
                        IdSuscripcionMP = cm.IdSuscripcionMP ?? "",
                        IdClienteMP = cm.IdClienteMP ?? "",
                        MesesAcumulacionPersonalizado = cm.MesesAcumulacionPersonalizado
                    })
                    .ToListAsync();

                return Ok(new { count = memberships.Count, data = memberships, timestamp = DateTime.UtcNow });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor al obtener las membresías." });
            }
        }

        /// <summary>
        /// Obtiene una membresía activa por ID.
        /// </summary>
        /// <param name="id">ID de la membresía.</param>
        /// <returns>Detalles de la membresía.</returns>
        /// <response code="200">Membresía obtenida exitosamente.</response>
        /// <response code="404">Membresía no encontrada o inactiva.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// {
        ///   "data": {
        ///     "Id": 1,
        ///     "CodCliente": 123.45,
        ///     "PlanId": 1,
        ///     "NombrePlan": "Premium",
        ///     "FechaInicio": "2025-06-06T00:00:00Z",
        ///     "FechaFin": "2026-06-06T00:00:00Z",
        ///     "Estado": "ACTIVO",
        ///     "IdSuscripcionMP": "sub_123",
        ///     "IdClienteMP": "cust_456",
        ///     "MesesAcumulacionPersonalizado": 12
        ///   },
        ///   "timestamp": "2025-06-06T17:21:00Z"
        /// }
        /// </remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMembership(int id)
        {
            try
            {
                var membership = await _context.CustomerMemberships
                    .Where(cm => cm.Estado == "ACTIVO" && cm.Id == id)
                    .Include(cm => cm.Plan)
                    .Select(cm => new CustomerMembershipDto
                    {
                        Id = cm.Id,
                        CodCliente = cm.CodCliente,
                        PlanId = cm.PlanId,
                        FechaInicio = cm.FechaInicio,
                        FechaFin = cm.FechaFin,
                        Estado = cm.Estado,
                        IdSuscripcionMP = cm.IdSuscripcionMP ?? "",
                        IdClienteMP = cm.IdClienteMP ?? "",
                        MesesAcumulacionPersonalizado = cm.MesesAcumulacionPersonalizado
                    })
                    .FirstOrDefaultAsync();

                if (membership == null)
                    return NotFound(new { error = "Membresía no encontrada o inactiva." });

                return Ok(new { data = membership, timestamp = DateTime.UtcNow });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor al obtener la membresía." });
            }
        }

        /// <summary>
        /// Actualiza una membresía activa.
        /// </summary>
        /// <param name="id">ID de la membresía.</param>
        /// <param name="updateDto">Datos actualizados de la membresía.</param>
        /// <returns>Ningún contenido si la actualización es exitosa.</returns>
        /// <response code="204">Membresía actualizada exitosamente.</response>
        /// <response code="400">Cliente o plan no existe, o plan inactivo.</response>
        /// <response code="404">Membresía no encontrada o inactiva.</response>
        /// <response code="409">Ya existe otra membresía activa para este cliente y plan.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// {
        ///   "CodCliente": 123.45,
        ///   "PlanId": 1,
        ///   "FechaInicio": "2025-06-06T00:00:00Z",
        ///   "FechaFin": "2026-06-06T00:00:00Z",
        ///   "IdSuscripcionMP": "sub_123",
        ///   "IdClienteMP": "cust_456",
        ///   "MesesAcumulacionPersonalizado": 12
        /// }
        /// </remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMembership(int id, CreateCustomerMembershipDto updateDto)
        {
            try
            {
                var membership = await _context.CustomerMemberships.FindAsync(id);
                if (membership == null || membership.Estado != "ACTIVO")
                    return NotFound(new { error = "Membresía no encontrada o inactiva." });

                var cliente = await _context.Clientes.FindAsync(updateDto.CodCliente);
                if (cliente == null)
                    return BadRequest(new { error = "El cliente especificado no existe." });

                var plan = await _context.MembershipPlans.FindAsync(updateDto.PlanId);
                if (plan == null)
                    return BadRequest(new { error = "El plan especificado no existe." });
                if (!plan.Activo)
                    return BadRequest(new { error = "El plan especificado está inactivo." });

                if (await _context.CustomerMemberships.AnyAsync(cm =>
                    cm.CodCliente == updateDto.CodCliente &&
                    cm.PlanId == updateDto.PlanId &&
                    cm.Estado == "ACTIVO" &&
                    cm.Id != id))
                {
                    return Conflict(new { error = "Ya existe otra membresía activa para este cliente y plan." });
                }

                if (updateDto.FechaInicio > updateDto.FechaFin)
                    return BadRequest(new { error = "La fecha de inicio debe ser anterior a la fecha de fin." });

                membership.CodCliente = updateDto.CodCliente;
                membership.PlanId = updateDto.PlanId;
                membership.FechaInicio = updateDto.FechaInicio;
                membership.FechaFin = updateDto.FechaFin;
                membership.IdSuscripcionMP = string.IsNullOrEmpty(updateDto.IdSuscripcionMP) ? null : updateDto.IdSuscripcionMP;
                membership.IdClienteMP = string.IsNullOrEmpty(updateDto.IdClienteMP) ? null : updateDto.IdClienteMP;
                membership.MesesAcumulacionPersonalizado = updateDto.MesesAcumulacionPersonalizado == 0 ? null : updateDto.MesesAcumulacionPersonalizado;

                _context.Entry(membership).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor al actualizar la membresía." });
            }
        }

        /// <summary>
        /// Desactiva una membresía (cambia estado a "INACTIVO").
        /// </summary>
        /// <param name="id">ID de la membresía.</param>
        /// <returns>Ningún contenido si la desactivación es exitosa.</returns>
        /// <response code="204">Membresía desactivada exitosamente.</response>
        /// <response code="404">Membresía no encontrada o ya inactiva.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMembership(int id)
        {
            try
            {
                var membership = await _context.CustomerMemberships.FindAsync(id);
                if (membership == null || membership.Estado != "ACTIVO")
                    return NotFound(new { error = "Membresía no encontrada o ya inactiva." });

                membership.Estado = "INACTIVO";
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor al desactivar la membresía." });
            }
        }


        /// <summary>
        /// Obtiene el estado de la membresía activa de un cliente.
        /// </summary>
        /// <param name="codCliente">Código del cliente.</param>
        /// <returns>Estado y detalles de la membresía, incluyendo entradas disponibles.</returns>
        /// <response code="200">Estado de membresía obtenido exitosamente.</response>
        /// <response code="400">Código de cliente inválido.</response>
        /// <response code="404">Cliente sin membresía activa o expirada.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// {
        ///   "data": {
        ///     "Estado": "ACTIVO",
        ///     "PlanId": 1,
        ///     "NombrePlan": "Premium",
        ///     "PrecioMensual": 29.99,
        ///     "EntradasMensuales": 10,
        ///     "EntradasDisponibles": 7,
        ///     "Nivel": 2,
        ///     "Beneficios": ["Entradas gratis", "Descuentos 10%"]
        ///   },
        ///   "timestamp": "2025-06-06T17:21:00Z"
        /// }
        /// </remarks>
        [HttpGet("status/{codCliente}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMembershipStatus(string codCliente)
        {
            try
            {
                if (!decimal.TryParse(codCliente, out var codClienteDecimal))
                    return BadRequest(new { error = "CodCliente inválido." });

                var membership = await _context.CustomerMemberships
                    .Include(cm => cm.Plan)
                    .Include(cm => cm.Plan.Benefits)
                    .Where(cm => cm.CodCliente == codClienteDecimal && cm.Estado == "ACTIVO")
                    .Select(cm => new
                    {
                        cm.Estado,
                        PlanId = cm.Plan.Id,
                        NombrePlan = cm.Plan.Nombre,
                        PrecioMensual = cm.Plan.PrecioMensual,
                        EntradasMensuales = cm.Plan.EntradasMensuales,
                        Nivel = cm.Plan.Nivel, // Ahora es int
                        Beneficios = cm.Plan.Benefits.Select(b => b.Observacion),
                        CustomerMembershipId = cm.Id,
                        cm.FechaFin
                    })
                    .FirstOrDefaultAsync();

                if (membership == null)
                    return NotFound(new { error = "Cliente sin membresía activa." });

                if (membership.FechaFin < DateTime.UtcNow)
                    return NotFound(new { error = "La membresía ha expirado." });

                var periodoActual = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));

                var entradasUtilizadas = await _context.EntryBalances
                    .Where(eb => eb.CustomerMembershipId == membership.CustomerMembershipId &&
                                 eb.Periodo == periodoActual)
                    .SumAsync(eb => (int?)eb.EntradasUsadas) ?? 0;

                var entradasDisponibles = membership.EntradasMensuales - entradasUtilizadas;

                var dto = new MembershipStatusDto
                {
                    Estado = membership.Estado,
                    PlanId = membership.PlanId,
                    NombrePlan = membership.NombrePlan,
                    PrecioMensual = membership.PrecioMensual,
                    EntradasMensuales = membership.EntradasMensuales,
                    EntradasDisponibles = entradasDisponibles,
                    Nivel = membership.Nivel, // Ahora es int
                    Beneficios = membership.Beneficios.ToList()
                };

                return Ok(new { data = dto, timestamp = DateTime.UtcNow });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error interno del servidor al obtener el estado de la membresía." });
            }
        }



        /// <summary>
        /// Obtiene el estado de la membresía activa de un cliente (ruta alternativa).
        /// </summary>
        /// <param name="codCliente">Código del cliente.</param>
        /// <returns>Estado y detalles de la membresía, incluyendo entradas disponibles.</returns>
        /// <response code="200">Estado de membresía obtenido exitosamente.</response>
        /// <response code="400">Código de cliente inválido.</response>
        /// <response code="404">Cliente sin membresía activa o expirada.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// {
        ///   "data": {
        ///     "Estado": "ACTIVO",
        ///     "PlanId": 1,
        ///     "NombrePlan": "Premium",
        ///     "PrecioMensual": 29.99,
        ///     "EntradasMensuales": 10,
        ///     "EntradasDisponibles": 7,
        ///     "Nivel": "Gold",
        ///     "Beneficios": ["Entradas gratis", "Descuentos 10%"]
        ///   },
        ///   "timestamp": "2025-06-06T17:21:00Z"
        /// }
        /// </remarks>
        [HttpGet("customer/{codCliente}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomerMembershipStatus(string codCliente)
        {
            return await GetMembershipStatus(codCliente);
        }
    }
}