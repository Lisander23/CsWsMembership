namespace LoyaltyApi.Entities
{
    public class EntryUsage
    {
        public int Id { get; set; }
        public int EntryBalanceId { get; set; } // FK a EntryBalance
        public DateTime FechaUso { get; set; } // datetime
        public decimal? CodComplejo { get; set; } // numeric(18,0)
        public int? CodFuncion { get; set; } // int, nullable
        public int? IdEntrada { get; set; } // int, nullable
        public EntryBalance EntryBalance { get; set; }
    }
}
