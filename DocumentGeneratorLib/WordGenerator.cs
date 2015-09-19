using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Word;
using System.IO;

///<summary>Classic word template</summary>
///
namespace SoftwareEngineeringTools.Documentation
{
    /// <summary>
    /// Generator, what makes Word document
    /// </summary>
    public class WordGenerator : DocumentGenerator
    {
        private string path;
        //private bool cellBegin = false;
        private int depth = 0;
        private List<dynamic> markupStack;
        private List<dynamic> listStack;
        private List<dynamic> hyperStack;
        Table table;
        ListGallery listGallery;
        TableOfContents toc;
        bool isContent;
        bool newLine = false;
        WordTemplate currentTemplate;
        Dictionary<int, String> bookmarks = new Dictionary<int, string>();
        static int nextid = 0;
        bool required = false;
        

        private dynamic word = null;
        private dynamic doc = null;
        private dynamic sel = null;
        private dynamic normalStyle = null;
        private dynamic boldedStyle = null;
        private dynamic verbatimStyle = null;
        private dynamic headerColor = null;

        
        /// <summary>
        /// These are the standard, "build in" Word templates
        /// </summary>
        public enum WordTemplate 
        {
            ///Classic word template
            Classic, 
            Default,
            DefaultBlackAndWhite, /*! klasszikus kinézet fekete és fehér beállításokkal.*/
            Distinctive,
            Elegant,
            Fancy, ///<summary>Fancy word template</summary>
            Formal, 
            Manuscript,
            Modern,
            Newsprint,
            Perspective,
            Simple,
            Thatch,
            Traditional 
        }

        //! Constructor of WordGenerator. <param name="path">Elérési út</param>
       /// <summary>
       /// Constuctor of WorldGenerator
       /// </summary>
        /// <example> This sample shows how to call the WordGenerator method.
        /// <code>
        ///   class MyClass 
        ///   {
        ///      public static int Main() 
        ///      {
        ///         DocumentGenerator g = new WorldGenerator("C:\\", true)
        ///      }
        ///   }
        /// </code>
        /// </example>
        /// <param name="path"><summary>The path of the document</summary><remarks>There would be generated the new document.</remarks></param>
       /// <param name="showWord">Should the Word be visible?</param>
       /// <param name="template">The name of the template, what the program uses</param>
        public WordGenerator(string path, bool showWord = true, WordTemplate template = WordTemplate.Default)
        {
            currentTemplate = template;
            this.path = path;
            this.ShowWord = showWord;          
            this.BeginDocument();
        }

        //! A dispose.
        /*!
          Calls the ending of the document.
        */
        /// <summary>
        /// Dispose the generator. <c>Dispose</c> is in the class <c>WordGenerator</c>
        /// </summary>
        public override void Dispose()
        {
            this.EndDocument();
        }

        public bool ShowWord
        {
            get;
            private set;
        }

