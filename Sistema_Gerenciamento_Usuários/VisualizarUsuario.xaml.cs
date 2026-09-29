using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class VisualizarUsuario : Window
    {
        public string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";

        public VisualizarUsuario()
        {
            InitializeComponent();
            CarregarUsuariosCards();
        }

        private void CarregarUsuariosCards()
        {
            ContainerCards.Children.Clear();

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();
                    // Consulta SQL com as novas colunas de data
                    string query = "SELECT id, nome_completo, email, nome_usuario, avatar, IsAdmin, data_criacao, data_ultima_alteracao, ultimo_login FROM usuarios";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new UsuarioModel
                            {
                                Id = reader.GetInt32("id"),
                                NomeCompleto = reader.IsDBNull(reader.GetOrdinal("nome_completo")) ? "" : reader.GetString("nome_completo"),
                                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString("email"),
                                NomeUsuario = reader.IsDBNull(reader.GetOrdinal("nome_usuario")) ? "" : reader.GetString("nome_usuario"),
                                Avatar = reader.IsDBNull(reader.GetOrdinal("avatar")) ? "Usuario 1.png" : reader.GetString("avatar"),
                                IsAdmin = reader.GetBoolean("IsAdmin"),

                                // Mapeamento das colunas de data
                                DataCriacao = reader.IsDBNull(reader.GetOrdinal("data_criacao")) ? DateTime.Now : reader.GetDateTime("data_criacao"),
                                DataUltimaAlteracao = reader.IsDBNull(reader.GetOrdinal("data_ultima_alteracao")) ? DateTime.Now : reader.GetDateTime("data_ultima_alteracao"),
                                UltimoLogin = reader.IsDBNull(reader.GetOrdinal("ultimo_login")) ? (DateTime?)null : reader.GetDateTime("ultimo_login")
                            };

                            ContainerCards.Children.Add(CriarCardUsuario(user));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar utilizadores: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CriarCardUsuario(UsuarioModel user)
        {
            Border card = new Border
            {
                Width = 230,
                Height = 310,
                Margin = new Thickness(10),
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(1)
            };

            StackPanel stack = new StackPanel
            {
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // 1. Imagem de Perfil
            Image img = new Image
            {
                Width = 75,
                Height = 75,
                Margin = new Thickness(0, 5, 0, 8)
            };

            try
            {
                string nomeFoto = string.IsNullOrEmpty(user.Avatar) ? "Usuario 1.png" : user.Avatar;
                string caminhoArquivoLocal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Imagens", nomeFoto);

                if (File.Exists(caminhoArquivoLocal))
                {
                    img.Source = new BitmapImage(new Uri(caminhoArquivoLocal, UriKind.Absolute));
                }
                else
                {
                    string caminhoPack = $"pack://application:,,,/Imagens/{nomeFoto}";
                    img.Source = new BitmapImage(new Uri(caminhoPack, UriKind.Absolute));
                }
            }
            catch
            {
                try
                {
                    img.Source = new BitmapImage(new Uri("pack://application:,,,/Imagens/Usuario 1.png", UriKind.Absolute));
                }
                catch { }
            }

            // 2. Nome Completo
            TextBlock txtNome = new TextBlock
            {
                Text = string.IsNullOrEmpty(user.NomeCompleto) ? user.NomeUsuario : user.NomeCompleto,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 200
            };

            // 3. Nome de Utilizador
            TextBlock txtUser = new TextBlock
            {
                Text = $"@{user.NomeUsuario}",
                Foreground = Brushes.Gray,
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 3)
            };

            // 4. E-mail
            TextBlock txtEmail = new TextBlock
            {
                Text = user.Email,
                FontSize = 11,
                Foreground = Brushes.DarkGray,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 200
            };

            // 5. Tipo (Admin / Usuário)
            TextBlock txtTipo = new TextBlock
            {
                Text = user.IsAdmin ? "ADMINISTRADOR" : "USUÁRIO",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = user.IsAdmin ? Brushes.Red : Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 6, 0, 6)
            };

            // 6. Data de Criação
            TextBlock txtCriacao = new TextBlock
            {
                Text = $"Criado em: {user.DataCriacao:dd/MM/yyyy HH:mm}",
                FontSize = 10,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // 7. Último Login
            string textoLogin = user.UltimoLogin.HasValue
                ? $"Último login: {user.UltimoLogin.Value:dd/MM/yyyy HH:mm}"
                : "Último login: Nunca";

            TextBlock txtUltimoLogin = new TextBlock
            {
                Text = textoLogin,
                FontSize = 10,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            };

            // Adição dos elementos ao StackPanel
            stack.Children.Add(img);
            stack.Children.Add(txtNome);
            stack.Children.Add(txtUser);
            stack.Children.Add(txtEmail);
            stack.Children.Add(txtTipo);
            stack.Children.Add(txtCriacao);
            stack.Children.Add(txtUltimoLogin);

            card.Child = stack;
            return card;
        }
    }
}