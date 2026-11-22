using System.Windows;
using PyroSnap.Views;

namespace PyroSnap;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // DS-06: pasang tema tersimpan sebelum jendela utama dibuat (StartupUri diproses setelah ini).
    protected override void OnStartup(StartupEventArgs e)
    {
        Tema.Terapkan(Tema.BacaPilihan());
        base.OnStartup(e);
    }
}
