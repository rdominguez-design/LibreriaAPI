namespace LibreriaAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        // El nombre con el que el empleado va a ingresar al sistema
        public string NombreUsuario { get; set; } = string.Empty;

        // La contraseña del empleado
        public string Password { get; set; } = string.Empty;

        // Para diferenciar si es un "Admin" (dueño) o un "Cajero" normal
        public string Rol { get; set; } = "Cajero";

        // ¡Baja Lógica! Por si un empleado renuncia y le cortamos el acceso
        public bool Activo { get; set; } = true;
    }
}
