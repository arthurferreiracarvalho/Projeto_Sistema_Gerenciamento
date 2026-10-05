using System;
using System.Windows;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class EditarPerfilUsuario : Window
    {
        public string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        private readonly UsuarioModel usuarioLogado;
        private string avatarSelecionado = string.Empty;

        public EditarPerfilUsuario() : this(SessaoSistema.UsuarioLogado)
        {
        }

        public EditarPerfilUsuario(UsuarioModel usuario)
        {
            InitializeComponent();
            this.usuarioLogado = usuario ?? SessaoSistema.UsuarioLogado;

            PreencherCampos();
        }

        private void PreencherCampos()
        {
            if (usuarioLogado != null)
            {
                txtNomeCompleto.Text = usuarioLogado.NomeCompleto;
                txtNomeUsuario.Text = usuarioLogado.NomeUsuario;
                avatarSelecionado = usuarioLogado.Avatar;

                AtualizarImagemPreview(avatarSelecionado);
            }
        }

        private void AtualizarImagemPreview(string nomeAvatar)
        {
            if (!string.IsNullOrEmpty(nomeAvatar))
            {
                try
                {
                    string caminho = $"pack://application:,,,/Imagens/{nomeAvatar}";
                    imgFotoPerfil.ImageSource = new BitmapImage(new Uri(caminho, UriKind.RelativeOrAbsolute));
                }
                catch
                {
                
                }
            }
        }

        private void btnTrocarFoto_Click(object sender, RoutedEventArgs e)
        {
            FotoDePerfil janelaFoto = new FotoDePerfil();
            if (janelaFoto.ShowDialog() == true)
            {
                avatarSelecionado = janelaFoto.fotoSelecionada;
                AtualizarImagemPreview(avatarSelecionado);
            }
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e)
        {
            string novoNomeCompleto = txtNomeCompleto.Text.Trim();
            string novoNomeUsuario = txtNomeUsuario.Text.Trim();

            if (string.IsNullOrEmpty(novoNomeCompleto))
            {
                MessageBox.Show("Informe o seu Nome Completo!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (novoNomeUsuario.Length < 3)
            {
                MessageBox.Show("O Nome de Usuário deve conter pelo menos 3 caracteres!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    string queryCheck = "SELECT COUNT(*) FROM usuarios WHERE nome_usuario = @novoUsuario AND id != @id";
                    using (MySqlCommand cmdCheck = new MySqlCommand(queryCheck, conexao))
                    {
                        cmdCheck.Parameters.AddWithValue("@novoUsuario", novoNomeUsuario);
                        cmdCheck.Parameters.AddWithValue("@id", usuarioLogado.Id);

                        long jaExiste = Convert.ToInt64(cmdCheck.ExecuteScalar());
                        if (jaExiste > 0)
                        {
                            MessageBox.Show("Este nome de usuário já está em uso por outra pessoa!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    string queryUpdate = @"UPDATE usuarios 
                                           SET nome_completo = @nomeCompleto, 
                                           nome_usuario = @novoUsuario, 
                                           avatar = @avatar 
                                           WHERE id = @id";

                    using (MySqlCommand cmdUpdate = new MySqlCommand(queryUpdate, conexao))
                    {
                        cmdUpdate.Parameters.AddWithValue("@nomeCompleto", novoNomeCompleto);
                        cmdUpdate.Parameters.AddWithValue("@novoUsuario", novoNomeUsuario);
                        cmdUpdate.Parameters.AddWithValue("@avatar", avatarSelecionado);
                        cmdUpdate.Parameters.AddWithValue("@id", usuarioLogado.Id);

                        cmdUpdate.ExecuteNonQuery();
                    }
                }

                RegistrosAuditoria.RegistrarAcao(
                    novoNomeUsuario,
                    "EDIÇÃO DE PERFIL",
                    $"O usuário '{usuarioLogado.NomeUsuario}' alterou seus dados de perfil para: Nome: '{novoNomeCompleto}', Usuário: '{novoNomeUsuario}'."
                );

                usuarioLogado.NomeCompleto = novoNomeCompleto;
                usuarioLogado.NomeUsuario = novoNomeUsuario;
                usuarioLogado.Avatar = avatarSelecionado;

                MessageBox.Show("Perfil atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar perfil: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}