using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoyaltyApi.Entities;
using LoyaltyApi.Data;
using Microsoft.AspNetCore.Http;

namespace LoyaltyApi.Controllers
{
    [ApiController]
    [Route("api")]
    [Produces("application/json")]
    public class EntryBalancesController : ControllerBase
    {
        private readonly LoyaltyContext _context;

        public EntryBalancesController(LoyaltyContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene los saldos de entradas asociados a una membresía.
        /// </summary>
        /// <param name="id">ID de la membresía.</param>
        /// <returns>Lista de saldos encontrados.</returns>
        /// <response code="200">Saldos encontrados exitosamente.</response>
        /// <response code="404">No existe la membresía especificada.</response>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// 
        ///     {
        ///       "data": [
        ///         {
        ///           "Id": 1,
        ///           "Periodo": 202406,
        ///           "EntradasAsignadas": 10,
        ///           "EntradasUsadas": 2,
        ///           "FechaVencimiento": "2024-06-30T00:00:00Z"
        ///         }
        ///       ],
        ///       "timestamp": "2025-06-06T18:10:00Z"
        ///     }
        /// </remarks>
        [HttpGet("memberships/{id}/balances")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEntryBalances(int id)
        {
            var exists = await _context.CustomerMemberships.AnyAsync(cm => cm.Id == id);
            if (!exists)
                return NotFound(new { error = "La membresía especificada no existe." });

            var balances = await _context.EntryBalances
                .Where(b => b.CustomerMembershipId == id)
                .Select(b => new
                {
                    b.Id,
                    b.Periodo,
                    b.EntradasAsignadas,
                    b.EntradasUsadas,
                    b.FechaVencimiento
                })
                .ToListAsync();

            return Ok(new { data = balances, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Crea un nuevo saldo de entradas para una membresía.
        /// </summary>
        /// <param name="id">ID de la membresía.</param>
        /// <param name="dto">Datos del saldo a crear.</param>
        /// <returns>Saldo creado.</returns>
        /// <response code="201">Saldo creado correctamente.</response>
        /// <response code="400">Datos inválidos o membresía inexistente.</response>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// 
        ///     {
        ///       "Periodo": 202406,
        ///       "EntradasAsignadas": 10,
        ///       "FechaVencimiento": "2024-06-30T00:00:00Z"
        ///     }
        ///
        /// Ejemplo de respuesta:
        /// 
        ///     {
        ///       "data": {
        ///         "customerMembershipId": 1,
        ///         "periodo": 202406,
        ///         "entradasAsignadas": 10,
        ///         "entradasUsadas": 0,
        ///         "fechaVencimiento": "2024-06-30T00:00:00Z",
        ///         "id": 1
        ///       },
        ///       "timestamp": "2025-06-06T18:12:00Z"
        ///     }
        /// </remarks>
        [HttpPost("memberships/{id}/balances")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEntryBalance(int id, [FromBody] EntryBalanceDto dto)
        {
            var exists = await _context.CustomerMemberships.AnyAsync(cm => cm.Id == id);
            if (!exists)
                return BadRequest(new { error = "La membresía especificada no existe." });

            if (dto.EntradasAsignadas < 0)
                return BadRequest(new { error = "El número de entradas asignadas no puede ser negativo." });

            if (dto.Periodo <= 0)
                return BadRequest(new { error = "El período debe ser mayor a cero." });

            var entry = new EntryBalance
            {
                CustomerMembershipId = id,
                Periodo = dto.Periodo,
                EntradasAsignadas = dto.EntradasAsignadas,
                EntradasUsadas = 0,
                FechaVencimiento = dto.FechaVencimiento
            };

            _context.EntryBalances.Add(entry);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEntryBalances), new { id }, new { data = entry, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Actualiza los datos de un saldo de entradas.
        /// </summary>
        /// <param name="id">ID del saldo a actualizar.</param>
        /// <param name="dto">Campos a modificar.</param>
        /// <response code="204">Actualización exitosa.</response>
        /// <response code="404">No se encontró el saldo.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// 
        ///     {
        ///       "EntradasAsignadas": 12,
        ///       "EntradasUsadas": 4,
        ///       "FechaVencimiento": "2024-07-31T00:00:00Z"
        ///     }
        /// </remarks>
        [HttpPut("balances/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateBalance(int id, [FromBody] UpdateBalanceDto dto)
        {
            var balance = await _context.EntryBalances.FindAsync(id);
            if (balance == null)
                return NotFound(new { error = "El saldo especificado no existe." });

            if (dto.EntradasAsignadas.HasValue && dto.EntradasAsignadas < 0)
                return BadRequest(new { error = "Las entradas asignadas no pueden ser negativas." });

            if (dto.EntradasUsadas.HasValue && dto.EntradasUsadas < 0)
                return BadRequest(new { error = "Las entradas usadas no pueden ser negativas." });

            if (dto.EntradasAsignadas.HasValue)
                balance.EntradasAsignadas = dto.EntradasAsignadas.Value;

            if (dto.EntradasUsadas.HasValue)
                balance.EntradasUsadas = dto.EntradasUsadas.Value;

            if (dto.FechaVencimiento.HasValue)
                balance.FechaVencimiento = dto.FechaVencimiento.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// DTO para creación de saldos.
        /// </summary>
        public class EntryBalanceDto
        {
            public int Periodo { get; set; }
            public int EntradasAsignadas { get; set; }
            public DateTime FechaVencimiento { get; set; }
        }

        /// <summary>
        /// DTO para actualización de saldos.
        /// </summary>
        public class UpdateBalanceDto
        {
            public int? EntradasAsignadas { get; set; }
            public int? EntradasUsadas { get; set; }
            public DateTime? FechaVencimiento { get; set; }
        }
    }
}
