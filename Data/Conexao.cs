using MySql.Data.MySqlClient;

namespace Sistema_Embrapa.Data;

public class Conexao
{
    private readonly string _stringConexao = " Server=localhost;Port=3306;Database=sistema_embrapa;Uid=root;Pwd=;";

    public MySqlConnection Criar() => new MySqlConnection (_stringConexao);
}