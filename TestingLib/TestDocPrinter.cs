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

        public void PrintDocCmd(DocCmd cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            cmd.print(dg, sectionLevel);
        }        
    }
}
