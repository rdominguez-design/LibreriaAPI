
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

        // GET: api/Productos (Lista todos los productos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            // Usamos Include() para traer también los datos de la Categoría.
            // Esto es VITAL para que la app pueda calcular el PrecioVenta usando el margen heredado.
            return await _context.Productos.Include(p => p.Categoria).ToListAsync();
        }

        // POST: api/Productos (Guarda un producto nuevo)
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto(Producto producto)
        {
            // Verificamos que la categoría que eligió el usuario realmente exista
            var categoriaExiste = await _context.Categorias.FindAsync(producto.CategoriaId);
            if (categoriaExiste == null)
            {
                return BadRequest("La categoría ingresada no existe.");
            }

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            // Recargamos el producto con su categoría unida para mostrar el precio final en la respuesta
            await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();

            return Ok(producto);
        }
        // POST: api/Productos/1/vender?cantidad=2
        [HttpPost("{id}/vender")]
        public async Task<IActionResult> RegistrarVenta(int id, int cantidad)
        {
            // 1. Buscamos el producto en el "depósito" (Base de datos)
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound("El producto ingresado no existe.");
            }

            // 1.5 VALIDACIÓN DE SEGURIDAD: Evitar números negativos o cero
            if (cantidad <= 0)
            {
                return BadRequest("La cantidad a vender debe ser mayor a cero. ¡No se aceptan ventas fantasmas ni devoluciones truchas!");
            }

            // 2. Verificamos que haya suficiente stock
            if (producto.StockActual < cantidad)
            {
                return BadRequest($"Stock insuficiente. Solo te quedan {producto.StockActual} unidades de {producto.Nombre}.");
            }

            // 3. Descontamos el stock matemático
            producto.StockActual -= cantidad;

            // --- NUEVO PASO 3.5: Generamos el "ticket" de la venta para el historial ---
            var nuevaVenta = new Venta
            {
                Fecha = DateTime.Now, // Toma la fecha y hora exacta de la computadora
                ProductoId = producto.Id,
                ProductoNombre = producto.Nombre,
                Cantidad = cantidad,
                Total = producto.PrecioVenta * cantidad // Multiplica el precio unitario por la cantidad vendida
            };

            // Agregamos la venta a la tabla
            _context.Ventas.Add(nuevaVenta);
            // ----------------------------------------------------------------------------

            // 4. Guardamos los cambios físicos en el archivo
            // La magia de esto es que guarda el descuento de stock Y el ticket al mismo tiempo.
            await _context.SaveChangesAsync();

            // Actualizamos el mensaje de respuesta para que muestre el total cobrado
            return Ok(new
            {
                Mensaje = "Venta registrada y facturada con éxito.",
                Producto = producto.Nombre,
                CantidadVendida = cantidad,
                TotalCobrado = nuevaVenta.Total,
                StockRestante = producto.StockActual
            });
        }
        // PUT: api/Productos/AumentoMasivo?categoriaId=1&porcentaje=15
        [HttpPut("AumentoMasivo")]
        public async Task<IActionResult> AumentoMasivo(int categoriaId, decimal porcentaje)
        {
            // 1. Buscamos todos los productos que pertenezcan a esa categoría
            var productos = await _context.Productos.Where(p => p.CategoriaId == categoriaId).ToListAsync();

            if (!productos.Any())
            {
                return NotFound("No se encontraron productos para esta categoría.");
            }

            // 2. Recorremos uno por uno y les aplicamos el aumento matemático
            foreach (var producto in productos)
            {
                decimal factorAumento = 1 + (porcentaje / 100);
                producto.CostoProveedor = Math.Round(producto.CostoProveedor * factorAumento, 2);
            }

            // 3. Guardamos todos los cambios juntos en la base de datos
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensaje = $"Éxito. Se actualizó el costo de {productos.Count} productos.",
                AumentoAplicado = $"{porcentaje}%"
            });
        }
        // GET: api/Productos/Buscar?nombre=cuaderno
        [HttpGet("Buscar")]
        public async Task<IActionResult> BuscarPorNombre(string nombre)
        {
            // 1. Validamos que el usuario no haya mandado un texto vacío
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest("Por favor, ingresá una palabra para buscar.");
            }

            // 2. Buscamos cualquier producto que CONTENGA la palabra ingresada
            var productos = await _context.Productos
                                          .Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()))
                                          .ToListAsync();

            // 3. Verificamos si hubo suerte
            if (!productos.Any())
            {
                return NotFound($"No encontramos ningún producto que contenga la palabra '{nombre}'.");
            }

            // 4. Devolvemos la lista de coincidencias
            return Ok(productos);
        }
        // DELETE: api/Productos/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            // 1. Buscamos si el producto realmente existe
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound($"No se encontró ningún producto con el ID {id}.");
            }

            // 2. Le damos la orden a la base de datos para que lo elimine
            _context.Productos.Remove(producto);

            // 3. Confirmamos y guardamos los cambios físicos
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensaje = $"El producto '{producto.Nombre}' fue eliminado para siempre del sistema."
            });
        }
        // GET: api/Productos/Paginados?pagina=1&cantidadPorPagina=20
        [HttpGet("Paginados")]
        public async Task<IActionResult> ObtenerProductosPaginados(int pagina = 1, int cantidadPorPagina = 20)
        {
            // Con "Skip" saltamos los productos de las páginas anteriores
            // Con "Take" agarramos solo la cantidad que necesitamos ahora para no saturar la pantalla
            var productos = await _context.Productos
                                          .Skip((pagina - 1) * cantidadPorPagina)
                                          .Take(cantidadPorPagina)
                                          .ToListAsync();

            return Ok(productos);
        }

        // GET: api/Productos/ReporteMensual?mes=6&anio=2026
        [HttpGet("ReporteMensual")]
        public async Task<IActionResult> ReporteMensual(int mes, int anio)
        {
            // 1. Buscamos todas las ventas que correspondan a ese mes y año
            var ventasDelMes = await _context.Ventas
                                             .Where(v => v.Fecha.Month == mes && v.Fecha.Year == anio)
                                             .ToListAsync();

            if (!ventasDelMes.Any())
            {
                return Ok($"No se registraron ventas en la fecha {mes}/{anio}.");
            }

            // 2. Sumamos los totales de forma veloz en el disco
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
    }
}