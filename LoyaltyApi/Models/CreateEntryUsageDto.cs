using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para crear un registro de uso de entradas.
    /// </summary>
    public class CreateEntryUsageDto
    {
        /// <summary>
        /// Fecha de uso de la entrada (requerido, en formato UTC).
        /// Ejemplo: "2025-06-06T20:00:00Z".
        /// </summary>
        [Required(ErrorMessage = "La fecha de uso es requerida.")]
        public DateTime FechaUso { get; set; }

        /// <summary>
        /// Código del complejo donde se usó la entrada (opcional, decimal).
        /// Ejemplo: 1001.50.
        /// </summary>
        public decimal? CodComplejo { get; set; }

        /// <summary>
        /// Código de la función asociada (opcional, entero).
        /// Ejemplo: 5001.
        /// </summary>
        public int? CodFuncion { get; set; }

        /// <summary>
        /// ID de la entrada usada (opcional, entero).
        /// Ejemplo: 12345.
        /// </summary>
        public int? IdEntrada { get; set; }
    }
}
