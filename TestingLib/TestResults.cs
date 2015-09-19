using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SoftwareEngineeringTools.Testing
{
    [DataContract(Namespace = "http://http://www.iit.bme.hu/sb/test/results")]
    public enum TestCommandKind
    {
        [EnumMember]
        None,
        [EnumMember]
        Set,
        [EnumMember]
        Call,
        [EnumMember]
        Check
    }

    [DataContract(Namespace = "http://http://www.iit.bme.hu/sb/test/results")]
    public enum TestResultKind
    {
        [EnumMember]
        None,
        [EnumMember]
        Passed,
        [EnumMember]
        Failed,
        [EnumMember]
        Error
    }

    [DataContract(Namespace = "http://http://www.iit.bme.hu/sb/test/results")]
    public class TestVariableResult
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }

        public TestVariableResult() : this(null, null)
        {
        }

        public TestVariableResult(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    [DataContract(Namespace = "http://http://www.iit.bme.hu/sb/test/results")]
    public class TestCommand
    {
        [DataMember]
        public TestCommandKind Kind { get; set; }
        [DataMember]
        public string Text { get; set; }

        public TestCommand() : this(TestCommandKind.None, null)
        {
        }

        public TestCommand(TestCommandKind kind, string text)
        {
            this.Kind = kind;
            this.Text = text;
        }
    }

    [DataContract(Namespace = "http://http://www.iit.bme.hu/sb/test/results")]
    public class TestCommandResult
    {
        [DataMember]
        public TestCommandKind Kind { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public string ExpectedValue { get; set; }
        [DataMember]
        public string ActualValue { get; set; }
        [DataMember]
        public TestResultKind Result { get; set; }
        [DataMember]
        public string Comment { get; set; }

        public TestCommandResult() : this(TestCommandKind.None, null, null, null, TestResultKind.None, null)
        {
        }

        public TestCommandResult(TestCommandKind kind, string text, string expectedValue, string actualValue, TestResultKind result, string comment)
        {
            this.Kind = kind;
            this.Text = text;
            this.ExpectedValue = expectedValue;
            this.ActualValue = actualValue;
            this.Result = result;
            this.Comment = comment;
        }
    }

    [DataContract(Namespace = "http://http://www.iit.bme.hu/sb/test/results")]
    [KnownType(typeof(TestVariableResult))]
    [KnownType(typeof(TestCommandResult))]
    public class TestCaseResult
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<TestVariableResult> Variables { get; private set; }
        [DataMember]
        public List<TestCommandResult> Commands { get; private set; }
        [DataMember]
        public TestResultKind Result { get; set; }
        [DataMember]
        public string Comment { get; set; }

        public TestCaseResult() : this(null, TestResultKind.None, null)
        {
        }

        public TestCaseResult(string name, TestResultKind result, string comment)
        {
            this.Name = name;
            this.Result = result;
            this.Comment = comment;
            this.Variables = new List<TestVariableResult>();
            this.Commands = new List<TestCommandResult>();
        }
    }

    [DataContract(Namespace = "http://http://www.iit.bme.hu/sb/test/results")]
    [KnownType(typeof(TestCommand))]
    [KnownType(typeof(TestCaseResult))]
    public class TestScenarioResult
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<TestCommand> Commands { get; private set; }
        [DataMember]
        public List<TestCaseResult> TestCases { get; private set; }

        public TestScenarioResult() : this(null)
        {
        }

        public TestScenarioResult(string name)
        {
            this.Name = name;
            this.Commands = new List<TestCommand>();
            this.TestCases = new List<TestCaseResult>();
        }
    }

    [DataContract(Namespace = "http://http://www.iit.bme.hu/sb/test/results")]
    [KnownType(typeof(TestScenarioResult))]
    public class TestResults
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<TestScenarioResult> TestScenarios { get; private set; }

        public TestResults() : this(null)
        {
        }

        public TestResults(string name)
        {
            this.Name = name;
            this.TestScenarios = new List<TestScenarioResult>();
        }
    }

    public static class TestResultIO
    {
        public static void SaveTestResults(TestResults testResults, string fileName)
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(TestResults));
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                writer.WriteStartDocument();
                dcs.WriteObject(writer, testResults);
            }
        }

        public static TestResults LoadTestResults(string fileName)
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(TestResults));
            using (XmlReader writer = XmlReader.Create(fileName))
            {
                return (TestResults)dcs.ReadObject(writer);
            }
        }
    }
}
