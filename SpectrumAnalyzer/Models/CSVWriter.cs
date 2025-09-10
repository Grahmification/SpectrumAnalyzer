using System.IO;

namespace SpectrumAnalyzer.Models
{
    public class CSVWriter
    {
        public StreamWriter Writer { get; private set; }
        public Char Delimiter { get; set; } = ',';

        public CSVWriter(string folder, string fileName)
        {
            Writer = new StreamWriter(Path.Combine(folder, fileName));
        }
        public CSVWriter(string filePath)
        {
            Writer = new StreamWriter(filePath);
        }

        public void Close()
        {
            Writer.Close();
        }
        public void WriteLine(string[] lineItems)
        {
            string line = String.Join(Delimiter, lineItems);

            Writer.WriteLine(line);
            Writer.Flush();
        }
        public void WriteLines(List<string[]> lines)
        {
            foreach (string[] line in lines)
            {
                WriteLine(line);
            }

        }
        public void WriteMetaData(string title, string value)
        {
            WriteLine([title, value]);
        }
        public void WriteDataStartLine()
        {
            WriteLine(["#### DATA STARTS HERE ####"]);
        }
    }
}
