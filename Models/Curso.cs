namespace UdiAyudaProfe.Models;

public class Curso
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public int Creditos { get; set; }

    public List<Estudiante> Estudiantes { get; set; } = [];
}
