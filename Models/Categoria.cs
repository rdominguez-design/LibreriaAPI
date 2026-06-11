namespace LibreriaAPI.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal MargenPredeterminado { get; set; }

        // Relación: Una categoría tiene muchos productos (Propiedad de navegación)
        public List<Producto> Productos { get; set; } = new();
    }
}