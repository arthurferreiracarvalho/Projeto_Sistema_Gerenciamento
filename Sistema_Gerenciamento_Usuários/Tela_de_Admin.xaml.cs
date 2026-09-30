using System;
using System.Windows;

namespace Sistema_Gerenciamento_Usuários
{
    /// <summary>
    /// Lógica interna para Tela_de_Admin.xaml
    /// </summary>
    public partial class Tela_de_Admin : Window
    {
        private UsuarioModel usuarioLogado;
        public Tela_de_Admin(UsuarioModel usuario)
        {
            InitializeComponent();
            usuarioLogado = usuario;
        }
        public Tela_de_Admin()
        {
            InitializeComponent();
        }

        private void Cadastrar_usuarios_Click(object sender, RoutedEventArgs e)
        {
            CadastrarUsuario janela = new CadastrarUsuario();
            janela.Show();
            this.Close();
        }

        private void Visualizar_Usuarios_Click(object sender, RoutedEventArgs e)
        {
            VisualizarUsuario telaVisualizar = new VisualizarUsuario();
            telaVisualizar.Owner = this;
            telaVisualizar.ShowDialog();
        }

        private void Editar_usuarios_Click(object sender, RoutedEventArgs e)
        {
            VisualizarUsuario telaVisualizar = new VisualizarUsuario();
            telaVisualizar.Owner = this;
            telaVisualizar.ShowDialog();
        }

        private void Excluir_usuarios_Click(object sender, RoutedEventArgs e)
        {
            ExcluirUsuario telaExcluir = new ExcluirUsuario(usuarioLogado);
            telaExcluir.Owner = this;
            telaExcluir.ShowDialog();
        }

        private void Ativar_Desativar_usuários_Click(object sender, RoutedEventArgs e)
        {
            AtivarDesativarUsuario telaAtivarDesativar = new AtivarDesativarUsuario();
            telaAtivarDesativar.Owner = this;
            telaAtivarDesativar.ShowDialog();
        }

        private void Alterar_nivel_acesso_outros_usuarios_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Redefinir_senha_usuarios_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Consultar_registros_auditoria_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Sair_Click(object sender, RoutedEventArgs e)
        {
            Tela_Cadastro janela = new Tela_Cadastro();
            janela.Show();
            this.Close();
        }
    }
}