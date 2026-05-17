using NommusProject.Data;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NommusProject
{
    public class Receita : Transacao
    {
        public string FonteReceita { get; set; } = string.Empty;
        public bool ReceitaRecorrente { get; set; } = false;

        public Receita()
        {
            this.TipoTransacao = "Receita";
            this.FormaPagamento = "Depósito";
        }
    }
}