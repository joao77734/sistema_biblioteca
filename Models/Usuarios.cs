namespace sistemaProjetoPI.Models
{
    public class Usuarios
    {
        public int Id { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = "Ativo";

        public override string ToString()
        {
            return $"{Id,-4} {Nome,-25} {Email,-30} {Matricula}";
        }
    }
}
