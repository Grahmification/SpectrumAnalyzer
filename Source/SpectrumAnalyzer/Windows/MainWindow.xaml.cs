using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SpectrumAnalyzer.ViewModels;

namespace SpectrumAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var vm = new MainVM();
            this.DataContext = vm;

            // Set keyboard bindings
            InputBindings.Add(new KeyBinding(vm.Project.NewProjectCommand, Key.N, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(vm.Project.OpenProjectCommand, Key.O, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(vm.Project.SaveProjectCommand, Key.S, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(vm.Project.SaveProjectAsCommand, Key.S, ModifierKeys.Control | ModifierKeys.Shift));
        }
    }
}
