using System;
using System.Windows;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class Tela_de_Admin : Window
    {
        private readonly UsuarioModel usuarioLogado;

        public Tela_de_Admin()
        {
            InitializeComponent();
        }

        public Tela_de_Admin(UsuarioModel usuario) : this()
        {
            usuarioLogado = usuario;
        }

        private void Cadastrar_usuarios_Click(object sender, RoutedEventArgs e)
        {
            CadastrarUsuario janela = new CadastrarUsuario();
            janela.Owner = this;
            janela.ShowDialog();
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
            AtivarDesativarUsuario telaAtivarDesativar = new AtivarDesativarUsuario(usuarioLogado);
            telaAtivarDesativar.Owner = this;
            telaAtivarDesativar.ShowDialog();
        }

        private void Alterar_nivel_acesso_outros_usuarios_Click(object sender, RoutedEventArgs e)
        {
            AlterarNivelAcesso telaAlterarNivel = new AlterarNivelAcesso(usuarioLogado);
            telaAlterarNivel.Owner = this;
            telaAlterarNivel.ShowDialog();
        }

        private void Redefinir_senha_usuarios_Click(object sender, RoutedEventArgs e)
        {
            UsuarioModel adminAtual = this.usuarioLogado ?? SessaoSistema.UsuarioLogado;
            RedefinirSenhaUsuario telaRedefinir = new RedefinirSenhaUsuario(adminAtual);
            telaRedefinir.Owner = this;
            telaRedefinir.ShowDialog();
        }

        private void Consultar_registros_auditoria_Click(object sender, RoutedEventArgs e)
        {
            ConsultarRegistros telaAuditoria = new ConsultarRegistros();
            telaAuditoria.Owner = this;
            telaAuditoria.ShowDialog();
        }

        private void Sair_Click(object sender, RoutedEventArgs e)
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