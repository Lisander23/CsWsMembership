using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para crear un balance de entradas para una membresía.
    /// </summary>
    public class CreateEntryBalanceDto
    {
        /// <summary>
        /// Período del balance (opcional, formato yyyyMM, entre 200000 y 999999).
        /// Ejemplo: 202506 para junio de 2025.
        /// </summary>
        [Range(200000, 999999, ErrorMessage = "El período debe estar entre 200000 y 999999.")]
        public int? Periodo { get; set; }

        /// <summary>
        /// Número de entradas asignadas (requerido, mayor o igual a 0).
        /// Ejemplo: 10.
        /// </summary>
        [Required(ErrorMessage = "Las entradas asignadas son requeridas.")]
        [Range(0, int.MaxValue, ErrorMessage = "Las entradas asignadas deben ser mayores o iguales a 0.")]
        public int EntradasAsignadas { get; set; }

        /// <summary>
        /// Número de entradas usadas (requerido, mayor o igual a 0).
        /// Ejemplo: 3.
        /// </summary>
        [Required(ErrorMessage = "Las entradas usadas son requeridas.")]
        [Range(0, int.MaxValue, ErrorMessage = "Las entradas usadas deben ser mayores o iguales a 0.")]
        public int EntradasUsadas { get; set; }

        /// <summary>
        /// Fecha de vencimiento del balance (requerido, en formato UTC).
        /// Ejemplo: "2025-06-30T23:59:59Z".
        /// </summary>
        [Required(ErrorMessage = "La fecha de vencimiento es requerida.")]
        public DateTime FechaVencimiento { get; set; }
    }
}
