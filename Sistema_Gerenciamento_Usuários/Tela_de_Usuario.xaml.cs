using System;
using System.Windows;
using System.Windows.Media.Imaging;
using MySql.Data.MySqlClient;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class Tela_de_Usuario : Window
    {
        public string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        private readonly UsuarioModel usuarioLogado;

        public Tela_de_Usuario() : this(SessaoSistema.UsuarioLogado)
        {
        }

        public Tela_de_Usuario(UsuarioModel usuario)
        {
            InitializeComponent();
            this.usuarioLogado = usuario ?? SessaoSistema.UsuarioLogado;

            CarregarDadosPerfil();
        }

        public void CarregarDadosPerfil()
        {
            if (usuarioLogado != null)
            {
                txtNomeCompleto.Text = string.IsNullOrEmpty(usuarioLogado.NomeCompleto) ? usuarioLogado.NomeUsuario : usuarioLogado.NomeCompleto;
                txtNomeUsuario.Text = $"@{usuarioLogado.NomeUsuario}";
                txtEmail.Text = usuarioLogado.Email;

                if (!string.IsNullOrEmpty(usuarioLogado.Avatar))
                {
                    try
                    {
                        string caminhoFoto = $"pack://application:,,,/Imagens/{usuarioLogado.Avatar}";
                        imgFotoPerfil.ImageSource = new BitmapImage(new Uri(caminhoFoto, UriKind.RelativeOrAbsolute));
                    }
                    catch
                    {
                     
                    }
                }
            }
        }

        private void btnVisualizarUsuarios_Click(object sender, RoutedEventArgs e)
        {
            UsuariosCadastrados telaLista = new UsuariosCadastrados();
            telaLista.Owner = this;
            telaLista.Show();
            this.Hide();
        }

        private void btnEditarPerfil_Click(object sender, RoutedEventArgs e)
        {
            EditarPerfilUsuario telaEditar = new EditarPerfilUsuario(usuarioLogado);
            if (telaEditar.ShowDialog() == true)
            {
                CarregarDadosPerfil();
            }
        }

        private void btnAlterarSenha_Click(object sender, RoutedEventArgs e)
        {
            AlterarSenhaUsuario telaSenha = new AlterarSenhaUsuario(usuarioLogado);
            telaSenha.ShowDialog();
        }

        private void btnSair_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult resultado = MessageBox.Show("Deseja realmente encerrar a sessão e sair?", "Confirmar Saída", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                MainWindow telaLogin = new MainWindow();
                telaLogin.Show();
                this.Close();
            }
        }
    }
}