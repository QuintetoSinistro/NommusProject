using System;

namespace NommusProject
{
    public static class SessaoUsuario
    {
        public static Usuarios UsuarioLogado { get; set; }

        public static bool EstaLogado => UsuarioLogado != null;

        public static void Logout()
        {
            UsuarioLogado = null;
        }

        public static bool IsPremium()
        {
            return EstaLogado && UsuarioLogado.Tipo == TipoUsuario.Premium;
        }

        public static bool IsAdm()
        {
            return EstaLogado && UsuarioLogado.Tipo == TipoUsuario.Adm;
        }
    }
}