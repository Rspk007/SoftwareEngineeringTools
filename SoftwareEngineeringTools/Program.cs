using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftwareEngineeringTools.Documentation;
using SoftwareEngineeringTools.Testing;

namespace SoftwareEngineeringTools
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                
                JavaDocParser jdp = new JavaDocParser(@"..\..\javadoc\html\overview-summary.html");
                //DoxygenParser dp = new DoxygenParser(@"..\..\doxygen\xml\index.xml");
                //NetParser np = new NetParser(@"..\..\NET");
                //using (WordGenerator g = new WordGenerator(@"ApiDoc.docx", true, WordGenerator.WordTemplate.Elegant))
                //{
                //    //Generator_Test(g);
                //    DocumentTemplate t = DocumentTemplate.GroupByKindTemplate;
                //    //ApiDocGenerator adgW = new ApiDocGenerator(g, t, np.Model);
                //    if (t == DocumentTemplate.GroupByKindTemplate)
                //    {
                //        g.SetContent(true, 2);
                //    }
                //    else
                //    {
                //        g.SetContent(true, 3);
                //    }
                //    DocPrinter docp = new DocPrinter(g, dp.Model);
                //    docp.print();
                //    //adgW.Generate();
                //}

                using (HTMLGenerator g = new HTMLGenerator(@"..\..\", HTMLGenerator.GenerateMode.AllInOne, "Java"))
                {
                    DocumentTemplate t = DocumentTemplate.GroupByNamespaceTemplate;
                    DocPrinter docp = new DocPrinter(g, jdp.Model);
                    docp.print();

                    //ApiDocGenerator adgW = new ApiDocGenerator(g, t, dp.Model);

                    //adgW.Generate();
                }

                //using (LatexGenerator g = new LatexGenerator(@"..\..\ApiDoc.tex"))
                //{
                //    DocumentTemplate t = DocumentTemplate.GroupByKindTemplate;
                //    ApiDocGenerator adgW = new ApiDocGenerator(g, t, np.Model);
                //    if (t == DocumentTemplate.GroupByKindTemplate)
                //    {
                //        g.SetContent(true, 1);
                //    }
                //    else
                //    {
                //        g.SetContent(true, 2);
                //    }
                //    adgW.Generate();
                //}

                /*TestResults trg = Program.GenerateTestResults();
                TestResultIO.SaveTestResults(trg, @"..\..\testing\TestScenario_Results.xml");
                TestResults tr = TestResultIO.LoadTestResults(@"..\..\testing\TestScenario_Results.xml");
                using (WordGenerator g = new WordGenerator(@"TestReportDoc.docx", true))
                {
                    TestReportDocGenerator trdg = new TestReportDocGenerator(g, tr);
                    trdg.Generate();
                }
                using (LatexGenerator g = new LatexGenerator(@"..\..\TestReportDoc.tex"))
                {
                    TestReportDocGenerator trdg = new TestReportDocGenerator(g, tr);
                    trdg.Generate();
                }*/
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                
            }
            Console.ReadKey();
        }

        public static TestResults GenerateTestResults()
        {
            TestCaseResult tcr1 = new TestCaseResult("Successful login", TestResultKind.Passed, "");
            tcr1.Variables.Add(new TestVariableResult("url", "http://localhost/Movies/"));
            tcr1.Variables.Add(new TestVariableResult("loginScreen", "TestScenario_Results/LoginTest/Successful login/loginScreen.png"));
            tcr1.Variables.Add(new TestVariableResult("username", "xyz"));
            tcr1.Variables.Add(new TestVariableResult("password", "uvw"));
            tcr1.Variables.Add(new TestVariableResult("exception", ""));
            tcr1.Variables.Add(new TestVariableResult("success", "true"));
            tcr1.Variables.Add(new TestVariableResult("mainScreen", "TestScenario_Results/LoginTest/Successful login/mainScreen.png"));
            tcr1.Commands.Add(new TestCommandResult(TestCommandKind.Set, @"url=""http://localhost/Movies/""", "", "", TestResultKind.Passed, ""));
            tcr1.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"TestHelper.OpenWebPage(url)", "", "", TestResultKind.Passed, ""));
            tcr1.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"loginScreen=TestHelper.TakeScreenShot()", "", "", TestResultKind.Passed, ""));
            tcr1.Commands.Add(new TestCommandResult(TestCommandKind.Set, @"username", "xyz", "xyz", TestResultKind.Passed, ""));
            tcr1.Commands.Add(new TestCommandResult(TestCommandKind.Set, @"password", "uvw", "uvw", TestResultKind.Passed, ""));
            tcr1.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"success=TestHelper.Login(username,password) HANDLE exception", "", "", TestResultKind.Passed, ""));
            tcr1.Commands.Add(new TestCommandResult(TestCommandKind.Check, @"exception", "", "", TestResultKind.Passed, ""));
            tcr1.Commands.Add(new TestCommandResult(TestCommandKind.Check, @"success", "true", "true", TestResultKind.Passed, ""));
            tcr1.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"mainScreen=TestHelper.TakeScreenShot()", "", "", TestResultKind.Passed, ""));
            TestCaseResult tcr2 = new TestCaseResult("Wrong username or password", TestResultKind.Error, "No exception expected but NullReferenceException was thrown.");
            tcr2.Variables.Add(new TestVariableResult("url", "http://localhost/Movies/"));
            tcr2.Variables.Add(new TestVariableResult("loginScreen", "TestScenario_Results/LoginTest/Wrong username or password/loginScreen.png"));
            tcr2.Variables.Add(new TestVariableResult("username", "abc"));
            tcr2.Variables.Add(new TestVariableResult("password", "def"));
            tcr2.Variables.Add(new TestVariableResult("exception", "NullReferenceException"));
            tcr2.Variables.Add(new TestVariableResult("success", ""));
            tcr2.Variables.Add(new TestVariableResult("mainScreen", "TestScenario_Results/LoginTest/Wrong username or password/mainScreen.png"));
            tcr2.Commands.Add(new TestCommandResult(TestCommandKind.Set, @"url=""http://localhost/Movies/""", "", "", TestResultKind.Passed, ""));
            tcr2.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"TestHelper.OpenWebPage(url)", "", "", TestResultKind.Passed, ""));
            tcr2.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"loginScreen=TestHelper.TakeScreenShot()", "", "", TestResultKind.Passed, ""));
            tcr2.Commands.Add(new TestCommandResult(TestCommandKind.Set, @"username", "abc", "abc", TestResultKind.Passed, ""));
            tcr2.Commands.Add(new TestCommandResult(TestCommandKind.Set, @"password", "def", "def", TestResultKind.Passed, ""));
            tcr2.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"success=TestHelper.Login(username,password) HANDLE exception", "", "", TestResultKind.Passed, ""));
            tcr2.Commands.Add(new TestCommandResult(TestCommandKind.Check, @"exception", "", "NullReferenceException", TestResultKind.Error, "No exception expected but NullReferenceException was thrown."));
            tcr2.Commands.Add(new TestCommandResult(TestCommandKind.Check, @"success", "false", "", TestResultKind.None, ""));
            tcr2.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"mainScreen=TestHelper.TakeScreenShot()", "", "", TestResultKind.None, ""));
            TestCaseResult tcr3 = new TestCaseResult("Username required", TestResultKind.Failed, "UserNameRequiredException expected but NullReferenceException was thrown.");
            tcr3.Variables.Add(new TestVariableResult("url", "http://localhost/Movies/"));
            tcr3.Variables.Add(new TestVariableResult("loginScreen", "TestScenario_Results/LoginTest/Username required/loginScreen.png"));
            tcr3.Variables.Add(new TestVariableResult("username", ""));
            tcr3.Variables.Add(new TestVariableResult("password", "aaa"));
            tcr3.Variables.Add(new TestVariableResult("exception", "NullReferenceException"));
            tcr3.Variables.Add(new TestVariableResult("success", ""));
            tcr3.Variables.Add(new TestVariableResult("mainScreen", "TestScenario_Results/LoginTest/Username required/mainScreen.png"));
            tcr3.Commands.Add(new TestCommandResult(TestCommandKind.Set, @"url=""http://localhost/Movies/""", "", "", TestResultKind.Passed, ""));
            tcr3.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"TestHelper.OpenWebPage(url)", "", "", TestResultKind.Passed, ""));
            tcr3.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"loginScreen=TestHelper.TakeScreenShot()", "", "", TestResultKind.Passed, ""));
            tcr3.Commands.Add(new TestCommandResult(TestCommandKind.Set, @"username", "", "", TestResultKind.Passed, ""));
            tcr3.Commands.Add(new TestCommandResult(TestCommandKind.Set, @"password", "aaa", "aaa", TestResultKind.Passed, ""));
            tcr3.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"success=TestHelper.Login(username,password) HANDLE exception", "", "", TestResultKind.Passed, ""));
            tcr3.Commands.Add(new TestCommandResult(TestCommandKind.Check, @"exception", "UserNameRequiredException", "NullReferenceException", TestResultKind.Error, "UserNameRequiredException expected but NullReferenceException was thrown."));
            tcr3.Commands.Add(new TestCommandResult(TestCommandKind.Check, @"success", "false", "", TestResultKind.None, ""));
            tcr3.Commands.Add(new TestCommandResult(TestCommandKind.Call, @"mainScreen=TestHelper.TakeScreenShot()", "", "", TestResultKind.None, ""));

            TestScenarioResult tsr1 = new TestScenarioResult("LoginTest");
            tsr1.TestCases.Add(tcr1);
            tsr1.TestCases.Add(tcr2);
            tsr1.TestCases.Add(tcr3);
            tsr1.Commands.Add(new TestCommand(TestCommandKind.Set, @"url=""http://localhost/Movies/"""));
            tsr1.Commands.Add(new TestCommand(TestCommandKind.Call, @"TestHelper.OpenWebPage(url)"));
            tsr1.Commands.Add(new TestCommand(TestCommandKind.Call, @"loginScreen=TestHelper.TakeScreenShot()"));
            tsr1.Commands.Add(new TestCommand(TestCommandKind.Set, @"username"));
            tsr1.Commands.Add(new TestCommand(TestCommandKind.Set, @"password"));
            tsr1.Commands.Add(new TestCommand(TestCommandKind.Call, @"success=TestHelper.Login(username,password) HANDLE exception"));
            tsr1.Commands.Add(new TestCommand(TestCommandKind.Check, @"exception"));
            tsr1.Commands.Add(new TestCommand(TestCommandKind.Check, @"success"));
            tsr1.Commands.Add(new TestCommand(TestCommandKind.Call, @"mainScreen=TestHelper.TakeScreenShot()"));

            TestScenarioResult tsr2 = new TestScenarioResult("InsertMovieTest");
            TestScenarioResult tsr3 = new TestScenarioResult("DeleteMovieTest");

            TestResults results = new TestResults();
            results.Name = "TestScenario";
            results.TestScenarios.Add(tsr1);
            results.TestScenarios.Add(tsr2);
            results.TestScenarios.Add(tsr3);

            return results;
        }

        //! Old Tester method for WordGenerator.
        /*!
          It use all method of the WordGenerator, so it can be tested by this method.
        */
        //public static void Generator_Test(DocumentGenerator g)
        //{
        //    DocumentGenerator dg = g;
        //    dg.PrintText("Begin Text");
        //    dg.NewParagraph();
        //    for (int i = 0; i < 4; i++)
        //    {
        //        dg.BeginSectionTitle(i, "Section Title "+i, null);
        //        dg.EndSectionTitle();
        //    }
        //    dg.PrintVerbatimText("Verbatim Text");
        //    dg.NewLabel("Test");
        //    dg.PrintText("New Label");
        //    dg.BeginMarkup(DocumentMarkupKind.Bold);
        //    dg.PrintText("Bold Markup");
        //    dg.EndMarkup(DocumentMarkupKind.Bold);
        //    dg.NewParagraph();
        //    dg.BeginMarkup(DocumentMarkupKind.Emphasis);
        //    dg.PrintText("Emphasis Markup");
        //    dg.EndMarkup(DocumentMarkupKind.Emphasis);
        //    dg.NewParagraph();
        //    dg.BeginMarkup(DocumentMarkupKind.SubScript);
        //    dg.PrintText("SubScript Markup");
        //    dg.EndMarkup(DocumentMarkupKind.SubScript);
        //    dg.NewParagraph();
        //    dg.BeginMarkup(DocumentMarkupKind.SuperScript);
        //    dg.PrintText("SuperScript Markup");
        //    dg.EndMarkup(DocumentMarkupKind.SuperScript);
        //    dg.NewParagraph();
        //    dg.BeginMarkup(DocumentMarkupKind.Center);
        //    dg.PrintText("Center Markup");
        //    dg.EndMarkup(DocumentMarkupKind.Center);
        //    dg.NewParagraph();
        //    dg.BeginMarkup(DocumentMarkupKind.ComputerOutput);
        //    dg.PrintText("ComputerOutput Markup");
        //    dg.EndMarkup(DocumentMarkupKind.ComputerOutput);
        //    dg.NewParagraph();
        //    dg.BeginMarkup(DocumentMarkupKind.Small);
        //    dg.PrintText("Small Markup");
        //    dg.EndMarkup(DocumentMarkupKind.Small);
        //    dg.NewParagraph();
        //    dg.BeginReference("Test",false);
        //    dg.NewParagraph();
        //    dg.PrintText("Reference tester");
        //    dg.EndReference();
        //    dg.NewParagraph();
        //    dg.BeginList();
        //    dg.BeginListItem(0, "First element");
        //    dg.EndListItem(0);
        //    dg.BeginList();
        //    dg.BeginListItem(0, "Secound list, first element");
        //    dg.EndListItem(0);
        //    dg.EndList();
        //    dg.BeginListItem(1, "Secound element");
        //    dg.EndListItem(1);
        //    dg.BeginListItem(2, "Index.hu");
        //    dg.EndListItem(2);
        //    dg.EndList();
        //    dg.BeginReference("http://www.index.hu", true);
        //    dg.PrintText("Index");
        //    dg.EndReference();
        //    dg.BeginTable(3, 3);
        //    dg.BeginTableCell(1, 3, false);
        //    dg.PrintText("Table with 2 row and 2 col.");
        //    dg.NewParagraph();
        //    dg.PrintText("Verbatim Text in the table cell");
        //    dg.EndTableCell(1, 3, false);
        //    dg.BeginTableCell(2, 2, true);
        //    dg.PrintText("Head test");
        //    dg.EndTableCell(2, 2, true);
        //    dg.EndTable();
        //    dg.NewParagraph();
        //    dg.PrintText("End of the doc");
        //    dg.NewParagraph();
        //}

    }
}
