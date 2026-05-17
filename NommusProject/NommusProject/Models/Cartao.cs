using NommusProject.Data;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NommusProject
{
    public class Cartao
    {
        public int IdCartao { get; set; }
        public string NomeCartao { get; set; }
        public double LimiteCartao { get; set; }
        public DateTime DataVencimento { get; set; }
        public string BandeiraCartao { get; set; }
        public int IdUsuario { get; set; }
        public string NumeroCartao { get; set; }
    }
}