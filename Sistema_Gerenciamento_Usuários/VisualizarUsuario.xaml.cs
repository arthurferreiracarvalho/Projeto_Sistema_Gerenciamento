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
            if (ContainerCards == null) return;
            ContainerCards.Children.Clear();

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    string query = "SELECT id, nome_completo, email, nome_usuario, avatar, IsAdmin, bloqueado, data_criacao, data_ultima_alteracao, ultimo_login FROM usuarios WHERE 1=1";

                    if (txtPesquisa != null && !string.IsNullOrWhiteSpace(txtPesquisa.Text))
                    {
                        query += " AND (nome_completo LIKE @busca OR nome_usuario LIKE @busca OR email LIKE @busca)";
                    }

                    if (cmbPerfil != null && cmbPerfil.SelectedIndex > 0)
                    {
                        if (cmbPerfil.SelectedIndex == 1)
                            query += " AND IsAdmin = 1";
                        else if (cmbPerfil.SelectedIndex == 2)
                            query += " AND IsAdmin = 0";
                    }

                    if (cmbStatus != null && cmbStatus.SelectedIndex > 0)
                    {
                        if (cmbStatus.SelectedIndex == 1)
                            query += " AND bloqueado = 0";
                        else if (cmbStatus.SelectedIndex == 2)
                            query += " AND bloqueado = 1";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        if (txtPesquisa != null && !string.IsNullOrWhiteSpace(txtPesquisa.Text))
                        {
                            cmd.Parameters.AddWithValue("@busca", $"%{txtPesquisa.Text.Trim()}%");
                        }

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
                                    Bloqueado = reader.GetBoolean("bloqueado"),

                                    DataCriacao = reader.IsDBNull(reader.GetOrdinal("data_criacao")) ? DateTime.Now : reader.GetDateTime("data_criacao"),
                                    DataUltimaAlteracao = reader.IsDBNull(reader.GetOrdinal("data_ultima_alteracao")) ? DateTime.Now : reader.GetDateTime("data_ultima_alteracao"),
                                    UltimoLogin = reader.IsDBNull(reader.GetOrdinal("ultimo_login")) ? (DateTime?)null : reader.GetDateTime("ultimo_login")
                                };

                                ContainerCards.Children.Add(CriarCardUsuario(user));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar utilizadores: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtPesquisa_TextChanged(object sender, TextChangedEventArgs e)
        {
            CarregarUsuariosCards();
        }

        private void cmbPerfil_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CarregarUsuariosCards();
        }

        private void cmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CarregarUsuariosCards();
        }

        private Border CriarCardUsuario(UsuarioModel user)
        {
            Border card = new Border
            {
                Width = 230,
                Height = 370,
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

            Image img = new Image
            {
                Width = 70,
                Height = 70,
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

            TextBlock txtNome = new TextBlock
            {
                Text = string.IsNullOrEmpty(user.NomeCompleto) ? user.NomeUsuario : user.NomeCompleto,
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 200
            };

            TextBlock txtUser = new TextBlock
            {
                Text = $"@{user.NomeUsuario}",
                Foreground = Brushes.Gray,
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 3)
            };

            TextBlock txtEmail = new TextBlock
            {
                Text = user.Email,
                FontSize = 15,
                Foreground = Brushes.DarkGray,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 200
            };

            TextBlock txtTipo = new TextBlock
            {
                Text = user.IsAdmin ? "ADMINISTRADOR" : "USUÁRIO",
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                Foreground = user.IsAdmin ? Brushes.Red : Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 2)
            };

            TextBlock txtStatus = new TextBlock
            {
                Text = user.Bloqueado ? "Status: INATIVO" : "Status: ATIVO",
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                Foreground = user.Bloqueado ? Brushes.Red : Brushes.Green,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 6)
            };

            TextBlock txtCriacao = new TextBlock
            {
                Text = $"Criado em: {user.DataCriacao:dd/MM/yyyy}",
                FontSize = 12,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            string textoLogin = user.UltimoLogin.HasValue
                ? $"Último login: {user.UltimoLogin.Value:dd/MM/yyyy HH:mm}"
                : "Último login: Nenhum acesso.";

            TextBlock txtUltimoLogin = new TextBlock
            {
                Text = textoLogin,
                FontSize = 12,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 6)
            };

            Button btnEditar = new Button
            {
                Content = "Editar",
                Width = 90,
                Height = 26,
                Margin = new Thickness(0, 5, 0, 0),
                Tag = user
            };
            btnEditar.Click += BtnEditar_Click;

            stack.Children.Add(img);
            stack.Children.Add(txtNome);
            stack.Children.Add(txtUser);
            stack.Children.Add(txtEmail);
            stack.Children.Add(txtTipo);
            stack.Children.Add(txtStatus);
            stack.Children.Add(txtCriacao);
            stack.Children.Add(txtUltimoLogin);
            stack.Children.Add(btnEditar);

            card.Child = stack;
            return card;
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is UsuarioModel user)
            {
                EditarUsuario telaEdicao = new EditarUsuario(user);
                telaEdicao.Owner = this;

                if (telaEdicao.ShowDialog() == true)
                {
                    CarregarUsuariosCards();
                }
            }
        }
    }
}