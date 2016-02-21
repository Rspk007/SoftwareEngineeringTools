using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.IO;

namespace SoftwareEngineeringTools.Documentation
{
    public class HTMLGenerator : DocumentGenerator
    {
        private HtmlTextWriter indexWriter;
        private HtmlTextWriter pageWriter;
        private TextWriter textWriter = null;
        private TextWriter pageTextWriter = null;

        private String generatePath;
        private String currentPath;
        private String CSSFileName;
        private GenerateMode mode;

        public enum GenerateMode
        {
            AllInOne,
            UseDocumentTemplate
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <param name="CSSName"></param>
        public HTMLGenerator(string path, GenerateMode mode, string CSSName) 
        {
            this.textWriter = new StreamWriter(path + @"\index.html");
            generatePath = path;
            this.mode = mode;
            this.CSSFileName = CSSName;
            this.indexWriter = new HtmlTextWriter(textWriter);
            this.BeginDocument();
        }

        public HTMLGenerator(TextWriter inputWriter, GenerateMode mode, string CSSName)
        {
            if (indexWriter == null) throw new ArgumentNullException("inputWriter","Is null.");
            this.textWriter = inputWriter;
            this.mode = mode;
            this.CSSFileName = CSSName;
            this.indexWriter = new HtmlTextWriter(textWriter);
            this.BeginDocument();
        }

        public override void SetContent(bool enabled, int depth)
        {
        }

        public override void Dispose()
        {
            this.EndDocument();
        }

        protected override void BeginDocument()
        {
            //Make a index html file
            GenerateBegin(indexWriter, "Index");
            if (mode == GenerateMode.AllInOne)
            {
                this.pageTextWriter = textWriter;
                this.pageWriter = indexWriter; 
            }
        }

        //Generate the begining of every html file, inculde the title and the content type
        protected void GenerateBegin(HtmlTextWriter htw, String title)
        {
            htw.Write("<!DOCTYPE HTML>" + Environment.NewLine);
            htw.RenderBeginTag(HtmlTextWriterTag.Html);
            htw.RenderBeginTag(HtmlTextWriterTag.Head);
            htw.RenderBeginTag(HtmlTextWriterTag.Title);
            htw.WriteEncodedText(title);
            htw.RenderEndTag();
            htw.Write("<meta http-equiv=\"Content-type\" content=\"text/html;charset=iso-8859-2\">" + Environment.NewLine);
            htw.RenderEndTag();
            htw.Write("<link rel=\"stylesheet\" type=\"text/css\" href=\"css\\"+ this.CSSFileName + ".css\">");
            htw.RenderBeginTag(HtmlTextWriterTag.Body);
        }

        //!Close the index file
        protected override void EndDocument()
        {
            pageTextWriter.Flush();
            textWriter.Flush();
            indexWriter.RenderEndTag();
            indexWriter.RenderEndTag();
            if (this.mode == GenerateMode.UseDocumentTemplate)
            {
                pageWriter.RenderEndTag();
                pageWriter.RenderEndTag();
                pageWriter.Flush();
            }
            indexWriter.Flush();
            this.pageTextWriter.Dispose();
            this.indexWriter.Dispose();
            this.pageWriter.Dispose();
            this.textWriter.Dispose();
            indexWriter = null;
            textWriter = null;
            pageTextWriter = null;
            pageWriter = null;
        }

        protected void GenerateEnd()
        {
            this.pageWriter.RenderEndTag();
            this.pageWriter.RenderEndTag();
            this.pageWriter.Dispose();
            this.pageTextWriter.Dispose();
            this.pageWriter = null;
            this.pageTextWriter = null;
        }

