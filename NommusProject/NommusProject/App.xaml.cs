using System.Configuration;
using System.Data;
using System.Windows;

namespace NommusProject;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Data.DatabaseInitializer.Initialize();
    }
}