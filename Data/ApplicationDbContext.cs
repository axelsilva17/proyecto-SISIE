using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Models.Entities;

namespace proyecto_SISIE.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Nuevas entidades del modelo de datos
    public DbSet<Contacto> Contactos { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Provincia> Provincias { get; set; } = null!;
    public DbSet<Ciudad> Ciudades { get; set; } = null!;
    public DbSet<Direccion> Direcciones { get; set; } = null!;
    public DbSet<Venta> Ventas { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Producto> Productos { get; set; } = null!;
    public DbSet<DetalleVenta> DetallesVenta { get; set; } = null!;
    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<MetodoPago> MetodosPago { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==== CONFIGURACIÓN DE CATEGORÍA ====
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.NombreCategoria).IsRequired().HasMaxLength(50);
        });

        // ==== CONFIGURACIÓN DE PRODUCTO ====
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.NombreProducto).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Descripcion).HasMaxLength(100);
            entity.Property(p => p.PrecioUnitario).HasPrecision(18, 2);
            
            // Relación con Categoría
            entity.HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ==== CONFIGURACIÓN DE CONTACTO ====
        modelBuilder.Entity<Contacto>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(40);
            entity.Property(c => c.Telefono).IsRequired();
        });

        // ==== CONFIGURACIÓN DE USUARIO ====
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(50);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
            entity.Property(u => u.FechaCreacion).IsRequired();
            entity.Property(u => u.Activo).IsRequired();
            
            // Relación con Contacto
            entity.HasOne(u => u.Contacto)
                .WithMany()
                .HasForeignKey(u => u.IdContacto)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ==== CONFIGURACIÓN DE PROVINCIA ====
        modelBuilder.Entity<Provincia>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.NombreProvincia).IsRequired().HasMaxLength(50);
        });

        // ==== CONFIGURACIÓN DE CIUDAD ====
        modelBuilder.Entity<Ciudad>(entity =>
        {
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.NombreCiudad).IsRequired().HasMaxLength(50);
            entity.Property(ci => ci.Cp).IsRequired();
            
            // Relación con Provincia
            entity.HasOne(ci => ci.Provincia)
                .WithMany(p => p.Ciudades)
                .HasForeignKey(ci => ci.IdProvincia)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ==== CONFIGURACIÓN DE DIRECCIÓN ====
        modelBuilder.Entity<Direccion>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Calle).IsRequired().HasMaxLength(50);
            entity.Property(d => d.Numero).IsRequired();
            
            // Relación con Ciudad
            entity.HasOne(d => d.Ciudad)
                .WithMany(ci => ci.Direcciones)
                .HasForeignKey(d => d.IdCiudad)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ==== CONFIGURACIÓN DE VENTA ====
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.ToTable(tb => tb.UseSqlOutputClause(false));
            entity.Property(v => v.NumeroVenta).IsRequired();
            entity.Property(v => v.Descuento);
            entity.Property(v => v.Total).IsRequired().HasPrecision(18, 2);
            entity.Property(v => v.TipoEntrega).IsRequired().HasMaxLength(30);
            entity.Property(v => v.Notas).HasMaxLength(200);
            entity.Property(v => v.Estado).IsRequired().HasMaxLength(20);
            entity.Property(v => v.FechaCreacion).IsRequired();
            
            // Relación con MetodoPago
            entity.HasOne(v => v.MetodoPago)
                .WithMany()
                .HasForeignKey(v => v.IdMetodoPago)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Relación con Usuario
            entity.HasOne(v => v.Usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(v => v.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Relación con Dirección (nullable para ventas por mostrador)
            entity.HasOne(v => v.Direccion)
                .WithMany(d => d.Ventas)
                .HasForeignKey(v => v.IdDireccion)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ==== CONFIGURACIÓN DE DETALLE VENTA (FK Compuesta) ====
        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.Property(d => d.Id).ValueGeneratedOnAdd().UseIdentityColumn();
            entity.Property(d => d.SubTotal).IsRequired().HasPrecision(18, 2);
            entity.Property(d => d.Cantidad).IsRequired();
            entity.Property(d => d.PrecioUnitario).IsRequired().HasPrecision(18, 2);
            
            // FK Compuesta - Primary Key separado
            entity.HasKey(d => new { d.IdVenta, d.IdProducto });
            
            // Relación con Venta
            entity.HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Relación con Producto
            entity.HasOne(d => d.Producto)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ==== CONFIGURACIÓN DE CLIENTE ====
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Dni).IsRequired().HasMaxLength(15);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Telefono).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Email).HasMaxLength(40);
            entity.Property(c => c.DireccionDefault).HasMaxLength(100);
            entity.Property(c => c.DepartamentoDefault).HasMaxLength(20);
            entity.Property(c => c.FechaCreacion).IsRequired();
            entity.Property(c => c.Activo).IsRequired();
            
            // Relación con Ciudad (nullable)
            entity.HasOne(c => c.Ciudad)
                .WithMany(ci => ci.Clientes)
                .HasForeignKey(c => c.IdCiudad)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ==== CONFIGURACIÓN DE METODO PAGO ====
        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Nombre).IsRequired().HasMaxLength(50);
            entity.Property(m => m.RecargoPorcentaje).HasPrecision(5, 2);
        });
    }
}