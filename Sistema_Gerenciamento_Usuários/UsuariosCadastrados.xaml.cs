using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class UsuariosCadastrados : Window
    {
        public string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        private List<UsuarioCardModel> listaTodosUsuarios = new List<UsuarioCardModel>();

        public UsuariosCadastrados()
        {
            InitializeComponent();
            CarregarUsuarios();
        }

        private void CarregarUsuarios()
        {
            listaTodosUsuarios.Clear();

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    string query = @"SELECT nome_completo, nome_usuario, email, avatar 
                                     FROM usuarios 
                                     WHERE IsAdmin = 0 AND bloqueado = 0";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string nomeCompleto = reader["nome_completo"]?.ToString();
                                string nomeUsuario = reader["nome_usuario"]?.ToString();
                                string email = reader["email"]?.ToString();
                                string avatar = reader["avatar"]?.ToString();

                                string caminhoFoto = !string.IsNullOrEmpty(avatar)
                                    ? $"pack://application:,,,/Imagens/{avatar}"
                                    : "pack://application:,,,/Imagens/default.png";

                                listaTodosUsuarios.Add(new UsuarioCardModel
                                {
                                    NomeCompleto = string.IsNullOrEmpty(nomeCompleto) ? nomeUsuario : nomeCompleto,
                                    NomeUsuarioFormatado = $"@{nomeUsuario}",
                                    Email = email,
                                    CaminhoAvatar = caminhoFoto
                                });
                            }
                        }
                    }
                }

                listaUsuarios.ItemsSource = listaTodosUsuarios;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar usuários: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtPesquisa_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = txtPesquisa.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(filtro))
            {
                listaUsuarios.ItemsSource = listaTodosUsuarios;
            }
            else
            {
                var filtrados = listaTodosUsuarios.Where(u =>
                    u.NomeCompleto.ToLower().Contains(filtro) ||
                    u.NomeUsuarioFormatado.ToLower().Contains(filtro) ||
                    u.Email.ToLower().Contains(filtro)
                ).ToList();

                listaUsuarios.ItemsSource = filtrados;
            }
        }

        private void btnVoltar_Click(object sender, RoutedEventArgs e)
        {
            if (this.Owner != null)
            {
                this.Owner.Show();
            }
            this.Close();
        }
    }

    public class UsuarioCardModel
    {
        public string NomeCompleto { get; set; }
        public string NomeUsuarioFormatado { get; set; }
        public string Email { get; set; }
        public string CaminhoAvatar { get; set; }
    }
}