using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MySql.Data.MySqlClient;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class AtivarDesativarUsuario : Window
    {
        private readonly string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        private int usuarioIdEncontrado = -1;
        private string usuarioNomeEncontrado = string.Empty;
        private bool statusAtualBloqueado = false;
        private bool usuarioEncontradoIsAdmin = false;

        private readonly UsuarioModel usuarioLogado;

        public AtivarDesativarUsuario()
        {
            InitializeComponent();
        }

        public AtivarDesativarUsuario(UsuarioModel logado) : this()
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
                    string query = "SELECT id, nome_completo, nome_usuario, email, bloqueado, IsAdmin FROM usuarios WHERE nome_usuario = @termo OR email = @termo LIMIT 1";

                    using (var cmd = new MySqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@termo", termo);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                usuarioIdEncontrado = reader.GetInt32("id");
                                string nomeCompleto = reader.IsDBNull(reader.GetOrdinal("nome_completo")) ? "" : reader.GetString("nome_completo");
                                string nomeUsuario = reader.GetString("nome_usuario");

                                usuarioNomeEncontrado = nomeUsuario;
                                statusAtualBloqueado = reader.GetBoolean("bloqueado");
                                usuarioEncontradoIsAdmin = reader.GetBoolean("IsAdmin");

                                txtNomeCompleto.Text = $"Nome: {(string.IsNullOrEmpty(nomeCompleto) ? nomeUsuario : nomeCompleto)}";
                                txtNomeUsuario.Text = $"Usuário: @{nomeUsuario}";

                                var converter = new BrushConverter();

                                if (statusAtualBloqueado)
                                {
                                    txtStatusAtual.Text = "Status Atual: INATIVO";
                                    txtStatusAtual.Foreground = Brushes.Red;
                                    btnAlternarStatus.Content = "✅ ATIVAR USUÁRIO";
                                    btnAlternarStatus.Background = (Brush)converter.ConvertFromString("#4CAF50")!;
                                }
                                else
                                {
                                    txtStatusAtual.Text = "Status Atual: ATIVO";
                                    txtStatusAtual.Foreground = Brushes.Green;
                                    btnAlternarStatus.Content = "🚫 DESATIVAR USUÁRIO";
                                    btnAlternarStatus.Background = (Brush)converter.ConvertFromString("#F44336")!;
                                }

                                btnAlternarStatus.IsEnabled = true;
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
            txtStatusAtual.Text = "Status Atual: -";
            txtStatusAtual.Foreground = Brushes.Black;
            btnAlternarStatus.IsEnabled = false;
        }

        private void btnAlternarStatus_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioIdEncontrado == -1) return;

            if (usuarioLogado == null)
            {
                MessageBox.Show("Operação inválida! Não foi possível identificar o usuário logado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (!usuarioLogado.IsAdmin)
            {
                MessageBox.Show("Acesso negado! Apenas administradores podem alterar o status de usuários.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (usuarioIdEncontrado == usuarioLogado.Id)
            {
                MessageBox.Show("Você não pode alterar o status da sua própria conta enquanto estiver conectado ao sistema.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            bool novoStatusBloqueado = !statusAtualBloqueado;

            if (novoStatusBloqueado && usuarioEncontradoIsAdmin)
            {
                try
                {
                    using (var conexao = new MySqlConnection(connectionString))
                    {
                        conexao.Open();
                        string queryContarAdmins = "SELECT COUNT(*) FROM usuarios WHERE IsAdmin = 1 AND bloqueado = 0";

                        using (var cmd = new MySqlCommand(queryContarAdmins, conexao))
                        {
                            long totalAdminsAtivos = Convert.ToInt64(cmd.ExecuteScalar());

                            if (totalAdminsAtivos <= 1)
                            {
                                MessageBox.Show("Não é possível desativar este usuário. O sistema deve possuir pelo menos 1 administrador ativo cadastrado.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao validar administradores ativos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            string acaoTexto = novoStatusBloqueado ? "desativar" : "ativar";
            string mensagemSucesso = novoStatusBloqueado ? "Usuário desativado com sucesso!" : "Usuário ativado com sucesso!";

            MessageBoxResult result = MessageBox.Show($"Deseja realmente {acaoTexto} este usuário?", "Confirmar Operação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var conexao = new MySqlConnection(connectionString))
                    {
                        conexao.Open();

                        string query = "UPDATE usuarios SET bloqueado = @novoBloqueado, status = @novoStatus, data_ultima_alteracao = NOW() WHERE id = @id";

                        using (var cmd = new MySqlCommand(query, conexao))
                        {
                            cmd.Parameters.AddWithValue("@novoBloqueado", novoStatusBloqueado ? 1 : 0);
                            cmd.Parameters.AddWithValue("@novoStatus", novoStatusBloqueado ? "Inativo" : "Ativo");
                            cmd.Parameters.AddWithValue("@id", usuarioIdEncontrado);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    string nomeAdminLogado = usuarioLogado != null ? usuarioLogado.NomeUsuario : "SISTEMA";
                    string acaoStatus = novoStatusBloqueado ? "bloqueou/desativou" : "desbloqueou/ativou";

                    RegistrosAuditoria.RegistrarAcao(
                        nomeAdminLogado,
                        "ALTERAÇÃO DE STATUS",
                        $"O administrador '{nomeAdminLogado}' {acaoStatus} o usuário '{usuarioNomeEncontrado}' (ID: {usuarioIdEncontrado})."
                    );

                    MessageBox.Show(mensagemSucesso, "Sucesso!", MessageBoxButton.OK, MessageBoxImage.Information);
                    BuscarUsuario();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao alterar status: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}