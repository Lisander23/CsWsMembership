namespace LoyaltyApi.Entities
{
    public class CustomerMembership
    {
        public int Id { get; set; }
        public decimal CodCliente { get; set; }
        public int PlanId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Estado { get; set; } = "ACTIVO"; // nvarchar(20), no nullable
        public string? IdSuscripcionMP { get; set; } // nvarchar(100), nullable
        public string? IdClienteMP { get; set; } // nvarchar(100), nullable
        public int? MesesAcumulacionPersonalizado { get; set; } // int, nullable
        public Cliente Cliente { get; set; }
        public MembershipPlan Plan { get; set; }
        public ICollection<MembershipPayment> Payments { get; set; }
        public ICollection<EntryBalance> Balances { get; set; }
    }
}
