using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareEngineeringTools.WikiReader
{
    public class MediaWikiCompiler : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
    {
        public MediaWikiCompiler(string source)
        {
            AntlrInputStream inputStream = new AntlrInputStream(source);
            MediaWikiLexer lexer = new MediaWikiLexer(inputStream);
            lexer.AddErrorListener(this);
            CommonTokenStream stream = new CommonTokenStream(lexer);
            MediaWikiParser parser = new MediaWikiParser(stream);
            parser.AddErrorListener(this);
            this.ParseTree = parser.main();
        }

        public MediaWikiParser.MainContext ParseTree { get; set; }

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
        }

        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
        }
    }
}
