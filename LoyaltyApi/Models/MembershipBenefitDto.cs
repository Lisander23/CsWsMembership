using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para representar un beneficio de un plan de membresía.
    /// </summary>
    public class MembershipBenefitDto
    {
        /// <summary>
        /// ID del beneficio.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del plan de membresía asociado.
        /// Ejemplo: 1.
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// Clave del beneficio (opcional, máximo 50 caracteres).
        /// Ejemplo: "DESCUENTO_PELICULA".
        /// </summary>
        [StringLength(50, ErrorMessage = "La clave no puede exceder los 50 caracteres.")]
        public string Clave { get; set; }

        /// <summary>
        /// Valor del beneficio.
        /// Ejemplo: 10.50.
        /// </summary>
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
    }
}
