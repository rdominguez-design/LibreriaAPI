using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibreriaAPI.Data;
using LibreriaAPI.Models;

namespace LibreriaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly LibreriaContext _context;

        public ProductosController(LibreriaContext context)
        {
            _context = context;
        }

        // PUT: api/Productos/ReactivarTodos
        [HttpPut("ReactivarTodos")]
        public async Task<IActionResult> ReactivarTodos()
        {
            var todosLosProductos = await _context.Productos.ToListAsync();
            foreach (var p in todosLosProductos)
            {
                p.Activo = true;
            }
            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "¡Todos los productos viejos fueron reactivados y ahora volverán a aparecer!" });
        }

        // GET: api/Productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            return await _context.Productos
                                 .Include(p => p.Categoria)
                                 .Where(p => p.Activo == true)
                                 .ToListAsync();
        }

        // POST: api/Productos
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            var categoriaExiste = await _context.Categorias.FindAsync(producto.CategoriaId);
            if (categoriaExiste == null) return BadRequest("La categoría ingresada no existe.");

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();

            return Ok(producto);
        }

        // --- 👇 ACÁ ESTÁ LA MAGIA DEL MÉTODO DE PAGO 👇 ---
        // POST: api/Productos/1/vender?cantidad=2&metodoPago=Transferencia
        [HttpPost("{id}/vender")]
        public async Task<IActionResult> RegistrarVenta(int id, int cantidad, string metodoPago = "Efectivo")
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null) return NotFound("El producto ingresado no existe.");
            if (producto.Activo == false) return BadRequest("No se puede vender un producto que está dado de baja.");
            if (cantidad <= 0) return BadRequest("La cantidad a vender debe ser mayor a cero.");
            if (producto.StockActual < cantidad) return BadRequest($"Stock insuficiente. Solo te quedan {producto.StockActual} unidades.");

            producto.StockActual -= cantidad;

            // Guardamos el ticket con el Método de Pago incluido
            var nuevaVenta = new Venta
            {
                Fecha = DateTime.Now,
                ProductoId = producto.Id,
                ProductoNombre = producto.Nombre,
                Cantidad = cantidad,
                Total = producto.PrecioVenta * cantidad,
                MetodoPago = metodoPago // <--- ¡Dato guardado!
            };

            _context.Ventas.Add(nuevaVenta);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensaje = "Venta registrada y facturada con éxito.",
                Producto = producto.Nombre,
                CantidadVendida = cantidad,
                TotalCobrado = nuevaVenta.Total,
                MetodoDePago = metodoPago, // <--- Lo mostramos en el ticket
                StockRestante = producto.StockActual
            });
        }
        // --------------------------------------------------

        // PUT: api/Productos/AumentoMasivo
        [HttpPut("AumentoMasivo")]
        public async Task<IActionResult> AumentoMasivo(int categoriaId, decimal porcentaje)
        {
            var productos = await _context.Productos.Where(p => p.CategoriaId == categoriaId && p.Activo == true).ToListAsync();
            if (!productos.Any()) return NotFound("No se encontraron productos activos para esta categoría.");

            foreach (var producto in productos)
            {
                decimal factorAumento = 1 + (porcentaje / 100);
                producto.CostoProveedor = Math.Round(producto.CostoProveedor * factorAumento, 2);
            }
            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = $"Éxito. Se actualizó el costo de {productos.Count} productos.", AumentoAplicado = $"{porcentaje}%" });
        }

        // GET: api/Productos/Buscar
        [HttpGet("Buscar")]
        public async Task<IActionResult> BuscarPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return BadRequest("Por favor, ingresá una palabra para buscar.");
            var productos = await _context.Productos.Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()) && p.Activo == true).ToListAsync();
            if (!productos.Any()) return NotFound($"No encontramos ningún producto activo que contenga la palabra '{nombre}'.");
            return Ok(productos);
        }

        // DELETE: api/Productos/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound($"No se encontró ningún producto con el ID {id}.");
            producto.Activo = false;
            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = $"El producto '{producto.Nombre}' fue dado de baja exitosamente." });
        }

        // GET: api/Productos/Paginados
        [HttpGet("Paginados")]
        public async Task<IActionResult> ObtenerProductosPaginados(int pagina = 1, int cantidadPorPagina = 20)
        {
            var productos = await _context.Productos.Where(p => p.Activo == true).Skip((pagina - 1) * cantidadPorPagina).Take(cantidadPorPagina).ToListAsync();
            return Ok(productos);
        }

        // GET: api/Productos/ReporteMensual
        [HttpGet("ReporteMensual")]
        public async Task<IActionResult> ReporteMensual(int mes, int anio)
        {
            var ventasDelMes = await _context.Ventas.Where(v => v.Fecha.Month == mes && v.Fecha.Year == anio).ToListAsync();
            if (!ventasDelMes.Any()) return Ok($"No se registraron ventas en la fecha {mes}/{anio}.");

            decimal totalFacturado = ventasDelMes.Sum(v => v.Total);
            int articulosVendidos = ventasDelMes.Sum(v => v.Cantidad);

            return Ok(new
            {
                Periodo = $"{mes}/{anio}",
                TotalFacturado = totalFacturado,
                ArticulosVendidos = articulosVendidos,
                TicketsEmitidos = ventasDelMes.Count
            });
        }
        // --- NUEVO REPORTE FLEXIBLE (Diario, Semanal, etc.) ---
        // GET: api/Productos/ReportePorFechas?inicio=2026-06-01&fin=2026-06-07
        [HttpGet("ReportePorFechas")]
        public async Task<IActionResult> ReportePorFechas(DateTime inicio, DateTime fin)
        {
            // 1. Buscamos las ventas que cayeron exactamente entre esas dos fechas
            var ventasDelPeriodo = await _context.Ventas
                                                 .Where(v => v.Fecha.Date >= inicio.Date && v.Fecha.Date <= fin.Date)
                                                 .ToListAsync();

            if (!ventasDelPeriodo.Any())
            {
                return Ok($"No se registraron ventas entre el {inicio.ToShortDateString()} y el {fin.ToShortDateString()}.");
            }

            // 2. Sumamos los totales
            decimal totalFacturado = ventasDelPeriodo.Sum(v => v.Total);
            int articulosVendidos = ventasDelPeriodo.Sum(v => v.Cantidad);

            return Ok(new
            {
                Periodo = $"Desde {inicio.ToShortDateString()} hasta {fin.ToShortDateString()}",
                TotalFacturado = totalFacturado,
                ArticulosVendidos = articulosVendidos,
                TicketsEmitidos = ventasDelPeriodo.Count
            });
        }
    }
}