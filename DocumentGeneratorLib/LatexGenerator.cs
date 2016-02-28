using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SoftwareEngineeringTools.Documentation
{
    public class LatexGenerator : DocumentGenerator
    {
        private TextWriter writer;
        private bool paraBegin = false;
        private bool cellBegin = false;
        private bool newline = false;

        public LatexGenerator(string path) 
        {
            this.writer = new StreamWriter(path);
            this.BeginDocument();
        }

        public LatexGenerator(TextWriter writer)
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

        private static string EscapeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Replace(@"\", @"\\").Replace(@"{", @"\{").Replace(@"}", @"\}").Replace(@"&", @"\&")
                .Replace(@"%", @"\%").Replace(@"$", @"\$").Replace(@"#", @"\#").Replace(@"_", @"\_").Replace(@".",@".{}");
        }

        protected override void BeginDocument()
        {
            writer.WriteLine(@"\documentclass[11pt, oneside, a4paper]{book}");
            writer.WriteLine();
            writer.WriteLine(@"\usepackage{graphicx}");
            writer.WriteLine(@"\usepackage[margin=1.5cm]{geometry}");
            writer.WriteLine(@"\usepackage{hyperref}");
            writer.WriteLine(@"\usepackage[magyar]{babel}");
            writer.WriteLine(@"\usepackage[utf8]{inputenc}");
            writer.WriteLine(@"\usepackage{t1enc}");
            writer.WriteLine();
            writer.WriteLine(@"\begin{document}");
            this.paraBegin = true;
        }

        protected override void EndDocument()
        {
            writer.WriteLine(@"\end{document}");
            this.writer.Dispose();
            writer = null;
        }
        public override void SetContent(bool enabled, int depth)
        {
            if (enabled)
            {
                writer.WriteLine(@"\setcounter{tocdepth}{"+depth+"}");
                writer.WriteLine(@"\tableofcontents");
            }
        }
        public override void BeginSectionTitle(int level, string title, string label)
        {
            writer.WriteLine();
            title = EscapeText(title);
            if (label != null)
            {
                writer.WriteLine(@"\hypertarget{" + EscapeText(label) + "}{}");
            }
            switch (level)
            {
                case 0:
                    writer.Write(@"\chapter{" + title);
                    break;
                case 1:
                    writer.Write(@"\section{" + title);
                    break;
                case 2:
                    writer.Write(@"\subsection{" + title);
                    break;
                case 3:
                    writer.Write(@"\subsubsection{" + title);
                    break;
                default:
                    writer.Write(@"\noindent");
                    writer.Write(@"\textbf{" + title);
                    break;
            }
            this.paraBegin = true;
        }

        public override void EndSectionTitle()
        {
            writer.WriteLine(@"}");
        }

        public override void NewParagraph()
        {
            if (cellBegin == false)
            {
                writer.WriteLine();
                writer.WriteLine();
                this.paraBegin = true;
            }
            else
            {
                newline = true;
                //writer.WriteLine(@"\newline");
            }

        }

        public override void PrintText(string text, bool insertSpace = true)
        {
            if (newline == true)
            {
                if (cellBegin == false)
                {
                    writer.WriteLine(@"\newline");
                }
                else { 
                    writer.WriteLine(); 
                }
                newline = false;
            }            
            if (!this.paraBegin && insertSpace) writer.Write(" ");
            writer.Write(EscapeText(text));
            this.paraBegin = false;
        }

        public override void PrintVerbatimText(string text)
        {

            writer.Write(@"\begin{verbatim}");
            writer.Write(EscapeText(text));
            writer.Write(@"\end{verbatim}");
            writer.WriteLine();
            this.paraBegin = true;
        }

        public override void BeginMarkup(DocumentMarkupKind markupKind)
        {
            switch (markupKind)
            {
                case DocumentMarkupKind.Bold:
                    writer.Write(@"\textbf{");
                    break;
                case DocumentMarkupKind.Emphasis:
                    writer.Write(@"\textem{");
                    break;
                case DocumentMarkupKind.Small:
                    writer.Write(@"\texttt{\small ");
                    break;
                case DocumentMarkupKind.SubScript:
                    writer.Write(@"_{");
                    break;
                case DocumentMarkupKind.SuperScript:
                    writer.Write(@"^{");
                    break;
                case DocumentMarkupKind.Fail:
                    writer.Write(@"\colorbox[rgb]{0.89,0,0.19}{");
                    break;
                case DocumentMarkupKind.Success:
                    writer.Write(@"\colorbox[rgb]{0.52,0.98,0.6}{");
                    break;
                default:
                    throw new DocumentException("Invalid DocumentMarkupKind: " + markupKind);
            }
            this.paraBegin = false;
        }

        public override void EndMarkup(DocumentMarkupKind markupKind)
        {
            writer.Write("}");
        }

        public override void NewLabel(string id)
        {
            writer.Write(@"\hypertarget{" + EscapeText(id) + "}{}");
        }

        public override void BeginReference(string id, bool url)
        {
            writer.Write(@"\hyperlink{" + EscapeText(id) + "}{");
        }

        public override void EndReference()
        {
            writer.Write("}");
        }

        public override void BeginList()
        {
            writer.WriteLine(@"\begin{itemize}");
        }

        public override void EndList()
        {
            writer.WriteLine(@"\end{itemize}");
        }

        public override void BeginListItem(int index, string title)
        {
            if (title != null)
            {
                writer.WriteLine(@"\item{" + EscapeText(title) + "} ");
            }
            else
            {
                writer.Write(@"\item ");
            }
        }

        public override void EndListItem(int index)
        {
            writer.WriteLine();
        }

        public override void BeginTable(int rowCount, int colCount)
        {
            writer.WriteLine(@"\begin{center}");
            writer.Write(@"\begin{tabular}{");
            string sep = " ";
            if (colCount < 2)
            {
                writer.Write("|" + sep + "p{15cm}" + sep);
            }
            else
            {
                for (int i = 0; i < colCount; i++)
                {
                    if (i == 0)
                    {
                        writer.Write("|" + sep + "p{3cm}" + sep);
                    }
                    else
                    {
                        writer.Write("|" + sep + "p{12cm}" + sep);
                    }
                }
            }
            writer.WriteLine("| " + "}");
        }

        public override void BeginTableRow(int rowIndex)
        {
            writer.WriteLine(@"\hline");
        }

        public override void EndTableRow(int rowIndex)
        {
            writer.WriteLine(@"\\");
        }

        public override void BeginTableCell(int rowIndex, int colIndex, bool head)
        {
            newline = false;
            cellBegin = true;
            if (colIndex > 0)
            {
                writer.Write(@" & ");
            }
            if (head)
            {
                writer.Write(@"\textbf{");
            }
            else
            {
                writer.Write(@"");
            }
        }

        public override void EndTableCell(int rowIndex, int colIndex, bool head)
        {
            cellBegin = false;
            if (head)
            {
                writer.Write(@"}");
            }
            else
            {
                writer.Write(@"");
            }            
        }

        public override void EndTable()
        {
            newline = false;
            writer.WriteLine(@"\hline");
            writer.WriteLine(@"\end{tabular}");
            writer.WriteLine(@"\end{center}");
        }

        public override void AddImage(string path, string Width = null, string Height = null)
        {
            writer.WriteLine(@"\graphicspath{ {"+ path.Substring(0,path.LastIndexOf("\"")) +"} }");
            writer.WriteLine(@"\begin{figure}[p]");
            writer.Write(@"\includegraphics");
            if(Width != null)
            {
                writer.Write("[width=" + Int32.Parse(Width)+"\textwidth]");
            }
            writer.WriteLine("{" + path.Substring(path.LastIndexOf("\""))+"}");
            writer.WriteLine(@"\end{figure}");
        }
    }
}
