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
        private readonly UsuarioModel usuarioLogado;

        // Construtor sem parâmetros (agora encadeia para o construtor principal com a sessão global se existir)
        public VisualizarUsuario() : this(SessaoSistema.UsuarioLogado)
        {
        }

        // Construtor principal para receber a sessão do utilizador logado
        public VisualizarUsuario(UsuarioModel logado)
        {
            InitializeComponent();
            usuarioLogado = logado;
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
                Height = 325,
                Margin = new Thickness(10),
                Background = Brushes.WhiteSmoke,
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
                Width = 80,
                Height = 80,
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
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 200
            };

            TextBlock txtUser = new TextBlock
            {
                Text = $"@{user.NomeUsuario}",
                Foreground = Brushes.DarkSlateGray,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 3)
            };

            TextBlock txtEmail = new TextBlock
            {
                Text = user.Email,
                FontSize = 16,
                Foreground = Brushes.DarkSlateGray,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 200
            };

            TextBlock txtTipo = new TextBlock
            {
                Text = user.IsAdmin ? "ADMINISTRADOR" : "USUÁRIO",
                FontWeight = FontWeights.Bold,
                FontSize = 17,
                Foreground = user.IsAdmin ? Brushes.Red : Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 2)
            };

            TextBlock txtStatus = new TextBlock
            {
                Text = user.Bloqueado ? "Status: INATIVO" : "Status: ATIVO",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = user.Bloqueado ? Brushes.Red : Brushes.Green,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 6)
            };

            TextBlock txtCriacao = new TextBlock
            {
                Text = $"Criado em: {user.DataCriacao:dd/MM/yyyy}",
                FontSize = 13,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            string textoLogin = user.UltimoLogin.HasValue
                ? $"Último login: {user.UltimoLogin.Value:dd/MM/yyyy HH:mm}"
                : "Último login: Nenhum acesso.";

            TextBlock txtUltimoLogin = new TextBlock
            {
                Text = textoLogin,
                FontSize = 13,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 6)
            };

            Button btnEditar = new Button
            {
                Content = "✏ EDITAR",
                Width = 105,
                Height = 30,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD2D2D2")),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                Cursor = System.Windows.Input.Cursors.Hand,
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

        // Evento de clique do botão EDITAR
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is UsuarioModel usuarioAlvo)
            {
                // Regra 1: Se o alvo for o Administrador Principal (ID 1)
                if (usuarioAlvo.Id == 1)
                {
                    // Permite a edição caso o usuário logado seja o próprio ID 1
                    // Bloqueia apenas se houver outro usuário logado com ID diferente tentando editar o Admin Principal
                    if (usuarioLogado != null && usuarioLogado.Id != 1)
                    {
                        MessageBox.Show("Acesso Negado! As informações e a foto do Administrador Principal (ID 1) não podem ser alteradas por outros utilizadores.", "Acesso Restrito", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                }
                else
                {
                    // Regra 2: Para demais usuários, verifica hierarquia de ID (ID maior não edita ID menor)
                    if (usuarioLogado != null && usuarioLogado.Id > usuarioAlvo.Id)
                    {
                        MessageBox.Show("Acesso Negado! A sua hierarquia não permite editar os dados deste utilizador.", "Hierarquia Insuficiente", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                }

                // Abre a janela EditarUsuario repassando a sessão e o ID do utilizador selecionado
                EditarUsuario telaEditar = new EditarUsuario(usuarioLogado, usuarioAlvo.Id);
                telaEditar.Owner = this;
                telaEditar.ShowDialog();

                // Recarrega os cards atualizados
                CarregarUsuariosCards();
            }
        }
    }

    // Classe auxiliar de Sessão Global para manter o login ativo
    public static class SessaoSistema
    {
        public static UsuarioModel UsuarioLogado { get; set; }
    }
}