        public override void BeginSectionTitle(int level, string title, string label)
        {
           
            if (level < 4)
            {
             
                if (level == 0)
                {
                    //Ha nem mindent egy állományba generálunk
                    if (this.mode != GenerateMode.AllInOne)
                    {                    
                        //Az oldal lezárása, mivel új oldalt kell majd nyitni
                        if (this.pageTextWriter != null && this.pageWriter != null)
                        {
                            GenerateEnd();
                        }                    
                        //Amennyiben a DocumentTemplate mentén osztjuk fel az oldalakat, akkor a 0. szintű
                        //címeknél kell új oldalt generálni
                        if (this.mode == GenerateMode.UseDocumentTemplate)
                        {
                            this.pageTextWriter = new StreamWriter(generatePath + @"\" + title.Replace(":",string.Empty) + ".html");
                            this.currentPath = title.Replace(":", string.Empty) + ".html";
                            this.pageWriter = new HtmlTextWriter(pageTextWriter);
                            GenerateBegin(pageWriter, title);
                        }
                        indexWriter.RenderBeginTag(HtmlTextWriterTag.H1);
                        indexWriter.WriteEncodedText(title);
                        indexWriter.RenderEndTag();
                    }                   
                    
                    
                }
                else if (level == 1 && mode != GenerateMode.AllInOne)
                {
                    indexWriter.AddAttribute(HtmlTextWriterAttribute.Href, currentPath + "#" + label);
                    indexWriter.RenderBeginTag(HtmlTextWriterTag.A);
                    indexWriter.WriteEncodedText(title);
                    indexWriter.RenderEndTag();
                    indexWriter.WriteBreak();
                }
                if (label != null)
                {
                    NewLabel(label);
                }
                pageWriter.RenderBeginTag("h" + (level + 1));
                pageWriter.WriteEncodedText(title);
                
            }
            else
            {
                pageWriter.RenderBeginTag(HtmlTextWriterTag.B);
                pageWriter.WriteEncodedText(title);                
            }
            
            pageWriter.WriteBreak();
        }

        public override void EndSectionTitle()
        {
            pageWriter.RenderEndTag();
        }

        public override void NewParagraph()
        {
            pageWriter.WriteBreak();            
        }

        public override void PrintText(string text, bool insertSpace = true)
        {
            if (insertSpace) pageWriter.Write(" ");
            String[] texts = text.Replace("&nbsp;"," ").Replace(System.Environment.NewLine, "\n").Split('\n');
            foreach (var writetext in texts)
            {
                pageWriter.WriteEncodedText(writetext);
                if (writetext != texts[0])
                {
                    pageWriter.WriteBreak();
                }
            }
        }

        public override void PrintVerbatimText(string text)
        {            
            String[] texts = text.Replace("&nbsp;", " ").Replace(System.Environment.NewLine, "\n").Split('\n');
            pageWriter.RenderBeginTag(HtmlTextWriterTag.B);
            foreach (var writetext in texts)
            {
                pageWriter.WriteEncodedText(writetext);
                if (writetext != texts[0])
                {
                    pageWriter.WriteBreak();
                }
            }
            pageWriter.RenderEndTag();
        }

        public override void BeginMarkup(DocumentMarkupKind markupKind)
        {
            switch (markupKind)
            {
                case DocumentMarkupKind.Bold:
                    pageWriter.RenderBeginTag(HtmlTextWriterTag.B);
                    break;
                case DocumentMarkupKind.Emphasis:
                    pageWriter.RenderBeginTag(HtmlTextWriterTag.I);
                    break;
                case DocumentMarkupKind.Small:
                    pageWriter.RenderBeginTag(HtmlTextWriterTag.Small);
                    break;
                case DocumentMarkupKind.SubScript:
                    pageWriter.RenderBeginTag(HtmlTextWriterTag.Sub);
                    break;
                case DocumentMarkupKind.SuperScript:
                    pageWriter.RenderBeginTag(HtmlTextWriterTag.Sup);
                    break;
                case DocumentMarkupKind.Center:
                    pageWriter.AddAttribute(HtmlTextWriterAttribute.Class, "center");
                    pageWriter.RenderBeginTag(HtmlTextWriterTag.Div);
                    break;
                case DocumentMarkupKind.Preformatted:
                    pageWriter.RenderBeginTag(HtmlTextWriterTag.Pre);
                    break;
                default:
                    throw new DocumentException("Invalid DocumentMarkupKind: " + markupKind);
            }
        }

        public override void EndMarkup(DocumentMarkupKind markupKind)
        {
            switch (markupKind)
            {
                case DocumentMarkupKind.Bold:
                    pageWriter.RenderEndTag();
                    break;
                case DocumentMarkupKind.Emphasis:
                    pageWriter.RenderEndTag();
                    break;
                case DocumentMarkupKind.Small:
                    pageWriter.RenderEndTag();
                    break;
                case DocumentMarkupKind.SubScript:
                    pageWriter.RenderEndTag();
                    break;
                case DocumentMarkupKind.SuperScript:
                    pageWriter.RenderEndTag();
                    break;
                case DocumentMarkupKind.Center:
                    pageWriter.RenderEndTag();
                    break;
                case DocumentMarkupKind.Preformatted:
                    pageWriter.RenderEndTag();
                    break;
                default:
                    throw new DocumentException("Invalid DocumentMarkupKind: " + markupKind);
            }
        }

        public override void NewLabel(string id)
        {
            pageWriter.AddAttribute(HtmlTextWriterAttribute.Name, id);
            pageWriter.RenderBeginTag(HtmlTextWriterTag.A);
            pageWriter.RenderEndTag();
        }

        public override void BeginReference(string id, bool url)
        {
            if (url)
            {
                pageWriter.AddAttribute(HtmlTextWriterAttribute.Href, id);
            }
            else
            {
                pageWriter.AddAttribute(HtmlTextWriterAttribute.Href, "#" + id);
            }
            pageWriter.RenderBeginTag(HtmlTextWriterTag.A);
        }

        public override void EndReference()
        {
            pageWriter.RenderEndTag();
        }

        public override void BeginList()
        {
            pageWriter.RenderBeginTag(HtmlTextWriterTag.Ul);
        }

        public override void EndList()
        {
            pageWriter.RenderEndTag();
        }

        public override void BeginListItem(int index, string title)
        {
            pageWriter.RenderBeginTag(HtmlTextWriterTag.Li);
            pageWriter.WriteEncodedText(title);
            
        }

        public override void EndListItem(int index)
        {
            pageWriter.RenderEndTag();
        }

        public override void BeginTable(int rowCount, int colCount)
        {
                pageWriter.AddAttribute(HtmlTextWriterAttribute.Style, "width:70%");
                pageWriter.RenderBeginTag(HtmlTextWriterTag.Table);
        }

        public override void BeginTableRow(int rowIndex)
        {
            if ((rowIndex % 2) == 0)
            {
                pageWriter.AddAttribute(HtmlTextWriterAttribute.Class, "rowColor");
            }
            else
            {
                pageWriter.AddAttribute(HtmlTextWriterAttribute.Class, "altColor");
            }
            pageWriter.RenderBeginTag(HtmlTextWriterTag.Tr);
        }

        public override void EndTableRow(int rowIndex)
        {
            pageWriter.RenderEndTag();
        }

        public override void BeginTableCell(int rowIndex, int colIndex, bool head)
        {
            if (head)
            {
                pageWriter.RenderBeginTag(HtmlTextWriterTag.Th);
            }
            else
            {
                pageWriter.RenderBeginTag(HtmlTextWriterTag.Td);
            }
        }

        public override void EndTableCell(int rowIndex, int colIndex, bool head)
        {
            pageWriter.RenderEndTag();
        }

        public override void EndTable()
        {
            pageWriter.RenderEndTag();
        }


        public override void AddImage(string path, string Width = null, string Height = null)
        {
            pageWriter.AddAttribute(HtmlTextWriterAttribute.Src, path);
            if(Width != null)
            {
                pageWriter.AddAttribute(HtmlTextWriterAttribute.Width, Width);
            }
            if(Height != null)
            {
                pageWriter.AddAttribute(HtmlTextWriterAttribute.Height, Height);
            }
            pageWriter.RenderBeginTag(HtmlTextWriterTag.Img);

        }
    }
}
