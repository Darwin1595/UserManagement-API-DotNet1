using UsuariosApi.DTOs;
using UsuariosApi.Entities;
using UsuariosApi.Interfaces;

namespace UsuariosApi.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repo;

    public UsuarioService(IUsuarioRepository repo)
    {
        _repo = repo;
    }

    // ==========================================
    // OBTENER TODOS
    // ==========================================
    public async Task<List<UsuarioDto>> ObtenerTodosAsync()
    {
        var usuarios = await _repo.ObtenerTodosAsync();

        return usuarios
            .Select(Map)
            .ToList();
    }

    // ==========================================
    // OBTENER POR ID
    // ==========================================
    public async Task<UsuarioDto?> ObtenerPorIdAsync(int id)
    {
        var usuario = await _repo.ObtenerPorIdAsync(id);

        return usuario is null
            ? null
            : Map(usuario);
    }

    // ==========================================
    // CREAR USUARIO
    // ==========================================
    public async Task<UsuarioDto> CrearAsync(CrearUsuarioDto d)
    {
        var correo = d.Correo
            .Trim()
            .ToLowerInvariant();

        // Validar que el correo no esté registrado
        if (await _repo.ObtenerPorCorreoAsync(correo) is not null)
        {
            throw new InvalidOperationException(
                "Ya existe un usuario con ese correo."
            );
        }

        // Convertir contraseña en HASH
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(d.Password);

        // Cálculo exacto de la edad considerando día, mes y año
        int edadCalculada = CalcularEdad(d.FechaNacimiento);

        var usuario = new Usuario
        {
            Nombre = d.Nombre.Trim(),
            Apellido = d.Apellido.Trim(),
            Correo = correo,
            PasswordHash = passwordHash,
            FechaNacimiento = d.FechaNacimiento,
            Edad = edadCalculada,
            Universidad = d.Universidad.Trim(),
            Semestre = d.Semestre
        };

        await _repo.CrearAsync(usuario);

        return Map(usuario);
    }

    // ==========================================
    // LOGIN
    // ==========================================
    public async Task<UsuarioDto?> LoginAsync(LoginDto dto)
    {
        var correo = dto.Correo
            .Trim()
            .ToLowerInvariant();

        // Buscar usuario por correo
        var usuario = await _repo.ObtenerPorCorreoAsync(correo);

        // Usuario no encontrado
        if (usuario is null)
        {
            return null;
        }

        // Validar que esté activo
        if (!usuario.Activo)
        {
            return null;
        }

        // Validar que tenga contraseña registrada
        if (string.IsNullOrWhiteSpace(usuario.PasswordHash))
        {
            return null;
        }

        // Comparar la contraseña ingresada con el HASH
        var passwordCorrecto = BCrypt.Net.BCrypt.Verify(
            dto.Password,
            usuario.PasswordHash
        );

        if (!passwordCorrecto)
        {
            return null;
        }

        // LOGIN CORRECTO
        return Map(usuario);
    }

    // ==========================================
    // ACTUALIZAR (CON CÁLCULO DE EDAD Y NUEVOS CAMPOS)
    // ==========================================
    public async Task<bool> ActualizarAsync(int id, ActualizarUsuarioDto d)
    {
        var usuario = await _repo.ObtenerPorIdAsync(id);

        if (usuario is null)
        {
            return false;
        }

        var correo = d.Correo
            .Trim()
            .ToLowerInvariant();

        var otro = await _repo.ObtenerPorCorreoAsync(correo);

        if (otro is not null && otro.Id != id)
        {
            throw new InvalidOperationException(
                "El correo pertenece a otro usuario."
            );
        }

        // Recalcular edad automáticamente basada en la FechaNacimiento
        int edadCalculada = CalcularEdad(d.FechaNacimiento);

        usuario.Nombre = d.Nombre.Trim();
        usuario.Apellido = d.Apellido.Trim();
        usuario.Correo = correo;
        usuario.FechaNacimiento = d.FechaNacimiento;
        usuario.Edad = edadCalculada;
        usuario.Universidad = d.Universidad.Trim();
        usuario.Semestre = d.Semestre;
        usuario.Activo = d.Activo;

        if (!string.IsNullOrWhiteSpace(d.Password))
        {
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(d.Password);
        }

        await _repo.ActualizarAsync(usuario);

        return true;
    }

    // ==========================================
    // ELIMINAR
    // ==========================================
    public async Task<bool> EliminarAsync(int id)
    {
        var usuario = await _repo.ObtenerPorIdAsync(id);

        if (usuario is null)
        {
            return false;
        }

        await _repo.EliminarAsync(usuario);

        return true;
    }

    // ==========================================
    // CÁLCULO DE EDAD DE FORMA EXACTA
    // ==========================================
    private static int CalcularEdad(DateTime fechaNacimiento)
    {
        var hoy = DateTime.Today;
        var edad = hoy.Year - fechaNacimiento.Year;

        // Si aún no ha cumplido años en este año calendario, restamos 1
        if (fechaNacimiento.Date > hoy.AddYears(-edad))
        {
            edad--;
        }

        return edad;
    }

    // ==========================================
    // MAPEO ENTITY -> DTO
    // ==========================================
    private static UsuarioDto Map(Usuario u)
    {
        return new UsuarioDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Correo = u.Correo,
            FechaNacimiento = u.FechaNacimiento,
            Edad = u.Edad,
            Universidad = u.Universidad,
            Semestre = u.Semestre,
            Activo = u.Activo,
            FechaCreacion = u.FechaCreacion
        };
    }
}