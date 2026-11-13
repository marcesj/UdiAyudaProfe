namespace UdiAyudaProfe.Models;

public class Curso
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public int Creditos { get; set; }

    // Profesor que dicta el curso. Es opcional (int?): un curso puede
    // quedar "sin profesor asignado" por un tiempo.
    public int? ProfesorId { get; set; }
    public Profesor? Profesor { get; set; }

    public List<Estudiante> Estudiantes { get; set; } = [];
}
