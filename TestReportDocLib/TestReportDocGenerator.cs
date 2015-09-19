using SoftwareEngineeringTools.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareEngineeringTools.Documentation
{
    public class TestReportDocGenerator
    {
        private IDocumentGenerator dg;
        private TestResults testResults;

        public TestReportDocGenerator(IDocumentGenerator documentGenerator, TestResults testResults)
        {
            this.dg = documentGenerator;
            this.testResults = testResults;
        }

        public void Generate()
        {
            this.dg.BeginSectionTitle(0, "Test results", null);
            this.dg.EndSectionTitle();
        }
    }
}
