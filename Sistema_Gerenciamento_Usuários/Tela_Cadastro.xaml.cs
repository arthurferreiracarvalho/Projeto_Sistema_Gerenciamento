using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using BCrypt.Net;


namespace Sistema_Gerenciamento_Usuários
{
    /// <summary>
    /// Lógica interna para Tela_Cadastro.xaml
    /// </summary>
    public partial class Tela_Cadastro : Window
    {
        public string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        public Tela_Cadastro()
        {
            InitializeComponent();
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
            string email = Email.Text.Trim();
            string usuario = Usuario.Text.Trim();
            string senha = Senha.Password.Trim();


            if (usuario.Length < 3)
            {
                MessageBox.Show("Nome inválido. (Deve conter pelo menos 3 caracteres!)", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            if (Comfirmar_senha.Password != senha || string.IsNullOrEmpty(Comfirmar_senha.Password))
            {
                MessageBox.Show("As senhas não são iguais. Tente novamente!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();


                    string queryCheck = "SELECT COUNT(*) FROM usuarios WHERE usuario = @usuario";
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

                    string queryInsert = "INSERT INTO usuarios (email, usuario, senha, IsAdmin) VALUES (@email, @usuario, @senha, @IsAdmin)";

                    using (MySqlCommand comandoInsert = new MySqlCommand(queryInsert, conexao))
                    {
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
       
    }
}

