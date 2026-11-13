namespace UdiAyudaProfe.Models;

public class Profesor
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public string? Especialidad { get; set; }

    // Un profesor puede dictar muchos cursos
    public List<Curso> Cursos { get; set; } = [];
}
