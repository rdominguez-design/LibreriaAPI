using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibreriaAPI.Data;
using LibreriaAPI.Models;

namespace LibreriaAPI.Controllers
{
    // Le decimos que la ruta en internet va a ser: localhost:puerto/api/Categorias
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly LibreriaContext _context;

        // El constructor recibe el puente a la base de datos
        public CategoriasController(LibreriaContext context)
        {
            _context = context;
        }

        // GET: api/Categorias (Para que la app móvil pida la lista de categorías)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            return await _context.Categorias.ToListAsync();
        }

        // POST: api/Categorias (Para guardar una categoría nueva desde la app)
        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return Ok(categoria);
        }
    }
}