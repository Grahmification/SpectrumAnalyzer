using System;

namespace SpectrumAnalyzer.Models
{
    public class SelectableData<T>
    {
        public T Data { get; private set; }

        public bool Selected 
        {
            get { return _selected; }
            set { _selected = value; SelectionChanged?.Invoke(this, _selected); }
        }

        public event EventHandler<bool> SelectionChanged;

        private bool _selected = false;

        public SelectableData(T data)
        {
            Data = data;
        }
    }
}
