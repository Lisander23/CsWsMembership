using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para representar un balance de entradas.
    /// </summary>
    public class EntryBalanceDto
    {
        /// <summary>
        /// ID del balance.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la membresía asociada.
        /// Ejemplo: 1.
        /// </summary>
        public int CustomerMembershipId { get; set; }

        /// <summary>
        /// Período del balance (opcional, formato yyyyMM).
        /// Ejemplo: 202506.
        /// </summary>
        [Range(200000, 999999, ErrorMessage = "El período debe estar entre 200000 y 999999.")]
        public int? Periodo { get; set; }

        /// <summary>
        /// Número de entradas asignadas.
        /// Ejemplo: 10.
        /// </summary>
        public int EntradasAsignadas { get; set; }

        /// <summary>
        /// Número de entradas usadas.
        /// Ejemplo: 3.
        /// </summary>
        public int EntradasUsadas { get; set; }

        /// <summary>
        /// Fecha de vencimiento del balance (en formato UTC).
        /// Ejemplo: "2025-06-30T23:59:59Z".
        /// </summary>
        public DateTime FechaVencimiento { get; set; }
    }
}
