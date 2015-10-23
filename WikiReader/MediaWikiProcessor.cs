using SoftwareEngineeringTools.Documentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareEngineeringTools.WikiReader
{
    public class MediaWikiProcessor : MediaWikiParserBaseVisitor<object>
    {
        WikiParser reader;

        DoxNamespace currentNS;
        DoxClassifier currentClassifier;
        DoxMember currentMember;
        CompoundIndex currentCompIndex;
        string identifier;
        CompoundKind currentCompoundKind;
        MemberKind currentMemberKind;

        public MediaWikiProcessor(WikiParser reader)
        {
            this.reader = reader;
        }

        private void setCompound(Compound comp, bool isNamespace)
        {
            if (isNamespace == true)
            {
                if (this.identifier == null)
                {
                    comp.Identifier = comp.Name;
                }
                else
                {
                    comp.Identifier = this.identifier;
                    identifier = null;
                }
            }
            else
            {
                if(currentNS != null)
                {
                    currentNS.Classifiers.Add((DoxClassifier)comp);
                }
                if (this.identifier == null)
                {
                    comp.Identifier = currentNS.Identifier + "_" + comp.Name;
                }
                else
                {
                    comp.Identifier = this.identifier;
                    identifier = null;
                }
                
            }
            currentCompIndex = new CompoundIndex();
            currentCompIndex.Identifier = comp.Identifier;
            currentCompIndex.Name = comp.Name;
            reader.Index.CompoundIndexRefs.Add(currentCompIndex.Identifier, currentCompIndex);
            reader.Model.Compounds.Add(comp);
            reader.Model.CompoundRefs.Add(currentCompIndex.Identifier, currentCompIndex.Compound);
        }


        public override object VisitHeading(MediaWikiParser.HeadingContext context)
        {            
            //string[] TypeAndName = context.headingText().GetText().Split(':');
            //IDEnd = TypeAndName[0].IndexOf('>');
            //TypeAndName[0] = TypeAndName[0].Substring(IDEnd);
            //switch (TypeAndName[0].Trim())
            //{
            //    case "Namespace":
            //        currentNS = new DoxNamespace();
            //        currentClassifier = null;
            //        currentMember = null;
            //        currentNS.Name = TypeAndName[1].Trim();
            //        if (IDEnd == -1)
            //        {
            //            setCompound(currentNS, true);
            //        }
            //        break;
            //    case "Class":
            //        currentClassifier = new DoxClassifier();
            //        currentMember = null;
            //        currentClassifier.Name = TypeAndName[1].Trim();
            //        currentClassifier.Kind = CompoundKind.Class;
            //        if (IDEnd == -1)
            //        {
            //            setCompound(currentClassifier, false);
            //        }
            //        break;
            //    case "Interface":
            //        currentClassifier = new DoxClassifier();
            //        currentMember = null;
            //        currentClassifier.Name = TypeAndName[1].Trim();
            //        currentClassifier.Kind = CompoundKind.Interface;
            //        if (IDEnd == -1)
            //        {
            //            setCompound(currentClassifier, false);
            //        }
            //        break;
            //    case "Struct":
            //        currentClassifier = new DoxClassifier();
            //        currentMember = null;
            //        currentClassifier.Name = TypeAndName[1].Trim();
            //        currentClassifier.Kind = CompoundKind.Struct;
            //        if (IDEnd == -1)
            //        {
            //            setCompound(currentClassifier, false);
            //        }
            //        break;
            //    case "Enums":
            //        currentClassifier = new DoxClassifier();
            //        currentMember = null;
            //        currentClassifier.Name = TypeAndName[1].Trim();
            //        currentClassifier.Kind = CompoundKind.Enum;
            //        if (IDEnd == -1)
            //        {
            //            setCompound(currentClassifier, false);
            //        }
            //        break; 
            //    case "Methods":
            //        currentMemberKind = MemberKind.Function;
            //        break;
            //    case "Properties":
            //        currentMemberKind = MemberKind.Property;
            //        break;
            //    case "Fields":
            //        currentMemberKind = MemberKind.Variable;
            //        break;
            //    case "Enum values":
            //        currentMemberKind = MemberKind.EnumValue;
            //        break;
            //}
            return base.VisitHeading(context);
        }
        public override object VisitCellText(MediaWikiParser.CellTextContext context)
        {
            Console.WriteLine("VisitCellText: " + context.GetText());
            return base.VisitCellText(context);
        }
        public override object VisitCellTextElement(MediaWikiParser.CellTextElementContext context)
        {  
            
            Console.WriteLine("VisitCellTextElement: " + context.GetText());
            char[] separatechars = { ' ', '(', ',', ')' };
            string[] data = context.GetText().Replace("<nowiki>", string.Empty).Replace("</nowiki>", string.Empty).Split(separatechars);
            foreach (var item in data)
            {
                switch (item)
                {
                    case "Public":
                        currentMember.ProtectionKind = ProtectionKind.Public;
                        break;
                    case "Private":
                        currentMember.ProtectionKind = ProtectionKind.Private;
                        break;
                    case "Protected":
                        currentMember.ProtectionKind = ProtectionKind.Protected;
                        break;
                    case "new":
                        currentMember.New = true;
                        break;
                    case "final":
                        currentMember.Final = true;
                        break;
                    case "volatile":
                        currentMember.Volatile = true;
                        break;
                    case "Abstract":
                        currentMember.VirtualKind = VirtualKind.Abstract;
                        break;
                    case "Virtual":
                        currentMember.VirtualKind = VirtualKind.Virtual;
                        break;
                    case "inline":
                        currentMember.Inline = true;
                        break;
                    case "static":
                        currentMember.Static = true;
                        break;
                    default:
                        break;
                }
            }
            return base.VisitCellTextElement(context);
        }
        public override object VisitDefinitionItem(MediaWikiParser.DefinitionItemContext context)
        {
            Console.WriteLine("VisitDefinitionItem: " + context.GetText());
            return base.VisitDefinitionItem(context);
        }
        public override object VisitCellTextElements(MediaWikiParser.CellTextElementsContext context)
        {
            Console.WriteLine("VisitCellTextElements: " + context.GetText());
            return base.VisitCellTextElements(context);
        }
        public override object VisitDefinitionText(MediaWikiParser.DefinitionTextContext context)
        {
            Console.WriteLine("VisitDefinitionText: " + context.GetText());
            return base.VisitDefinitionText(context);
        }
        public override object VisitDefinitionTextElement(MediaWikiParser.DefinitionTextElementContext context)
        {
            Console.WriteLine("VisitDefinitionTextElement: " + context.GetText());
            return base.VisitDefinitionTextElement(context);
        }
        public override object VisitDefinitionTextElements(MediaWikiParser.DefinitionTextElementsContext context)
        {
            Console.WriteLine("VisitDefinitionTextElements: " + context.GetText());
            return base.VisitDefinitionTextElements(context);
        }
        public override object VisitHeadingText(MediaWikiParser.HeadingTextContext context)
        {
            Console.WriteLine("VisitHeadingText: " + context.GetText());
            return base.VisitHeadingText(context);
        }
        public override object VisitHeadingTextElement(MediaWikiParser.HeadingTextElementContext context)
        {
            Console.WriteLine("VisitHeadingTextElement: " + context.GetText());
            string TextElement = context.GetText();
            if (TextElement.Trim() != "" && TextElement != "</div>" && TextElement.Substring(0, 4) != "<div")
            {
                TextElement = TextElement.Substring(8, TextElement.Length - 17);
                string[] TypeAndName = TextElement.Split(':');
                switch (TypeAndName[0].Trim())
                {
                    case "Namespace":
                        currentNS = new DoxNamespace();
                        currentClassifier = null;
                        currentMember = null;
                        currentNS.Name = TypeAndName[1].Trim();
                        setCompound(currentNS, true);
                        break;
                    case "Class":
                        currentClassifier = new DoxClassifier();
                        currentMember = null;
                        currentClassifier.Name = TypeAndName[1].Trim();
                        currentClassifier.Kind = CompoundKind.Class;
                        setCompound(currentClassifier, false);
                        break;
                    case "Interface":
                        currentClassifier = new DoxClassifier();
                        currentMember = null;
                        currentClassifier.Name = TypeAndName[1].Trim();
                        currentClassifier.Kind = CompoundKind.Interface;
                        setCompound(currentClassifier, false);
                        break;
                    case "Struct":
                        currentClassifier = new DoxClassifier();
                        currentMember = null;
                        currentClassifier.Name = TypeAndName[1].Trim();
                        currentClassifier.Kind = CompoundKind.Struct;
                        setCompound(currentClassifier, false);
                        break;
                    case "Enums":
                        currentClassifier = new DoxClassifier();
                        currentMember = null;
                        currentClassifier.Name = TypeAndName[1].Trim();
                        currentClassifier.Kind = CompoundKind.Enum;
                        setCompound(currentClassifier, false);
                        break;
                    case "Methods":
                        currentMember = new DoxMember();
                        currentMemberKind = MemberKind.Function;
                        break;
                    case "Properties":
                        currentMember = new DoxMember();
                        currentMemberKind = MemberKind.Property;
                        break;
                    case "Fields":
                        currentMember = new DoxMember();
                        currentMemberKind = MemberKind.Variable;
                        break;
                    case "Enum values":
                        currentMember = new DoxMember();
                        currentMemberKind = MemberKind.EnumValue;
                        break;
                }
            }
            return base.VisitHeadingTextElement(context);
        }
        public override object VisitHeadingTextElements(MediaWikiParser.HeadingTextElementsContext context)
        {
            Console.WriteLine("VisitHeadingTextElements: " + context.GetText());
            return base.VisitHeadingTextElements(context);
        }
        public override object VisitHorizontalRule(MediaWikiParser.HorizontalRuleContext context)
        {
            Console.WriteLine("VisitHorizontalRule: " + context.GetText());
            return base.VisitHorizontalRule(context);
        }
        public override object VisitHtmlAttribute(MediaWikiParser.HtmlAttributeContext context)
        {
            Console.WriteLine("VisitHtmlAttribute: " + context.GetText());
            string HtmlAttribute = context.GetText();
            if(HtmlAttribute.Substring(0,3)==" id")
            {
                string id = HtmlAttribute.Substring(HtmlAttribute.IndexOf("\"")+1);
                id = id.Substring(0, id.Length - 1);
                this.identifier = id;

            }
            return base.VisitHtmlAttribute(context);
        }
        public override object VisitHtmlComment(MediaWikiParser.HtmlCommentContext context)
        {
            Console.WriteLine("VisitHtmlComment: " + context.GetText());
            return base.VisitHtmlComment(context);
        }
        public override object VisitHtmlReference(MediaWikiParser.HtmlReferenceContext context)
        {
            Console.WriteLine("VisitHtmlReference: " + context.GetText());
            return base.VisitHtmlReference(context);
        }
        public override object VisitHtmlScript(MediaWikiParser.HtmlScriptContext context)
        {
            Console.WriteLine("VisitHtmlScript: " + context.GetText());
            return base.VisitHtmlScript(context);
        }
        public override object VisitHtmlStyle(MediaWikiParser.HtmlStyleContext context)
        {
            Console.WriteLine("VisitHtmlStyle: " + context.GetText());
            return base.VisitHtmlStyle(context);
        }
        public override object VisitHtmlTag(MediaWikiParser.HtmlTagContext context)
        {
            Console.WriteLine("VisitHtmlTag: " + context.GetText());
            return base.VisitHtmlTag(context);
        }
        public override object VisitHtmlTagClose(MediaWikiParser.HtmlTagCloseContext context)
        {
            Console.WriteLine("VisitHtmlTagClose: " + context.GetText());
            return base.VisitHtmlTagClose(context);
        }
        public override object VisitHtmlTagEmpty(MediaWikiParser.HtmlTagEmptyContext context)
        {
            Console.WriteLine("VisitHtmlTagEmpty: " + context.GetText());
            return base.VisitHtmlTagEmpty(context);
        }
        public override object VisitHtmlTagName(MediaWikiParser.HtmlTagNameContext context)
        {
            Console.WriteLine("VisitHtmlTagName: " + context.GetText());
            return base.VisitHtmlTagName(context);
        }
        public override object VisitHtmlTagOpen(MediaWikiParser.HtmlTagOpenContext context)
        {
            Console.WriteLine("VisitHtmlTagOpen: " + context.GetText());
            return base.VisitHtmlTagOpen(context);
        }
        public override object VisitInlineText(MediaWikiParser.InlineTextContext context)
        {
            Console.WriteLine("VisitInlineText: " + context.GetText());
            return base.VisitInlineText(context);
        }
        public override object VisitInlineTextElement(MediaWikiParser.InlineTextElementContext context)
        {
            Console.WriteLine("VisitInlineTextElement: " + context.GetText());
            return base.VisitInlineTextElement(context);
        }
        public override object VisitInlineTextElements(MediaWikiParser.InlineTextElementsContext context)
        {
            Console.WriteLine("VisitInlineTextElements: " + context.GetText());
            return base.VisitInlineTextElements(context);
        }
        public override object VisitLinkTextElement(MediaWikiParser.LinkTextElementContext context)
        {
            Console.WriteLine("VisitLinkTextElement: " + context.GetText());
            return base.VisitLinkTextElement(context);
        }
        public override object VisitLinkTextElements(MediaWikiParser.LinkTextElementsContext context)
        {
            Console.WriteLine("VisitLinkTextElements: " + context.GetText());
            return base.VisitLinkTextElements(context);
        }
        public override object VisitListItem(MediaWikiParser.ListItemContext context)
        {
            Console.WriteLine("VisitListItem: " + context.GetText());
            return base.VisitListItem(context);
        }
        public override object VisitLinkText(MediaWikiParser.LinkTextContext context)
        {
            Console.WriteLine("VisitLinkText: " + context.GetText());
            return base.VisitLinkText(context);
        }
        public override object VisitNormalListItem(MediaWikiParser.NormalListItemContext context)
        {
            Console.WriteLine("VisitNormalListItem: " + context.GetText());
            return base.VisitNormalListItem(context);
        }
        public override object VisitNoWiki(MediaWikiParser.NoWikiContext context)
        {
            Console.WriteLine("VisitNoWiki: " + context.GetText());
            return base.VisitNoWiki(context);
        }
        public override object VisitParagraph(MediaWikiParser.ParagraphContext context)
        {
            Console.WriteLine("VisitParagraph: " + context.GetText());
            return base.VisitParagraph(context);
        }
        public override object VisitSpaceBlock(MediaWikiParser.SpaceBlockContext context)
        {
            Console.WriteLine("VisitSpaceBlock: " + context.GetText());
            return base.VisitSpaceBlock(context);
        }       
        public override object VisitTable(MediaWikiParser.TableContext context)
        {
            Console.WriteLine("VisitTable: " + context.GetText());
            return base.VisitTable(context);
        }
        public override object VisitTableCaption(MediaWikiParser.TableCaptionContext context)
        {
            Console.WriteLine("VisitTableCaption: " + context.GetText());
            return base.VisitTableCaption(context);
        }
        public override object VisitTableCell(MediaWikiParser.TableCellContext context)
        {
            Console.WriteLine("VisitTableCell: " + context.GetText());
            return base.VisitTableCell(context);
        }
        public override object VisitTableCells(MediaWikiParser.TableCellsContext context)
        {
            Console.WriteLine("VisitTableCells: " + context.GetText());
            return base.VisitTableCells(context);
        }
        public override object VisitTableColumn(MediaWikiParser.TableColumnContext context)
        {
            Console.WriteLine("VisitTableColumn: " + context.GetText());
            return base.VisitTableColumn(context);
        }
        public override object VisitTableFirstRow(MediaWikiParser.TableFirstRowContext context)
        {
            Console.WriteLine("VisitTableFirstRow: " + context.GetText());
            return base.VisitTableFirstRow(context);
        }
        public override object VisitTableHeaderCells(MediaWikiParser.TableHeaderCellsContext context)
        {
            Console.WriteLine("VisitTableHeaderCells: " + context.GetText());
            return base.VisitTableHeaderCells(context);
        }
        public override object VisitTableRow(MediaWikiParser.TableRowContext context)
        {
            Console.WriteLine("VisitTableRow: " + context.GetText());
            return base.VisitTableRow(context);
        }
        public override object VisitTableRows(MediaWikiParser.TableRowsContext context)
        {
            Console.WriteLine("VisitTableRows: " + context.GetText());
            return base.VisitTableRows(context);
        }
        public override object VisitTableSingleCell(MediaWikiParser.TableSingleCellContext context)
        {
            Console.WriteLine("VisitTableSingleCell: " + context.GetText());
            return base.VisitTableSingleCell(context);
        }
        public override object VisitTableSingleHeaderCell(MediaWikiParser.TableSingleHeaderCellContext context)
        {
            Console.WriteLine("VisitTableSingleHeaderCell: " + context.GetText());
            return base.VisitTableSingleHeaderCell(context);
        }
        public override object VisitTextElements(MediaWikiParser.TextElementsContext context)
        {
            Console.WriteLine("VisitTextElements: " + context.GetText());
            return base.VisitTextElements(context);
        }
        public override object VisitTextLine(MediaWikiParser.TextLineContext context)
        {
            Console.WriteLine("VisitTextLine: " + context.GetText());
            return base.VisitTextLine(context);
        }
        public override object VisitWikiExternalLink(MediaWikiParser.WikiExternalLinkContext context)
        {
            Console.WriteLine("VisitWikiExternalLink: " + context.GetText());
            return base.VisitWikiExternalLink(context);
        }
        public override object VisitWikiFormat(MediaWikiParser.WikiFormatContext context)
        {
            Console.WriteLine("VisitWikiFormat: " + context.GetText());
            return base.VisitWikiFormat(context);
        }
        public override object VisitWikiInternalLink(MediaWikiParser.WikiInternalLinkContext context)
        {
            Console.WriteLine("VisitWikiInternalLink: " + context.GetText());
            return base.VisitWikiInternalLink(context);
        }
        public override object VisitWikiLink(MediaWikiParser.WikiLinkContext context)
        {
            Console.WriteLine("VisitWikiLink: " + context.GetText());
            return base.VisitWikiLink(context);
        }
        public override object VisitWikiTemplate(MediaWikiParser.WikiTemplateContext context)
        {
            Console.WriteLine("VisitWikiTemplate: " + context.GetText());
            return base.VisitWikiTemplate(context);
        }
        public override object VisitWikiTemplateParam(MediaWikiParser.WikiTemplateParamContext context)
        {
            Console.WriteLine("VisitWikiTemplateParam: " + context.GetText());
            return base.VisitWikiTemplateParam(context);
        }
    }

}