using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SpectrumAnalyzer.ViewModels
{
    public class SelectedItemCollection<T> : ObservableCollection<T>
    {
        public void Reset(IEnumerable<T> items)
        {
            int oldCount = this.Count;

            this.Items.Clear();
            foreach (var item in items)
                this.Items.Add(item);

            if (!(oldCount == 0 && this.Count == 0))
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                if (this.Count != oldCount)
                    this.OnPropertyChanged(new PropertyChangedEventArgs("Count"));

                this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            }
        }
    }
}
