﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SoftwareEngineeringTools.Documentation
{
    public class WikiGenerator : DocumentGenerator
    {
        private int lastlevel;
        private TextWriter writer;
        private bool referenceBeginned = false;
        private bool onetable = false;
        private bool url = false;

        public WikiGenerator(string path)
        {
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

        public static string normalize(string input)
        {
            return HttpUtility.HtmlEncode(input).Replace("]", "&rsqb;").Replace("[", "&lsqb;");
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
            if (!String.IsNullOrEmpty(label))
            {
                String apos = "\"";
                writer.Write(" <div id=" + apos + label + apos + ">");
            }
            writer.Write(" " + normalize(title));
            if (!String.IsNullOrEmpty(label))
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
            writer.WriteLine();  
        }
        public override void PrintText(string text, bool insertSpace = true)
        {
            //text = text.Replace("\r\n", string.Empty).Replace("\n", string.Empty);
            if(referenceBeginned == true)
            {
                writer.Write("| ");
                referenceBeginned = false;
            }
            writer.Write(normalize(text));            
        }
        public override void PrintVerbatimText(string text)
        {
            writer.Write("'''" + normalize(text) + "'''");            
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
                case DocumentMarkupKind.Center:
                    writer.Write("<center>");
                    break;
                case DocumentMarkupKind.Success:
                    writer.Write(" <span style=\"background: #85FA99\">");
                    break;
                case DocumentMarkupKind.Fail:
                    writer.Write(" <span style=\"background: #FA8589\">");
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
                case DocumentMarkupKind.Center:
                    writer.Write("</center>");
                    break;
                case DocumentMarkupKind.Success:
                case DocumentMarkupKind.Fail:
                    writer.Write(" </span>");
                    break;

            }            
        }
        public override void NewLabel(string id)
        {
            String apos = "\"";
            writer.Write("<div id=" + apos + id + apos + "></div>");
        }
        public override void BeginReference(string id, bool is_url)
        {
            this.url = is_url;
            if(url)
            {
                writer.Write("["+id+" ");
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
            if (url)
            {
                writer.Write(" ]");
            }
            else
            {
                writer.Write("]]");
            }
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
            writer.Write("*");
            if (title != null)
            {
                writer.Write(normalize(title));
            }
        }
        public override void EndListItem(int index)
        {
       
        }
        public override void BeginTable(int rowCount, int colCount)
        {
            writer.WriteLine(writer.NewLine + "{|");
            if(rowCount == 2)
            {
                onetable = true;
            }
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
        }

        public override void AddImage(string image_path, string Width = null, string Height = null)
        {
            writer.Write("[[File:" + image_path);
            if(Width != null)
            {
                writer.Write("|"+Int32.Parse(Width)+"px");
            }
            writer.WriteLine("]]");
        }
    }
}
