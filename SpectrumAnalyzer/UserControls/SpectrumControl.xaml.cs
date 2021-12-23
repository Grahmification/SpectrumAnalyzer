using SpectrumAnalyzer.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SpectrumAnalyzer.Extensions;

namespace SpectrumAnalyzer.UserControls
{
    /// <summary>
    /// Interaction logic for SpectrumControl.xaml
    /// </summary>
    public partial class SpectrumControl : UserControl
    {
        public SpectrumControl()
        {
            InitializeComponent();

            //bind the datagrid - throws an error if done in xaml
            //Binding myBinding = new Binding("SelectedComponents");
            //myBinding.Source = this.DataContext;
            //BindingOperations.SetBinding(dg, MultiSelectorExtension.SelectedItemsProperty, myBinding);
        }

        private void OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
        
}
