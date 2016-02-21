
using SoftwareEngineeringTools.Documentation;
using SoftwareEngineeringTools.Testing;
using System;
using System.IO;
namespace SoftwareEngineeringTools.WikiReader
{
    public class WikiParser
    {
        public static DocumentGenerator dg;

        [STAThreadAttribute()]
        static void Main(string[] args)
        {
            WikiParser wp = new WikiParser(@"..\..\..\ApiDoc.wiki");
            //WordGenerator wg = new WordGenerator(@"Test.doc");
            if(dg == null)
            {
                dg = new WordGenerator(@"Test.doc");
            }
            wp.firstSection.print(dg, 0);
            /*TestDocPrinter dp = new TestDocPrinter(dg);
            dp.PrintDocCmd(wp.firstSection);*/
            dg.Dispose();
        }
        string path;
        DocSect firstSection;
        public WikiParser(string path)
        {
            this.path = path;
            firstSection = new DocSect();
            ParseWiki();
        }
        public void ParseWiki()
        {
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string source = reader.ReadToEnd();
                    MediaWikiCompiler compiler = new MediaWikiCompiler(source);
                    MediaWikiProcessor processor = new MediaWikiProcessor(this);
                    processor.Visit(compiler.ParseTree);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void addPararaphToFirstSection(DocPara newParagraph)
        {
            this.firstSection.Paragraphs.Add(newParagraph);
        }
        /// <summary>
        /// Paragraphs will be first writen
        /// </summary>
        /// <param name="newSection"></param>
        public void addSectionToFirstSection(DocSect newSection)
        {
            this.firstSection.Sections.Add(newSection);
        }

        public void setDocumentGenerator(DocumentGenerator docg)
        {
            dg = docg;
        }
    }
}
