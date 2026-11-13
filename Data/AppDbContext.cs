using Microsoft.EntityFrameworkCore;
using UdiAyudaProfe.Models;

namespace UdiAyudaProfe.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Profesor> Profesores => Set<Profesor>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profesor>(e =>
        {
            e.Property(p => p.Nombre).HasMaxLength(100);
            e.Property(p => p.Email).HasMaxLength(150);
            e.Property(p => p.Especialidad).HasMaxLength(100);
            e.HasIndex(p => p.Email).IsUnique();
        });

        modelBuilder.Entity<Curso>(e =>
        {
            e.Property(c => c.Nombre).HasMaxLength(100);

            // Relación Profesor 1 ── N Cursos.
            // SetNull: si se borra un profesor, sus cursos NO se borran,
            // solo quedan sin profesor (ProfesorId = null).
            e.HasOne(c => c.Profesor)
             .WithMany(p => p.Cursos)
             .HasForeignKey(c => c.ProfesorId)
             .OnDelete(DeleteBehavior.SetNull);
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
        modelBuilder.Entity<Profesor>().HasData(
            new Profesor { Id = 1, Nombre = "Laura Fernández", Email = "laura.fernandez@udi.edu", Especialidad = "Programación" },
            new Profesor { Id = 2, Nombre = "Jorge Medina",    Email = "jorge.medina@udi.edu",    Especialidad = "Bases de Datos" },
            new Profesor { Id = 3, Nombre = "Sofía Ramírez",   Email = "sofia.ramirez@udi.edu",   Especialidad = "Desarrollo Web" }
        );

        modelBuilder.Entity<Curso>().HasData(
            new Curso { Id = 1, Nombre = "Programación I", Creditos = 4, ProfesorId = 1 },
            new Curso { Id = 2, Nombre = "Bases de Datos", Creditos = 3, ProfesorId = 2 },
            new Curso { Id = 3, Nombre = "Desarrollo Web", Creditos = 3, ProfesorId = 3 }
        );

        modelBuilder.Entity<Estudiante>().HasData(
            new Estudiante { Id = 1, Nombre = "Ana Pérez",    Email = "ana@udi.edu",    CursoId = 1 },
            new Estudiante { Id = 2, Nombre = "Luis Gómez",   Email = "luis@udi.edu",   CursoId = 1 },
            new Estudiante { Id = 3, Nombre = "María Rojas",  Email = "maria@udi.edu",  CursoId = 2 },
            new Estudiante { Id = 4, Nombre = "Carlos Díaz",  Email = "carlos@udi.edu", CursoId = 3 }
        );
    }
}
