using Microsoft.AspNetCore.Mvc;
using LoyaltyApi.Data;
using LoyaltyApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using LoyaltyApi.Entities;

namespace LoyaltyApi.Controllers
{
    [Route("api/benefits")]
    [ApiController]
    public class MembershipBenefitsController : ControllerBase
    {
        private readonly LoyaltyContext _context;

        public MembershipBenefitsController(LoyaltyContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Obtiene todos los beneficios de planes de membresía.
        /// </summary>
        /// <returns>Lista de beneficios.</returns>
        /// <response code="200">Beneficios obtenidos.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// GET /api/benefits
        /// Salida de ejemplo:
        /// {
        ///   "data": [
        ///     {
        ///       "id": 1,
        ///       "planId": 1,
        ///       "clave": "DESCUENTO_PELICULA",
        ///       "valor": 10.50,
        ///       "diasAplicables": "LUN-MIE",
        ///       "observacion": "Descuento del 10% en entradas de lunes a miércoles."
        ///     },
        ///     {
        ///       "id": 2,
        ///       "planId": 1,
        ///       "clave": "ENTRADA_GRATIS",
        ///       "valor": 1.00,
        ///       "diasAplicables": null,
        ///       "observacion": "Una entrada gratis por mes."
        ///     }
        ///   ],
        ///   "timestamp": "2025-06-06T22:02:00Z"
        /// }
        /// </example>
        [HttpGet]
        public async Task<IActionResult> GetBenefits()
        {
            var benefits = await _context.MembershipBenefits
                .Select(b => new MembershipBenefitDto
                {
                    Id = b.Id,
                    PlanId = b.PlanId,
                    Clave = b.Clave,
                    Valor = b.Valor,
                    DiasAplicables = b.DiasAplicables,
                    Observacion = b.Observacion
                })
                .ToListAsync();

            return Ok(new { data = benefits, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Obtiene un beneficio específico por su ID.
        /// </summary>
        /// <param name="id">ID del beneficio.</param>
        /// <returns>Beneficio encontrado.</returns>
        /// <response code="200">Beneficio obtenido.</response>
        /// <response code="404">Beneficio no encontrado.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// GET /api/benefits/1
        /// Salida de ejemplo:
        /// {
        ///   "data": {
        ///     "id": 1,
        ///     "planId": 1,
        ///     "clave": "DESCUENTO_PELICULA",
        ///     "valor": 10.50,
        ///     "diasAplicables": "LUN-MIE",
        ///     "observacion": "Descuento del 10% en entradas de lunes a miércoles."
        ///   },
        ///   "timestamp": "2025-06-06T22:02:00Z"
        /// }
        /// </example>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBenefit(int id)
        {
            var benefit = await _context.MembershipBenefits
                .Where(b => b.Id == id)
                .Select(b => new MembershipBenefitDto
                {
                    Id = b.Id,
                    PlanId = b.PlanId,
                    Clave = b.Clave,
                    Valor = b.Valor,
                    DiasAplicables = b.DiasAplicables,
                    Observacion = b.Observacion
                })
                .FirstOrDefaultAsync();

            if (benefit == null)
                return NotFound(new { error = "El beneficio no existe." });

            return Ok(new { data = benefit, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Crea un nuevo beneficio para un plan de membresía.
        /// </summary>
        /// <param name="createDto">Datos del beneficio a crear.</param>
        /// <returns>Beneficio creado.</returns>
        /// <response code="201">Beneficio creado.</response>
        /// <response code="400">Datos inválidos o plan inactivo.</response>
        /// <response code="409">Clave duplicada para el plan.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// POST /api/benefits
        /// Entrada de ejemplo:
        /// {
        ///   "clave": "DESCUENTO_PELICULA",
        ///   "valor": 10.50,
        ///   "diasAplicables": "LUN-MIE",
        ///   "observacion": "Descuento del 10% en entradas de lunes a miércoles.",
        ///   "planId": 1
        /// }
        /// Salida de ejemplo:
        /// {
        ///   "data": {
        ///     "id": 1,
        ///     "planId": 1,
        ///     "clave": "DESCUENTO_PELICULA",
        ///     "valor": 10.50,
        ///     "diasAplicables": "LUN-MIE",
        ///     "observacion": "Descuento del 10% en entradas de lunes a miércoles."
        ///   },
        ///   "timestamp": "2025-06-06T22:02:00Z"
        /// }
        /// </example>
        [HttpPost]
        public async Task<IActionResult> CreateBenefit([FromBody] CreateMembershipBenefitDto createDto)
        {
            var plan = await _context.MembershipPlans
                .FirstOrDefaultAsync(p => p.Id == createDto.PlanId && p.Activo);
            if (plan == null)
                return BadRequest(new { error = "El plan especificado no existe o está inactivo." });

            if (await _context.MembershipBenefits
                .AnyAsync(b => b.PlanId == createDto.PlanId && b.Clave == createDto.Clave))
                return Conflict(new { error = "Ya existe un beneficio con esta clave para el plan especificado." });

            var benefit = new MembershipBenefit
            {
                PlanId = createDto.PlanId,
                Clave = createDto.Clave,
                Valor = createDto.Valor,
                DiasAplicables = createDto.DiasAplicables,
                Observacion = createDto.Observacion
            };

            try
            {
                _context.MembershipBenefits.Add(benefit);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Error al crear el beneficio." });
            }

            var benefitDto = new MembershipBenefitDto
            {
                Id = benefit.Id,
                PlanId = benefit.PlanId,
                Clave = benefit.Clave,
                Valor = benefit.Valor,
                DiasAplicables = benefit.DiasAplicables,
                Observacion = benefit.Observacion
            };

            return CreatedAtAction(nameof(GetBenefit), new { id = benefit.Id },
                new { data = benefitDto, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Actualiza un beneficio existente.
        /// </summary>
        /// <param name="id">ID del beneficio.</param>
        /// <param name="updateDto">Datos actualizados del beneficio.</param>
        /// <returns>Sin contenido si exitoso.</returns>
        /// <response code="204">Beneficio actualizado.</response>
        /// <response code="400">Datos inválidos o plan inactivo.</response>
        /// <response code="404">Beneficio no encontrado.</response>
        /// <response code="409">Clave duplicada para el plan.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// PUT /api/benefits/1
        /// Entrada de ejemplo:
        /// {
        ///   "clave": "DESCUENTO_PELICULA",
        ///   "valor": 15.00,
        ///   "diasAplicables": "LUN-VIE",
        ///   "observacion": "Descuento del 15% en entradas de lunes a viernes.",
        ///   "planId": 1
        /// }
        /// Salida de ejemplo:
        /// (Sin contenido, código 204)
        /// </example>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBenefit(int id, [FromBody] CreateMembershipBenefitDto updateDto)
        {
            var benefit = await _context.MembershipBenefits
                .FirstOrDefaultAsync(b => b.Id == id);
            if (benefit == null)
                return NotFound(new { error = "El beneficio no existe." });

            var plan = await _context.MembershipPlans
                .FirstOrDefaultAsync(p => p.Id == updateDto.PlanId && p.Activo);
            if (plan == null)
                return BadRequest(new { error = "El plan especificado no existe o está inactivo." });

            if (updateDto.Clave != benefit.Clave && await _context.MembershipBenefits
                .AnyAsync(b => b.PlanId == updateDto.PlanId && b.Clave == updateDto.Clave && b.Id != id))
                return Conflict(new { error = "Ya existe un beneficio con esta clave para el plan especificado." });

            benefit.PlanId = updateDto.PlanId;
            benefit.Clave = updateDto.Clave;
            benefit.Valor = updateDto.Valor;
            benefit.DiasAplicables = updateDto.DiasAplicables;
            benefit.Observacion = updateDto.Observacion;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Error al actualizar el beneficio." });
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina un beneficio existente.
        /// </summary>
        /// <param name="id">ID del beneficio.</param>
        /// <returns>Sin contenido si exitoso.</returns>
        /// <response code="204">Beneficio eliminado.</response>
        /// <response code="404">Beneficio no encontrado.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// DELETE /api/benefits/1
        /// Salida de ejemplo:
        /// (Sin contenido, código 204)
        /// </example>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBenefit(int id)
        {
            var benefit = await _context.MembershipBenefits
                .FirstOrDefaultAsync(b => b.Id == id);
            if (benefit == null)
                return NotFound(new { error = "El beneficio no existe." });

            try
            {
                _context.MembershipBenefits.Remove(benefit);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Error al eliminar el beneficio." });
            }

            return NoContent();
        }
    }
}