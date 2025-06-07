using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para representar un pago de una membresía.
    /// </summary>
    public class MembershipPaymentDto
    {
        /// <summary>
        /// ID del pago.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la membresía asociada.
        /// Ejemplo: 1.
        /// </summary>
        public int CustomerMembershipId { get; set; }

        /// <summary>
        /// Fecha del pago (en formato UTC).
        /// Ejemplo: "2025-06-06T00:00:00Z".
        /// </summary>
        public DateTime FechaPago { get; set; }

        /// <summary>
        /// Monto del pago.
        /// Ejemplo: 29.99.
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Estado del pago (máximo 50 caracteres).
        /// Ejemplo: "COMPLETADO".
        /// </summary>
        [StringLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres.")]
        public string Estado { get; set; }

        /// <summary>
        /// Referencia externa del pago (opcional, máximo 100 caracteres).
        /// Ejemplo: "TXN_123456".
        /// </summary>
        [StringLength(100, ErrorMessage = "La referencia externa no puede exceder los 100 caracteres.")]
        public string ReferenciaExterna { get; set; }

        /// <summary>
        /// Período asociado al pago (opcional, formato yyyyMM).
        /// Ejemplo: 202506.
        /// </summary>
        [Range(200000, 999999, ErrorMessage = "El período debe estar entre 200000 y 999999.")]
        public int? Periodo { get; set; }

        /// <summary>
        /// Observaciones adicionales (opcional, máximo 200 caracteres).
        /// Ejemplo: "Pago procesado por Mercado Pago."
        /// </summary>
        [StringLength(200, ErrorMessage = "Las observaciones no pueden exceder los 200 caracteres.")]
        public string Observaciones { get; set; }
    }
}
