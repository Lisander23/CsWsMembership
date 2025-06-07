using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoyaltyApi.Data;
using LoyaltyApi.Entities;
using LoyaltyApi.Models;

namespace LoyaltyApi.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class MembershipPaymentsController : ControllerBase
    {
        private readonly LoyaltyContext _context;

        public MembershipPaymentsController(LoyaltyContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los pagos de membresías.
        /// </summary>
        /// <returns>Lista de pagos.</returns>
        /// <response code="200">Pagos obtenidos.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// GET /api/payments
        /// Salida de ejemplo:
        /// {
        ///   "data": [
        ///     {
        ///       "id": 1,
        ///       "customerMembershipId": 1,
        ///       "fechaPago": "2025-06-06T00:00:00Z",
        ///       "monto": 29.99,
        ///       "estado": "COMPLETADO",
        ///       "referenciaExterna": "TXN_123456",
        ///       "periodo": 202506,
        ///       "observacion": "Pago procesado por Mercado Pago."
        ///     },
        ///     {
        ///       "id": 2,
        ///       "customerMembershipId": 1,
        ///       "fechaPago": "2025-07-06T00:00:00Z",
        ///       "monto": 29.99,
        ///       "estado": "PENDIENTE",
        ///       "referenciaExterna": null,
        ///       "periodo": 202507,
        ///       "observacion": null
        ///     }
        ///   ],
        ///   "timestamp": "2025-06-06T22:02:00Z"
        /// }
        /// </example>
        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _context.MembershipPayments
                .Select(p => new MembershipPaymentDto
                {
                    Id = p.Id,
                    CustomerMembershipId = p.CustomerMembershipId,
                    FechaPago = p.FechaPago,
                    Monto = p.Monto,
                    Estado = p.Estado,
                    ReferenciaExterna = p.ReferenciaExterna,
                    Periodo = p.Periodo,
                    Observaciones = p.Observaciones
                })
                .ToListAsync();

            return Ok(new { data = payments, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Obtiene un pago específico por su ID.
        /// </summary>
        /// <param name="id">ID del pago.</param>
        /// <returns>Pago encontrado.</returns>
        /// <response code="200">Pago obtenido.</response>
        /// <response code="404">Pago no encontrado.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// GET /api/payments/1
        /// Salida de ejemplo:
        /// {
        ///   "data": {
        ///     "id": 1,
        ///     "customerMembershipId": 1,
        ///     "fechaPago": "2025-06-06T00:00:00Z",
        ///     "monto": 29.99,
        ///     "estado": "COMPLETADO",
        ///     "referenciaExterna": "TXN_123456",
        ///     "periodo": 202506,
        ///       "observacion": "Pago procesado por Mercado Pago."
        ///   },
        ///   "timestamp": "2025-06-06T22:02:00Z"
        /// }
        /// </example>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            var payment = await _context.MembershipPayments
                .Where(p => p.Id == id)
                .Select(p => new MembershipPaymentDto
                {
                    Id = p.Id,
                    CustomerMembershipId = p.CustomerMembershipId,
                    FechaPago = p.FechaPago,
                    Monto = p.Monto,
                    Estado = p.Estado,
                    ReferenciaExterna = p.ReferenciaExterna,
                    Periodo = p.Periodo,
                    Observaciones = p.Observaciones
                })
                .FirstOrDefaultAsync();

            if (payment == null)
                return NotFound(new { error = "El pago no existe." });

            return Ok(new { data = payment, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Crea un nuevo pago para una membresía.
        /// </summary>
        /// <param name="createDto">Datos del pago a crear.</param>
        /// <returns>Pago creado.</returns>
        /// <response code="201">Pago creado.</response>
        /// <response code="400">Datos inválidos o membresía inactiva.</response>
        /// <response code="409">Pago duplicado para el período o referencia.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// POST /api/payments
        /// Entrada de ejemplo:
        /// {
        ///   "customerMembershipId": 1,
        ///   "fechaPago": "2025-06-06T00:00:00Z",
        ///   "monto": 29.99,
        ///   "estado": "COMPLETADO",
        ///   "referenciaExterna": "TXN_123456",
        ///   "periodo": 202506,
        ///   "observacion": "Pago procesado por Mercado Pago."
        /// }
        /// Salida de ejemplo:
        /// {
        ///   "data": {
        ///     "id": 1,
        ///     "customerMembershipId": 1,
        ///     "fechaPago": "2025-06-06T00:00:00Z",
        ///     "monto": 29.99,
        ///     "estado": "COMPLETADO",
        ///     "referenciaExterna": "TXN_123456",
        ///     "periodo": 202506,
        ///     "observacion": "Pago procesado por Mercado Pago."
        ///   },
        ///   "timestamp": "2025-06-06T22:02:00Z"
        /// }
        /// </example>
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreateMembershipPaymentDto createDto)
        {
            var membership = await _context.CustomerMemberships
                .FirstOrDefaultAsync(m => m.Id == createDto.CustomerMembershipId && m.Estado == "ACTIVO");
            if (membership == null)
                return BadRequest(new { error = "La membresía especificada no existe o está inactiva." });

            if (createDto.Periodo.HasValue && !IsValidPeriodo(createDto.Periodo.Value))
                return BadRequest(new { error = "El Periodo debe estar en formato yyyyMM (ej. 202506)." });

            if (createDto.FechaPago > DateTime.UtcNow)
                return BadRequest(new { error = "La FechaPago no puede ser futura." });

            if (createDto.Periodo.HasValue && await _context.MembershipPayments
                .AnyAsync(p => p.CustomerMembershipId == createDto.CustomerMembershipId && p.Periodo == createDto.Periodo))
                return Conflict(new { error = "Ya existe un pago para este período y membresía." });

            if (!string.IsNullOrEmpty(createDto.ReferenciaExterna) && await _context.MembershipPayments
                .AnyAsync(p => p.ReferenciaExterna == createDto.ReferenciaExterna))
                return Conflict(new { error = "Ya existe un pago con esta referencia externa." });

            var payment = new MembershipPayment
            {
                CustomerMembershipId = createDto.CustomerMembershipId,
                FechaPago = createDto.FechaPago,
                Monto = createDto.Monto,
                Estado = createDto.Estado,
                ReferenciaExterna = createDto.ReferenciaExterna,
                Periodo = createDto.Periodo,
                Observaciones = createDto.Observaciones
            };

            try
            {
                _context.MembershipPayments.Add(payment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Error al crear el pago." });
            }

            var dto = new MembershipPaymentDto
            {
                Id = payment.Id,
                CustomerMembershipId = payment.CustomerMembershipId,
                FechaPago = payment.FechaPago,
                Monto = payment.Monto,
                Estado = payment.Estado,
                ReferenciaExterna = payment.ReferenciaExterna,
                Periodo = payment.Periodo,
                Observaciones = payment.Observaciones
            };

            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id },
                new { data = dto, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Actualiza un pago existente.
        /// </summary>
        /// <param name="id">ID del pago.</param>
        /// <param name="updateDto">Datos actualizados del pago.</param>
        /// <returns>Sin contenido si exitoso.</returns>
        /// <response code="204">Pago actualizado.</response>
        /// <response code="400">Datos inválidos o membresía inactiva.</response>
        /// <response code="404">Pago no encontrado.</response>
        /// <response code="409">Pago duplicado para el período o referencia.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// PUT /api/payments/1
        /// Entrada de ejemplo:
        /// {
        ///   "customerMembershipId": 1,
        ///   "fechaPago": "2025-06-06T00:00:00Z",
        ///   "monto": 35.00,
        ///   "estado": "COMPLETADO",
        ///   "referenciaExterna": "TXN_123456",
        ///   "periodo": 202506,
        ///   "observacion": "Pago actualizado por Mercado Pago."
        /// }
        /// Salida de ejemplo:
        /// (Sin contenido, código 204)
        /// </example>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] CreateMembershipPaymentDto updateDto)
        {
            var payment = await _context.MembershipPayments
                .FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
                return NotFound(new { error = "El pago no existe." });

            var membership = await _context.CustomerMemberships
                .FirstOrDefaultAsync(m => m.Id == updateDto.CustomerMembershipId && m.Estado == "ACTIVO");
            if (membership == null)
                return BadRequest(new { error = "La membresía especificada no existe o está inactiva." });

            if (updateDto.Periodo.HasValue && !IsValidPeriodo(updateDto.Periodo.Value))
                return BadRequest(new { error = "El Periodo debe estar en formato yyyyMM (ej. 202506)." });

            if (updateDto.FechaPago > DateTime.UtcNow)
                return BadRequest(new { error = "La FechaPago no puede ser futura." });

            if (updateDto.Periodo.HasValue && updateDto.Periodo != payment.Periodo && await _context.MembershipPayments
                .AnyAsync(p => p.CustomerMembershipId == updateDto.CustomerMembershipId && p.Periodo == updateDto.Periodo && p.Id != id))
                return Conflict(new { error = "Ya existe un pago para este período y membresía." });

            if (!string.IsNullOrEmpty(updateDto.ReferenciaExterna) && updateDto.ReferenciaExterna != payment.ReferenciaExterna && await _context.MembershipPayments
                .AnyAsync(p => p.ReferenciaExterna == updateDto.ReferenciaExterna && p.Id != id))
                return Conflict(new { error = "Ya existe un pago con esta nueva referencia externa." });

            payment.CustomerMembershipId = updateDto.CustomerMembershipId;
            payment.FechaPago = updateDto.FechaPago;
            payment.Monto = updateDto.Monto;
            payment.Estado = updateDto.Estado;
            payment.ReferenciaExterna = updateDto.ReferenciaExterna;
            payment.Periodo = updateDto.Periodo;
            payment.Observaciones = updateDto.Observaciones;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Error al actualizar el pago." });
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina un pago existente.
        /// </summary>
        /// <param name="id">ID del pago.</param>
        /// <returns>Sin contenido si exitoso.</returns>
        /// <response code="204">Pago eliminado.</response>
        /// <response code="404">Pago no encontrado.</response>
        /// <response code="500">Error interno.</response>
        /// <example>
        /// DELETE /api/payments/1
        /// Salida de ejemplo:
        /// (Sin contenido, código 204)
        /// </example>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.MembershipPayments
                .FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
                return NotFound(new { error = "El pago no existe." });

            try
            {
                _context.MembershipPayments.Remove(payment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { error = "Error al eliminar el pago." });
            }

            return NoContent();
        }

        private bool IsValidPeriodo(int periodo)
        {
            var str = periodo.ToString();
            if (str.Length != 6)
                return false;

            if (!int.TryParse(str.Substring(0, 4), out var year) || year < 2000 || year > 9999)
                return false;

            if (!int.TryParse(str.Substring(4, 2), out var month) || month < 1 || month > 12)
                return false;

            return true;
        }
    }
}

