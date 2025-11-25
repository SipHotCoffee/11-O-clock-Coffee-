using CG.Test.Editor.ViewModels;
using CG.Test.Editor.Views;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;

namespace CG.Test.Editor
{
    public partial class App : Application
    {
        private Window? _window;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var processFile = new FileInfo(Environment.ProcessPath!);

            var appSettingsNode = JsonNode.Parse(File.ReadAllText(Path.Combine(processFile.DirectoryName!, "appsettings.json")));
            var typesPath = appSettingsNode!["typesPath"]!.GetValue<string>();
            
            var viewModel = new MainViewModel()
            {
                TypesPath = typesPath,
            };

            _window = new MainWindow(viewModel);

            foreach (var argument in e.Args)
            {
                viewModel.FilesToOpen.Enqueue(new FileInfo(argument));
            }
            _window.Show();
        }
    }

}
