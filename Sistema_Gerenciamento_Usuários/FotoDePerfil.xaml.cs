using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sistema_Gerenciamento_Usuários
{
    public partial class FotoDePerfil : Window
    {
        public string fotoSelecionada = "";

        public FotoDePerfil()
        {
            InitializeComponent();
        }

        private void Avatar_Click(object sender, MouseButtonEventArgs e)
        {
            var imagemClicada = sender as Image;

            if (imagemClicada?.Tag != null)
            {
                fotoSelecionada = imagemClicada.Tag.ToString() ?? "";

                this.DialogResult = true; 
                this.Close();
            }
        }
    }
}