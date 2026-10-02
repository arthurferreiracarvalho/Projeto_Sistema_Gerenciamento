using System;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class ExcluirUsuario : Window
    {
        private readonly string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        private int usuarioIdEncontrado = -1;
        private bool usuarioEncontradoIsAdmin = false;
        private string usuarioNomeEncontrado = string.Empty;
        private readonly UsuarioModel usuarioLogado;

        public ExcluirUsuario()
        {
            InitializeComponent();
        }

        public ExcluirUsuario(UsuarioModel logado) : this()
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
            usuarioEncontradoIsAdmin = false;
            usuarioNomeEncontrado = string.Empty;
            txtNomeCompleto.Text = "Nome: -";
            txtNomeUsuario.Text = "Usuário: -";
            txtEmail.Text = "E-mail: -";
        }

        private void btnConfirmarExclusao_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioIdEncontrado == -1)
            {
                MessageBox.Show("Busque um usuário válido antes de realizar a exclusão.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (usuarioLogado == null)
            {
                MessageBox.Show("Operação inválida! Não foi possível identificar o usuário logado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (!usuarioLogado.IsAdmin)
            {
                MessageBox.Show("Acesso negado! Apenas administradores podem excluir usuários.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (usuarioIdEncontrado == usuarioLogado.Id)
            {
                MessageBox.Show("Você não pode excluir a sua própria conta enquanto estiver conectado ao sistema.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (usuarioIdEncontrado == 1)
            {
                MessageBox.Show("Acesso Negado! O Administrador Principal do sistema não pode ser excluído.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (usuarioLogado.Id > usuarioIdEncontrado)
            {
                MessageBox.Show("Acesso Negado! Você não tem permissão para excluir este administrador.", "Operação inválida!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            if (usuarioEncontradoIsAdmin)
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
                                MessageBox.Show("Não é possível excluir este usuário. O sistema deve possuir pelo menos um administrador cadastrado.", "Operação Não Permitida", MessageBoxButton.OK, MessageBoxImage.Stop);
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

            MessageBoxResult result = MessageBox.Show("Deseja realmente EXCLUIR este usuário permanentemente?", "Confirmar Exclusão", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var conexao = new MySqlConnection(connectionString))
                    {
                        conexao.Open();
                        string query = "DELETE FROM usuarios WHERE id = @id";

                        using (var cmd = new MySqlCommand(query, conexao))
                        {
                            cmd.Parameters.AddWithValue("@id", usuarioIdEncontrado);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    string nomeAdminLogado = usuarioLogado != null ? usuarioLogado.NomeUsuario : "SISTEMA";

                    RegistrosAuditoria.RegistrarAcao(
                        nomeAdminLogado,
                        "EXCLUSÃO",
                        $"O administrador '{nomeAdminLogado}' excluiu o usuário '{usuarioNomeEncontrado}' (ID: {usuarioIdEncontrado})."
                    );

                    MessageBox.Show("Usuário excluído com sucesso!", "Sucesso!", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimparCampos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir usuário: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}