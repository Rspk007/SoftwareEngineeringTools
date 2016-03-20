using SoftwareEngineeringTools.Documentation;
using SoftwareEngineeringTools.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoftwareEngineeringTools.WikiReader
{
    public class MediaWikiProcessor : MediaWikiParserBaseVisitor<object>
    {
        WikiParser reader;
        DocSect currentSection;
        DocPara currentParagraph;
        DocSect[] allSection;
        DocTable currentTable;
        DocTableRow currentTableRow;
        DocTableCell currentTableCell;
        DocMarkup currentMarkup;
        DocAnchor currentAnchor;
        DocList currentList;
        int currentDepth;
        bool headerCell;
        bool ifCommand = false;
        IfCommand lastIfCommand;
        bool isTrue = false;
        bool markupStart = false;
        bool isTableCommand = false;
        string tableCommandName;
        string tableCommandParameter;
        string tableCommandTemplateParameter;
        string[] parameterSequence;

        private enum parentClass
        {
            NONE,
            SECTION,
            CELL,
            MARKUP,
            ANCHOR,
            LIST
        }
        parentClass lastClass;
        parentClass previousClass;

        public MediaWikiProcessor(WikiParser reader)
        {
            this.reader = reader;
            this.allSection = new DocSect[7];
        }    
   
        private void addElementToLastClass(DocCmd Element)
        {
            switch (this.lastClass)
            {
                case parentClass.ANCHOR:
                    if (string.IsNullOrEmpty(currentSection.Title) && Element.Kind == DocKind.Text)
                    {
                        currentSection.Title = ((DocText)Element).Text;
                    }
                    else
                    {
                        currentAnchor.Commands.Add(Element);
                    }
                    break;
                case parentClass.SECTION:
                    if (string.IsNullOrEmpty(currentSection.Title) && Element.Kind == DocKind.Text)
                    {
                        currentSection.Title = ((DocText)Element).Text;
                    }
                    else
                    {
                        if (currentSection.Paragraphs == null || currentSection.Paragraphs.Count == 0)
                        {
                            currentSection.Paragraphs.Add(new DocPara());
                        }
                        currentSection.Paragraphs.Last().Commands.Add(Element);
                    }
                    break;
                case parentClass.CELL:
                    if (currentTableCell.Paragraphs == null || currentTableCell.Paragraphs.Count == 0)
                    {
                        currentTableCell.Paragraphs.Add(new DocPara());
                    }
                    currentTableCell.Paragraphs.Last().Commands.Add(Element);
                    break;
                case parentClass.MARKUP:
                    currentMarkup.Commands.Add(Element);
                    break;
                case parentClass.NONE:
                    if(currentParagraph == null)
                    {
                        currentParagraph = new DocPara();
                        reader.addPararaphToFirstSection(currentParagraph);                        
                    }
                    currentParagraph.Commands.Add(Element);
                    
                    
                    break;
            }

        }

        private void generateAndLinkAllMissingSection(int depth)
        {
            int i = depth - 1;
            for (; this.allSection[i] == null&& i>0; i--){}
            for(int j = i; j<depth-1; j++)
            {
                this.allSection[j] = new DocSect();
                if(j == 1)
                {
                    reader.addSectionToFirstSection(allSection[j]);
                }
                else
                {
                    allSection[j - 1].Sections.Add(allSection[j]);
                }
            }            
        }

        private void resetAllCurrentVariable()
        {
           currentParagraph = null;
           currentTable = null;
           currentTableRow = null;
           currentTableCell = null;
        }

        private void clearOldSections(int depth)
        {
            if (depth != 0)
            {
                for (int i = depth; allSection[i] != null && i < allSection.Length; i++)
                {
                    allSection[i] = null;
                }
            }
        }

        string RemoveBetween(string s, char begin, char end)
        {
            Regex regex = new Regex(string.Format("\\{0}.*?\\{1}", begin, end));
            return regex.Replace(s, string.Empty);
        }

        public override object VisitHeading(MediaWikiParser.HeadingContext context)
        {
            string depth = context.children.First().GetText();
            //string title = RemoveBetween(RemoveBetween(context.headingText().GetText(), '<', '>'), '[', ']');
            int count = -1;
            foreach (char c in depth)
                if (c == '=') count++;
            resetAllCurrentVariable();
            currentDepth = count;
            clearOldSections(currentDepth);
            currentSection = new DocSect();
            lastClass = parentClass.SECTION;
            if (currentDepth != 0 && allSection[0] != null)
            {
                generateAndLinkAllMissingSection(currentDepth);
                allSection[currentDepth - 1].Sections.Add(currentSection);
                allSection[currentDepth] = currentSection;
            }
            else if (currentDepth == 1 && allSection[0] == null)
            {
                allSection[currentDepth - 1] = new DocSect();
                reader.addSectionToFirstSection(allSection[currentDepth - 1]);
                allSection[currentDepth - 1].Sections.Add(currentSection);
                allSection[currentDepth] = currentSection;
            }
            else
            {
                allSection[0] = currentSection;
                reader.addSectionToFirstSection(currentSection);
            }
            //currentSection.Title = title;          
            currentParagraph = new DocPara();
            currentSection.Paragraphs.Add(currentParagraph);
            return base.VisitHeading(context);
        }
        public override object VisitCellText(MediaWikiParser.CellTextContext context)
        {
            Console.WriteLine("VisitCellText: " + context.GetText());
            string text = RemoveBetween(context.GetText(),'{','}');
            text = text.Replace("}",string.Empty);
            DocText cellText = new DocText { TextKind = DocTextKind.Plain, Text = text };
            if (currentTableCell.Paragraphs.Count == 0)
            {
                currentTableCell.Paragraphs.Add(new DocPara { });
            }
            currentTableCell.Paragraphs.Last().Commands.Add(cellText);
            if (isTableCommand && !currentTableCell.IsHeader)
            {
                string parameter = parameterSequence[currentTableRow.Cells.Count - 1];
                tableCommandParameter = tableCommandParameter.Replace(parameter, text);
            }

            return base.VisitCellText(context);
        }
        public override object VisitCellTextElement(MediaWikiParser.CellTextElementContext context)
        {              
            Console.WriteLine("VisitCellTextElement: " + context.GetText());            
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
            DocText dt = new DocText();
            dt.Text = context.GetText();
            if (!String.IsNullOrWhiteSpace(dt.Text))
            {
                addElementToLastClass(dt);
            }
            return base.VisitHeadingText(context);
        }
        public override object VisitHeadingTextElement(MediaWikiParser.HeadingTextElementContext context)
        {
            Console.WriteLine("VisitHeadingTextElement: " + context.GetText());           
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

        public void createHTMLTag(string HtmlTag)
        {
            HtmlTag = HtmlTag.Substring(1);
            if (HtmlTag.Substring(0, 1) != "/" && HtmlTag.Substring(HtmlTag.Length - 2, 2) != "/>")
            {
                DocHtmlTag currentHtmlTag = new DocHtmlTag();
                currentHtmlTag.isClosing = false;
                //currentParagraph.Commands.Add(currentHtmlTag);
                String[] HtmlTagParts = HtmlTag.Substring(0, HtmlTag.Length - 1).Split(' ');
                currentHtmlTag.TagType = (HtmlTagType)Enum.Parse(typeof(HtmlTagType), HtmlTagParts[0]);
                foreach (var Attribute in HtmlTagParts.Skip(1).ToArray())
                {
                    String[] NameAndValue = Attribute.Split('=');
                    DocIndexEntry currentAttribute = new DocIndexEntry();
                    currentAttribute.PrimaryEntry = NameAndValue[0];
                    currentAttribute.SecondaryEntry = NameAndValue[1].Substring(1, NameAndValue[1].Length - 3);
                    currentHtmlTag.Attributes.Add(currentAttribute);
                }
                previousClass = lastClass;
                bool succesProcessing = tryProcess(currentHtmlTag);
                if (!succesProcessing)
                {
                    addElementToLastClass(currentHtmlTag);
                }

            }
            else if (HtmlTag.Substring(HtmlTag.Length - 2, 2) == "/>")
            {
                DocHtmlTag currentHtmlTag = new DocHtmlTag();
                currentHtmlTag.isClosing = true;
                //currentParagraph.Commands.Add(currentHtmlTag);
                String[] HtmlTagParts = HtmlTag.Substring(0, HtmlTag.Length - 2).Split(' ');
                currentHtmlTag.TagType = (HtmlTagType)Enum.Parse(typeof(HtmlTagType), HtmlTagParts[0]);
                foreach (var Attribute in HtmlTagParts.Skip(1).ToArray())
                {
                    String[] NameAndValue = Attribute.Split('=');
                    DocIndexEntry currentAttribute = new DocIndexEntry();
                    currentAttribute.PrimaryEntry = NameAndValue[0];
                    currentAttribute.SecondaryEntry = NameAndValue[1].Substring(1, NameAndValue[1].Length - 3);
                    currentHtmlTag.Attributes.Add(currentAttribute);
                }
                previousClass = lastClass;
                bool succesProcessing = tryProcess(currentHtmlTag);
                lastClass = previousClass;
                if (!succesProcessing)
                {
                    addElementToLastClass(currentHtmlTag);
                }
            }
            else
            {
                DocHtmlTag currentHtmlTag = new DocHtmlTag();
                currentHtmlTag.isClosing = true;
                lastClass = previousClass;
                //currentParagraph.Commands.Add(currentHtmlTag);
                String[] HtmlTagParts = HtmlTag.Replace("/", "").Replace("\\", "").Split(' ');
                currentHtmlTag.TagType = (HtmlTagType)Enum.Parse(typeof(HtmlTagType), HtmlTagParts[0].Substring(0, HtmlTagParts[0].Length - 1));
                addElementToLastClass(currentHtmlTag);
            }
        }
        public override object VisitHtmlTag(MediaWikiParser.HtmlTagContext context)
        {
            Console.WriteLine("VisitHtmlTag: " + context.GetText());
            String HtmlTag = context.GetText().Substring(1);
            //if (HtmlTag.Substring(0, 1) != "/" && HtmlTag.Substring(HtmlTag.Length - 2, 2) != "/>")
            //{
            //    DocHtmlTag currentHtmlTag = new DocHtmlTag();
            //    currentHtmlTag.isClosing = false;
            //    //currentParagraph.Commands.Add(currentHtmlTag);
            //    String[] HtmlTagParts = HtmlTag.Substring(0,HtmlTag.Length-1).Split(' ');
            //    currentHtmlTag.TagType = (HtmlTagType)Enum.Parse(typeof(HtmlTagType), HtmlTagParts[0]);
            //    foreach (var Attribute in HtmlTagParts.Skip(1).ToArray())
            //    {
            //        String[] NameAndValue = Attribute.Split('=');
            //        DocIndexEntry currentAttribute = new DocIndexEntry();
            //        currentAttribute.PrimaryEntry = NameAndValue[0];
            //        currentAttribute.SecondaryEntry = NameAndValue[1].Substring(1, NameAndValue[1].Length - 3);
            //        currentHtmlTag.Attributes.Add(currentAttribute);
            //    }
            //    previousClass = lastClass;
            //    bool succesProcessing = tryProcess(currentHtmlTag);
            //    if (!succesProcessing)
            //    {
            //        addElementToLastClass(currentHtmlTag);
            //    }
                
            //}
            //else if(HtmlTag.Substring(HtmlTag.Length-2, 2) == "/>")
            //{
            //    DocHtmlTag currentHtmlTag = new DocHtmlTag();
            //    currentHtmlTag.isClosing = true;
            //    //currentParagraph.Commands.Add(currentHtmlTag);
            //    String[] HtmlTagParts = HtmlTag.Substring(0,HtmlTag.Length-2).Split(' ');
            //    currentHtmlTag.TagType = (HtmlTagType)Enum.Parse(typeof(HtmlTagType), HtmlTagParts[0]);
            //    foreach (var Attribute in HtmlTagParts.Skip(1).ToArray())
            //    {
            //        String[] NameAndValue = Attribute.Split('=');
            //        DocIndexEntry currentAttribute = new DocIndexEntry();
            //        currentAttribute.PrimaryEntry = NameAndValue[0];
            //        currentAttribute.SecondaryEntry = NameAndValue[1].Substring(1, NameAndValue[1].Length - 3);
            //        currentHtmlTag.Attributes.Add(currentAttribute);
            //    }
            //    previousClass = lastClass;
            //    bool succesProcessing = tryProcess(currentHtmlTag);
            //    lastClass = previousClass;
            //    if (!succesProcessing)
            //    {
            //        addElementToLastClass(currentHtmlTag);
            //    }
            //}
            //else
            //{
            //    DocHtmlTag currentHtmlTag = new DocHtmlTag();
            //    currentHtmlTag.isClosing = true;
            //    lastClass = previousClass;
            //    //currentParagraph.Commands.Add(currentHtmlTag);
            //    String[] HtmlTagParts = HtmlTag.Replace("/","").Replace("\\","").Split(' ');
            //    currentHtmlTag.TagType = (HtmlTagType)Enum.Parse(typeof(HtmlTagType), HtmlTagParts[0].Substring(0,HtmlTagParts[0].Length-1));
            //    addElementToLastClass(currentHtmlTag);
            //}
            return base.VisitHtmlTag(context);
        }

        private bool tryProcess(DocHtmlTag currentHtmlTag)
        {
            switch (currentHtmlTag.TagType)
            {
               case HtmlTagType.b:
               case HtmlTagType.strong:
                    currentMarkup = new DocMarkup();
                    currentMarkup.MarkupKind = DocMarkupKind.Bold;                    
                    //DocCmd lastcommand = currentSection.Paragraphs.Last().Commands.Last();
                    //currentSection.Paragraphs.Last().Commands.Remove(lastcommand);
                    //currentMarkup.Commands.Add(lastcommand);
                    addElementToLastClass(currentMarkup);
                    lastClass = parentClass.MARKUP;
                    return true;
                case HtmlTagType.bdi:
                    return true;
                case HtmlTagType.p:
                case HtmlTagType.br:
                    currentParagraph = new DocPara();
                    addElementToLastClass(currentParagraph);
                    return true;
                case HtmlTagType.center:
                    currentMarkup = new DocMarkup();
                    currentMarkup.MarkupKind = DocMarkupKind.Center;
                    //lastcommand = currentSection.Paragraphs.Last().Commands.Last();
                    //currentSection.Paragraphs.Last().Commands.Remove(lastcommand);
                    //currentMarkup.Commands.Add(lastcommand);
                    addElementToLastClass(currentMarkup);
                    lastClass = parentClass.MARKUP;
                    return true;
                case HtmlTagType.citre:
                case HtmlTagType.em:
                case HtmlTagType.i:
                case HtmlTagType.var:
                    currentMarkup = new DocMarkup();
                    currentMarkup.MarkupKind = DocMarkupKind.Emphasis;
                    //lastcommand = currentSection.Paragraphs.Last().Commands.Last();
                    //currentSection.Paragraphs.Last().Commands.Remove(lastcommand);
                    //currentMarkup.Commands.Add(lastcommand);
                    addElementToLastClass(currentMarkup);
                    lastClass = parentClass.MARKUP;
                    return true;
                case HtmlTagType.pre:
                    currentMarkup = new DocMarkup();
                    currentMarkup.MarkupKind = DocMarkupKind.Preformatted;
                    //lastcommand = currentSection.Paragraphs.Last().Commands.Last();
                    //currentSection.Paragraphs.Last().Commands.Remove(lastcommand);
                    //currentMarkup.Commands.Add(lastcommand);
                    addElementToLastClass(currentMarkup);
                    lastClass = parentClass.MARKUP;
                    return true;                    
                case HtmlTagType.small:
                    currentMarkup = new DocMarkup();
                    currentMarkup.MarkupKind = DocMarkupKind.Small;
                    //lastcommand = currentSection.Paragraphs.Last().Commands.Last();
                    //currentSection.Paragraphs.Last().Commands.Remove(lastcommand);
                    //currentMarkup.Commands.Add(lastcommand);
                    addElementToLastClass(currentMarkup);
                    lastClass = parentClass.MARKUP;
                    return true; 
                case HtmlTagType.sub:
                    currentMarkup = new DocMarkup();
                    currentMarkup.MarkupKind = DocMarkupKind.SubScript;
                    //lastcommand = currentSection.Paragraphs.Last().Commands.Last();
                    //currentSection.Paragraphs.Last().Commands.Remove(lastcommand);
                    //currentMarkup.Commands.Add(lastcommand);
                    addElementToLastClass(currentMarkup);
                    lastClass = parentClass.MARKUP;
                    return true; 
                case HtmlTagType.sup:
                    currentMarkup = new DocMarkup();
                    currentMarkup.MarkupKind = DocMarkupKind.SuperScript;
                    //lastcommand = currentSection.Paragraphs.Last().Commands.Last();
                    //currentSection.Paragraphs.Last().Commands.Remove(lastcommand);
                    //currentMarkup.Commands.Add(lastcommand);
                    addElementToLastClass(currentMarkup);
                    lastClass = parentClass.MARKUP;
                    return true; 
                case HtmlTagType.div:
                    currentAnchor = new DocAnchor();
                    try 
                    { 
                    currentAnchor.Id = currentHtmlTag.Attributes.Single(a => a.PrimaryEntry.Equals("id")).SecondaryEntry;
                    }
                    catch(InvalidOperationException)
                    {
                        return false;
                    }
                    addElementToLastClass(currentAnchor);
                    lastClass = parentClass.ANCHOR;
                    return true;
                default:
                    return false;
            }
        }
        public override object VisitHtmlTagClose(MediaWikiParser.HtmlTagCloseContext context)
        {
            Console.WriteLine("VisitHtmlTagClose: " + context.GetText());
            string HtmlTag = context.GetText().Replace("<",string.Empty).Replace(">",string.Empty);
            DocHtmlTag currentHtmlTag = new DocHtmlTag();
            currentHtmlTag.isClosing = true;
            String[] HtmlTagParts = HtmlTag.Substring(1, HtmlTag.Length-1).Split(' ');
            currentHtmlTag.TagType = (HtmlTagType)Enum.Parse(typeof(HtmlTagType), HtmlTagParts[0]);
            foreach (var Attribute in HtmlTagParts.Skip(1).ToArray())
            {
                String[] NameAndValue = Attribute.Split('=');
                DocIndexEntry currentAttribute = new DocIndexEntry();
                currentAttribute.PrimaryEntry = NameAndValue[0];
                currentAttribute.SecondaryEntry = NameAndValue[1].Substring(1, NameAndValue[1].Length - 3);
                currentHtmlTag.Attributes.Add(currentAttribute);
            }
            previousClass = lastClass;
            bool succesProcessing = tryProcess(currentHtmlTag);
            lastClass = previousClass;
            if (!succesProcessing)
            {
                addElementToLastClass(currentHtmlTag);
            }
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
            string HtmlTag = context.GetText().Replace("<", string.Empty).Replace(">", string.Empty);
            DocHtmlTag currentHtmlTag = new DocHtmlTag();
            currentHtmlTag.isClosing = false;
            String[] HtmlTagParts = HtmlTag.Split(' ');
            currentHtmlTag.TagType = (HtmlTagType)Enum.Parse(typeof(HtmlTagType), HtmlTagParts[0]);
            foreach (var Attribute in HtmlTagParts.Skip(1).ToArray())
            {
                String[] NameAndValue = Attribute.Split('=');
                DocIndexEntry currentAttribute = new DocIndexEntry();
                currentAttribute.PrimaryEntry = NameAndValue[0];
                currentAttribute.SecondaryEntry = NameAndValue[1].Substring(1, NameAndValue[1].Length - 3);
                currentHtmlTag.Attributes.Add(currentAttribute);
            }
            previousClass = lastClass;
            bool succesProcessing = tryProcess(currentHtmlTag);
            if (!succesProcessing)
            {
                addElementToLastClass(currentHtmlTag);
            }
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
            string text = context.GetText();
            if (text[0] != '\'' && text[0] != '{' && text[0] != '[')
            {
                DocText dt = new DocText() { TextKind = DocTextKind.Plain, Text = text };
                addElementToLastClass(dt);
            }            
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
            if(lastClass != parentClass.LIST)
            {
                currentList = new DocList();
                addElementToLastClass(currentList);
                lastClass = parentClass.LIST;
            }
            DocListItem dli = new DocListItem();
            currentList.Items.Add(dli);
            String listItem = context.GetText();
            listItem = listItem.Replace("*","").Replace("#","");
            DocPara listPara = new DocPara();
            DocText dt = new DocText();
            dt.Text = listItem;
            listPara.Commands.Add(dt);
            dli.Paragraphs.Add(listPara);
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
             DocHtmlTag currentHtmlTag = new DocHtmlTag();
             currentHtmlTag.isClosing = false;
             currentHtmlTag.TagType = HtmlTagType.nowiki;
             addElementToLastClass(currentHtmlTag);
             string insideText = context.GetText().Replace("<nowiki>", "").Replace("</nowiki>", "");
             DocText text = new DocText();
             text.Text = insideText;
             text.TextKind = DocTextKind.Plain;
             addElementToLastClass(text);
             currentHtmlTag = new DocHtmlTag();
             currentHtmlTag.isClosing = true;
             currentHtmlTag.TagType = HtmlTagType.nowiki;
             addElementToLastClass(currentHtmlTag);
            return base.VisitNoWiki(context);
        }
        public override object VisitParagraph(MediaWikiParser.ParagraphContext context)
        {
            Console.WriteLine("VisitParagraph: " + context.GetText());
            lastClass = parentClass.SECTION;
            if (currentParagraph.Commands.Count>0)
            {
                currentParagraph = new DocPara();
                addElementToLastClass(currentParagraph);
            }
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
            //if(currentTable != null)
            //{
            currentTable = new DocTable();
            currentParagraph.Commands.Add(currentTable);
            //}
            
            lastClass = parentClass.CELL;
            headerCell = false;
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
            currentTableCell = new DocTableCell();
            currentTableCell.IsHeader = headerCell;            
            currentTableRow.Cells.Add(currentTableCell);
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
            string rowdata = context.GetText();
            int numberOfColumn = rowdata.Split(new[] { "!!" }, StringSplitOptions.None).Count();
            int numberOfColumn2 = rowdata.Replace("!!", string.Empty).Split(new[] { "!" }, StringSplitOptions.None).Count()-1;
            if (numberOfColumn2 > numberOfColumn)
                numberOfColumn = numberOfColumn2;
            currentTable.ColCount = numberOfColumn;
            currentTable.RowCount = 1;
            DocTableRow dtr = new DocTableRow();
            currentTableRow = dtr;
            currentTable.Rows.Add(currentTableRow);
            headerCell = true;
            return base.VisitTableFirstRow(context);
        }
        public override object VisitTableHeaderCells(MediaWikiParser.TableHeaderCellsContext context)
        {
            Console.WriteLine("VisitTableHeaderCells: " + context.GetText());
            return base.VisitTableHeaderCells(context);
        }

        public void createCommand()
        {
            Command currentTableCommand = new Command();
            currentTableCommand.commandName = tableCommandName;
            currentTableCommand.parameter = tableCommandParameter;
            currentTableRow.Cells.Last().Paragraphs.Last().Commands.Add(currentTableCommand);
        }

        public override object VisitTableRow(MediaWikiParser.TableRowContext context)
        {
            Console.WriteLine("VisitTableRow: " + context.GetText());
            if (tableCommandParameter != null)
            {
                createCommand();
            }
            tableCommandParameter = tableCommandTemplateParameter;

            headerCell = false;
            currentTable.RowCount = currentTable.RowCount+1;
            DocTableRow dtr = new DocTableRow();
            currentTableRow = dtr;
            currentTable.Rows.Add(currentTableRow);           
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
            if(markupStart)
            {
                markupStart = false;
                lastClass = previousClass;
            }
            else
            {
                markupStart = true;
                switch (context.GetText().Length)
                {
                    case 2:
                        DocMarkup dm = new DocMarkup();
                        dm.MarkupKind = DocMarkupKind.Emphasis;
                        addElementToLastClass(dm);
                        currentMarkup = dm;
                        break;
                    case 3:
                        dm = new DocMarkup();
                        dm.MarkupKind = DocMarkupKind.Bold;
                        addElementToLastClass(dm);
                        currentMarkup = dm;
                        break;
                    case 5:
                        dm = new DocMarkup();
                        dm.MarkupKind = DocMarkupKind.Bold;
                        DocMarkup dm2 = new DocMarkup();
                        dm.MarkupKind = DocMarkupKind.Emphasis;
                        dm.Commands.Add(dm2);
                        addElementToLastClass(dm);
                        currentMarkup = dm2;
                        break;
                    default:
                        break;
                }
                previousClass = lastClass;
                lastClass = parentClass.MARKUP;
            }            
            return base.VisitWikiFormat(context);
        }
        
        public override object VisitWikiInternalLink(MediaWikiParser.WikiInternalLinkContext context)
        {
            Console.WriteLine("VisitWikiInternalLink: " + context.GetText());
            string linkString = context.GetText().Replace("[", string.Empty).Replace("]", string.Empty);
            string[] linkData = linkString.Split('|');
            if (linkData.Count() == 1)
            {
                DocReference currentReference = new DocReference();
                currentReference.referenceID = linkData[0];
                currentReference.RefKind = DocRefKind.CustomID;
                addElementToLastClass(currentReference);
            }
            else
            {
                DocReference currentReference = new DocReference();
                currentReference.referenceID = linkData[0];
                DocText dt = new DocText();
                dt.Text = linkData[1];
                currentReference.Commands.Add(dt);
                currentReference.RefKind = DocRefKind.CustomID;
                addElementToLastClass(currentReference);
            }
            return base.VisitWikiInternalLink(context);
        }

        public void setOutput(Command newCommand)
        {
            Dictionary<string, string> outputDatas = new Dictionary<string, string>();
            foreach (var variable in newCommand.parameter.Split(','))
            {
                string paramName = variable.Split('=')[0].ToLower();
                string paramValue = variable.Split('=')[1];
                outputDatas.Add(paramName, paramValue);
            }
            string type;
            outputDatas.TryGetValue("type", out type);
            string path;
            outputDatas.TryGetValue("path", out path);
            DocumentGenerator newGenerator;
            switch (type.ToLower())
            {
                case "word":
                case "ms word":
                case "microsoft word":
                    string visible;
                    string template;
                    outputDatas.TryGetValue("visible", out visible);
                    outputDatas.TryGetValue("template", out template);

                    if (visible != null && template != null)
                    {
                        bool showWord;
                        if (visible == "1")
                        {
                            showWord = true;
                        }
                        else
                        {
                            showWord = false;
                        }
                        try
                        {
                            newGenerator = new WordGenerator(path, showWord, (WordGenerator.WordTemplate)Enum.Parse(typeof(WordGenerator.WordTemplate), template));
                        }
                        catch (Exception)
                        {
                            newGenerator = new WordGenerator(path, showWord);
                        }
                    }
                    else if (visible != null)
                    {
                        bool showWord;
                        if (visible == "1")
                        {
                            showWord = true;
                        }
                        else
                        {
                            showWord = false;
                        }
                        newGenerator = new WordGenerator(path, showWord);
                    }
                    else
                    {
                        newGenerator = new WordGenerator(path);
                    }
                    this.reader.setDocumentGenerator(newGenerator);
                    break;
                case "latex":
                    newGenerator = new LatexGenerator(path);
                    this.reader.setDocumentGenerator(newGenerator);
                    break;
                case "wiki":
                case "wikipedia":
                    newGenerator = new WikiGenerator(path);
                    this.reader.setDocumentGenerator(newGenerator);
                    break;
                case "html":
                case "web":
                    string generateMode;
                    string css;
                    outputDatas.TryGetValue("generateMode", out generateMode);
                    outputDatas.TryGetValue("css", out css);
                    if (generateMode != null && css != null)
                    {
                        try
                        {
                            newGenerator = new HTMLGenerator(path, (HTMLGenerator.GenerateMode)Enum.Parse(typeof(HTMLGenerator.GenerateMode), generateMode), css);
                        }
                        catch (Exception)
                        {
                            newGenerator = new HTMLGenerator(path, HTMLGenerator.GenerateMode.AllInOne, css);
                        }
                    }
                    else
                    {
                        newGenerator = new HTMLGenerator(path, HTMLGenerator.GenerateMode.AllInOne, "");
                    }
                    this.reader.setDocumentGenerator(newGenerator);
                    break;
            }
            }

        public override object VisitWikiLink(MediaWikiParser.WikiLinkContext context)
        {
            Console.WriteLine("VisitWikiLink: " + context.GetText());
            return base.VisitWikiLink(context);
        }
        public override object VisitWikiTemplate(MediaWikiParser.WikiTemplateContext context)
        {
            Console.WriteLine("VisitWikiTemplate: " + context.GetText());
            string item = context.GetText();
            string commandString = item.Substring(2, item.Length - 4);
            if (isTableCommand && commandString != "tableend")
            {
                for (int i = 0; i < parameterSequence.Length; i++)
                {
                    if (string.IsNullOrEmpty(parameterSequence[i]))
                    {
                        parameterSequence[i] = commandString;
                        break;
                    }
                }
            }
            else if(isTableCommand)
            {
                createCommand();
                isTableCommand = false;
            }
            else
            {
                if (commandString.Split('(').Length == 1)
                {
                    string paramName = commandString.Split('=')[0];
                    string paramValue = commandString.Split('=')[1];
                    Command.variables.Add(paramName, paramValue);
                }
                else
                {
                    Command newCommand = new Command();
                    newCommand.commandName = commandString.Split('(')[0];
                    newCommand.parameter = commandString.Remove(0, commandString.Split('(')[0].Length+1);
                    newCommand.parameter = newCommand.parameter.Substring(0, newCommand.parameter.Length - 1);
                    if (newCommand.commandName.ToLower() == "insert")
                    {
                        DocImage dImage = new DocImage();
                        foreach (var variable in newCommand.parameter.Split(','))
                        {
                            string paramName = variable.Split('=')[0].ToLower();
                            string paramValue = variable.Split('=')[1];
                            switch (paramName)
                            {
                                case "filepath":
                                case "path":
                                    dImage.Path = paramValue;
                                    break;
                                case "width":
                                    dImage.Width = paramValue;
                                    break;
                                case "height":
                                    dImage.Height = paramValue;
                                    break;
                                default:
                                    dImage.Name = paramValue;
                                    break;
                            }
                        }
                        addElementToLastClass(dImage);
                    }
                    else if (newCommand.commandName.ToLower() == "output")
                    {
                        setOutput(newCommand);
                    }
                    else if (newCommand.commandName.ToLower() == "tablestart")
                    {
                        tableCommandName = newCommand.parameter.Split('(')[0];
                        tableCommandTemplateParameter = newCommand.parameter.Split('(')[1];
                        tableCommandTemplateParameter = tableCommandTemplateParameter.Substring(0, tableCommandTemplateParameter.Length - 1);
                        parameterSequence = new string[tableCommandTemplateParameter.Split(',').Length];
                        isTableCommand = true;
                    }
                    else if (newCommand.commandName.ToLower() == "if")
                    {
                        IfCommand currentIfCommand = new IfCommand();
                        DecisionCommand currentDecisionCommand = new DecisionCommand();
                        Dictionary<string, string> outputDatas = new Dictionary<string, string>();
                        string insideParamater = newCommand.parameter.Split('(')[1];
                        insideParamater = insideParamater.Substring(0, insideParamater.Length - 1);
                        foreach (var variable in insideParamater.Split(','))
                        {
                            string paramName = variable.Split('=')[0].ToLower();
                            string paramValue = variable.Split('=')[1];
                            Command.variables.Add(paramName, paramValue);
                        }
                        currentIfCommand.decisionCommand = currentDecisionCommand;
                        if (ifCommand == false)
                        {
                            ifCommand = true;
                            isTrue = true;
                            lastIfCommand = currentIfCommand;
                            addElementToLastClass(currentIfCommand);
                        }
                        else
                        {
                            currentIfCommand.parent = lastIfCommand;
                            if (isTrue)
                            {
                                lastIfCommand.trueCommand.Add(currentIfCommand);
                            }
                            else
                            {
                                lastIfCommand.falseCommand.Add(currentIfCommand);
                            }
                            lastIfCommand = currentIfCommand;
                        }

                    }
                    else if (ifCommand == true && newCommand.CommandName != Command.commandType.ELSE && newCommand.CommandName != Command.commandType.ENDIF)
                    {

                        if (isTrue)
                        {
                            lastIfCommand.trueCommand.Add(newCommand);
                        }
                        else
                        {
                            lastIfCommand.falseCommand.Add(newCommand);
                        }

                    }
                    else if (ifCommand == true && newCommand.CommandName == Command.commandType.ELSE)
                    {
                        isTrue = false;
                    }
                    else if (ifCommand == true && newCommand.CommandName == Command.commandType.ENDIF)
                    {
                        if (lastIfCommand.parent == null)
                        {
                            ifCommand = false;
                        }
                        else
                        {
                            lastIfCommand = lastIfCommand.parent;

                        }
                    }
                    else
                    {
                        addElementToLastClass(newCommand);
                    }
                }
            }
            return base.VisitWikiTemplate(context);
        }
        public override object VisitWikiTemplateParam(MediaWikiParser.WikiTemplateParamContext context)
        {
            Console.WriteLine("VisitWikiTemplateParam: " + context.GetText());
            return base.VisitWikiTemplateParam(context);
        }
    }
}