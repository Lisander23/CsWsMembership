using Microsoft.AspNetCore.Mvc;
using LoyaltyApi.Data;
using LoyaltyApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using LoyaltyApi.Entities;

namespace LoyaltyApi.Controllers
{
    [Route("api/plans")]
    [ApiController]
    public class MembershipPlansController : ControllerBase
    {
        private readonly LoyaltyContext _context;

        public MembershipPlansController(LoyaltyContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los planes de membresía activos.
        /// </summary>
        /// <returns>Lista de planes activos.</returns>
        /// <response code="200">Planes obtenidos.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// GET /api/plans
        /// Salida de ejemplo:
        /// {
        ///   "data": [
        ///     {
        ///       "id": 1,
        ///       "nombre": "Plan Básico",
        ///       "precioMensual": 29.99,
        ///       "entradasMensuales": 4,
        ///       "mesesAcumulacionMax": 3,
        ///       "nivel": 1,
        ///       "activo": true,
        ///       "fechaCreacion": "2025-06-06T00:00:00Z"
        ///     },
        ///     {
        ///       "id": 2,
        ///       "nombre": "Plan Premium",
        ///       "precioMensual": 49.99,
        ///       "entradasMensuales": 8,
        ///       "mesesAcumulacionMax": 6,
        ///       "nivel": 2,
        ///       "activo": true,
        ///       "fechaCreacion": "2025-06-06T00:00:00Z"
        ///     }
        ///   ],
        ///   "timestamp": "2025-06-06T22:02:00Z"
        /// }
        /// </example>
        [HttpGet]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _context.MembershipPlans
                .Where(p => p.Activo)
                .Select(p => new MembershipPlanDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    PrecioMensual = p.PrecioMensual,
                    EntradasMensuales = p.EntradasMensuales,
                    MesesAcumulacionMax = p.MesesAcumulacionMax,
                    Nivel = p.Nivel,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion
                })
                .ToListAsync();

