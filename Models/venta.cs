using System;

namespace LibreriaAPI.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } // Acá se guarda el día y la hora exacta
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;// Guardamos el nombre por si el producto se borra en el futuro
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }
}
