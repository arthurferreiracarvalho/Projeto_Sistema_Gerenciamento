using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace Sistema_Gerenciamento_Usuários
{
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
                                Id = reader.GetInt32("id"),
                                NomeUsuario = reader.GetString("nome_usuario"),
                                Acao = reader.GetString("acao"),
                                Descricao = reader.GetString("descricao"),
                                DataHora = reader.GetDateTime("data_hora")
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
            string termo = txtFiltro.Text.Trim().ToLower();

            var listaFiltrada = listaCompleta.Where(x =>
                string.IsNullOrEmpty(termo) ||
                (x.NomeUsuario != null && x.NomeUsuario.ToLower().Contains(termo)) ||
                (x.Acao != null && x.Acao.ToLower().Contains(termo)) ||
                (x.Descricao != null && x.Descricao.ToLower().Contains(termo))
            ).ToList();

            dgAuditoria.ItemsSource = listaFiltrada;
            lblTotalRegistros.Text = $"Total de Registros: {listaFiltrada.Count}";
        }

        private void btnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            AplicarFiltro();
        }

        private void txtFiltro_KeyUp(object sender, KeyEventArgs e)
        {
            AplicarFiltro();
        }

        private void btnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            txtFiltro.Clear();
            CarregarAuditoria();
        }
    }
}