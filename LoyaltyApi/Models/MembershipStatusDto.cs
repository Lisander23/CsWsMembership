using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para el estado de una membresía de cliente.
    /// </summary>
    public class MembershipStatusDto
    {
        /// <summary>
        /// Estado de la membresía.
        /// Ejemplo: "ACTIVO".
        /// </summary>
        [Required(ErrorMessage = "El estado es requerido.")]
        public string Estado { get; set; }

        /// <summary>
        /// ID del plan de membresía.
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// Nombre del plan.
        /// Ejemplo: "Premium".
        /// </summary>
        [Required(ErrorMessage = "El nombre del plan es requerido.")]
        public string NombrePlan { get; set; }

        /// <summary>
        /// Precio mensual del plan.
        /// Ejemplo: 29.99.
        /// </summary>
        public decimal PrecioMensual { get; set; }

        /// <summary>
        /// Número de entradas mensuales incluidas.
        /// Ejemplo: 10.
        /// </summary>
        public int EntradasMensuales { get; set; }

        /// <summary>
        /// Número de entradas disponibles en el periodo actual.
        /// Ejemplo: 7.
        /// </summary>
        public int EntradasDisponibles { get; set; }

        /// <summary>
        /// Nivel del plan (entero).
        /// Ejemplo: 2 para "Gold".
        /// </summary>
        public int Nivel { get; set; }

        /// <summary>
        /// Lista de beneficios del plan.
        /// Ejemplo: ["Entradas gratis", "Descuentos 10%"].
        /// </summary>
        public List<string> Beneficios { get; set; }
    }
}
