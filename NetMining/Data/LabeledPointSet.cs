using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMining.Files;
namespace NetMining.Data
{
    /// <summary>
    /// Holds both a PointSet and the LabelList associated with it
    /// </summary>
    public class LabeledPointSet
    {
        public PointSet Points;
        public LabelList Labels;

        public enum LabelLocation
        {
            LastColumn,
            FirstColumn
        }

        public LabeledPointSet(String filename, LabelLocation labelLocation)
        {
            DelimitedFile file = new DelimitedFile(filename);
            int labelIndex = 0;
            if (labelLocation == LabelLocation.LastColumn)
                labelIndex = file.Data[0].Length - 1;
            Labels = new LabelList(file, labelIndex);
            Points = new PointSet(file.RemoveColumns(labelIndex));
        }

        public LabeledPointSet(String filename, int labelIndex)
        {
            DelimitedFile file = new DelimitedFile(filename);
            Labels = new LabelList(file, labelIndex);
            Points = new PointSet(file.RemoveColumns(labelIndex));
        }
    }
}
