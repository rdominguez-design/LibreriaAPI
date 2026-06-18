using Microsoft.AspNetCore.Mvc;

namespace LibreriaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO credenciales)
        {
            // Más adelante conectaremos esto a tu tabla SQLite real.
            // Por ahora, blindamos la validación del lado del servidor.
            if (credenciales.Usuario == "admin" && credenciales.Password == "admin123")
            {
                // Status 200 OK (Acceso permitido)
                return Ok(new { mensaje = "Login exitoso", token = "token-seguro-12345" });
            }

            // Status 401 Unauthorized (Acceso Denegado)
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
        }
    }

    // Un "molde" (DTO) exclusivo para recibir los datos desde tu ventanita gris
    public class LoginDTO
    {
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}