using LoyaltyApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApi.Data
{
    public class LoyaltyContext : DbContext
    {
        public LoyaltyContext(DbContextOptions<LoyaltyContext> options) : base(options) { }

        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<MembershipBenefit> MembershipBenefits { get; set; }
        public DbSet<CustomerMembership> CustomerMemberships { get; set; }
        public DbSet<MembershipPayment> MembershipPayments { get; set; }
        public DbSet<EntryBalance> EntryBalances { get; set; }
        public DbSet<EntryUsage> EntryUsages { get; set; }
        public DbSet<Cliente> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapear entidades a tablas
            modelBuilder.Entity<MembershipPlan>()
                .ToTable("MEM_MembershipPlan", "dbo");

            modelBuilder.Entity<MembershipBenefit>()
                .ToTable("MEM_MembershipBenefit", "dbo");

            modelBuilder.Entity<CustomerMembership>()
                .ToTable("MEM_CustomerMembership", "dbo");

            modelBuilder.Entity<MembershipPayment>()
                .ToTable("MEM_MembershipPayment", "dbo");

            modelBuilder.Entity<EntryBalance>()
                .ToTable("MEM_EntryBalance", "dbo");

            modelBuilder.Entity<EntryUsage>()
                .ToTable("MEM_EntryUsage", "dbo");

            modelBuilder.Entity<Cliente>()
                .ToTable("Cliente", "dbo")
                .HasKey(c => c.CodCliente);

            // Configurar relaciones
            modelBuilder.Entity<MembershipPlan>()
                .HasMany(p => p.Benefits)
                .WithOne(b => b.Plan)
                .HasForeignKey(b => b.PlanId);

            modelBuilder.Entity<MembershipPlan>()
                .HasMany(p => p.CustomerMemberships)
                .WithOne(cm => cm.Plan)
                .HasForeignKey(cm => cm.PlanId);

            modelBuilder.Entity<CustomerMembership>()
                .HasOne(cm => cm.Cliente)
                .WithMany()
                .HasForeignKey(cm => cm.CodCliente);

            modelBuilder.Entity<CustomerMembership>()
                .HasMany(cm => cm.Payments)
                .WithOne(p => p.CustomerMembership)
                .HasForeignKey(p => p.CustomerMembershipId);

            modelBuilder.Entity<CustomerMembership>()
                .HasMany(cm => cm.Balances)
                .WithOne(b => b.CustomerMembership)
                .HasForeignKey(b => b.CustomerMembershipId);

            modelBuilder.Entity<EntryBalance>()
                .HasMany(b => b.Usages)
                .WithOne(u => u.EntryBalance)
                .HasForeignKey(u => u.EntryBalanceId);

            // Configurar índices y restricciones
            modelBuilder.Entity<CustomerMembership>()
                .HasIndex(cm => cm.CodCliente);

            modelBuilder.Entity<MembershipPlan>()
                .HasIndex(p => p.Nombre)
                .IsUnique();

            // Configurar tipos de datos y restricciones
            modelBuilder.Entity<MembershipPlan>()
                .Property(p => p.PrecioMensual)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            modelBuilder.Entity<MembershipPlan>()
                .Property(p => p.Nombre)
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            modelBuilder.Entity<MembershipPlan>()
                .Property(p => p.EntradasMensuales)
                .HasColumnType("int")
                .IsRequired();

            modelBuilder.Entity<MembershipPlan>()
                .Property(p => p.MesesAcumulacionMax)
                .HasColumnType("int")
                .IsRequired();

            modelBuilder.Entity<MembershipPlan>()
                .Property(p => p.Nivel)
                .HasColumnType("int")
                .IsRequired();

            modelBuilder.Entity<MembershipPlan>()
                .Property(p => p.Activo)
                .HasColumnType("bit")
                .IsRequired();

            modelBuilder.Entity<MembershipPlan>()
                .Property(p => p.FechaCreacion)
                .HasColumnType("datetime")
                .IsRequired();

            modelBuilder.Entity<MembershipBenefit>()
                .Property(b => b.PlanId)
                .HasColumnType("int")
                .IsRequired();

            modelBuilder.Entity<MembershipBenefit>()
                .Property(b => b.Valor)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<CustomerMembership>()
                .Property(cm => cm.CodCliente)
                .HasColumnType("numeric(18,0)")
                .IsRequired();

            modelBuilder.Entity<CustomerMembership>()
                .Property(cm => cm.PlanId)
                .HasColumnType("int")
                .IsRequired();

            modelBuilder.Entity<CustomerMembership>()
                .Property(cm => cm.FechaInicio)
                .HasColumnType("date")
                .IsRequired();

            modelBuilder.Entity<CustomerMembership>()
                .Property(cm => cm.FechaFin)
                .HasColumnType("date");

            modelBuilder.Entity<CustomerMembership>()
                .Property(cm => cm.Estado)
                .HasColumnType("nvarchar(20)")
                .IsRequired();

            modelBuilder.Entity<CustomerMembership>()
                .Property(cm => cm.IdSuscripcionMP)
                .HasColumnType("nvarchar(100)");

            modelBuilder.Entity<CustomerMembership>()
                .Property(cm => cm.IdClienteMP)
                .HasColumnType("nvarchar(100)");

            modelBuilder.Entity<CustomerMembership>()
                .Property(cm => cm.MesesAcumulacionPersonalizado)
                .HasColumnType("int");

            modelBuilder.Entity<MembershipPayment>()
                .Property(p => p.Monto)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<EntryUsage>()
                .Property(u => u.CodComplejo)
                .HasColumnType("numeric(18,0)");

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>()
                .Property(c => c.CodCliente)
                .HasColumnType("numeric(18,0)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.NomCliente)
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Apellido)
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.TelCliente)
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Direccion)
                .HasColumnType("nvarchar(250)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Observaciones)
                .HasColumnType("nvarchar(1000)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.CantPagos)
                .HasColumnType("decimal(18,0)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.VencTarjeta)
                .HasColumnType("char(10)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Email)
                .HasColumnType("nvarchar(120)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.CodBarrio)
                .HasColumnType("numeric(18,0)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.FecNac)
                .HasColumnType("datetime")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.CodLocalidad)
                .HasColumnType("numeric(18,0)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.NroPuerta)
                .HasColumnType("varchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.NroApto)
                .HasColumnType("varchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.CodPostal)
                .HasColumnType("varchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Sexo)
                .HasColumnType("char(1)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.CodNacionalidad)
                .HasColumnType("numeric(18,0)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.TelTrabajo)
                .HasColumnType("varchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Celular)
                .HasColumnType("varchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Fax)
                .HasColumnType("varchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.SitioWeb)
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.EstadoCivil)
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.CodOcupacion)
                .HasColumnType("numeric(18,0)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.CaminoFoto)
                .HasColumnType("nvarchar(200)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.NombreFoto)
                .HasColumnType("nvarchar(200)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Foto)
                .HasColumnType("varbinary(max)");

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Fidelizado)
                .HasColumnType("bit")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Login)
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Passw)
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.EnviaCarteleraAMail)
                .HasColumnType("bit")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.AvisaEstrenos)
                .HasColumnType("bit")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.AvisaPromoSnacks)
                .HasColumnType("bit")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.EnviaEstadoPuntos)
                .HasColumnType("bit")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Habilitado)
                .HasColumnType("bit")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Puntos)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Cliente>()
                .Property(c => c.IdComplejo)
                .HasColumnType("numeric(18,0)")
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.FechaActualizacion)
                .HasColumnType("datetime");

            modelBuilder.Entity<Cliente>()
                .Property(c => c.Pendiente)
                .HasColumnType("char(1)");
        }
    }
}