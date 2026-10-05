using System;
using System.Windows;
using MySql.Data.MySqlClient;
using BCrypt.Net;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class Tela_Cadastro : Window
    {
        public string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";

        public Tela_Cadastro()
        {
            InitializeComponent();
            InicializarAdminAutomatico();
        }

        private void InicializarAdminAutomatico()
        {
            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    string checkQuery = "SELECT COUNT(*) FROM usuarios";
                    using (MySqlCommand cmdCheck = new MySqlCommand(checkQuery, conexao))
                    {
                        long total = (long)cmdCheck.ExecuteScalar();
                        if (total == 0)
                        {
                            string hashSenha = BCrypt.Net.BCrypt.HashPassword("12345678");
                            string insertQuery = "INSERT INTO usuarios (nome_completo, email, nome_usuario, senha, IsAdmin, bloqueado, tentativas_falhas) " +
                                                "VALUES ('Arthur ADM', 'arthur_adm@gmail.com', 'Arthur ADM', @senha, 1, 0, 0)";

                            using (MySqlCommand cmdInsert = new MySqlCommand(insertQuery, conexao))
                            {
                                cmdInsert.Parameters.AddWithValue("@senha", hashSenha);
                                cmdInsert.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro na inicialização: {ex.Message}", "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LimparCampos()
        {
            Email.Clear();
            Usuario.Clear();
            Senha.Clear();
            Comfirmar_senha.Clear();
        }

        private void Confirmar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = Usuario.Text.Trim();
            string senha = Senha.Password.Trim();

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(senha))
            {
                MessageBox.Show("Preencha todos os campos!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    string query = "SELECT id, nome_completo, nome_usuario, email, avatar, senha, IsAdmin, bloqueado FROM usuarios WHERE nome_usuario = @usuario";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuario);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int id = reader.GetInt32("id");
                                string hashSenha = reader.GetString("senha");
                                bool isAdmin = reader.GetBoolean("IsAdmin");
                                bool bloqueado = reader.GetBoolean("bloqueado");

                                if (bloqueado)
                                {
                                    MessageBox.Show("Conta bloqueada por excesso de tentativas.", "Acesso Negado", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }

                                if (BCrypt.Net.BCrypt.Verify(senha, hashSenha))
                                {
                                    UsuarioModel usuarioLogado = new UsuarioModel
                                    {
                                        Id = id,
                                        NomeCompleto = reader.IsDBNull(reader.GetOrdinal("nome_completo")) ? "" : reader.GetString("nome_completo"),
                                        NomeUsuario = reader.GetString("nome_usuario"),
                                        Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString("email"),
                                        Avatar = reader.IsDBNull(reader.GetOrdinal("avatar")) ? "" : reader.GetString("avatar"),
                                        IsAdmin = isAdmin,
                                        Bloqueado = bloqueado
                                    };

                                    reader.Close();
                                    SessaoSistema.UsuarioLogado = usuarioLogado;

                                    string updateSucesso = "UPDATE usuarios SET ultimo_login = NOW(), tentativas_falhas = 0 WHERE id = @id";
                                    using (MySqlCommand cmdUpdate = new MySqlCommand(updateSucesso, conexao))
                                    {
                                        cmdUpdate.Parameters.AddWithValue("@id", id);
                                        cmdUpdate.ExecuteNonQuery();
                                    }

                                    if (isAdmin)
                                    {
                                        Tela_de_Admin telaAdmin = new Tela_de_Admin(usuarioLogado);
                                        telaAdmin.Show();
                                    }
                                    else
                                    {
                                        Tela_de_Usuario telaUser = new Tela_de_Usuario();
                                        telaUser.Show();
                                    }

                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show("Senha incorreta.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Usuário não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de conexão: {ex.Message}", "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}