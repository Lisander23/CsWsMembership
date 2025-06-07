namespace LoyaltyApi.Models
{
    /// <summary>
    /// DTO para representar un registro de uso de entradas.
    /// </summary>
    public class EntryUsageDto
    {
        /// <summary>
        /// ID del registro de uso.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del balance de entradas asociado.
        /// Ejemplo: 1.
        /// </summary>
        public int EntryBalanceId { get; set; }

        /// <summary>
        /// Fecha de uso de la entrada (en formato UTC).
        /// Ejemplo: "2025-06-06T20:00:00Z".
        /// </summary>
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
