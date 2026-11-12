namespace UdiAyudaProfe.Models;

public class Estudiante
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Email { get; set; }

    public int CursoId { get; set; }
    public Curso? Curso { get; set; }
}
