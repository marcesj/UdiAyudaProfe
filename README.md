# UdiAyudaProfe – API mínima con ASP.NET Core + PostgreSQL

API mínima (.NET 10) con Entity Framework Core, PostgreSQL y datos iniciales (seeding).

Tres tablas relacionadas:
```
Profesores 1 ──── N Cursos 1 ──── N Estudiantes
(un profesor dicta     (un curso tiene
 muchos cursos)         muchos estudiantes)
```

## Requisitos
- .NET SDK 10
- PostgreSQL en `localhost:5432`
- Herramienta EF: `dotnet tool install --global dotnet-ef`
- VS Code con la extensión **C# Dev Kit** (y opcional **REST Client** para el archivo `.http`)

## Estructura
```
Models/Profesor.cs, Curso.cs, Estudiante.cs → entidades
Data/AppDbContext.cs                        → DbContext + relaciones + seeding (HasData)
Migrations/                                 → InicialConSeed + AgregarProfesores
Program.cs                              → configuración y endpoints
UdiAyudaProfe.http                      → peticiones de prueba
```

## Pasos (en tu PC)
1. Pon tu contraseña en `appsettings.Development.json` → `ConnectionStrings:DefaultConnection`.
2. Ejecuta la API:
   ```
   dotnet run
   ```
   La API aplica sola las migraciones al arrancar: crea la base `udiayudaprofe`, las tablas y los datos del seed.
   (También puedes hacerlo a mano con `dotnet ef database update`.)
3. Prueba: http://localhost:5010/estudiantes  — o abre `UdiAyudaProfe.http` y pulsa "Send Request".
4. En VS Code también puedes pulsar **F5** (configuración "API (.NET)").

## Endpoints
| Método | Ruta | Descripción |
|---|---|---|
| GET | /profesores | Lista de profesores con su número de cursos |
| GET | /profesores/{id} | Un profesor |
| GET | /profesores/{id}/cursos | Cursos que dicta un profesor |
| POST | /profesores | Crear profesor (email único → 409 si se repite) |
| PUT | /profesores/{id} | Actualizar profesor |
| DELETE | /profesores/{id} | Eliminar profesor (sus cursos quedan sin profesor) |
| GET | /cursos | Lista de cursos con su profesor y número de estudiantes |
| GET | /cursos/{id} | Un curso |
| GET | /cursos/{id}/estudiantes | Estudiantes de un curso |
| POST | /cursos | Crear curso (`profesorId` opcional) |
| PUT | /cursos/{id} | Actualizar curso / cambiar o quitar el profesor |
| GET | /estudiantes | Lista de estudiantes |
| GET | /estudiantes/{id} | Un estudiante |
| POST | /estudiantes | Crear estudiante |
| PUT | /estudiantes/{id} | Actualizar |
| DELETE | /estudiantes/{id} | Eliminar |
| GET | /health | Responde "OK" si la API está viva |

Documento OpenAPI (solo en Development): http://localhost:5010/openapi/v1.json

## Docker
La API y PostgreSQL corren en dos contenedores con `docker-compose.yml`. La base no expone su puerto: solo la ve la API.
```
cp .env.example .env      # y poner una contraseña real en DB_PASSWORD
docker compose up -d --build
curl http://localhost:5000/health
```
Para el deploy en un VPS, ver [DEPLOY.md](DEPLOY.md).

## Cambiar los datos del seed
1. Edita `HasData(...)` en `Data/AppDbContext.cs`.
2. `dotnet ef migrations add CambioSeed`
3. `dotnet run` (o `dotnet ef database update`).
