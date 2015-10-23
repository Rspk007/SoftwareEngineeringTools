
using SoftwareEngineeringTools.Documentation;
using System;
using System.IO;
namespace SoftwareEngineeringTools.WikiReader
{
    public class WikiParser : Parser
    {
        static void Main(string[] args)
        { }
        string path;
        public WikiParser(string path)
        {
            this.Index = new DoxygenIndex();
            this.Model = new DoxygenModel();
            this.path = path;
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
    }
}
