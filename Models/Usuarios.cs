//Cadastro de usuarios abaixo

namespace Cadastro_de_usuario.Models;
public class Usuarios
{
    public int Id {get; set;}
    public string Nome{get; set;} = "";
    public string Email {get; set;} = "";
    public string Pin {get; set;} = "";
    public int Matricula {get; set;} 
    
    public override string ToString() => $"{Id, 4} {Nome, -25} {Email} {Matricula}" ;
}