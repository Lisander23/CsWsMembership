using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoyaltyApi.Entities;
using LoyaltyApi.Data;
using LoyaltyApi.Models;

namespace LoyaltyApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class EntryUsagesController : ControllerBase
    {
        private readonly LoyaltyContext _context;

        public EntryUsagesController(LoyaltyContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene los registros de uso de entradas para un saldo específico.
        /// </summary>
        /// <param name="id">ID del saldo de entradas.</param>
        /// <returns>Lista de registros de uso.</returns>
        /// <response code="200">Registros obtenidos.</response>
        /// <response code="400">Saldo no válido o membresía inactiva.</response>
        /// <response code="404">Saldo no encontrado.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// GET /api/balances/1/usages
        /// Salida de ejemplo:
        /// {
        ///   "data": [
        ///     {
        ///       "id": 1,
        ///       "entryBalanceId": 1,
        ///       "fechaUso": "2025-06-06T20:00:00Z",
        ///       "codComplejo": 1001.50,
        ///       "codFuncion": 5001,
        ///       "idEntrada": 12345
        ///     },
        ///     {
        ///       "id": 2,
        ///       "entryBalanceId": 1,
        ///       "fechaUso": "2025-06-07T18:30:00Z",
        ///       "codComplejo": null,
        ///       "codFuncion": null,
        ///       "idEntrada": 12346
        ///     }
        ///   ],
        ///   "timestamp": "2025-06-06T22:02:00Z"
        /// }
        /// </example>
        [HttpGet("balances/{id}/usages")]
        public async Task<IActionResult> GetEntryUsages(int id)
        {
            var balance = await _context.EntryBalances
                .Include(b => b.CustomerMembership)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (balance == null)
                return NotFound(new { error = "El saldo no existe." });

            if (balance.CustomerMembership == null || balance.CustomerMembership.Estado != "ACTIVO")
                return BadRequest(new { error = "La membresía asociada no existe o está inactiva." });

            var usages = await _context.EntryUsages
                .Where(eu => eu.EntryBalanceId == id)
                .Select(eu => new EntryUsageDto
                {
                    Id = eu.Id,
                    EntryBalanceId = eu.EntryBalanceId,
                    FechaUso = eu.FechaUso,
                    CodComplejo = eu.CodComplejo,
                    CodFuncion = eu.CodFuncion,
                    IdEntrada = eu.IdEntrada
                })
                .ToListAsync();

            return Ok(new { data = usages, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Registra un nuevo uso de entrada para un saldo específico.
        /// </summary>
        /// <param name="id">ID del saldo de entradas.</param>
        /// <param name="dto">Datos del uso de la entrada.</param>
        /// <returns>Registro de uso creado.</returns>
        /// <response code="201">Uso registrado.</response>
        /// <response code="400">Datos inválidos, saldo vencido o sin entradas disponibles.</response>
        /// <response code="404">Saldo no encontrado.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// POST /api/balances/1/usages
        /// Entrada de ejemplo:
        /// {
        ///   "fechaUso": "2025-06-06T20:00:00Z",
        ///   "codComplejo": 1001.50,
        ///   "codFuncion": 5001,
        ///   "idEntrada": 12345
        /// }
        /// Salida de ejemplo:
        /// {
        ///   "data": {
        ///     "id": 1,
        ///     "entryBalanceId": 1,
        ///     "fechaUso": "2025-06-06T20:00:00Z",
        ///     "codComplejo": 1001.50,
        ///     "codFuncion": 5001,
        ///     "idEntrada": 12345
        ///   },
        ///   "timestamp": "2025-06-06T22:02:00Z"
        /// }
        /// </example>
        [HttpPost("balances/{id}/usages")]
        public async Task<IActionResult> CreateEntryUsage(int id, [FromBody] CreateEntryUsageDto dto)
        {
            var balance = await _context.EntryBalances
                .Include(b => b.CustomerMembership)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (balance == null)
                return NotFound(new { error = "El saldo no existe." });

            if (balance.CustomerMembership == null || balance.CustomerMembership.Estado != "ACTIVO")
                return BadRequest(new { error = "La membresía asociada no existe o está inactiva." });

            if (balance.FechaVencimiento < DateTime.UtcNow)
                return BadRequest(new { error = "El saldo está vencido." });

            if (balance.EntradasUsadas >= balance.EntradasAsignadas)
                return BadRequest(new { error = "No hay entradas disponibles en este saldo." });

            if (dto.FechaUso > DateTime.UtcNow)
                return BadRequest(new { error = "La FechaUso no puede ser futura." });

            var usage = new EntryUsage
            {
                EntryBalanceId = id,
                FechaUso = dto.FechaUso,
                CodComplejo = dto.CodComplejo,
                CodFuncion = dto.CodFuncion,
                IdEntrada = dto.IdEntrada
            };

            try
            {
                _context.EntryUsages.Add(usage);
                balance.EntradasUsadas += 1;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Error al registrar el uso de la entrada." });
            }

            var response = new EntryUsageDto
            {
                Id = usage.Id,
                EntryBalanceId = usage.EntryBalanceId,
                FechaUso = usage.FechaUso,
                CodComplejo = usage.CodComplejo,
                CodFuncion = usage.CodFuncion,
                IdEntrada = usage.IdEntrada
            };

            return CreatedAtAction(nameof(GetEntryUsages), new { id },
                new { data = response, timestamp = DateTime.UtcNow });
        }
    }
}
