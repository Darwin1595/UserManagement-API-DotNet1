namespace UsuariosApi.Entities;

public class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    // La contraseña NO se guarda en texto plano
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }

    public int Edad { get; set; }

    public string Universidad { get; set; } = string.Empty;

    public int Semestre { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}