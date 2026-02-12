using CG.Test.Editor.FrontEnd.Views;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace CG.Test.Editor.FrontEnd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = new MainWindow(e.Args.Select((fileName) => new FileInfo(fileName)));
            mainWindow.Show();
        }
    }
}
