using Microsoft.EntityFrameworkCore;
using LibreriaAPI.Models;
using System.Collections.Generic;

namespace LibreriaAPI.Data
{
    public class LibreriaContext : DbContext
    {
        public LibreriaContext(DbContextOptions<LibreriaContext> options) : base(options)
        {
        }

        // Estas van a ser nuestras tablas físicas en la base de datos
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Le indicamos a EF Core que ignore "PrecioVenta" en la BD física 
            // porque la calculamos nosotros desde C#
            modelBuilder.Entity<Producto>().Ignore(p => p.PrecioVenta);
        }
    }
}