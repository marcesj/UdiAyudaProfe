using Microsoft.EntityFrameworkCore;
using UdiAyudaProfe.Models;

namespace UdiAyudaProfe.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Curso>(e =>
        {
            e.Property(c => c.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Estudiante>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(150);
            e.HasIndex(x => x.Email).IsUnique();
        });

        // ---------- Seeding de datos ----------
        // HasData guarda estos registros dentro de la migración,
        // así se insertan al ejecutar "dotnet ef database update".
        // Los Id deben ir escritos a mano.
        modelBuilder.Entity<Curso>().HasData(
            new Curso { Id = 1, Nombre = "Programación I", Creditos = 4 },
            new Curso { Id = 2, Nombre = "Bases de Datos", Creditos = 3 },
            new Curso { Id = 3, Nombre = "Desarrollo Web", Creditos = 3 }
        );

        modelBuilder.Entity<Estudiante>().HasData(
            new Estudiante { Id = 1, Nombre = "Ana Pérez",    Email = "ana@udi.edu",    CursoId = 1 },
            new Estudiante { Id = 2, Nombre = "Luis Gómez",   Email = "luis@udi.edu",   CursoId = 1 },
            new Estudiante { Id = 3, Nombre = "María Rojas",  Email = "maria@udi.edu",  CursoId = 2 },
            new Estudiante { Id = 4, Nombre = "Carlos Díaz",  Email = "carlos@udi.edu", CursoId = 3 }
        );
    }
}
