using System;
using System.Windows;
using MySql.Data.MySqlClient;
using BCrypt.Net;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class RedefinirSenhaUsuario : Window
    {
        public string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        private readonly UsuarioModel adminLogado;

        public RedefinirSenhaUsuario() : this(SessaoSistema.UsuarioLogado)
        {
        }

        public RedefinirSenhaUsuario(UsuarioModel logado)
        {
            InitializeComponent();
            this.adminLogado = logado ?? SessaoSistema.UsuarioLogado;
        }

        private void btnRedefinir_Click(object sender, RoutedEventArgs e)
        {
            string usuarioEmail = UsuarioEmail.Text.Trim();
            string novaSenha = NovaSenha.Password.Trim();
            string confirmarSenha = ConfirmarSenha.Password.Trim();

            if (string.IsNullOrEmpty(usuarioEmail) || string.IsNullOrEmpty(novaSenha) || string.IsNullOrEmpty(confirmarSenha))
            {
                MessageBox.Show("Preencha todos os campos!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (novaSenha != confirmarSenha)
            {
                MessageBox.Show("As senhas não coincidem!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (novaSenha.Length < 8)
            {
                MessageBox.Show("A nova senha deve ter no mínimo 8 caracteres.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    string checkQuery = "SELECT id, nome_usuario FROM usuarios WHERE nome_usuario = @busca OR email = @busca LIMIT 1";
                    int usuarioAlvoId = 0;
                    string usuarioAlvoNome = string.Empty;

                    using (MySqlCommand cmdCheck = new MySqlCommand(checkQuery, conexao))
                    {
                        cmdCheck.Parameters.AddWithValue("@busca", usuarioEmail);

                        using (MySqlDataReader reader = cmdCheck.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                usuarioAlvoId = reader.GetInt32("id");
                                usuarioAlvoNome = reader.GetString("nome_usuario");
                            }
                            else
                            {
                                MessageBox.Show("Usuário ou e-mail não encontrado no sistema.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }

                    if (adminLogado != null)
                    {
                        if (usuarioAlvoId == 1 && adminLogado.Id != 1)
                        {
                            MessageBox.Show("Acesso Negado! A senha do Administrador Principal não pode ser alterada.", "Operação Inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }

                        if (usuarioAlvoId != adminLogado.Id && adminLogado.Id > usuarioAlvoId)
                        {
                            MessageBox.Show("Acesso Negado! Você não pode alterar a senha desse administrador.", "Operação Inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }
                    }

                    string novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
                    string updateQuery = "UPDATE usuarios SET senha = @senha, tentativas_falhas = 0, bloqueado = 0 WHERE id = @id";

                    using (MySqlCommand cmdUpdate = new MySqlCommand(updateQuery, conexao))
                    {
                        cmdUpdate.Parameters.AddWithValue("@senha", novaSenhaHash);
                        cmdUpdate.Parameters.AddWithValue("@id", usuarioAlvoId);

                        int linhasAfetadas = cmdUpdate.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            string nomeAdminLogado = adminLogado != null ? adminLogado.NomeUsuario : "SISTEMA";

                            RegistrosAuditoria.RegistrarAcao(
                                nomeAdminLogado,
                                "REDEFINIÇÃO DE SENHA",
                                $"O administrador '{nomeAdminLogado}' redefiniu a senha do usuário '{usuarioAlvoNome}' (ID: {usuarioAlvoId})."
                            );

                            MessageBox.Show("Senha redefinida com sucesso!", "Sucesso!", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao redefinir a senha: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}