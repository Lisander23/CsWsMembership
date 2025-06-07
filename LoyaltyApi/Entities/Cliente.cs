namespace LoyaltyApi.Entities
{
    public class Cliente
    {
        public decimal CodCliente { get; set; }
        public string NomCliente { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string TelCliente { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public decimal CantPagos { get; set; }
        public string VencTarjeta { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal CodBarrio { get; set; }
        public DateTime FecNac { get; set; }
        public decimal CodLocalidad { get; set; }
        public string NroPuerta { get; set; } = string.Empty;
        public string NroApto { get; set; } = string.Empty;
        public string CodPostal { get; set; } = string.Empty;
        public char Sexo { get; set; } = 'M';
        public decimal CodNacionalidad { get; set; }
        public string TelTrabajo { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string SitioWeb { get; set; } = string.Empty;
        public string EstadoCivil { get; set; } = string.Empty;
        public decimal CodOcupacion { get; set; }
        public string CaminoFoto { get; set; } = string.Empty;
        public string NombreFoto { get; set; } = string.Empty;
        public byte[]? Foto { get; set; }
        public bool Fidelizado { get; set; } = true;
        public string Login { get; set; } = string.Empty;
        public string Passw { get; set; } = string.Empty;
        public bool EnviaCarteleraAMail { get; set; }
        public bool AvisaEstrenos { get; set; }
        public bool AvisaPromoSnacks { get; set; }
        public bool EnviaEstadoPuntos { get; set; }
        public bool Habilitado { get; set; } = true;
        public decimal? Puntos { get; set; }
        public decimal IdComplejo { get; set; } = 1;
        public DateTime? FechaActualizacion { get; set; }
        public char? Pendiente { get; set; }
    }
}

