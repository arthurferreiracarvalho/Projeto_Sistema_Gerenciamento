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

                    string query = "SELECT id, usuario, senha, IsAdmin, bloqueado, tentativas_falhas FROM usuarios WHERE usuario = @usuario";

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
                                int tentativas = reader.GetInt32("tentativas_falhas");

                                if (bloqueado)
                                {
                                    MessageBox.Show("Conta bloqueada por excesso de tentativas. Contate o Administrador.", "Acesso Negado", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }

                                bool senhaValida = BCrypt.Net.BCrypt.Verify(senha, hashSenha);

                                if (senhaValida)
                                {
                                    reader.Close();

                                    string updateSucesso = "UPDATE usuarios SET ultimo_login = NOW(), tentativas_falhas = 0 WHERE id = @id";
                                    using (MySqlCommand cmdUpdate = new MySqlCommand(updateSucesso, conexao))
                                    {
                                        cmdUpdate.Parameters.AddWithValue("@id", id);
                                        cmdUpdate.ExecuteNonQuery();
                                    }

                                    if (isAdmin)
                                    {
                                        Tela_de_Admin telaAdmin = new Tela_de_Admin();
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
                                    reader.Close();

                                    tentativas++;
                                    bool deveBloquear = tentativas >= 3;

                                    string updateFalha = "UPDATE usuarios SET tentativas_falhas = @tentativas, bloqueado = @bloquear WHERE id = @id";
                                    using (MySqlCommand cmdFalha = new MySqlCommand(updateFalha, conexao))
                                    {
                                        cmdFalha.Parameters.AddWithValue("@tentativas", tentativas);
                                        cmdFalha.Parameters.AddWithValue("@bloquear", deveBloquear);
                                        cmdFalha.Parameters.AddWithValue("@id", id);
                                        cmdFalha.ExecuteNonQuery();
                                    }

                                    MessageBox.Show("Usuário ou senha inválidos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Usuário ou senha inválidos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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

