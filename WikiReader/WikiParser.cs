
using SoftwareEngineeringTools.Documentation;
using System;
using System.IO;
using wem;
namespace SoftwareEngineeringTools.WikiReader
{
    public class WikiParser
    {
        static void Main(string[] args)
        {
            AutoItController aic = new AutoItController();
            aic.test();
            WikiParser wp = new WikiParser(@"..\..\..\ApiDoc.wiki");
            WikiGenerator wg = new WikiGenerator(@"..\..\..\ApiDocOut.wiki");
            DocPrinter dp = new DocPrinter(wg);
            dp.PrintDocCmd(wp.firstSection);
            wg.Dispose();
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
    }
}
