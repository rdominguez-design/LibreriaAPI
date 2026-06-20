using System;

namespace LibreriaAPI.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Total { get; set; }

        // 👇 LA NUEVA LÍNEA PARA EL MÉTODO DE PAGO 👇
        public string MetodoPago { get; set; } = "Efectivo"; // Le ponemos Efectivo por defecto
    }
}