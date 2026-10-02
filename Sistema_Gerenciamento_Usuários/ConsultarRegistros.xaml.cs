using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace Sistema_Gerenciamento_Usuários
{
    public static class RegistrosAuditoria
    {
        private static string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";

        public static void RegistrarAcao(string nomeUsuario, string acao, string descricao)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomeUsuario))
                {
                    nomeUsuario = "SISTEMA";
                }

                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();

                    string query = "INSERT INTO auditoria (nome_usuario, acao, descricao, data_hora) " +
                                   "VALUES (@usuario, @acao, @descricao, NOW())";

                    using (MySqlCommand comando = new MySqlCommand(query, conexao))
                    {
                        comando.Parameters.AddWithValue("@usuario", nomeUsuario);
                        comando.Parameters.AddWithValue("@acao", acao);
                        comando.Parameters.AddWithValue("@descricao", descricao);

                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"[ERRO AUDITORIA] Falha ao registrar log no banco: {ex.Message}",
                                "Erro de Auditoria", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class RegistroAuditoriaModel
    {
        public int Id { get; set; }
        public string NomeUsuario { get; set; }
        public string Acao { get; set; }
        public string Descricao { get; set; }
        public DateTime DataHora { get; set; }
    }


    public partial class ConsultarRegistros : Window
    {
        private string connectionString = "Server=localhost;Database=login;Uid=root;Pwd=;";
        private List<RegistroAuditoriaModel> listaCompleta = new List<RegistroAuditoriaModel>();

        public ConsultarRegistros()
        {
            InitializeComponent();
            CarregarAuditoria();
        }

        private void CarregarAuditoria()
        {
            listaCompleta.Clear();

            try
            {
                using (MySqlConnection conexao = new MySqlConnection(connectionString))
                {
                    conexao.Open();
                    string query = "SELECT id, nome_usuario, acao, descricao, data_hora FROM auditoria ORDER BY data_hora DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conexao))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listaCompleta.Add(new RegistroAuditoriaModel
                            {
                                Id = reader.IsDBNull(reader.GetOrdinal("id")) ? 0 : reader.GetInt32("id"),
                                NomeUsuario = reader.IsDBNull(reader.GetOrdinal("nome_usuario")) ? "SISTEMA" : reader.GetString("nome_usuario"),
                                Acao = reader.IsDBNull(reader.GetOrdinal("acao")) ? "" : reader.GetString("acao"),
                                Descricao = reader.IsDBNull(reader.GetOrdinal("descricao")) ? "" : reader.GetString("descricao"),
                                DataHora = reader.IsDBNull(reader.GetOrdinal("data_hora")) ? DateTime.Now : reader.GetDateTime("data_hora")
                            });
                        }
                    }
                }

                AplicarFiltro();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar os registros de auditoria: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltro()
        {
            string termo = txtFiltro != null ? txtFiltro.Text.Trim().ToLower() : string.Empty;

            var listaFiltrada = listaCompleta.Where(x =>
                string.IsNullOrEmpty(termo) ||
                (x.NomeUsuario != null && x.NomeUsuario.ToLower().Contains(termo)) ||
                (x.Acao != null && x.Acao.ToLower().Contains(termo)) ||
                (x.Descricao != null && x.Descricao.ToLower().Contains(termo))
            ).ToList();

            if (dgAuditoria != null)
            {
                dgAuditoria.ItemsSource = null;
                dgAuditoria.ItemsSource = listaFiltrada;
            }

            if (lblTotalRegistros != null)
            {
                lblTotalRegistros.Text = $"Total de Registros: {listaFiltrada.Count}";
            }
        }

        private void btnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            AplicarFiltro();
        }

        private void btnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            if (txtFiltro != null)
                txtFiltro.Clear();

            CarregarAuditoria();
        }

        private void txtFiltro_KeyUp(object sender, KeyEventArgs e)
        {
            AplicarFiltro();
        }
    }
}