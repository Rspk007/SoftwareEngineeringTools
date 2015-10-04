using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareEngineeringTools.Documentation
{
    public class WikiGenerator : DocumentGenerator
    {
        private string path;
        private bool allowNewLine = false;
        private int lastlevel;
        private TextWriter writer;
        private bool referenceBeginned = false;
        private bool onetable = false;

        public WikiGenerator(string path)
        {
            this.path = path;
            this.writer = new StreamWriter(path);
            this.BeginDocument();
        }

        public WikiGenerator(TextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            this.writer = writer;
            this.BeginDocument();
        }

        public override void Dispose()
        {
            writer.Flush();
            this.EndDocument();
        }

        protected override void BeginDocument() { }
        protected override void EndDocument() 
        {
            this.writer.Dispose();
            writer = null;
        }
        public override void BeginSectionTitle(int level, string title, string label)
        {
            writer.WriteLine();            
            int currentlevel = level + 1;
            while (currentlevel > 0)
            {
                writer.Write("=");
                currentlevel--;
            }
            if (label != null && label != string.Empty)
            {
                String apos = "";
                writer.Write("<div id=" + apos + label + apos + ">");
            }
            writer.Write(" " + title);
            if (label != null && label != string.Empty)
            {
                writer.Write("</div>");
            }
            this.lastlevel = level;            
        }
        //It has automatic TOC generation
        public override void SetContent(bool enabled, int depth)
        {}

        public override void EndSectionTitle()
        {
            int currentlevel = this.lastlevel + 1;
            writer.Write(" ");
            while (currentlevel > 0)
            {
                writer.Write("=");
                currentlevel--;
            }
            this.lastlevel = -1;
            writer.WriteLine();
            writer.Flush();
        }
        public override void NewParagraph()
        {
            if (allowNewLine)
            {
                writer.WriteLine();                
            }
        }
        public override void PrintText(string text, bool insertSpace = true)
        {
            text = text.Replace("\r\n", string.Empty).Replace("\n", string.Empty);
            if(referenceBeginned == true)
            {
                writer.Write("| ");
                referenceBeginned = false;
            }
            writer.Write(text);            
        }
        public override void PrintVerbatimText(string text)
        {
            writer.Write("'''" + text + "'''");            
        }
        public override void BeginMarkup(DocumentMarkupKind markupKind)
        {
            switch (markupKind)
            {
                case DocumentMarkupKind.Bold:
                    writer.Write("'''");
                    break;
                case DocumentMarkupKind.ComputerOutput:
                    writer.Write("<pre>");
                    break;
                case DocumentMarkupKind.Emphasis:
                    writer.Write("''");
                    break;                

            }            
        }
        public override void EndMarkup(DocumentMarkupKind markupKind)
        {
            switch (markupKind)
            {
                case DocumentMarkupKind.Bold:
                    writer.Write("'''");
                    break;
                case DocumentMarkupKind.ComputerOutput:
                    writer.Write("</pre>");
                    break;
                case DocumentMarkupKind.Emphasis:
                    writer.Write("''");
                    break;

            }            
        }
        public override void NewLabel(string id)
        {
            String apos = "";
            writer.Write("<div id=" + apos + id + apos + "></div>");
        }
        public override void BeginReference(string id, bool url)
        {
            if(url)
            {
                writer.Write(id);
            }
            else
            {
                writer.Write("[[#" + id);
            }
            referenceBeginned = true;
        }
        public override void EndReference()
        {
            this.referenceBeginned = false;
            writer.Write("]]");
        }
        public override void BeginList() 
        {
            writer.WriteLine();
        }
        public override void EndList() 
        {
            writer.WriteLine();
        }
        public override void BeginListItem(int index, string title)
        {
            int currentIndex = index;
            while (currentIndex > 0)
            {
                writer.Write("*");
                currentIndex--;
            }
            writer.Write(title);            
        }
        public override void EndListItem(int index)
        {
            int currentIndex = index;
            while (currentIndex > 0)
            {
                writer.Write("*");
                currentIndex--;
            }            
        }
        public override void BeginTable(int rowCount, int colCount)
        {
            writer.WriteLine(writer.NewLine + "{|");
            if(rowCount == 2)
            {
                onetable = true;
            }
            allowNewLine = false;
        }
        public override void BeginTableRow(int rowIndex)
        {
            if(rowIndex > 0)
            {
                writer.WriteLine(writer.NewLine + "|-");
            }
        }
        public override void EndTableRow(int index)
        {
            if (onetable && index == 1)
            {
                writer.WriteLine();
                writer.WriteLine("|-");
                writer.Write("| || ");
                onetable = false;
            }            
        }
        public override void BeginTableCell(int rowIndex, int colIndex, bool head)
        {
            if(colIndex==0 && head == false)
            {
                writer.Write("| ");
            }
            else if(colIndex == 0)
            {
                writer.Write("! ");
            }
            else if (colIndex > 0 && head == false)
            {
                writer.Write("||");
            }
            else if (colIndex > 0)
            {
                writer.Write("!!");
            }
            
        }
        public override void EndTableCell(int rowIndex, int colIndex, bool head)
        {

        }
        public override void EndTable()
        {
            
            writer.WriteLine(writer.NewLine + "|}");
            allowNewLine = true;
        }
    }
}
