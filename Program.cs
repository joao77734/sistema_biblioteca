using System;
using System.Collections.Generic;
using Cadastro_de_usuario.Data;
using Cadastro_de_usuario.Models;
using MySql.Data.MySqlClient;

var repositorio = new UsuarioRepositorio(new Conexao());

try
{
    if (!Login(repositorio)) return;

    MenuPrincipal(repositorio);
}
catch (MySqlException ex)
{
    Console.WriteLine($"Erro de Banco de dados: {ex.Message}");
    Console.WriteLine("Verifique a conexão");
}

static bool Login(UsuarioRepositorio repo)
{
    for (int tentativa = 1; tentativa <= 3; tentativa++)
    {
        Console.Clear();
        Console.WriteLine("=== LOGIN ===");
        string email = Ler("Email: ");

        var usuario = repo.Autenticar(email);
        if (usuario != null)
        {
            Console.WriteLine($"Bem vindo, {usuario.Nome} !");
            Pausar();
            return true;
        }
        Console.WriteLine($"Email inválido ou não cadastrado ({tentativa}/3)");
        Pausar();
    }
    Console.WriteLine($"Número máximo de tentativas atingido");
    return false;
}

static void MenuPrincipal(UsuarioRepositorio repo)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("=== Sistema de Gestão de Ações — EMBRAPA ===");
        Console.WriteLine("1 - Cadastrar Usuário");
        Console.WriteLine("2 - Mostrar Cadastros");
        Console.WriteLine("3 - Editar Usuário");
        Console.WriteLine("4 - Excluir Usuário");
        Console.WriteLine("0 - Sair");
        Console.Write("Opção: ");

        switch (Console.ReadLine())
        {
            case "1": Cadastrar(repo); break;
            case "2": Mostrar(repo); break;
            case "3": Editar(repo); break;
            case "4": Excluir(repo); break;
            case "0": return;
            default: Console.WriteLine("Opção Inválida"); Pausar(); break;
        }
    }
}

static void Cadastrar(UsuarioRepositorio repo)
{
    Console.WriteLine("\n--- Novo Usuário ---");

    var u = new Usuarios
    {
        Matricula = Ler("Matrícula (Única): "),
        Nome = Ler("Nome: "),
        Email = Ler("Email Corporativo: "),
        Status = "Ativo"
    };
    try
    {
        repo.Cadastrar(u);
        Console.WriteLine("Cadastrado com sucesso ! ");
    }
    catch (MySqlException ex) when (ex.Number == 1062)
    {
        Console.WriteLine("Erro: Matrícula ou E-mail já existente no sistema.");
    }
    Pausar();
}

static void Mostrar(UsuarioRepositorio repo)
{
    Console.WriteLine("--- Usuários Cadastrados ---");
    var lista = repo.Listar();
    if (lista.Count == 0)
    {
        Console.WriteLine("Nenhum usuário cadastrado !");
    }
    else
    {
        Console.WriteLine($"{"ID",-4} {"NOME",-25} {"EMAIL",-30} MATRÍCULA");
        lista.ForEach(Console.WriteLine);
    }
    Pausar();
}

static void Editar(UsuarioRepositorio repo)
{
    ListarNaTela(repo);
    if (!int.TryParse(Ler("\nID do usuário a editar: "), out int id))
    {
        Console.WriteLine("ID Inválido");
        Pausar();
        return;
    }
    
    var u = new Usuarios 
    { 
        Id = id, 
        Matricula = Ler("Nova Matrícula: "),
        Nome = Ler("Novo Nome: "), 
        Email = Ler("Novo Email Corp: ")
    };
    
    try
    {
        Console.WriteLine(repo.Editar(u) ? "Usuário Atualizado!" : "ID não encontrado");
    }
    catch (MySqlException ex) when (ex.Number == 1062)
    {
        Console.WriteLine("Erro: Matrícula ou E-mail já utilizado por outro usuário.");
    }
    Pausar();
}

static void Excluir(UsuarioRepositorio repo)
{
    ListarNaTela(repo);
    if (!int.TryParse(Ler("\nID do usuário a excluir: "), out int id))
    {
        Console.WriteLine("ID Inválido");
        Pausar();
        return;
    }
    if (Ler("Tem certeza? (s/n): ").ToLower() != "s")
    {
        Console.WriteLine("Operação Cancelada !");
    }
    else
    {
        Console.WriteLine(repo.Excluir(id) ? "Usuário Excluído" : "ID não encontrado.");
    }
    Pausar();
}

static void ListarNaTela(UsuarioRepositorio repo)
{
    Console.WriteLine("--- Usuários cadastrados ---");
    var lista = repo.Listar();
    Console.WriteLine($"{"ID",-4} {"Nome",-25} {"Email",-30} Matrícula");
    lista.ForEach(Console.WriteLine);
}

static void Pausar()
{
    Console.WriteLine("Pressione ENTER para Continuar...");
    Console.ReadLine();
}

static string Ler(string texto)
{
    Console.Write(texto);
    return Console.ReadLine()?.Trim() ?? "";
}