        //! Begin the document.
        /*!
          Open a new Word application and a new document with the given template. After that it set the styles 
        */
        /// <summary>
        /// Begin the document.
        /// Open a new Word application and a new document with the given template. After that it set the styles
        /// </summary>
        protected override void BeginDocument()
        {
            this.markupStack = new List<dynamic>();
            this.hyperStack = new List<dynamic>();
            //this.tableStack = new List<dynamic>();
            this.listStack = new List<dynamic>();
            /*Type wordType = Type.GetTypeFromProgID("Word.Application");
            if (wordType == null)
            {
                throw new DocumentException("Failed to create word application. Check whether Word installation is correct");
            }*/
            //this.word = Activator.CreateInstance(wordType);
            this.word = new Application();
            if (this.word == null)
            {
                throw new DocumentException("Failed to create word application. Check whether Word installation is correct");
            }
            
            String oTemplate = Path.GetFullPath(@"..\..\quickstyles\"+ currentTemplate.ToString() +".dotx");
            this.doc = this.word.Documents.Add(oTemplate);
            //setTableStyle();
            this.word.Visible = ShowWord;
            if (this.doc == null)
            {
                throw new DocumentException("Failed to create word document. Check whether Word installation is correct");
            }
            this.sel = this.word.Selection;
            if (this.word.Options.Overtype)
            {
                this.word.Options.Overtype = false;
            }
            this.normalStyle = this.doc.Styles[WdBuiltinStyle.wdStyleNormal];
            this.boldedStyle = this.doc.Styles.Add(Name: "Bolded", Type: WdStyleType.wdStyleTypeCharacter);
            this.verbatimStyle = this.doc.Styles.Add(Name: "Verbatim", Type: WdStyleType.wdStyleTypeCharacter);
            this.verbatimStyle.Font.Name = "Courier New";
            this.verbatimStyle.Font.Size = this.normalStyle.Font.Size - 2;
            listGallery = this.doc.Application.ListGalleries[WdListGalleryType.wdBulletGallery];
            setColor();

        }

        protected override void EndDocument()
        {
            System.Threading.Thread.Sleep(1000);            
            if (this.isContent)
            {                          
                toc.Update();
            }
            if (this.word == null) return;
            this.doc.SaveAs(FileName: path, FileFormat: WdSaveFormat.wdFormatDocumentDefault);
            if (this.ShowWord)
            {
                System.Threading.Thread.Sleep(1000);                
                //this.word.Quit();
                this.doc = null;
                this.word = null;
            }
            else
            {
                this.doc.Close();
                this.word.Quit();
                this.doc = null;
                this.word = null;
            }
            this.markupStack = null;
        }

        //! Set the Table of Content.
        /*!
          Set the depth of the Table of Content in relation of the usen template.
        */
        public override void SetContent(bool enabled, int depth)
        {
            this.isContent = enabled;
            if (enabled)
            {
                toc = this.doc.TablesOfContents.Add(sel.Range, true, 1, depth);
            }
        }


        public override void BeginSectionTitle(int level, string title, string label)
        {
            dynamic headingType = null;
            switch (level)
            {
                case 0:
                    headingType = WdBuiltinStyle.wdStyleHeading1;
                    break;
                case 1:
                    headingType = WdBuiltinStyle.wdStyleHeading2;
                    break;
                case 2:
                    headingType = WdBuiltinStyle.wdStyleHeading3;
                    break;
                case 3:
                    headingType = WdBuiltinStyle.wdStyleHeading4;
                    break;
                default:
                    headingType = boldedStyle;
                    break;
            }

            sel.Style = headingType;
            sel.TypeText(title);
            sel.TypeParagraph();
            if (label != null)
            {
                NewLabel(label);
            }
        }

        public override void EndSectionTitle()
        {

        }

        //! Make new Paragraph.
        /*!
          It make a new paragraph with normal style.
        */
        public override void NewParagraph()
        {
                     
            if (this.required)
            {                
                sel.TypeParagraph();
                this.required = false;
            }
            else
            {
                newLine = true;
            }

        }

        public override void PrintText(string text, bool insertSpace = true)
        {
            if (newLine)
            {
                sel.TypeParagraph();
            }
            //if (cellBegin)
            //{
            //    //tableStack.Add(text);   
            //    //if (newLine)
            //    //{
            //    //    CellRange.Text = CellRange.Text.Substring(0, CellRange.Text.Length - 3) + +'\r' + text; ;
            //    //}
            //    //else
            //    //{
            //    //    CellRange.Text = CellRange.Text.Substring(0, CellRange.Text.Length - 2) + text;
            //    //}
            //}
            //else
            //{
                //if (!this.paraBegin && insertSpace) sel.TypeText(" ")            
                sel.TypeText(text);
            //}
            newLine = false;
        }

        public override void PrintVerbatimText(string text)
        {
            if (newLine)
            {
                sel.TypeParagraph();
                //tableStack.Add(System.Environment.NewLine);
            }
            //if (cellBegin)
            //{
            //    tableStack.Add(text);              
            //}
            //else
            //{
                sel.Style = verbatimStyle;
                sel.TypeText(text);
               // sel.TypeParagraph();
            //}
            newLine = false;
        }

        //! Begin Markup.
        /*!
          Store the starting position of the markup, so it can be used in the ending.
        */
        public override void BeginMarkup(DocumentMarkupKind markupKind)
        {
            this.markupStack.Add(sel.Start);
        }

        public override void EndMarkup(DocumentMarkupKind markupKind)
        {
            dynamic end = sel.End;
            dynamic start = this.markupStack[this.markupStack.Count - 1];
            this.markupStack.RemoveAt(this.markupStack.Count - 1);
            dynamic range = doc.Range(start, end);
            switch (markupKind)
            {
                case DocumentMarkupKind.Bold:
                    range.Font.Bold = true;
                    break;
                case DocumentMarkupKind.Emphasis:
                    range.Font.Italic = true;
                    break;
                case DocumentMarkupKind.Small:
                    range.Font.Name = "Courier New";
                    range.Font.Size = this.normalStyle.Font.Size - 2;
                    break;
                case DocumentMarkupKind.SubScript:
                    range.Font.Subscript = true;
                    break;
                case DocumentMarkupKind.SuperScript:
                    range.Font.Superscript = true;
                    break;
                case DocumentMarkupKind.Center:
                    range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    break;
                case DocumentMarkupKind.ComputerOutput:
                    range.Font.Name = "Courier New";
                    range.Font.Size = this.normalStyle.Font.Size - 2;
                    break;
                default:
                    throw new DocumentException("Invalid DocumentMarkupKind: " + markupKind);
            }
        }

        public override void NewLabel(string id)
        {
            dynamic start = sel.Start - 1;
            dynamic end = sel.End;
            dynamic range = doc.Range(start, end);
            bookmarks.Add(nextid, id);
            nextid++;
            sel.Bookmarks.Add('_' + nextid.ToString(), range);
        }

        public override void BeginReference(string id, bool url = false)
        {
            int key = bookmarks.First(c => c.Value == id).Key;
            this.hyperStack.Add('_' + key);
            this.hyperStack.Add(sel.Start);
            this.hyperStack.Add(url);
        }

        public override void EndReference()
        {
            dynamic end = sel.End;
            bool url = this.hyperStack[this.hyperStack.Count - 1];
            this.hyperStack.RemoveAt(this.hyperStack.Count - 1);

            dynamic start = this.hyperStack[this.hyperStack.Count - 1];
            this.hyperStack.RemoveAt(this.hyperStack.Count - 1);

            dynamic id = this.hyperStack[this.hyperStack.Count - 1];
            this.hyperStack.RemoveAt(this.hyperStack.Count - 1);
            dynamic range;
            //if (cellBegin)
            //{
            //    tableStack.Add(id);
            //    return;
            //}
            //else
            //{
                range = doc.Range(start, end);
            //}
            if (url)
            {
                sel.Hyperlinks.Add(range, id);
            }
            else
            {
                sel.Hyperlinks.Add(range, Type.Missing, id);
            }
        }

        private void ApplyListTemplate(ListGallery listGallery, ListFormat listFormat, int level = 1)
        {
            listFormat.ApplyListTemplateWithLevel(
                listGallery.ListTemplates[level],
                ContinuePreviousList: true,
                ApplyTo: WdListApplyTo.wdListApplyToSelection,
                DefaultListBehavior: WdDefaultListBehavior.wdWord10ListBehavior,
                ApplyLevel: level);
        }

        public override void BeginList()
        {
            this.depth += 1;
        }

        public override void EndList()
        {

            /* dynamic range = doc.Range(listStack[listStack.Count - 1], sel.End);
             listStack.RemoveAt(listStack.Count - 1);
             ListFormat listFormat = null;
             listFormat = range.ListFormat;
             this.ApplyListTemplate(listGallery, listFormat, 1);            
             range.ListFormat.ApplyBulletDefault();
             this.paraBegin = true;*/
            this.depth -= 1;
        }

        public override void BeginListItem(int index, string title)
        {
            listStack.Add(sel.Start);
            if (title != null)
            {
                /*sel.Font.Bold = true;
                title = title + "\n";
                sel.TypeText(title);*/
                ListFormat listFormat = null;
                listFormat = sel.Range.ListFormat;
                sel.Range.Text = title;
                this.ApplyListTemplate(listGallery, listFormat, 1);
                sel.Range.InsertParagraphAfter();
            }
        }

        public override void EndListItem(int index)
        {
            dynamic range = doc.Range(listStack[listStack.Count - 1], sel.End);
            listStack.RemoveAt(listStack.Count - 1);
            ListFormat listFormat = null;
            listFormat = range.ListFormat;
            this.ApplyListTemplate(listGallery, listFormat, this.depth);
        }

        public override void BeginTable(int rowCount, int colCount)
        {
            if (rowCount > 0 && colCount > 0)
            {
                              
                this.table = this.doc.Tables.Add(this.word.Selection.Range, rowCount, colCount, WdDefaultTableBehavior.wdWord9TableBehavior,
                    WdAutoFitBehavior.wdAutoFitWindow);
                if (colCount > 1)
                {
                    this.table.Columns[1].SetWidth(100, WdRulerStyle.wdAdjustSameWidth);
                }
                table.set_Style(WdBuiltinStyle.wdStyleTableLightList);
                table.ApplyStyleFirstColumn = false;
                this.word.Selection.InsertParagraphAfter();
            }
        }

        public override void BeginTableRow(int rowIndex)
        {


        }

        public override void EndTableRow(int rowIndex)
        {

        }

        public override void BeginTableCell(int rowIndex, int colIndex, bool head)
        {
            newLine = false;
            var begin = this.table.Cell(rowIndex+1, colIndex+1).Range.Start;
            var end = this.table.Cell(rowIndex+1, colIndex+1).Range.End;
            this.word.Selection.SetRange(begin,end);
            //this.tableStack = new List<dynamic>();
            //this.tableStack.Add(sel.End);
            //cellBegin = true;
            //this.tableCell = this.table.Cell(rowIndex + 1, colIndex + 1);
            //CellRange = tableCell.Range;
            if (head)
            {
                this.table.Rows[rowIndex + 1].Shading.BackgroundPatternColor = headerColor;
            }
            else
            {
                //this.table.Cell(rowIndex + 1, colIndex + 1).Range.Font.Bold = 1;
                
            }
        }

        public override void EndTableCell(int rowIndex, int colIndex, bool head)
        {
            //dynamic end = sel.End;
            //String cellText = "";
            //while ((this.tableStack.Count) > 1)
            //{
            //    cellText = cellText + this.tableStack[1];
            //    this.tableStack.RemoveAt(1);
            //}
            //dynamic start = this.tableStack[this.tableStack.Count - 1];
            //this.tableStack.RemoveAt(this.tableStack.Count - 1);
            //dynamic range = doc.Range(start, end);
            //table.Cell(rowIndex + 1, colIndex + 1).Range.Text = cellText;
            //if (head)
            //{
            //    //table.Rows[1].HeadingFormat = -1;                
            //}
            //cellBegin = false;
        }

        public override void EndTable()
        {
            dynamic end = word.Selection.Range.StoryLength - 1;
            this.word.Selection.SetRange(end, end);
            
            this.table = null;
            this.required = true;
            //this.word.Selection.SetRange(this.doc.Content.End, this.doc.Content.End);            
        }

        void setColor()
        {
           switch (currentTemplate)
            {
                case WordTemplate.Default:
                    headerColor = (Microsoft.Office.Interop.Word.WdColor)(33 + 0x100 * 121 + 0x10000 * 142);
                    return;
                case WordTemplate.Fancy:
                case WordTemplate.Formal:
                    headerColor = (Microsoft.Office.Interop.Word.WdColor)(148 + 0x100 * 54 + 0x10000 * 52);
                    return;
                case WordTemplate.Elegant:
                    headerColor = (Microsoft.Office.Interop.Word.WdColor)(229 + 0x100 * 223 + 0x10000 * 236);
                    return;
                case WordTemplate.Modern:
                    headerColor = (Microsoft.Office.Interop.Word.WdColor)(79 + 0x100 * 129 + 0x10000 * 189);
                    return;
                case WordTemplate.Newsprint:
                    headerColor = (Microsoft.Office.Interop.Word.WdColor)(173 + 0x100 * 1 + 0x10000 * 1);
                    return;
                case WordTemplate.Perspective:
                    headerColor = (Microsoft.Office.Interop.Word.WdColor)(40 + 0x100 * 49 + 0x10000 * 56);
                    return;
                case WordTemplate.Thatch:
                    headerColor = (Microsoft.Office.Interop.Word.WdColor)(154 + 0x100 * 141 + 0x10000 * 9);
                    return;
                case WordTemplate.Traditional:
                    headerColor = (Microsoft.Office.Interop.Word.WdColor)(54 + 0x100 * 95 + 0x10000 * 145);
                    return;
                default:
                    headerColor = WdColor.wdColorBlack;
                    return;
            }
            
        }
    }
}
        