using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;

namespace NommusProject.Utils
{
    public static class MaskHelper
    {
        public static void AplicarMascaraValor(TextBox textBox)
        {
            // Permite apenas números e uma vírgula
            textBox.PreviewTextInput += (s, e) =>
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9,]$"))
                    e.Handled = true;
                if (e.Text == "," && textBox.Text.Contains(","))
                    e.Handled = true;
            };

            // Formata ao perder o foco
            textBox.LostFocus += (s, e) =>
            {
                if (double.TryParse(textBox.Text, out double valor))
                    textBox.Text = valor.ToString("N2");
                else if (string.IsNullOrWhiteSpace(textBox.Text))
                    textBox.Text = "0,00";
            };

            // Limpa ao ganhar foco se for "0,00"
            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == "0,00")
                    textBox.Text = "";
            };
        }
    }
}