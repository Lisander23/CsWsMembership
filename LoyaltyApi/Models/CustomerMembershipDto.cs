using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para representar una membresía de cliente.
    /// </summary>
    public class CustomerMembershipDto
    {
        /// <summary>
        /// ID de la membresía.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Código del cliente.
        /// </summary>
        public decimal CodCliente { get; set; }

        /// <summary>
        /// ID del plan de membresía.
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// Nombre del plan de membresía.
        /// Ejemplo: "Premium".
        /// </summary>
        [Required(ErrorMessage = "El nombre del plan es requerido.")]
        public string NombrePlan { get; set; }

        /// <summary>
        /// Fecha de inicio de la membresía (UTC).
        /// Ejemplo: "2025-06-06T00:00:00Z".
        /// </summary>
        [Required(ErrorMessage = "La fecha de inicio es requerida.")]
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha de fin de la membresía (UTC, opcional).
        /// Ejemplo: "2026-06-06T00:00:00Z".
        /// </summary>
        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// Estado de la membresía (requerido, máximo 20 caracteres).
        /// Ejemplo: "ACTIVO" o "INACTIVO".
        /// </summary>
        [Required(ErrorMessage = "El estado es requerido.")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder los 20 caracteres.")]
        public string Estado { get; set; }

        /// <summary>
        /// ID de suscripción de Mercado Pago (vacío si no aplica, máximo 100 caracteres).
        /// </summary>
        [StringLength(100, ErrorMessage = "El ID de suscripción de Mercado Pago no puede exceder los 100 caracteres.")]
        public string IdSuscripcionMP { get; set; }

        /// <summary>
        /// ID de cliente de Mercado Pago (vacío si no aplica, máximo 100 caracteres).
        /// </summary>
        [StringLength(100, ErrorMessage = "El ID de cliente de Mercado Pago no puede exceder los 100 caracteres.")]
        public string IdClienteMP { get; set; }

        /// <summary>
        /// Meses de acumulación personalizados (nulo si no se usa).
        /// </summary>
        public int? MesesAcumulacionPersonalizado { get; set; }
    }
}
