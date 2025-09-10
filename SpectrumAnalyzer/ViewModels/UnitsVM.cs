using SpectrumAnalyzer.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SpectrumAnalyzer.ViewModels
{
    public class UnitsVM : ObservableObject
    {
        public ObservableCollection<XAxisUnits> XUnits { get; set; } = new ObservableCollection<XAxisUnits>(XAxisUnits.DefaultUnits());
        public XAxisUnits SelectedXUnit { get; set; } = XAxisUnits.DefaultUnits()[0];

        public string SelectedXUnitString => SelectedXUnit.TimeUnit;
        public string DataTitle { get; set; } = "";
        public string YAxisTitle { get; set; } = "Y Data";
        public string XAxisTitle => SelectedXUnit.TimeString;

        public event EventHandler? OnUnitsUpdate;


        /// <summary>
        /// RelayCommand for <see cref="UpdateUnits"/>
        /// </summary>
        public ICommand UpdateUnitsCommand { get; private set; }

        public UnitsVM()
        {
            UpdateUnitsCommand = new RelayCommand<object>(UpdateUnits);
            SelectedXUnit = XUnits[0];
        }
        public void UpdateUnits(object? parameter)
        {
            OnUnitsUpdate?.Invoke(this, new EventArgs());
        }
    }
}
