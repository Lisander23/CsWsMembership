using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para representar un plan de membresía.
    /// </summary>
    public class MembershipPlanDto
    {
        /// <summary>
        /// ID del plan.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del plan (requerido, máximo 100 caracteres).
        /// Ejemplo: "Premium".
        /// </summary>
        [Required(ErrorMessage = "El nombre del plan es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; }

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
        /// Meses máximos de acumulación de entradas.
        /// Ejemplo: 6.
        /// </summary>
        public int MesesAcumulacionMax { get; set; }

        /// <summary>
        /// Nivel del plan (entero).
        /// Ejemplo: 2 para un plan "Gold".
        /// </summary>
        public int Nivel { get; set; }

        /// <summary>
        /// Indica si el plan está activo.
        /// Ejemplo: true.
        /// </summary>
        public bool Activo { get; set; }

        /// <summary>
        /// Fecha de creación del plan (en formato UTC).
        /// Ejemplo: "2025-06-06T00:00:00Z".
        /// </summary>
        public DateTime FechaCreacion { get; set; }
    }
}

