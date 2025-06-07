namespace LoyaltyApi.Entities
{
    public class EntryBalance
    {
        public int Id { get; set; }
        public int CustomerMembershipId { get; set; } // FK a CustomerMembership
        public int? Periodo { get; set; } // int, nullable
        public int EntradasAsignadas { get; set; }
        public int EntradasUsadas { get; set; } = 0; // int, DEFAULT 0
        public DateTime FechaVencimiento { get; set; } // date
        public CustomerMembership CustomerMembership { get; set; }
        public ICollection<EntryUsage> Usages { get; set; }
    }
}
