using Microsoft.EntityFrameworkCore;
using UdiAyudaProfe.Data;
using UdiAyudaProfe.Models;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL con EF Core.
// La cadena de conexión sale de appsettings.json, pero en Docker se pisa con la
// variable de entorno ConnectionStrings__DefaultConnection (ver docker-compose.yml).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.EnableRetryOnFailure())); // reintenta si la base tarda en estar lista

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // http://localhost:5010/openapi/v1.json
}

// Aplica las migraciones pendientes (y por lo tanto el seeding) al arrancar.
// Se hace en todos los entornos para que en Docker/VPS la base quede creada sola.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

// Endpoint simple para saber si la API está viva (útil en el servidor).
app.MapGet("/health", () => Results.Ok("OK"));

// ---------- Profesores ----------
var profesores = app.MapGroup("/profesores");

profesores.MapGet("/", async (AppDbContext db) =>
    await db.Profesores
        .OrderBy(p => p.Id)
        .Select(p => new { p.Id, p.Nombre, p.Email, p.Especialidad, TotalCursos = p.Cursos.Count })
        .ToListAsync());

profesores.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Profesores.FindAsync(id) is Profesor p ? Results.Ok(p) : Results.NotFound());

profesores.MapGet("/{id:int}/cursos", async (int id, AppDbContext db) =>
    await db.Cursos
        .Where(c => c.ProfesorId == id)
        .OrderBy(c => c.Id)
        .Select(c => new { c.Id, c.Nombre, c.Creditos, TotalEstudiantes = c.Estudiantes.Count })
        .ToListAsync());

profesores.MapPost("/", async (Profesor profesor, AppDbContext db) =>
{
    if (await db.Profesores.AnyAsync(x => x.Email == profesor.Email))
        return Results.Conflict($"Ya existe un profesor con el email {profesor.Email}");

    db.Profesores.Add(profesor);
    await db.SaveChangesAsync();
    return Results.Created($"/profesores/{profesor.Id}", profesor);
});

profesores.MapPut("/{id:int}", async (int id, Profesor datos, AppDbContext db) =>
{
    var p = await db.Profesores.FindAsync(id);
    if (p is null) return Results.NotFound();

    if (await db.Profesores.AnyAsync(x => x.Email == datos.Email && x.Id != id))
        return Results.Conflict($"Ya existe un profesor con el email {datos.Email}");

    p.Nombre = datos.Nombre;
    p.Email = datos.Email;
    p.Especialidad = datos.Especialidad;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

profesores.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var p = await db.Profesores.FindAsync(id);
    if (p is null) return Results.NotFound();

    // Sus cursos NO se borran: quedan con ProfesorId = null (OnDelete SetNull).
    db.Profesores.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ---------- Cursos ----------
var cursos = app.MapGroup("/cursos");

cursos.MapGet("/", async (AppDbContext db) =>
    await db.Cursos
        .OrderBy(c => c.Id)
        .Select(c => new
        {
            c.Id, c.Nombre, c.Creditos,
            c.ProfesorId,
            Profesor = c.Profesor == null ? null : c.Profesor.Nombre,
            TotalEstudiantes = c.Estudiantes.Count
        })
        .ToListAsync());

cursos.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Cursos.FindAsync(id) is Curso c ? Results.Ok(c) : Results.NotFound());

cursos.MapGet("/{id:int}/estudiantes", async (int id, AppDbContext db) =>
    await db.Estudiantes
        .Where(e => e.CursoId == id)
        .Select(e => new { e.Id, e.Nombre, e.Email })
        .ToListAsync());

cursos.MapPost("/", async (Curso curso, AppDbContext db) =>
{
    if (curso.ProfesorId is int pid && !await db.Profesores.AnyAsync(p => p.Id == pid))
        return Results.BadRequest($"No existe el profesor {pid}");

    db.Cursos.Add(curso);
    await db.SaveChangesAsync();
    return Results.Created($"/cursos/{curso.Id}", curso);
});

cursos.MapPut("/{id:int}", async (int id, Curso datos, AppDbContext db) =>
{
    var c = await db.Cursos.FindAsync(id);
    if (c is null) return Results.NotFound();

    if (datos.ProfesorId is int pid && !await db.Profesores.AnyAsync(p => p.Id == pid))
        return Results.BadRequest($"No existe el profesor {pid}");

    c.Nombre = datos.Nombre;
    c.Creditos = datos.Creditos;
    c.ProfesorId = datos.ProfesorId; // null = dejar el curso sin profesor
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ---------- Estudiantes ----------
var estudiantes = app.MapGroup("/estudiantes");

estudiantes.MapGet("/", async (AppDbContext db) =>
    await db.Estudiantes
        .OrderBy(e => e.Id)
        .Select(e => new { e.Id, e.Nombre, e.Email,
        e.CursoId, Curso = e.Curso!.Nombre })
        .ToListAsync());

estudiantes.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Estudiantes.FindAsync(id) is Estudiante e ? Results.Ok(e) : Results.NotFound());

estudiantes.MapPost("/", async (Estudiante estudiante, AppDbContext db) =>
{
    if (!await db.Cursos.AnyAsync(c => c.Id == estudiante.CursoId))
        return Results.BadRequest($"No existe el curso {estudiante.CursoId}");

    if (await db.Estudiantes.AnyAsync(x => x.Email == estudiante.Email))
        return Results.Conflict($"Ya existe un estudiante con el email {estudiante.Email}");

    db.Estudiantes.Add(estudiante);
    await db.SaveChangesAsync();
    return Results.Created($"/estudiantes/{estudiante.Id}", estudiante);
});

estudiantes.MapPut("/{id:int}", async (int id, Estudiante datos, AppDbContext db) =>
{
    var e = await db.Estudiantes.FindAsync(id);
    if (e is null) return Results.NotFound();

    if (!await db.Cursos.AnyAsync(c => c.Id == datos.CursoId))
        return Results.BadRequest($"No existe el curso {datos.CursoId}");

    if (await db.Estudiantes.AnyAsync(x => x.Email == datos.Email && x.Id != id))
        return Results.Conflict($"Ya existe un estudiante con el email {datos.Email}");

    e.Nombre = datos.Nombre;
    e.Email = datos.Email;
    e.CursoId = datos.CursoId;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

estudiantes.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var e = await db.Estudiantes.FindAsync(id);
    if (e is null) return Results.NotFound();

    db.Estudiantes.Remove(e);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