            return Ok(new { data = plans, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Obtiene un plan de membresía específico por su ID.
        /// </summary>
        /// <param name="id">ID del plan.</param>
        /// <returns>Plan encontrado.</returns>
        /// <response code="200">Plan obtenido.</response>
        /// <response code="404">Plan no encontrado o inactivo.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// GET /api/plans/1
        /// Salida de ejemplo:
        /// {
        ///   "data": {
        ///     "id": 1,
        ///     "nombre": "Plan Básico",
        ///     "precioMensual": 29.99,
        ///     "entradasMensuales": 4,
        ///     "mesesAcumulacionMax": 3,
        ///     "nivel": 1,
        ///     "activo": true,
        ///     "fechaCreacion": "2025-06-06T00:00:00Z"
        ///   },
        ///   "timestamp": "2025-06-06T22:02:00Z"
        /// }
        /// </example>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlan(int id)
        {
            var plan = await _context.MembershipPlans
                .Where(p => p.Id == id && p.Activo)
                .Select(p => new MembershipPlanDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    PrecioMensual = p.PrecioMensual,
                    EntradasMensuales = p.EntradasMensuales,
                    MesesAcumulacionMax = p.MesesAcumulacionMax,
                    Nivel = p.Nivel,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion
                })
                .FirstOrDefaultAsync();

            if (plan == null)
                return NotFound(new { error = "El plan no existe o está inactivo." });

            return Ok(new { data = plan, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Crea un nuevo plan de membresía.
        /// </summary>
        /// <param name="createDto">Datos del plan a crear.</param>
        /// <returns>Plan creado.</returns>
        /// <response code="201">Plan creado.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="409">Ya existe un plan activo con ese nombre.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// POST /api/plans
        /// Entrada de ejemplo:
        /// {
        ///   "nombre": "Plan Básico",
        ///   "precioMensual": 29.99,
        ///   "entradasMensuales": 4,
        ///   "mesesAcumulacionMax": 3,
        ///   "nivel": 1,
        ///   "activo": true
        /// }
        /// Salida de ejemplo:
        /// {
        ///   "data": {
        ///     "id": 1,
        ///     "nombre": "Plan Básico",
        ///     "precioMensual": 29.99,
        ///     "entradasMensuales": 4,
        ///     "mesesAcumulacionMax": 3,
        ///     "nivel": 1,
        ///     "activo": true,
        ///     "fechaCreacion": "2025-06-06T22:02:00Z"
        ///   },
        ///   "timestamp": "2025-06-06T22:02:00Z"
        /// }
        /// </example>
        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] CreateMembershipPlanDto createDto)
        {
            if (await _context.MembershipPlans.AnyAsync(p => p.Nombre == createDto.Nombre && p.Activo))
                return Conflict(new { error = "Ya existe un plan activo con ese nombre." });

            if (createDto.MesesAcumulacionMax != null && (createDto.MesesAcumulacionMax < 1 || createDto.MesesAcumulacionMax > 12))
                return BadRequest(new { error = "Los meses de acumulación máxima deben estar entre 1 y 12." });

            var plan = new MembershipPlan
            {
                Nombre = createDto.Nombre,
                PrecioMensual = createDto.PrecioMensual,
                EntradasMensuales = createDto.EntradasMensuales,
                MesesAcumulacionMax = createDto.MesesAcumulacionMax,
                Nivel = createDto.Nivel,
                Activo = createDto.Activo,
                FechaCreacion = DateTime.UtcNow
            };

            try
            {
                _context.MembershipPlans.Add(plan);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Error al crear el plan." });
            }

            var planDto = new MembershipPlanDto
            {
                Id = plan.Id,
                Nombre = plan.Nombre,
                PrecioMensual = plan.PrecioMensual,
                EntradasMensuales = plan.EntradasMensuales,
                MesesAcumulacionMax = plan.MesesAcumulacionMax,
                Nivel = plan.Nivel,
                Activo = plan.Activo,
                FechaCreacion = plan.FechaCreacion
            };

            return CreatedAtAction(nameof(GetPlan), new { id = plan.Id },
                new { data = planDto, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Actualiza un plan de membresía existente.
        /// </summary>
        /// <param name="id">ID del plan.</param>
        /// <param name="updateDto">Datos actualizados del plan.</param>
        /// <returns>Sin contenido si exitoso.</returns>
        /// <response code="204">Plan actualizado.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="404">Plan no encontrado.</response>
        /// <response code="409">Ya existe otro plan activo con ese nombre.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// PUT /api/plans/1
        /// Entrada de ejemplo:
        /// {
        ///   "nombre": "Plan Básico Actualizado",
        ///   "precioMensual": 34.99,
        ///   "entradasMensuales": 5,
        ///   "mesesAcumulacionMax": 4,
        ///   "nivel": 1,
        ///   "activo": true
        /// }
        /// Salida de ejemplo:
        /// (Sin contenido, código 204)
        /// </example>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] CreateMembershipPlanDto updateDto)
        {
            var plan = await _context.MembershipPlans
                .FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null)
                return NotFound(new { error = "El plan no existe." });

            if (updateDto.Nombre != plan.Nombre && await _context.MembershipPlans
                .AnyAsync(p => p.Nombre == updateDto.Nombre && p.Id != id && p.Activo))
                return Conflict(new { error = "Ya existe otro plan activo con ese nombre." });

            if (updateDto.MesesAcumulacionMax != null && (updateDto.MesesAcumulacionMax < 1 || updateDto.MesesAcumulacionMax > 12))
                return BadRequest(new { error = "Los meses de acumulación máxima deben estar entre 1 y 12." });

            plan.Nombre = updateDto.Nombre;
            plan.PrecioMensual = updateDto.PrecioMensual;
            plan.EntradasMensuales = updateDto.EntradasMensuales;
            plan.MesesAcumulacionMax = updateDto.MesesAcumulacionMax;
            plan.Nivel = updateDto.Nivel;
            plan.Activo = updateDto.Activo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Error al actualizar el plan." });
            }

            return NoContent();
        }


        /// <summary>
        /// Desactiva un plan de membresía existente (soft delete).
        /// </summary>
        /// <param name="id">ID del plan.</param>
        /// <returns>Sin contenido si exitoso.</returns>
        /// <response code="204">Plan desactivado.</response>
        /// <response code="404">Plan no encontrado o ya inactivo.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// DELETE /api/plans/1
        /// Salida de ejemplo:
        /// (Sin contenido, código 204)
        /// </example>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var plan = await _context.MembershipPlans
                .FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null || !plan.Activo)
                return NotFound(new { error = "El plan no existe o ya está inactivo." });

            plan.Activo = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Error al desactivar el plan." });
            }

            return NoContent();
        }
    }
}