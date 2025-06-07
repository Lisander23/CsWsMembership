namespace LoyaltyApi.Entities
{
    public class MembershipBenefit
    {
        public int Id { get; set; }
        public int PlanId { get; set; } // FK a MembershipPlan
        public string Clave { get; set; } // nvarchar(50)
        public decimal Valor { get; set; } = 0; // decimal(10,2)
        public string? DiasAplicables { get; set; } // nvarchar(20)
        public string? Observacion { get; set; } // nvarchar(200)
        public MembershipPlan Plan { get; set; }
    }
}
