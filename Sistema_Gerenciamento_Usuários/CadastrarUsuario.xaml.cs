using System;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using BCrypt.Net;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class CadastrarUsuario : Window
    {
        public string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";

        public CadastrarUsuario()
        {
            InitializeComponent();
        }

        public void LimparCampos()
        {
            Nome_Completo.Clear();
            Nome_Usuario.Clear();
            Email_Usuario.Clear();
            Senha_Usuario.Clear();
            Confirmar_Senha.Clear();
        }

        private void Cadastrar_Click(object sender, RoutedEventArgs e)
        {
            string nomeCompleto = Nome_Completo.Text.Trim();
            string usuario = Nome_Usuario.Text.Trim();
            string email = Email_Usuario.Text.Trim();
            string senha = Senha_Usuario.Password.Trim();
            string confirmarSenha = Confirmar_Senha.Password.Trim();

            if (string.IsNullOrEmpty(nomeCompleto))
            {
                MessageBox.Show("Informe o Nome Completo!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (usuario.Length < 3)
            {
                MessageBox.Show("Nome de usuário inválido. (Deve conter pelo menos 3 caracteres!)", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!email.Contains("@") || !new EmailAddressAttribute().IsValid(email))
            {
                MessageBox.Show("E-mail inválido! Certifique-se de incluir o '@'", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (senha.Length < 8)
            {
                MessageBox.Show("Senha inválida. (Deve conter pelo menos 8 caracteres!)", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (confirmarSenha != senha || string.IsNullOrEmpty(confirmarSenha))
            {
                MessageBox.Show("As senhas não são iguais. Tente novamente!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();
                    string queryCheck = "SELECT COUNT(*) FROM usuarios WHERE nome_usuario = @usuario";
                    using (MySqlCommand comandoCheck = new MySqlCommand(queryCheck, conexao))
                    {
                        comandoCheck.Parameters.AddWithValue("@usuario", usuario);
                        long usuarioExiste = (long)comandoCheck.ExecuteScalar();

                        if (usuarioExiste > 0)
                        {
                            MessageBox.Show("Este nome de usuário já está em uso!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    string senhaCriptografada = BCrypt.Net.BCrypt.HashPassword(senha);

                    string queryInsert = "INSERT INTO usuarios (nome_completo, email, nome_usuario, senha, IsAdmin, bloqueado, tentativas_falhas) " +
                                        "VALUES (@nomeCompleto, @email, @usuario, @senha, @IsAdmin, 0, 0)";

                    using (MySqlCommand comandoInsert = new MySqlCommand(queryInsert, conexao))
                    {
                        comandoInsert.Parameters.AddWithValue("@nomeCompleto", nomeCompleto);
                        comandoInsert.Parameters.AddWithValue("@email", email);
                        comandoInsert.Parameters.AddWithValue("@usuario", usuario);
                        comandoInsert.Parameters.AddWithValue("@senha", senhaCriptografada);
                        comandoInsert.Parameters.AddWithValue("@IsAdmin", false); 

                        comandoInsert.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Usuário cadastrado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                LimparCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao cadastrar: {ex.Message}", "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Foto_Perfil(object sender, RoutedEventArgs e)
        {
            FotoDePerfil janela = new FotoDePerfil();
            janela.Show();
            this.Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}