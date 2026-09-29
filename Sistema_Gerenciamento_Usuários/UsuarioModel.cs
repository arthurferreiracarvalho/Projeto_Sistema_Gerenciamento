using System;

namespace Sistema_Gerenciamento_Usuários
{
    public class UsuarioModel
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime DataUltimaAlteracao { get; set; }
        public DateTime? UltimoLogin { get; set; }
    }
}