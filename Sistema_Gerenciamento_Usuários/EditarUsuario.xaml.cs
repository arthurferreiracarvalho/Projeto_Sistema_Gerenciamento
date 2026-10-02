using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class EditarUsuario : Window
    {
        public string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        private readonly UsuarioModel usuarioLogado;
        private readonly int usuarioIdParaEditar;
        private readonly string[] imagensPadrao = new string[]
        {
            "Usuario 1.png",
            "Usuario 2.png",
            "Usuario 3.png",
            "Usuario 4.png",
            "Usuario 5.png"
        };

        private int indiceImagemAtual = 0;

        public EditarUsuario()
        {
            InitializeComponent();
        }

        public EditarUsuario(UsuarioModel logado, int idParaEditar) : this()
        {
            usuarioLogado = logado;
            usuarioIdParaEditar = idParaEditar;

            CarregarDadosUsuario();
        }

        private void CarregarDadosUsuario()
        {
            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();
                    string query = "SELECT id, nome_completo, email, nome_usuario, avatar FROM usuarios WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@id", usuarioIdParaEditar);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (NomeCompleto != null)
                                    NomeCompleto.Text = reader.IsDBNull(reader.GetOrdinal("nome_completo")) ? "" : reader.GetString("nome_completo");

                                if (EmailUsuario != null)
                                    EmailUsuario.Text = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString("email");

                                if (NomeUsuario != null)
                                    NomeUsuario.Text = reader.IsDBNull(reader.GetOrdinal("nome_usuario")) ? "" : reader.GetString("nome_usuario");

                                string avatarBanco = reader.IsDBNull(reader.GetOrdinal("avatar")) ? "Usuario 1.png" : reader.GetString("avatar");
                                string nomeCurto = Path.GetFileName(avatarBanco);
                                int pos = Array.IndexOf(imagensPadrao, nomeCurto);
                                if (pos >= 0)
                                {
                                    indiceImagemAtual = pos;
                                }

                                AtualizarExibicaoFoto();
                            }
                            else
                            {
                                MessageBox.Show("Utilizador não encontrado no banco de dados.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                                Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados do utilizador: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AtualizarExibicaoFoto()
        {
            if (FotoPerfil == null) return;

            string nomeFoto = imagensPadrao[indiceImagemAtual];

            if (NomePerfil != null)
            {
                NomePerfil.Text = nomeFoto;
            }

            try
            {
                string caminhoArquivoLocal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Imagens", nomeFoto);

                if (File.Exists(caminhoArquivoLocal))
                {
                    FotoPerfil.Source = new BitmapImage(new Uri(caminhoArquivoLocal, UriKind.Absolute));
                }
                else
                {
                    string caminhoPack = $"pack://application:,,,/Imagens/{nomeFoto}";
                    FotoPerfil.Source = new BitmapImage(new Uri(caminhoPack, UriKind.Absolute));
                }
            }
            catch
            {
                try
                {
                    FotoPerfil.Source = new BitmapImage(new Uri("pack://application:,,,/Imagens/Usuario 1.png", UriKind.Absolute));
                }
                catch { }
            }
        }

        private void AnteriorFoto_Click(object sender, RoutedEventArgs e)
        {
            indiceImagemAtual--;
            if (indiceImagemAtual < 0)
            {
                indiceImagemAtual = imagensPadrao.Length - 1;
            }
            AtualizarExibicaoFoto();
        }

        private void ProximaFoto_Click(object sender, RoutedEventArgs e)
        {
            indiceImagemAtual++;
            if (indiceImagemAtual >= imagensPadrao.Length)
            {
                indiceImagemAtual = 0;
            }
            AtualizarExibicaoFoto();
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            string txtUsuario = NomeUsuario != null ? NomeUsuario.Text.Trim() : "";
            string txtEmail = EmailUsuario != null ? EmailUsuario.Text.Trim() : "";
            string txtNome = NomeCompleto != null ? NomeCompleto.Text.Trim() : "";

            if (string.IsNullOrWhiteSpace(txtUsuario) || string.IsNullOrWhiteSpace(txtEmail))
            {
                MessageBox.Show("Por favor, preencha os campos obrigatórios (Nome de Usuário e E-mail).", "Campos Obrigatórios!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (usuarioIdParaEditar == 1 && (usuarioLogado == null || usuarioLogado.Id != 1))
            {
                MessageBox.Show("Acesso Negado! As informações do Administrador Principal não podem ser alteradas.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (usuarioLogado != null && usuarioLogado.Id > usuarioIdParaEditar)
            {
                MessageBox.Show("Acesso Negado! Você não pode alterar as informações desse administrador.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            string avatarSelecionado = imagensPadrao[indiceImagemAtual];

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    string query = @"UPDATE usuarios 
                                     SET nome_completo = @nome, 
                                         email = @email, 
                                         nome_usuario = @usuario, 
                                         avatar = @avatar, 
                                         data_ultima_alteracao = @dataAlteracao 
                                     WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@nome", txtNome);
                        cmd.Parameters.AddWithValue("@email", txtEmail);
                        cmd.Parameters.AddWithValue("@usuario", txtUsuario);
                        cmd.Parameters.AddWithValue("@avatar", avatarSelecionado);
                        cmd.Parameters.AddWithValue("@dataAlteracao", DateTime.Now);
                        cmd.Parameters.AddWithValue("@id", usuarioIdParaEditar);

                        int linhasAfetadas = cmd.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            string nomeAdminLogado = usuarioLogado != null ? usuarioLogado.NomeUsuario : "SISTEMA";

                            RegistrosAuditoria.RegistrarAcao(
                                nomeAdminLogado,
                                "EDIÇÃO",
                                $"O administrador '{nomeAdminLogado}' alterou os dados do usuário '{txtUsuario}' (ID: {usuarioIdParaEditar})."
                            );

                            MessageBox.Show("Utilizador atualizado com sucesso!", "Sucesso!", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Nenhuma alteração foi realizada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar o utilizador no banco de dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}