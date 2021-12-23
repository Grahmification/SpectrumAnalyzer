using System;
using System.Collections.Generic;
using System.Text;

namespace SpectrumAnalyzer.Models
{
    public class DatasetBase
    {
        public SortedSet<Datapoint> Data { get; private set; } = new SortedSet<Datapoint>();

    }
}
