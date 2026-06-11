namespace LibreriaAPI.Models
{
    public class Producto
    {
        public int Id { get; set; }

        public string? CodigoBarras { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal CostoProveedor { get; set; }

        // Puede ser nulo si decide usar el margen predeterminado de la categoría
        public decimal? MargenPersonalizado { get; set; }

        public int StockActual { get; set; }

        public int StockMinimo { get; set; }

        // Clave Foránea de la Categoría
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        // Propiedad Inteligente (No se guarda en la BD, se calcula al vuelo)
        public decimal PrecioVenta
        {
            get
            {
                // Si hay margen personalizado usa ese, sino usa el de la categoría, sino 0
                decimal margenAplicado = MargenPersonalizado ?? Categoria?.MargenPredeterminado ?? 0;
                return CostoProveedor * (1 + (margenAplicado / 100));
            }
        }
    }
}
