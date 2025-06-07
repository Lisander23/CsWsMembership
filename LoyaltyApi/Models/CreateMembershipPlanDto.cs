using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para crear un plan de membresía.
    /// </summary>
    public class CreateMembershipPlanDto
    {
        /// <summary>
        /// Nombre del plan (requerido, máximo 100 caracteres).
        /// Ejemplo: "Premium".
        /// </summary>
        [Required(ErrorMessage = "El nombre del plan es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; }

        /// <summary>
        /// Precio mensual del plan (requerido, entre 0 y 999999.99).
        /// Ejemplo: 29.99.
        /// </summary>
        [Required(ErrorMessage = "El precio mensual es requerido.")]
        [Range(0, 999999.99, ErrorMessage = "El precio mensual debe estar entre 0 y 999999.99.")]
        public decimal PrecioMensual { get; set; }

        /// <summary>
        /// Número de entradas mensuales incluidas (requerido, mayor o igual a 0).
        /// Ejemplo: 10.
        /// </summary>
        [Required(ErrorMessage = "Las entradas mensuales son requeridas.")]
        [Range(0, int.MaxValue, ErrorMessage = "Las entradas mensuales deben ser mayores o iguales a 0.")]
        public int EntradasMensuales { get; set; }

        /// <summary>
        /// Meses máximos de acumulación de entradas (requerido, mayor o igual a 0).
        /// Ejemplo: 6.
        /// </summary>
        [Required(ErrorMessage = "Los meses de acumulación son requeridos.")]
        [Range(0, int.MaxValue, ErrorMessage = "Los meses de acumulación deben ser mayores o iguales a 0.")]
        public int MesesAcumulacionMax { get; set; }

        /// <summary>
        /// Nivel del plan (requerido, mayor o igual a 0).
        /// Ejemplo: 2 para un plan "Gold".
        /// </summary>
        [Required(ErrorMessage = "El nivel del plan es requerido.")]
        [Range(0, int.MaxValue, ErrorMessage = "El nivel debe ser mayor o igual a 0.")]
        public int Nivel { get; set; }

        /// <summary>
        /// Indica si el plan está activo (requerido).
        /// Ejemplo: true.
        /// </summary>
        [Required(ErrorMessage = "El estado activo es requerido.")]
        public bool Activo { get; set; }
    }
}
