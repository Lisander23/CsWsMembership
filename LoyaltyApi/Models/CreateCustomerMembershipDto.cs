using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para crear o actualizar una membresía de cliente.
    /// </summary>
    public class CreateCustomerMembershipDto
    {
        /// <summary>
        /// Código del cliente (requerido, decimal).
        /// </summary>
        [Required(ErrorMessage = "El código del cliente es requerido.")]
        public decimal CodCliente { get; set; }

        /// <summary>
        /// ID del plan de membresía (requerido, entero).
        /// </summary>
        [Required(ErrorMessage = "El ID del plan es requerido.")]
        public int PlanId { get; set; }

        /// <summary>
        /// Fecha de inicio de la membresía (requerido, en formato UTC).
        /// Ejemplo: "2025-06-06T00:00:00Z".
        /// </summary>
        [Required(ErrorMessage = "La fecha de inicio es requerida.")]
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha de fin de la membresía (opcional, en formato UTC).
        /// Ejemplo: "2026-06-06T00:00:00Z".
        /// </summary>
        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// ID de suscripción de Mercado Pago (opcional, máximo 100 caracteres).
        /// </summary>
        [StringLength(100, ErrorMessage = "El ID de suscripción de Mercado Pago no puede exceder los 100 caracteres.")]
        public string IdSuscripcionMP { get; set; }

        /// <summary>
        /// ID de cliente de Mercado Pago (opcional, máximo 100 caracteres).
        /// </summary>
        [StringLength(100, ErrorMessage = "El ID de cliente de Mercado Pago no puede exceder los 100 caracteres.")]
        public string IdClienteMP { get; set; }

        /// <summary>
        /// Meses de acumulación personalizados (opcional, nulo si no se usa).
        /// </summary>
        public int? MesesAcumulacionPersonalizado { get; set; }
    }
}
