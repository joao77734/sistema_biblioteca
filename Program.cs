using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Sistema_Embrapa.Data;
using sistemaProjetoPI.Models;

var repositorio = new UsuarioRepositorio(new Conexao());

try
{
    var usuarioLogado = Login(repositorio);
    if (usuarioLogado == null) return;

    MenuPrincipal(repositorio, usuarioLogado);
}
catch (MySqlException ex)
{
    Console.WriteLine($"Erro de Banco de dados: {ex.Message}");
    Console.WriteLine("Verifique a conexão.");
}

static Usuarios? Login(UsuarioRepositorio repo)
{
    for (int tentativa = 1; tentativa <= 3; tentativa++)
    {
        Console.Clear();
        Console.WriteLine("=== SISTEMA EMBRAPA — LOGIN ===");
        string email = Ler("Digite seu Email: ");

        var usuario = repo.Autenticar(email);
        if (usuario != null)
        {
            Console.WriteLine($"\nBem-vindo, {usuario.Nome}!");
            Pausar();
            return usuario;
        }
        Console.WriteLine($"\nEmail inválido ou não cadastrado ({tentativa}/3)");
        Pausar();
    }
    Console.WriteLine("Número máximo de tentativas atingido.");
    return null;
}

static void MenuPrincipal(UsuarioRepositorio repo, Usuarios usuario)
{
    List<string> perfisDoUsuario = repo.BuscarPerfisDoUsuario(usuario.Id);

    bool ehAdmin = perfisDoUsuario.Contains("Administrador");
    bool ehPesquisador = perfisDoUsuario.Contains("Pesquisador");
    bool ehValidador = perfisDoUsuario.Contains("Validador");
    bool ehComite = perfisDoUsuario.Contains("Comitê");

    while (true)
    {
        Console.Clear();
        Console.WriteLine("=============================================");
        Console.WriteLine("   SISTEMA DE GESTÃO DE AÇÕES — EMBRAPA      ");
        Console.WriteLine("=============================================");
        Console.WriteLine($" Usuário: {usuario.Nome} ");
        Console.WriteLine($" Perfis Ativos: {string.Join(", ", perfisDoUsuario)}");
        Console.WriteLine("=============================================");

        var mapaOpcoes = new Dictionary<string, string>();
        int indiceMenu = 1;

        if (ehAdmin)
        {
            Console.WriteLine($"{indiceMenu} - Cadastrar Novo Usuário");
            mapaOpcoes.Add(indiceMenu.ToString(), "Cadastrar");
            indiceMenu++;

            Console.WriteLine($"{indiceMenu} - Listar Todos os Usuários");
            mapaOpcoes.Add(indiceMenu.ToString(), "Listar");
            indiceMenu++;

            Console.WriteLine($"{indiceMenu} - Editar Cadastro de Usuário");
            mapaOpcoes.Add(indiceMenu.ToString(), "Editar");
            indiceMenu++;

            Console.WriteLine($"{indiceMenu} - Excluir Usuário do Sistema");
            mapaOpcoes.Add(indiceMenu.ToString(), "Excluir");
            indiceMenu++;
        }

        if (ehPesquisador)
        {
            Console.WriteLine($"{indiceMenu} - Lançar Nova Ação");
            mapaOpcoes.Add(indiceMenu.ToString(), "LancarAcao");
            indiceMenu++;

            Console.WriteLine($"{indiceMenu} - Listar Minhas Ações Submetidas");
            mapaOpcoes.Add(indiceMenu.ToString(), "ListarMinhasAcoes");
            indiceMenu++;
        }

        if (ehValidador)
        {
            Console.WriteLine($"{indiceMenu} - Avaliar Acoes");
            mapaOpcoes.Add(indiceMenu.ToString(), "Validar");
            indiceMenu++;
        }

        if (ehComite)
        {
            Console.WriteLine($"{indiceMenu} - Consultar Logs de Auditoria (Geral)");
            mapaOpcoes.Add(indiceMenu.ToString(), "ConsultarLogs");
            indiceMenu++;
        }

        Console.WriteLine("---------------------------------------------");
        Console.WriteLine("0 - Sair do Sistema");
        Console.WriteLine("=============================================");
        Console.Write("Escolha uma opção: ");

        string escolha = Console.ReadLine() ?? "";

        if (escolha == "0") return;

        if (!mapaOpcoes.ContainsKey(escolha))
        {
            Console.WriteLine("\n[OPÇÃO INVÁLIDA]: Selecione um número visível no seu menu de acesso.");
            Pausar();
            continue;
        }

        switch (mapaOpcoes[escolha])
        {
            case "Cadastrar": Cadastrar(repo, usuario.Matricula); break;
            case "Listar": Mostrar(repo); break;
            case "Editar": Editar(repo, usuario.Matricula); break;
            case "Excluir": Excluir(repo, usuario.Matricula); break;
            case "LancarAcao": LancarAcaoPesquisador(repo, usuario.Matricula); break;
            case "ListarMinhasAcoes": ListarMinhasAcoes(repo, usuario.Matricula); break;
            case "ValidarAcao": ValidarAcaoValidador(repo, usuario.Matricula); break;
            case "ConsultarLogs": ConsultarLogsComite(repo); break;
        }
    }
}

