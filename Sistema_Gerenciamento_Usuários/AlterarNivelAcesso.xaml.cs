using System;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class AlterarNivelAcesso : Window
    {
        private readonly string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        private int usuarioIdEncontrado = -1;
        private string usuarioNomeEncontrado = string.Empty;
        private bool usuarioEncontradoIsAdmin = false;
        private readonly UsuarioModel usuarioLogado;

        public AlterarNivelAcesso()
        {
            InitializeComponent();
        }

        public AlterarNivelAcesso(UsuarioModel logado) : this()
        {
            usuarioLogado = logado;
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarUsuario();
        }

        private void txtBuscar_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarUsuario();
            }
        }

        private void BuscarUsuario()
        {
            string termo = txtBuscar.Text.Trim();

            if (string.IsNullOrEmpty(termo))
            {
                MessageBox.Show("Digite o nome de usuário ou e-mail para pesquisar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();
                    string query = "SELECT id, nome_completo, nome_usuario, email, IsAdmin FROM usuarios WHERE nome_usuario = @termo OR email = @termo LIMIT 1";

                    using (var cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@termo", termo);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                usuarioIdEncontrado = reader.GetInt32("id");
                                usuarioEncontradoIsAdmin = reader.GetBoolean("IsAdmin");

                                string nomeCompleto = reader.IsDBNull(reader.GetOrdinal("nome_completo")) ? "" : reader.GetString("nome_completo");
                                string nomeUsuario = reader.GetString("nome_usuario");
                                string email = reader.GetString("email");

                                usuarioNomeEncontrado = nomeUsuario;

                                txtNomeCompleto.Text = $"Nome: {(string.IsNullOrEmpty(nomeCompleto) ? nomeUsuario : nomeCompleto)}";
                                txtNomeUsuario.Text = $"Usuário: @{nomeUsuario}";
                                txtEmail.Text = $"E-mail: {email}";
                                txtNivelAtual.Text = $"Nível Atual: {(usuarioEncontradoIsAdmin ? "Administrador" : "Usuário Comum")}";
                                btnAlternarNivel.Content = usuarioEncontradoIsAdmin ? "ALTERAR PARA USUÁRIO" : "PROMOVER A ADMINISTRADOR";
                            }
                            else
                            {
                                LimparCampos();
                                MessageBox.Show("Usuário não encontrado!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar usuário: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimparCampos()
        {
            usuarioIdEncontrado = -1;
            usuarioNomeEncontrado = string.Empty;
            usuarioEncontradoIsAdmin = false;
            txtNomeCompleto.Text = "Nome: -";
            txtNomeUsuario.Text = "Usuário: -";
            txtEmail.Text = "E-mail: -";
            txtNivelAtual.Text = "Nível Atual: -";
            btnAlternarNivel.Content = "ALTERAR PERMISSÃO";
        }

        private void btnAlternarNivel_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioIdEncontrado == -1)
            {
                MessageBox.Show("Busque um usuário válido antes de alterar o nível de acesso.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (usuarioLogado == null)
            {
                MessageBox.Show("Operação inválida! Não foi possível identificar o usuário logado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (!usuarioLogado.IsAdmin)
            {
                MessageBox.Show("Acesso negado! Apenas administradores podem alterar permissões.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (usuarioIdEncontrado == usuarioLogado.Id)
            {
                MessageBox.Show("Você não pode alterar o seu próprio nível de acesso.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (usuarioIdEncontrado == 1)
            {
                MessageBox.Show("Acesso Negado! O Administrador Principal do sistema não pode ter suas permissões alteradas.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (usuarioLogado.Id > usuarioIdEncontrado)
            {
                MessageBox.Show("Acesso Negado! Você não tem permissão para alterar a permissão deste admistrador.", "Operação Inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            bool novoStatusIsAdmin = !usuarioEncontradoIsAdmin;

            if (!novoStatusIsAdmin && usuarioEncontradoIsAdmin)
            {
                try
                {
                    using (var conexao = new MySqlConnection(connectionString))
                    {
                        conexao.Open();
                        string queryContarAdmins = "SELECT COUNT(*) FROM usuarios WHERE IsAdmin = 1";

                        using (var cmd = new MySqlCommand(queryContarAdmins, conexao))
                        {
                            long totalAdmins = Convert.ToInt64(cmd.ExecuteScalar());

                            if (totalAdmins <= 1)
                            {
                                MessageBox.Show("Não é possivél alterar o acesso deste usuário. O sistema deve possuir pelo menos 1 administrador ativo cadastrado.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao verificar contagem de administradores: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            string acaoTexto = novoStatusIsAdmin ? "promover este Usuário a Administrador" : "promover este Administrador a Usuário";
            string mensagemSucesso = novoStatusIsAdmin ? "Usuário promovido a Administrador com sucesso!" : "Administrador promovido a Usuário com sucesso!";

            MessageBoxResult result = MessageBox.Show($"Deseja realmente {acaoTexto}?", "Confirmar Operação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var conexao = new MySqlConnection(connectionString))
                    {
                        conexao.Open();
                        string query = "UPDATE usuarios SET IsAdmin = @novoIsAdmin WHERE id = @id";

                        using (var cmd = new MySqlCommand(query, conexao))
                        {
                            cmd.Parameters.AddWithValue("@novoIsAdmin", novoStatusIsAdmin ? 1 : 0);
                            cmd.Parameters.AddWithValue("@id", usuarioIdEncontrado);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    string nomeAdminLogado = usuarioLogado != null ? usuarioLogado.NomeUsuario : "SISTEMA";
                    string novoNivel = novoStatusIsAdmin ? "Administrador" : "Usuário Comum";

                    RegistrosAuditoria.RegistrarAcao(
                        nomeAdminLogado,
                        "ALTERAÇÃO DE PERMISSÃO",
                        $"O administrador '{nomeAdminLogado}' alterou o nível de acesso do usuário '{usuarioNomeEncontrado}' para '{novoNivel}'."
                    );

                    MessageBox.Show(mensagemSucesso, "Sucesso!", MessageBoxButton.OK, MessageBoxImage.Information);
                    BuscarUsuario();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao alterar permissão: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}