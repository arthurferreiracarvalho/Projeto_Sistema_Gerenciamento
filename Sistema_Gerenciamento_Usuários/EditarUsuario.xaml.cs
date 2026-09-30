using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class EditarUsuario : Window
    {
        public string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        private readonly UsuarioModel usuarioAtual;

        private readonly List<string> avataresDisponiveis = new List<string>
        {
            "Usuario 1.png",
            "Usuario 2.png",
            "Usuario 3.png",
            "Usuario 4.png",
            "Usuario 5.png"
        };
        private int indexAvatarAtual = 0;

        public EditarUsuario(UsuarioModel usuario)
        {
            InitializeComponent();
            usuarioAtual = usuario;
            PreencherCampos();
        }

        private void PreencherCampos()
        {
            NomeCompleto.Text = usuarioAtual.NomeCompleto;
            NomeUsuario.Text = usuarioAtual.NomeUsuario;
            EmailUsuario.Text = usuarioAtual.Email;

            int idx = avataresDisponiveis.FindIndex(a => a.Equals(usuarioAtual.Avatar, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) indexAvatarAtual = idx;

            AtualizarExibicaoAvatar();
        }

        private void AtualizarExibicaoAvatar()
        {
            string nomeFoto = avataresDisponiveis[indexAvatarAtual];

            NomePerfil.Text = nomeFoto;

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
                FotoPerfil.Source = null;
            }
        }

        private void AnteriorFoto_Click(object sender, RoutedEventArgs e)
        {
            indexAvatarAtual--;
            if (indexAvatarAtual < 0) indexAvatarAtual = avataresDisponiveis.Count - 1;
            AtualizarExibicaoAvatar();
        }

        private void ProximaFoto_Click(object sender, RoutedEventArgs e)
        {
            indexAvatarAtual++;
            if (indexAvatarAtual >= avataresDisponiveis.Count) indexAvatarAtual = 0;
            AtualizarExibicaoAvatar();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomeUsuario.Text))
            {
                MessageBox.Show("O nome de usuário não pode estar vazio.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    string query = @"UPDATE usuarios SET 
                                    nome_completo = @nome, 
                                    nome_usuario = @usuario, 
                                    email = @email, 
                                    avatar = @avatar, 
                                    data_ultima_alteracao = NOW() 
                                    WHERE id = @id";

                    using (var cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@nome", NomeCompleto.Text.Trim());
                        cmd.Parameters.AddWithValue("@usuario", NomeUsuario.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", EmailUsuario.Text.Trim());
                        cmd.Parameters.AddWithValue("@avatar", avataresDisponiveis[indexAvatarAtual]);
                        cmd.Parameters.AddWithValue("@id", usuarioAtual.Id);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Usuário atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar usuário: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}