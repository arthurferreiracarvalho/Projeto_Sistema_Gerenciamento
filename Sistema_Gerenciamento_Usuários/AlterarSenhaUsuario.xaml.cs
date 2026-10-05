using System;
using System.Windows;
using MySql.Data.MySqlClient;
using BCrypt.Net;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class AlterarSenhaUsuario : Window
    {
        public string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        private readonly UsuarioModel usuarioLogado;

        public AlterarSenhaUsuario() : this(SessaoSistema.UsuarioLogado)
        {
        }

        public AlterarSenhaUsuario(UsuarioModel usuario)
        {
            InitializeComponent();
            this.usuarioLogado = usuario ?? SessaoSistema.UsuarioLogado;
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            string senhaAtual = pwdSenhaAtual.Password;
            string novaSenha = pwdNovaSenha.Password;
            string confirmarSenha = pwdConfirmarSenha.Password;

            if (string.IsNullOrEmpty(senhaAtual) || string.IsNullOrEmpty(novaSenha) || string.IsNullOrEmpty(confirmarSenha))
            {
                MessageBox.Show("Preencha todos os campos de senha!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (novaSenha != confirmarSenha)
            {
                MessageBox.Show("A nova senha e a confirmação não coincidem!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (novaSenha.Length < 4)
            {
                MessageBox.Show("A nova senha deve ter no mínimo 4 caracteres!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (usuarioLogado == null || usuarioLogado.Id <= 0)
            {
                MessageBox.Show("Sessão do usuário inválida!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    string queryBuscarSenha = "SELECT senha FROM usuarios WHERE id = @id";
                    string hashSenhaBanco = null;

                    using (MySqlCommand cmdCheck = new MySqlCommand(queryBuscarSenha, conexao))
                    {
                        cmdCheck.Parameters.AddWithValue("@id", usuarioLogado.Id);
                        object result = cmdCheck.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            hashSenhaBanco = result.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(hashSenhaBanco) || !BCrypt.Net.BCrypt.Verify(senhaAtual, hashSenhaBanco))
                    {
                        MessageBox.Show("A senha atual informada está incorreta!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string novoHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
                    string queryUpdate = "UPDATE usuarios SET senha = @novaSenha WHERE id = @id";
                    using (MySqlCommand cmdUpdate = new MySqlCommand(queryUpdate, conexao))
                    {
                        cmdUpdate.Parameters.AddWithValue("@novaSenha", novoHash);
                        cmdUpdate.Parameters.AddWithValue("@id", usuarioLogado.Id);

                        cmdUpdate.ExecuteNonQuery();
                    }
                }

                RegistrosAuditoria.RegistrarAcao(
                    usuarioLogado.NomeUsuario,
                    "ALTERAÇÃO DE SENHA",
                    $"O usuário '{usuarioLogado.NomeUsuario}' alterou sua própria senha com sucesso."
                );

                MessageBox.Show("Senha alterada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao alterar senha: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}