using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftwareEngineeringTools.Documentation;

namespace SoftwareEngineeringTools.Testing
{
    public class TestDocPrinter
    {
        private IDocumentGenerator dg;
        int sectionLevel = 0;

        public TestDocPrinter(IDocumentGenerator generator)
        {
            this.dg = generator;
            this.sectionLevel = 0;
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        private string NormalizeName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            int index = name.LastIndexOf(":");
            if (index > 0)
            {
                return name.Substring(index + 1);
            }
            else if (name.LastIndexOf('.') > 0)
            {
                return name.Substring(name.LastIndexOf('.') + 1);
            }
            else
            {
                return name;
            }
            //return name.Replace("::", ".");

        }

        public void PrintDocCmd(DocCmd cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            cmd.print(dg, 0);
        }        
    }
}
