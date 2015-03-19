using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetMining.Files
{
    public class DelimitedFile
    {
        public List<String[]> Data;
        private static readonly char[] SplitChars = { '\t', ',' };
        private static readonly char[] SplitCharsSpace = { '\t', ',', ' ' };
        public DelimitedFile(String filename, bool allowEmpty = false, bool allowSpaceSplit = true)
        {
            Data = new List<string[]>();

            var splitOptions = (allowEmpty)
                ? StringSplitOptions.None
                : StringSplitOptions.RemoveEmptyEntries;

            char[] delimiters = (allowSpaceSplit) ? SplitCharsSpace : SplitChars;

            using (StreamReader sr = new StreamReader(filename))
            {
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine() ?? ""; //if the line is null we must set it to something
                    String[] splitStrings = line.Split(delimiters, splitOptions);
                    if (splitStrings.Length > 0)
                        Data.Add(splitStrings);
                }
            }

        }

        public DelimitedFile RemoveColumns(List<int> removeIndex)
        {
            List<String[]> data = Data.Select(strings => strings.Where((t, i) => !removeIndex.Contains(i)).ToArray()).ToList();
            return new DelimitedFile(data);
        }

        private DelimitedFile(List<String[]> data)
        {
            Data = data;
        }

        public String[] GetColumn(int i)
        {
            String[] cols = Data.Select(x => x[i]).ToArray();
            return cols;
        }

    }
}