static void Cadastrar(UsuarioRepositorio repo, string matriculaOperador)
{
    Console.Clear();
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
        repo.Cadastrar(u, matriculaOperador);
        Console.WriteLine("Cadastrado com sucesso!");
    }
    catch (MySqlException ex) when (ex.Number == 1062)
    {
        Console.WriteLine("Erro: Matrícula ou E-mail já existente no sistema.");
    }
    Pausar();
}

static void Mostrar(UsuarioRepositorio repo)
{
    Console.Clear();
    Console.WriteLine("--- Usuários Cadastrados ---");
    var lista = repo.Listar();
    if (lista.Count == 0)
    {
        Console.WriteLine("Nenhum usuário cadastrado!");
    }
    else
    {
        Console.WriteLine($"{"ID",-4} {"NOME",-25} {"EMAIL",-30} MATRÍCULA");
        lista.ForEach(Console.WriteLine);
    }
    Pausar();
}

static void Editar(UsuarioRepositorio repo, string matriculaOperador)
{
    Console.Clear();
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
        Console.WriteLine(repo.Editar(u, matriculaOperador) ? "Usuário Atualizado!" : "ID não encontrado");
    }
    catch (MySqlException ex) when (ex.Number == 1062)
    {
        Console.WriteLine("Erro: Matrícula ou E-mail já utilizado por outro usuário.");
    }
    Pausar();
}

static void Excluir(UsuarioRepositorio repo, string matriculaOperador)
{
    Console.Clear();
    ListarNaTela(repo);
    if (!int.TryParse(Ler("\nID do usuário a excluir: "), out int id))
    {
        Console.WriteLine("ID Inválido");
        Pausar();
        return;
    }
    if (Ler("Tem certeza? (s/n): ").ToLower() != "s")
    {
        Console.WriteLine("Operação Cancelada!");
    }
    else
    {
        Console.WriteLine(repo.Excluir(id, matriculaOperador) ? "Usuário Excluído" : "ID não encontrado.");
    }
    Pausar();
}

static void LancarAcaoPesquisador(UsuarioRepositorio repo, string matriculaOperador)
{
    Console.Clear();
    Console.WriteLine("=== LANÇAR NOVA AÇÃO ===");
    
    string nomeAcao = Ler("Título da Ação: ");
    string descricao = Ler("Descrição: ");
    string idTipo = Ler("ID do Tipo de Ação (Use o número 1): ");
    string idAno = Ler("ID do Ano de Exercício (Use o número 1): ");

    bool sucesso = repo.PesquisadorLancarAcao(nomeAcao, descricao, idTipo, idAno, matriculaOperador);
    Console.WriteLine(sucesso ? "\nAção enviada com sucesso para avaliação!" : "\nErro ao submeter ação.");
    Pausar();
}

static void ListarMinhasAcoes(UsuarioRepositorio repo, string matriculaOperador)
{
    Console.Clear();
    Console.WriteLine("=== MINHAS AÇÕES SUBMETIDAS ===");
    repo.PesquisadorListarSuasAcoes(matriculaOperador);
    Pausar();
}

static void ValidarAcaoValidador(UsuarioRepositorio repo, string matriculaOperador)
{
    Console.Clear();
    Console.WriteLine("=== AVALIAÇÃO DE AÇÕES SELECIONADAS ===");
    repo.ListarAcoesPendentes();

    if (!int.TryParse(Ler("\nDigite o ID da Ação que deseja avaliar: "), out int idAcao))
    {
        Console.WriteLine("ID Inválido.");
        Pausar();
        return;
    }

    string resultado = Ler("Resultado (Aprovado/Recusado): ");
    string justificativa = Ler("Justificativa (Se recusado): ");

    bool sucesso = repo.ValidadorJulgarAcao(idAcao, resultado, justificativa, matriculaOperador);
    Console.WriteLine(sucesso ? "\nAvaliação publicada com sucesso!" : "\nErro ao registrar avaliação.");
    Pausar();
}

static void ConsultarLogsComite(UsuarioRepositorio repo)
{
    Console.Clear();
    Console.WriteLine("=== AUDITORIA GERAL — EXIBIÇÃO DE LOGS (COMITÊ) ===");
    Console.WriteLine($"{"ID LOG",-7} {"OPERAÇÃO / TABELA",-35} {"ID REGISTRO",-12} {"MATRÍCULA"}");
    Console.WriteLine("-------------------------------------------------------------------------");
    repo.ComiteConsultarLogs();
    Pausar();
}

static void ListarNaTela(UsuarioRepositorio repo)
{
    var lista = repo.Listar();
    Console.WriteLine($"{"ID",-4} {"Nome",-25} {"Email",-30} Matrícula");
    lista.ForEach(Console.WriteLine);
}

static void Pausar()
{
    Console.WriteLine("\nPressione ENTER para Continuar...");
    Console.ReadLine();
}

static string Ler(string texto)
{
    Console.Write(texto);
    return Console.ReadLine()?.Trim() ?? "";
}