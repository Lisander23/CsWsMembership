namespace LoyaltyApi.Entities
{
    public class MembershipPlan
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioMensual { get; set; }
        public int EntradasMensuales { get; set; }
        public int MesesAcumulacionMax { get; set; }
        public int Nivel { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public ICollection<MembershipBenefit> Benefits { get; set; }
        public ICollection<CustomerMembership> CustomerMemberships { get; set; }
    }
}
