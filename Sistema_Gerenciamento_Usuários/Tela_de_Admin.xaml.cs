using System;
using System.Windows;

namespace Sistema_Gerenciamento_Usuários
{
    /// <summary>
    /// Lógica interna para Tela_de_Admin.xaml
    /// </summary>
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
            // Abre a tela de cadastro como modal mantendo o Admin em segundo plano
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
            // Repassa o usuarioLogado para aplicar as travas de hierarquia de ID
            ExcluirUsuario telaExcluir = new ExcluirUsuario(usuarioLogado);
            telaExcluir.Owner = this;
            telaExcluir.ShowDialog();
        }

        private void Ativar_Desativar_usuários_Click(object sender, RoutedEventArgs e)
        {
            // Repassa o usuarioLogado para aplicar as travas de hierarquia de ID
            AtivarDesativarUsuario telaAtivarDesativar = new AtivarDesativarUsuario(usuarioLogado);
            telaAtivarDesativar.Owner = this;
            telaAtivarDesativar.ShowDialog();
        }

        private void Alterar_nivel_acesso_outros_usuarios_Click(object sender, RoutedEventArgs e)
        {
            // Repassa o usuarioLogado para aplicar as travas de hierarquia de ID
            AlterarNivelAcesso telaAlterarNivel = new AlterarNivelAcesso(usuarioLogado);
            telaAlterarNivel.Owner = this;
            telaAlterarNivel.ShowDialog();
        }

        private void Redefinir_senha_usuarios_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidade de redefinição de senha em desenvolvimento.", "Em Breve", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Consultar_registros_auditoria_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidade de consulta de auditoria em desenvolvimento.", "Em Breve", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Sair_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult resultado = MessageBox.Show("Deseja realmente encerar a sessão e sair?", "Confirmar Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                MainWindow telaLogin = new MainWindow();
                telaLogin.Show();
                this.Close();
            }
        }
    }
}