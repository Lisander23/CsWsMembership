using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para crear un beneficio asociado a un plan de membresía.
    /// </summary>
    public class CreateMembershipBenefitDto
    {
        /// <summary>
        /// Clave del beneficio (requerido, máximo 50 caracteres).
        /// Ejemplo: "DESCUENTO_PELICULA".
        /// </summary>
        [Required(ErrorMessage = "La clave del beneficio es requerida.")]
        [StringLength(50, ErrorMessage = "La clave no puede exceder los 50 caracteres.")]
        public string Clave { get; set; }

        /// <summary>
        /// Valor del beneficio (requerido, entre 0 y 999999.99).
        /// Ejemplo: 10.50 para un descuento de 10.50.
        /// </summary>
        [Required(ErrorMessage = "El valor del beneficio es requerido.")]
        [Range(0, 999999.99, ErrorMessage = "El valor debe estar entre 0 y 999999.99.")]
        public decimal Valor { get; set; }

        /// <summary>
        /// Días en los que aplica el beneficio (opcional, máximo 20 caracteres).
        /// Ejemplo: "LUN-MIE".
        /// </summary>
        [StringLength(20, ErrorMessage = "Los días aplicables no pueden exceder los 20 caracteres.")]
        public string DiasAplicables { get; set; }

        /// <summary>
        /// Observación o descripción del beneficio (opcional, máximo 200 caracteres).
        /// Ejemplo: "Descuento del 10% en entradas de lunes a miércoles."
        /// </summary>
        [StringLength(200, ErrorMessage = "La observación no puede exceder los 200 caracteres.")]
        public string Observacion { get; set; }

        /// <summary>
        /// ID del plan de membresía al que está asociado (requerido, entero).
        /// Ejemplo: 1.
        /// </summary>
        [Required(ErrorMessage = "El ID del plan es requerido.")]
        public int PlanId { get; set; }
    }
}
