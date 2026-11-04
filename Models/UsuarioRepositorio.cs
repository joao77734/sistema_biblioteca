using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Sistema_Embrapa.Data;

namespace sistemaProjetoPI.Models
{
    public class UsuarioRepositorio
    {
        private readonly Conexao _conexao;

        public UsuarioRepositorio(Conexao conexao)
        {
            _conexao = conexao;
        }

        public Usuarios? Autenticar(string email)
        {
            using var conn = _conexao.Criar();
            conn.Open();
            string sql = "SELECT id_usuario, matricula, nome, email_corp, status FROM USUARIO WHERE email_corp = @email";
            
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Usuarios
                {
                    Id = reader.GetInt32("id_usuario"),
                    Matricula = reader.GetString("matricula"),
                    Nome = reader.GetString("nome"),
                    Email = reader.GetString("email_corp"),
                    Status = reader.GetString("status")
                };
            }
            return null;
        }

        public List<string> BuscarPerfisDoUsuario(int idUsuario)
        {
            var perfis = new List<string>();
            using var conn = _conexao.Criar();
            conn.Open();
            
            string sql = @"SELECT p.nome_perfil FROM PERFIL p 
                           JOIN USUARIO_PERFIL up ON p.id_perfil = up.id_perfil 
                           WHERE up.id_usuario = @idUsuario";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                perfis.Add(reader.GetString("nome_perfil"));
            }
            return perfis;
        }

        private void GravarLog(MySqlConnection conn, MySqlTransaction trans, string operacao, int idRegistro, string matricula)
        {
            string sqlLog = "INSERT INTO LOG_AUDITOR (operacao_tabela, id_registro, matricula) VALUES (@op, @idReg, @mat)";
            using var cmdLog = new MySqlCommand(sqlLog, conn, trans);
            cmdLog.Parameters.AddWithValue("@op", operacao);
            cmdLog.Parameters.AddWithValue("@idReg", idRegistro);
            cmdLog.Parameters.AddWithValue("@mat", matricula);
            cmdLog.ExecuteNonQuery();
        }

        public void Cadastrar(Usuarios u, string operadorMatricula)
        {
            using var conn = _conexao.Criar();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                string sqlUser = "INSERT INTO USUARIO (matricula, nome, email_corp, status) VALUES (@matricula, @nome, @email, @status); SELECT LAST_INSERT_ID();";
                using var cmdUser = new MySqlCommand(sqlUser, conn, trans);
                cmdUser.Parameters.AddWithValue("@matricula", u.Matricula);
                cmdUser.Parameters.AddWithValue("@nome", u.Nome);
                cmdUser.Parameters.AddWithValue("@email", u.Email);
                cmdUser.Parameters.AddWithValue("@status", u.Status);
                int newId = Convert.ToInt32(cmdUser.ExecuteScalar());

                GravarLog(conn, trans, "INSERT / USUARIO", newId, operadorMatricula);

                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public List<Usuarios> Listar()
        {
            var lista = new List<Usuarios>();
            using var conn = _conexao.Criar();
            conn.Open();
            string sql = "SELECT id_usuario, matricula, nome, email_corp, status FROM USUARIO";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Usuarios
                {
                    Id = reader.GetInt32("id_usuario"),
                    Matricula = reader.GetString("matricula"),
                    Nome = reader.GetString("nome"),
                    Email = reader.GetString("email_corp"),
                    Status = reader.GetString("status")
                });
            }
            return lista;
        }

        public bool Editar(Usuarios u, string operadorMatricula)
        {
            using var conn = _conexao.Criar();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                string sql = "UPDATE USUARIO SET nome = @nome, email_corp = @email, matricula = @matricula WHERE id_usuario = @id";
                using var cmd = new MySqlCommand(sql, conn, trans);
                cmd.Parameters.AddWithValue("@nome", u.Nome);
                cmd.Parameters.AddWithValue("@email", u.Email);
                cmd.Parameters.AddWithValue("@matricula", u.Matricula);
                cmd.Parameters.AddWithValue("@id", u.Id);
                
                bool alterado = cmd.ExecuteNonQuery() > 0;

                if (alterado)
                {
                    GravarLog(conn, trans, "UPDATE / USUARIO", u.Id, operadorMatricula);
                }

                trans.Commit();
                return alterado;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public bool Excluir(int id, string operadorMatricula)
        {
            using var conn = _conexao.Criar();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                string sql = "DELETE FROM USUARIO WHERE id_usuario = @id";
                using var cmd = new MySqlCommand(sql, conn, trans);
                cmd.Parameters.AddWithValue("@id", id);
                
                bool excluido = cmd.ExecuteNonQuery() > 0;

                if (excluido)
                {
                    GravarLog(conn, trans, "DELETE / USUARIO", id, operadorMatricula);
                }

                trans.Commit();
                return excluido;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public bool PesquisadorLancarAcao(string nome, string desc, string idTipo, string idAno, string operadorMatricula)
        {
            using var conn = _conexao.Criar();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                string sqlPesq = "SELECT id_pesquisador FROM PESQUISADOR p JOIN USUARIO u ON p.id_usuario = u.id_usuario WHERE u.matricula = @mat";
                using var cmdP = new MySqlCommand(sqlPesq, conn, trans);
                cmdP.Parameters.AddWithValue("@mat", operadorMatricula);
                var res = cmdP.ExecuteScalar();
                if (res == null) return false;
                int idPesquisador = Convert.ToInt32(res);

                string sqlAcao = "INSERT INTO ACAO (nome_acao, descricao, status, id_pesquisador, id_tipo_acao, id_ano) VALUES (@nome, @desc, 'Pendente', @idP, @idT, @idA); SELECT LAST_INSERT_ID();";
                using var cmdA = new MySqlCommand(sqlAcao, conn, trans);
                cmdA.Parameters.AddWithValue("@nome", nome);
                cmdA.Parameters.AddWithValue("@desc", desc);
                cmdA.Parameters.AddWithValue("@idP", idPesquisador);
                cmdA.Parameters.AddWithValue("@idT", idTipo);
                cmdA.Parameters.AddWithValue("@idA", idAno);
                int newAcaoId = Convert.ToInt32(cmdA.ExecuteScalar());

                GravarLog(conn, trans, "INSERT / AÇÃO (Submissão Pesquisa)", newAcaoId, operadorMatricula);

                trans.Commit();
                return true;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public void PesquisadorListarSuasAcoes(string operadorMatricula)
        {
            using var conn = _conexao.Criar();
            conn.Open();
            string sql = @"SELECT a.id_acao, a.nome_acao, a.status, a.data_submissao FROM ACAO a 
                           JOIN PESQUISADOR p ON a.id_pesquisador = p.id_pesquisador 
                           JOIN USUARIO u ON p.id_usuario = u.id_usuario 
                           WHERE u.matricula = @mat";
            
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mat", operadorMatricula);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader.GetInt32("id_acao"),-5} | Título: {reader.GetString("nome_acao"),-25} | Data: {reader.GetDateTime("data_submissao"):dd/MM/yyyy} | Status: {reader.GetString("status")}");
            }
        }

        public void ListarAcoesPendentes()
        {
            using var conn = _conexao.Criar();
            conn.Open();
            string sql = "SELECT id_acao, nome_acao, status FROM ACAO WHERE status = 'Pendente'";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader.GetInt32("id_acao")} | Título: {reader.GetString("nome_acao")} | Status: {reader.GetString("status")}");
            }
        }

        public bool ValidadorJulgarAcao(int idAcao, string resultado, string justificativa, string operadorMatricula)
        {
            using var conn = _conexao.Criar();
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                string sqlUpdate = "UPDATE ACAO SET status = @status WHERE id_acao = @id";
                using var cmdU = new MySqlCommand(sqlUpdate, conn, trans);
                cmdU.Parameters.AddWithValue("@status", resultado);
                cmdU.Parameters.AddWithValue("@id", idAcao);
                cmdU.ExecuteNonQuery();

                string sqlEval = "INSERT INTO AVALIACAO (id_acao, id_avaliador, resultado, justificativa) VALUES (@idA, (SELECT id_usuario FROM USUARIO WHERE matricula = @mat), @res, @just); SELECT LAST_INSERT_ID();";
                using var cmdE = new MySqlCommand(sqlEval, conn, trans);
                cmdE.Parameters.AddWithValue("@idA", idAcao);
                cmdE.Parameters.AddWithValue("@mat", operadorMatricula);
                cmdE.Parameters.AddWithValue("@res", resultado);
                cmdE.Parameters.AddWithValue("@just", justificativa);
                int newAvalId = Convert.ToInt32(cmdE.ExecuteScalar());

                GravarLog(conn, trans, $"VALIDAÇÃO ({resultado}) / AVALIAÇÃO", newAvalId, operadorMatricula);

                trans.Commit();
                return true;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public void ComiteConsultarLogs()
        {
            using var conn = _conexao.Criar();
            conn.Open();
            string sql = "SELECT id_log, operacao_tabela, id_registro, matricula FROM LOG_AUDITOR ORDER BY data_hora DESC";
            
            using var cmd = new MySqlCommand(sql, conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                Console.WriteLine($"{r.GetInt32("id_log"),-7} {r.GetString("operacao_tabela"),-35} {r.GetInt32("id_registro"),-12} {r.GetString("matricula")}");
            }
        }
    }
}
