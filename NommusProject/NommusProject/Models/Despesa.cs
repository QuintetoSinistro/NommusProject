using NommusProject.Data;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NommusProject
{
    public class Despesa : Transacao
    {
        public bool DespesaEssencial { get; set; } = true;
        public bool DespesaRecorrente { get; set; } = false;

        public Despesa()
        {
            this.TipoTransacao = "Despesa";
            this.FormaPagamento = "Dinheiro";
        }
    }
}