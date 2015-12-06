using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftwareEngineeringTools.Documentation;

namespace SoftwareEngineeringTools.Testing
{
    public class TestDocPrinter
    {
        private IDocumentGenerator dg;
        int depth = 0;
        int sectionLevel = 0;

        public TestDocPrinter(IDocumentGenerator generator)
        {
            this.dg = generator;
            this.sectionLevel = 0;
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        private string NormalizeName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            int index = name.LastIndexOf(":");
            if (index > 0)
            {
                return name.Substring(index + 1);
            }
            else if (name.LastIndexOf('.') > 0)
            {
                return name.Substring(name.LastIndexOf('.') + 1);
            }
            else
            {
                return name;
            }
            //return name.Replace("::", ".");

        }

        public void PrintDocCmd(DocCmd cmd)
        {
            depth++;
            if (depth > 20)
            {

            }
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            switch (cmd.Kind)
            {
                case DocKind.Description:
                    Description description = cmd as Description;
                    if (description.Title != null)
                    {
                        dg.BeginSectionTitle(this.sectionLevel, description.Title, null);
                        dg.EndSectionTitle();
                        this.sectionLevel++;
                    }
                    try
                    {
                        foreach (var p in description.Paragraphs)
                        {
                            this.PrintDocCmd(p);
                        }
                        foreach (var s in description.Sections)
                        {
                            this.PrintDocCmd(s);
                        }
                    }
                    finally
                    {
                        if (description.Title != null)
                        {
                            this.sectionLevel--;
                        }
                    }
                    break;
                case DocKind.HtmlTag:
                    break;
                case DocKind.CmdGroup:
                    DocCmdGroup docCmdGroup = cmd as DocCmdGroup;
                    foreach (var c in docCmdGroup.Commands)
                    {
                        this.PrintDocCmd(c);
                    }
                    break;
                case DocKind.Para:
                    DocPara docPara = cmd as DocPara;
                    foreach (var c in docPara.Commands)
                    {
                        this.PrintDocCmd(c);
                    }
                    dg.NewParagraph();
                    break;
                case DocKind.ParaList:
                    DocParaList docParaList = cmd as DocParaList;
                    foreach (var p in docParaList.Paragraphs)
                    {
                        this.PrintDocCmd(p);
                    }
                    break;
                case DocKind.Markup:
                    DocMarkup docMarkup = cmd as DocMarkup;

                    switch (docMarkup.MarkupKind)
                    {
                        case DocMarkupKind.Bold:
                            dg.BeginMarkup(DocumentMarkupKind.Bold);
                            break;
                        case DocMarkupKind.Emphasis:
                            dg.BeginMarkup(DocumentMarkupKind.Emphasis);
                            break;
                        case DocMarkupKind.SubScript:
                            dg.BeginMarkup(DocumentMarkupKind.SubScript);
                            break;
                        case DocMarkupKind.SuperScript:
                            dg.BeginMarkup(DocumentMarkupKind.SuperScript);
                            break;
                        case DocMarkupKind.Center:
                            dg.BeginMarkup(DocumentMarkupKind.Center);
                            break;
                        case DocMarkupKind.ComputerOutput:
                            dg.BeginMarkup(DocumentMarkupKind.ComputerOutput);
                            break;
                        case DocMarkupKind.Preformatted:
                            dg.BeginMarkup(DocumentMarkupKind.Preformatted);
                            break;
                        default:
                            this.Log("Unsupported Markup:" + docMarkup);
                            break;
                    }
                    try
                    {
                        foreach (var p in docMarkup.Commands)
                        {
                            this.PrintDocCmd(p);
                        }
                    }
                    finally
                    {
                        switch (docMarkup.MarkupKind)
                        {
                            case DocMarkupKind.Bold:
                                dg.EndMarkup(DocumentMarkupKind.Bold);
                                break;
                            case DocMarkupKind.Emphasis:
                                dg.EndMarkup(DocumentMarkupKind.Emphasis);
                                break;
                            case DocMarkupKind.SubScript:
                                dg.EndMarkup(DocumentMarkupKind.SubScript);
                                break;
                            case DocMarkupKind.SuperScript:
                                dg.EndMarkup(DocumentMarkupKind.SuperScript);
                                break;
                            case DocMarkupKind.Center:
                                dg.EndMarkup(DocumentMarkupKind.Center);
                                break;
                            case DocMarkupKind.ComputerOutput:
                                dg.EndMarkup(DocumentMarkupKind.ComputerOutput);
                                break;
                            case DocMarkupKind.Preformatted:
                                dg.EndMarkup(DocumentMarkupKind.Preformatted);
                                break;
                            default:
                                this.Log("Unsupported Markup:" + docMarkup);
                                break;
                        }
                    }
                    break;
                case DocKind.Text:
                    DocText docText = cmd as DocText;
                    switch (docText.TextKind)
                    {
                        case DocTextKind.Plain:
                            dg.PrintText(docText.Text);
                            break;
                        case DocTextKind.Verbatim:
                            dg.PrintVerbatimText(docText.Text);
                            break;
                        default:
                            this.Log("WARNING: unsupported text kind: DocTextKind." + docText.TextKind);
                            break;
                    }
                    break;
                case DocKind.Char:
                    DocChar docChar = cmd as DocChar;
                    this.Log("WARNING: unsupported char kind: DocCharKind." + docChar.CharKind);
                    break;
                case DocKind.Empty:
                    DocEmpty docEmpty = cmd as DocEmpty;
                    this.Log("WARNING: unsupported empty kind: DocEmptyKind." + docEmpty.EmptyKind);
                    break;
                case DocKind.UrlLink:
                    DocUrlLink docUrlLink = cmd as DocUrlLink;
                    dg.BeginReference(docUrlLink.Url, true);
                    try
                    {
                        foreach (var p in docUrlLink.Commands)
                        {
                            this.PrintDocCmd(p);
                        }
                    }
                    finally
                    {
                        dg.EndReference();
                    }
                    break;
                case DocKind.Image:
                    DocImage docImage = cmd as DocImage;
                    dg.AddImage(docImage.Path,docImage.Width,docImage.Height);
                    break;
                case DocKind.Anchor:
                    DocAnchor docAnchor = cmd as DocAnchor;
                    dg.NewLabel(docAnchor.Id);
                    foreach (var p in docAnchor.Commands)
                    {
                        this.PrintDocCmd(p);
                    }
                    break;

                case DocKind.Command:
                    Command command = cmd as Command;
                    command.execute();
                    break;
                case DocKind.IfCommand:
                    IfCommand currentIfcommand = cmd as IfCommand;
                    currentIfcommand.execute();
                    break;
                case DocKind.Reference:
                    DocReference docReference = cmd as DocReference;
                    if (docReference.Compound != null || docReference.Member != null || docReference.referenceID != null)
                    {
                        switch (docReference.RefKind)
                        {
                            case DocRefKind.Compound:
                                dg.BeginReference(docReference.Compound.Identifier, false);
                                break;
                            case DocRefKind.Member:
                                dg.BeginReference(docReference.Member.Identifier, false);
                                break;
                            case DocRefKind.CustomID:
                                dg.BeginReference(docReference.referenceID, true);
                                break;
                            default:
                                this.Log("WARNING: unsupported reference kind: DocRefKind." + docReference.RefKind);
                                break;
                        }
                    }
                    try
                    {
                        if (docReference.Commands.Count > 0)
                        {
                            foreach (var p in docReference.Commands)
                            {
                                this.PrintDocCmd(p);
                            }
                        }
                        else if (docReference.Compound != null || docReference.Member != null)
                        {
                            string name = null;
                            switch (docReference.RefKind)
                            {
                                case DocRefKind.Compound:
                                    name = docReference.Compound.Name;
                                    break;
                                case DocRefKind.Member:
                                    name = docReference.Member.Name;
                                    break;
                                default:
                                    this.Log("WARNING: unsupported reference kind: DocRefKind." + docReference.RefKind);
                                    break;
                            }
                            if (name != null)
                            {
                                name = this.NormalizeName(name);
                            }
                            this.PrintDocCmd(new DocText() { TextKind = DocTextKind.Plain, Text = name });
                        }
                    }
                    finally
                    {
                        dg.EndReference();
                    }
                    break;
                case DocKind.Sect:
                    DocSect docSect = cmd as DocSect;
                    if (docSect.Title != null)
                    {
                        dg.BeginSectionTitle(this.sectionLevel, docSect.Title, docSect.Identifier);
                        dg.EndSectionTitle();
                        this.sectionLevel++;
                    }
                    try
                    {
                        foreach (var p in docSect.Paragraphs)
                        {
                            this.PrintDocCmd(p);
                        }
                        foreach (var s in docSect.Sections)
                        {
                            this.PrintDocCmd(s);
                        }
                    }
                    finally
                    {
                        if (docSect.Title != null)
                        {
                            this.sectionLevel--;
                        }
                    }
                    break;
                case DocKind.SimpleSect:
                    DocSimpleSect docSimpleSect = cmd as DocSimpleSect;
                    if (docSimpleSect.Title != null)
                    {
                        dg.BeginSectionTitle(this.sectionLevel, docSimpleSect.Title, null);
                        dg.EndSectionTitle();
                    }
                    else
                    {
                        switch (docSimpleSect.SimpleSectKind)
                        {
                            case DocSimpleSectKind.See:
                            case DocSimpleSectKind.Return:
                            case DocSimpleSectKind.Author:
                            case DocSimpleSectKind.Authors:
                            case DocSimpleSectKind.Version:
                            case DocSimpleSectKind.Since:
                            case DocSimpleSectKind.Date:
                            case DocSimpleSectKind.Note:
                            case DocSimpleSectKind.Warning:
                            case DocSimpleSectKind.Pre:
                            case DocSimpleSectKind.Post:
                            case DocSimpleSectKind.Copyright:
                            case DocSimpleSectKind.Invariant:
                            case DocSimpleSectKind.Remark:
                            case DocSimpleSectKind.Attention:
                            case DocSimpleSectKind.Par:
                            case DocSimpleSectKind.Rcs:
                                dg.BeginSectionTitle(this.sectionLevel, docSimpleSect.SimpleSectKind.ToString(), null);
                                dg.EndSectionTitle();
                                break;
                            default:
                                this.Log("WARNING: unsupported simple section kind: DocSimpleSectKind." + docSimpleSect.SimpleSectKind);
                                break;
                        }
                    }
                    this.sectionLevel++;
                    try
                    {
                        foreach (var p in docSimpleSect.Items)
                        {
                            this.PrintDocCmd(p);
                        }
                    }
                    finally
                    {
                        this.sectionLevel--;
                    }
                    break;
                case DocKind.SimpleSectItem:
                    DocSimpleSectItem docSimpleSectItem = cmd as DocSimpleSectItem;
                    foreach (var p in docSimpleSectItem.Paragraphs)
                    {
                        this.PrintDocCmd(p);
                    }
                    if (docSimpleSectItem.Separator != null)
                    {
                        this.PrintDocCmd(docSimpleSectItem.Separator);
                    }
                    break;
                case DocKind.Heading:
                    DocHeading docHeading = cmd as DocHeading;
                    if (docHeading.Commands.Count > 0)
                    {
                        dg.BeginSectionTitle(this.sectionLevel, "", null);
                        foreach (var c in docHeading.Commands)
                        {
                            this.PrintDocCmd(c);
                        }
                        dg.EndSectionTitle();
                    }
                    break;
                case DocKind.List:
                    DocList docList = cmd as DocList;
                    dg.BeginList();
                    try
                    {
                        int listItemIndex = 0;
                        foreach (var li in docList.Items)
                        {
                            dg.BeginListItem(listItemIndex, null);
                            try
                            {
                                foreach (var p in li.Paragraphs)
                                {
                                    this.PrintDocCmd(p);
                                }
                            }
                            finally
                            {
                                dg.EndListItem(listItemIndex);
                            }
                            ++listItemIndex;
                        }
                    }
                    finally
                    {
                        dg.EndList();
                    }
                    break;
                case DocKind.Table:
                    DocTable docTable = cmd as DocTable;
                    dg.BeginTable(docTable.RowCount, docTable.ColCount);
                    try
                    {
                        int rowIndex = 0;
                        foreach (var tr in docTable.Rows)
                        {
                            dg.BeginTableRow(rowIndex);
                            try
                            {
                                int colIndex = 0;
                                foreach (var tc in tr.Cells)
                                {

                                    dg.BeginTableCell(rowIndex, colIndex, tc.IsHeader);
                                    try
                                    {
                                        foreach (var p in tc.Paragraphs)
                                        {
                                            this.PrintDocCmd(p);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        this.Log(e.Message);
                                    }
                                    finally
                                    {
                                        dg.EndTableCell(rowIndex, colIndex, tc.IsHeader);
                                    }
                                    ++colIndex;
                                }
                            }
                            catch
                            {
                                this.Log("Error while make table");
                            }
                            finally
                            {
                                dg.EndTableRow(rowIndex);
                                ++rowIndex;
                            }
                        }
                    }
                    finally
                    {
                        dg.EndTable();
                    }
                    break;
                default:
                    this.Log("WARNING: unsupported command: DocCmd." + cmd.Kind);
                    break;
            }
        }
    }
}
