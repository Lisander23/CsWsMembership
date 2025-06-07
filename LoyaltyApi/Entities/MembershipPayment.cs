namespace LoyaltyApi.Entities
{
    public class MembershipPayment
    {
        public int Id { get; set; }
        public int CustomerMembershipId { get; set; } // FK a CustomerMembership
        public DateTime FechaPago { get; set; } // datetime
        public decimal Monto { get; set; } // decimal(10,2)
        public string Estado { get; set; } // nvarchar(20)
        public string ReferenciaExterna { get; set; } // nvarchar(100), nullable
        public int? Periodo { get; set; } // int, nullable
        public string? Observaciones { get; set; } // nvarchar(200), nullable
        public CustomerMembership CustomerMembership { get; set; }
    }
}
