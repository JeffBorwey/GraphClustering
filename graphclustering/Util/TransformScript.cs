using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Windows.Forms;
namespace GraphClustering
{
    /// <summary>
    /// This class is a provider for the scripting interface
    /// It will take a double[] of N dimensions and output a double[]
    /// of M dimensions
    /// </summary>
    class TransformScript
    {
        int dimCount;
        public bool compiledFine = true;
        Object transformObject;

        public TransformScript(String script, int dimensions)
        {
            dimCount = dimensions;

            //get transform object
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "GraphClustering.Transform.script";

            string result = "";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            result = result.Replace("<numDim>", dimensions.ToString());
            result = result.Replace("<transform>", script);

            CodeDomProvider prov = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters cp = new CompilerParameters();
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;
            cp.ReferencedAssemblies.Add("system.dll");
            CompilerResults cr = prov.CompileAssemblyFromSource(cp, result);
            if (cr.Errors.HasErrors)
            {
                compiledFine = false;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Compilation Error");
                foreach (CompilerError err in cr.Errors)
                {
                    sb.AppendLine(err.ErrorText);
                }
                MessageBox.Show(sb.ToString());
            }
            else
            {
                transformObject = cr.CompiledAssembly.CreateInstance("Transform");
            }
        }



        public double[] doTransform(double x, double y)
        {
            double[] tLoc = (double[])transformObject.GetType().InvokeMember("doTransform", BindingFlags.InvokeMethod, null, transformObject, new Object[] { x, y });
            return tLoc;
        }
    }
}
