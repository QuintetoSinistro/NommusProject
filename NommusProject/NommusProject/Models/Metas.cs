using NommusProject.Data;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NommusProject
{
    public class Metas
    {
        public int IdMeta { get; set; }
        public string NomeMeta { get; set; }
        public double ValorMeta { get; set; }
        public DateTime DataInicial { get; set; }
        public DateTime DataFinal { get; set; }
        public bool StatusMeta { get; set; }
        public int IdUsuario { get; set; }
        public double ValorAtual { get; set; }
    }
}