using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para crear un registro de pago de una membresía.
    /// </summary>
    public class CreateMembershipPaymentDto
    {
        /// <summary>
        /// ID de la membresía asociada (requerido, entero).
        /// Ejemplo: 1.
        /// </summary>
        [Required(ErrorMessage = "El ID de la membresía es requerido.")]
        public int CustomerMembershipId { get; set; }

        /// <summary>
        /// Fecha del pago (requerido, en formato UTC).
        /// Ejemplo: "2025-06-06T00:00:00Z".
        /// </summary>
        [Required(ErrorMessage = "La fecha del pago es requerida.")]
        public DateTime FechaPago { get; set; }

        /// <summary>
        /// Monto del pago (requerido, mayor o igual a 0).
        /// Ejemplo: 29.99.
        /// </summary>
        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser mayor o igual a 0.")]
        public decimal Monto { get; set; }

        /// <summary>
        /// Estado del pago (requerido, máximo 50 caracteres).
        /// Ejemplo: "COMPLETADO".
        /// </summary>
        [Required(ErrorMessage = "El estado del pago es requerido.")]
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
