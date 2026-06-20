using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibreriaAPI.Data;
using LibreriaAPI.Models;

namespace LibreriaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LibreriaContext _context;

        public AuthController(LibreriaContext context)
        {
            _context = context;
        }

        // POST: api/Auth/login
        // Este es el "patovica" del sistema. Revisa la lista de invitados en la Base de Datos.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Buscamos si existe un usuario activo con ese nombre exacto y esa contraseña exacta
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == request.Username && u.Password == request.Password && u.Activo == true);

            if (usuario == null)
            {
                // Error 401 Unauthorized (No autorizado)
                return Unauthorized("Usuario o contraseña incorrectos.");
            }

            // Si encontró al usuario en la base de datos, le damos la bienvenida
            return Ok(new
            {
                Mensaje = "Login exitoso",
                Usuario = usuario.NombreUsuario,
                Rol = usuario.Rol
            });
        }

        // POST: api/Auth/registrar 
        // Usaremos este método desde Swagger para crear a tu primer usuario administrador
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] Usuario nuevoUsuario)
        {
            // Verificamos que no exista otro empleado con el mismo nombre de usuario
            var existe = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nuevoUsuario.NombreUsuario);
            if (existe)
            {
                return BadRequest("Ya existe un usuario con ese nombre.");
            }

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();
            return Ok("Usuario creado con éxito en la base de datos.");
        }
    }

    // Un "molde" temporal pequeño (DTO) solo para recibir el intento de login
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}