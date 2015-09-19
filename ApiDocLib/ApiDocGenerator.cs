using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoftwareEngineeringTools.Documentation;

namespace SoftwareEngineeringTools.Documentation
{
    public class ApiDocGenerator
    {
        private IDocumentGenerator dg;
        private int sectionLevel;
        public DocumentTemplate Template { get; private set; }
        public DoxygenModel Model { get; private set; }

        public ApiDocGenerator(IDocumentGenerator documentGenerator, DocumentTemplate template, DoxygenModel model)
        {
            this.dg = documentGenerator;
            this.sectionLevel = 0;
            this.Template = template;
            this.Model = model;
        }

        public void Generate()
        {
            ApiDocTemplateProcessor processor = new ApiDocTemplateProcessor(this.Template, this.Model, this);
            processor.Generate();
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public string NormalizeName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (this.Template.ProgrammingLanguage == ProgrammingLanguage.CSharp ||
                this.Template.ProgrammingLanguage == ProgrammingLanguage.Java)
            {
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
            else
            {
                return name;
            }
        }
        public string GetPackage(string name)
        {
            if (this.Template.ProgrammingLanguage == ProgrammingLanguage.CSharp ||
                this.Template.ProgrammingLanguage == ProgrammingLanguage.Java)
            {
                int index = name.LastIndexOf(":");
                if (index > 0)
                {
                    return name.Substring(0, index);
                }
                else
                {
                    return "";
                }
                //return name.Replace("::", ".");
            }
            else
            {
                return "";
            }
        }

        public void PrintSection(string title, string label)
        {
            dg.BeginSectionTitle(this.sectionLevel, title, label);
            dg.EndSectionTitle();
        }

        public void PrintDocCmd(DocCmd cmd)
        {
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
                        this.NextSectionLevel();
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
                            this.PreviousSectionLevel();
                        }
                    }
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
                            this.Log("Not implemeted Markup: Preformatted");
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
                            this.Log("Not implemeted Markup: Preformatted");
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
                case DocKind.Anchor:
                    DocAnchor docAnchor = cmd as DocAnchor;
                    dg.NewLabel(docAnchor.Id);
                    foreach (var p in docAnchor.Commands)
                    {
                        this.PrintDocCmd(p);
                    }
                    break;
                case DocKind.Reference:
                    DocReference docReference = cmd as DocReference;
                    if (docReference.Compound != null || docReference.Member != null)
                    {
                        switch (docReference.RefKind)
                        {
                            case DocRefKind.Compound:
                                dg.BeginReference(docReference.Compound.Identifier, false);
                                break;
                            case DocRefKind.Member:
                                dg.BeginReference(docReference.Member.Identifier,false);
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
                        else
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
                        this.NextSectionLevel();
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
                            this.PreviousSectionLevel();
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
                    this.NextSectionLevel();
                    try
                    {
                        foreach (var p in docSimpleSect.Items)
                        {
                            this.PrintDocCmd(p);
                        }
                    }
                    finally
                    {
                        this.PreviousSectionLevel();
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

        public void NextSectionLevel()
        {
            this.sectionLevel += 1;
        }

        public void PreviousSectionLevel()
        {
            this.sectionLevel -= 1;
        }

        public void NewLabel(string id)
        {
            dg.NewLabel(id);
        }
    }

    public class ApiDocTemplateProcessor : IApiDocTemplateProcessor
    {
        private DocumentTemplate template;
        private DoxygenModel model;
        private ApiDocGenerator generator;

        private DoxNamespace currentNamespace;
        private DoxClassifier currentClassifier;
        private DoxMember currentMember;

        public ApiDocTemplateProcessor(DocumentTemplate template, DoxygenModel model, ApiDocGenerator generator)
        {
            this.model = model;
            this.generator = generator;
            this.template = template;
        }

        public void Generate()
        {
            this.currentNamespace = null;
            this.currentClassifier = null;
            this.currentMember = null;
            template.Process(this);
        }

        void IApiDocTemplateProcessor.Process(DocumentTemplate template)
        {
            template.ProcessItems(this);
        }

        void IApiDocTemplateProcessor.Process(SectionTemplate template)
        {
            generator.PrintSection(template.Title, null);
            this.generator.NextSectionLevel();
            template.ProcessItems(this);
            this.generator.PreviousSectionLevel();
        }

        private bool HasContent(DoxNamespace ns, NamespaceListTemplate template)
        {
            // TODO
            return ns.Classifiers.Count > 0;
        }

        void IApiDocTemplateProcessor.Process(NamespaceListTemplate template)
        {
            var namespaces =
                from c in this.model.Compounds
                where c.Kind == CompoundKind.Namespace
                orderby c.Name
                select c;
            foreach (var ns in namespaces)
            {
                this.currentNamespace = (DoxNamespace)ns;
                if (this.HasContent(this.currentNamespace, template))
                {
                    string name = generator.NormalizeName(ns.Name);
                    generator.PrintSection("Namespace: " + name, ns.Identifier);
                    this.generator.NextSectionLevel();
                    template.ProcessItems(this);
                    this.generator.PreviousSectionLevel();
                }
                this.currentNamespace = null;
            }
        }

        private bool SelectClassifier(DoxClassifier c, ClassifierListTemplate template)
        {
            if (template is InterfaceListTemplate)
            {
                return c.Kind == CompoundKind.Interface;
            }
            else if (template is ClassListTemplate)
            {
                return c.Kind == CompoundKind.Class;
            }
            else if (template is StructListTemplate)
            {
                return c.Kind == CompoundKind.Struct;
            }
            else if (template is EnumListTemplate)
            {
                return c.Kind == CompoundKind.Enum;
            }
            else
            {
                return c.Kind == CompoundKind.Interface ||
                        c.Kind == CompoundKind.Class ||
                        c.Kind == CompoundKind.Struct ||
                        c.Kind == CompoundKind.Enum;
            }
        }

        void IApiDocTemplateProcessor.Process(ClassifierListTemplate template)
        {
            List<DoxClassifier> classifiers;
            if (this.currentNamespace != null)
            {
                classifiers = this.currentNamespace.Classifiers.ToList();
            }
            else
            {
                classifiers =
                    (from c in this.model.Compounds
                    where c is DoxClassifier
                    select (DoxClassifier)c).ToList();
            }
            int i = 0;
            while (i < classifiers.Count)
            {
                foreach (var ic in classifiers[i].InnerClassifiers)
                {
                    if (!classifiers.Contains(ic)) classifiers.Add(ic);
                }
                ++i;
            }
            classifiers.RemoveAll(c => !this.SelectClassifier(c, template));
            classifiers = classifiers.OrderBy(c => c.Name).ToList();
            foreach (var c in classifiers)
            {
                this.currentClassifier = c;
                string name = generator.NormalizeName(c.Name);
                if (currentNamespace != null)
                {
                    name = generator.NormalizeName(c.Name);
                }
                generator.PrintSection(c.Kind + ": " + name, c.Identifier);                
                this.generator.NextSectionLevel();
                template.ProcessItems(this);
                this.generator.PreviousSectionLevel();
                this.currentClassifier = null;
            }
        }

        void IApiDocTemplateProcessor.Process(InterfaceListTemplate template)
        {
            ((IApiDocTemplateProcessor)this).Process((ClassifierListTemplate)template); 
            
            
        }

        void IApiDocTemplateProcessor.Process(ClassListTemplate template)
        {
            ((IApiDocTemplateProcessor)this).Process((ClassifierListTemplate)template);
        }

        void IApiDocTemplateProcessor.Process(StructListTemplate template)
        {
            ((IApiDocTemplateProcessor)this).Process((ClassifierListTemplate)template);
        }

        void IApiDocTemplateProcessor.Process(EnumListTemplate template)
        {
            ((IApiDocTemplateProcessor)this).Process((ClassifierListTemplate)template);
        }

        private bool SelectMember(DoxMember m, MemberListTemplate template)
        {
            if (template is FieldListTemplate)
            {
                return m.Kind == MemberKind.Variable;
            }
            else if (template is PropertyListTemplate)
            {
                return m.Kind == MemberKind.Property;
            }
            else if (template is MethodListTemplate)
            {
                return m.Kind == MemberKind.Function;
            }
            else if (template is EnumValueListTemplate)
            {
                return m.Kind == MemberKind.EnumValue;
            }
            else
            {
                return m.Kind == MemberKind.Variable ||
                    m.Kind == MemberKind.Property ||
                    m.Kind == MemberKind.Function ||
                    m.Kind == MemberKind.EnumValue ||
                    m.Kind == MemberKind.Enum;
            }
        }

        void IApiDocTemplateProcessor.Process(MemberListTemplate template)
        {
            if (this.currentClassifier == null) return;
            List<DoxMember> members = this.currentClassifier.Members.ToList();
            members.RemoveAll(m => !this.SelectMember(m, template));
            members = members.OrderBy(m => m.Kind).ThenBy(m => m.Name).ToList();
            DocTable dt = new DocTable();
            header_row(dt);
            foreach (var m in members)
            {
                //this.currentMember = m;
                //DocPara dp = new DocPara();
                //DocMarkup dm = new DocMarkup();
                //dm.MarkupKind = DocMarkupKind.Bold;
                //dm.Commands.Add(new DocAnchor() { Id = m.Identifier });
                //dm.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = m.Name });
                //dp.Commands.Add(dm);
                //generator.PrintDocCmd(dp);
                //template.ProcessItems(this);
                //this.currentMember = null;

                DocTable desdt = new DocTable();
                DocTableRow dtr = new DocTableRow();
                desdt.ColCount = 1;
                desdt.RowCount = 2;
                if (members.Count > 0)
                {
                    dt.RowCount = members.Count + 1;
                }
                else
                {
                    break;
                }
                dt.ColCount = 2;
                this.currentMember = m;
                GetTruePropertys(m, dtr, desdt);
                dt.Rows.Add(dtr);
                template.ProcessItems(this);
                this.currentMember = null;
            }
            if (members.Count > 0)
            {
                generator.PrintDocCmd(dt);
                
            }
       }
        

        void IApiDocTemplateProcessor.Process(EnumValueListTemplate template)
        {
            ((IApiDocTemplateProcessor)this).Process((MemberListTemplate)template);
        }

        void IApiDocTemplateProcessor.Process(FieldListTemplate template)
        {
            //((IApiDocTemplateProcessor)this).Process((MemberListTemplate)template);   
            if (this.currentClassifier == null) return;
            List<DoxMember> members = this.currentClassifier.Members.ToList();
            members.RemoveAll(m => !this.SelectMember(m, template));
            members = members.OrderBy(m => m.Kind).ThenBy(m => m.Name).ToList();
            DocTable dt = new DocTable();
            header_row(dt);
            DocTable desdt = new DocTable();
            foreach (var m in members)
            {
                DocTableRow dtr = new DocTableRow();                
                dt.RowCount = members.Count + 1;                
                dt.ColCount = 2;
                this.currentMember = m;
                GetTruePropertys(m, dtr,desdt);
                dt.Rows.Add(dtr);
                template.ProcessItems(this);
                this.currentMember = null;
            }
            if (members.Count > 0)
            {
                generator.PrintDocCmd(dt);
            }
        }

        void IApiDocTemplateProcessor.Process(PropertyListTemplate template)
        {
           // ((IApiDocTemplateProcessor)this).Process((MemberListTemplate)template);
            if (this.currentClassifier == null) return;
            List<DoxMember> members = this.currentClassifier.Members.ToList();
            members.RemoveAll(m => !this.SelectMember(m, template));
            members = members.OrderBy(m => m.Kind).ThenBy(m => m.Name).ToList();
            DocTable dt = new DocTable();
            header_row(dt);
            foreach (var m in members)
            {
                /*this.currentMember = m;
                DocPara dp = new DocPara();
                DocMarkup dm = new DocMarkup();
                dm.MarkupKind = DocMarkupKind.Bold;
                dm.Commands.Add(new DocAnchor() { Id = m.Identifier });
                dm.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = m.Name });
                dp.Commands.Add(dm);
                generator.PrintDocCmd(dp);
                template.ProcessItems(this);
                this.currentMember = null;
            }*/
                DocTable desdt = new DocTable();
                DocTableRow dtr = new DocTableRow();
                desdt.ColCount = 1;
                desdt.RowCount = 2;
                dt.RowCount = members.Count + 1;
                dt.ColCount = 2;
                this.currentMember = m;
                GetTruePropertys(m, dtr, desdt);
                dt.Rows.Add(dtr);
                template.ProcessItems(this);
                this.currentMember = null;
            }
            if (members.Count > 0)
            {
                generator.PrintDocCmd(dt);
                
            }
        }
        void GetTruePropertys(DoxMember m, DocTableRow dtr, DocTable des)
        {            
            DocPara dp = new DocPara();
            DocTableCell dtc = new DocTableCell(); //! Cell of the table
            DocTableRow desdtr = new DocTableRow(); //! First row of the description
            DocTableCell desdtc = new DocTableCell(); //! Cell of the description

            if (m.ProtectionKind != null)
            {
                dp.Commands.Add(new DocText()
                {
                    TextKind = DocTextKind.Plain,
                    Text = m.ProtectionKind.Value.ToString() + ' '
                });
            }
            if(m.New == true)
            {
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = "New " });              
            }
            if (m.Final == true)
            {
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = "Final " });
            }
            if(m.Volatile == true)
            {
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = "Volatile " });
            }
            if(m.VirtualKind != null && m.VirtualKind != VirtualKind.NonVirtual)
            {
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = m.VirtualKind.ToString()+ ' ' });
            }
            if (m.Inline == true && m.Kind != MemberKind.Function)
            {
                 dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = "Inline " });
            }
            if (m.Static == true)
            {
                dp.Commands.Add(new DocText()
                {
                    TextKind = DocTextKind.Plain,
                    Text = "Static "
                }); 
            }            
            if (m.Const == true)
            {
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = "Constant " });                               
            }
            
            if (m.Explicit == true)
            {               
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = "Explicit  "});               
            }

            if(m.Definition != null)
            {
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = m.Definition });
            }
            dtc.Paragraphs.Add(dp); //Add the data of the first columb
            dtr.Cells.Add(dtc);
            DocPara dp_des = new DocPara();
            foreach (var cmd in dp.Commands) 
            {
                dp_des.Commands.Add(cmd);
            }

            dtc = new DocTableCell();
            dp = new DocPara();     

            if (m.Params.Count != 0)
            {
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = m.Name + "(" });
                dp.Commands.Add(new DocAnchor() { Id = m.Identifier });
                
                for (int i = 0; i < m.Params.Count; i++)
                {
                    dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = m.Params[i].Type.Items[0].Text + " " });
                    dp.Commands.Add(new DocText()
                    {
                        TextKind = DocTextKind.Plain,
                        Text = m.Params[i].DeclarationName + " "
                    });
                    if (i != m.Params.Count - 1)
                    {
                        dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = ", " });
                    }
                }
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = ")" });
            }
            else if (m.Kind == MemberKind.Function)
            {
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = m.Name });
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = "()" });
                dp.Commands.Add(new DocAnchor() { Id = m.Identifier });
            }
            else
            {
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = m.Name });
                dp.Commands.Add(new DocAnchor() { Id = m.Identifier });
            }
            foreach (var cmd in dp.Commands)
            {
                dp_des.Commands.Add(cmd);
            }
            dtc.Paragraphs.Add(dp);           
            desdtc.Paragraphs.Add(dp_des);                          
            desdtc.IsHeader = true;
            desdtr.Cells.Add(desdtc);
            des.Rows.Add(desdtr);
           
            dp = new DocPara();
            dp_des = new DocPara();
            desdtc = new DocTableCell();
            desdtr = new DocTableRow();

            if (m.BriefDescription != null && m.BriefDescription.Paragraphs.Count != 0)
            {
                for (int i = 0; i < m.BriefDescription.Paragraphs.Count; i++)
                {
                    dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = "\n" });
                    for (int j = 0; j < m.BriefDescription.Paragraphs[i].Commands.Count; j++)
                    {
                        dp.Commands.Add(m.BriefDescription.Paragraphs[i].Commands[j]);
                    }
                }
                if (m.DetailedDescription != null && m.DetailedDescription.Paragraphs.Count != 0)
                {
                    for (int i = 0; i < m.DetailedDescription.Paragraphs.Count; i++)
                    {
                        for (int j = 0; j < m.DetailedDescription.Paragraphs[i].Commands.Count; j++)
                        {
                            dp_des.Commands.Add(m.DetailedDescription.Paragraphs[i].Commands[j]);
                        }
                    }
                }
            }
            else if (m.DetailedDescription != null && m.DetailedDescription.Paragraphs.Count != 0)
            {
                for (int i = 0; i < m.DetailedDescription.Paragraphs.Count; i++)
                {
                    for (int j = 0; j < m.DetailedDescription.Paragraphs[i].Commands.Count; j++)
                    {
                        if (m.Kind != MemberKind.Function)
                        {
                            dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = "\n" });
                            dp.Commands.Add(m.DetailedDescription.Paragraphs[i].Commands[j]);
                        }
                        dp_des.Commands.Add(m.DetailedDescription.Paragraphs[i].Commands[j]);
                    }
                }
            }
            dtc.Paragraphs.Add(dp);
            dtr.Cells.Add(dtc);
            if (dp_des.Commands.Count == 0)
            {
                des.ColCount = 0;
                return;
            }
            desdtc.Paragraphs.Add(dp_des);
            desdtr.Cells.Add(desdtc);
            des.Rows.Add(desdtr);
            
        }
        void header_row(DocTable dt)
        {
            DocTableRow dtr = new DocTableRow();
            DocTableCell dtc = new DocTableCell();
            dtc.IsHeader = true;
            DocPara dp = new DocPara();
            dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = " " });
            dtc.Paragraphs.Add(dp);
            dtr.Cells.Add(dtc);
            dp = new DocPara();
            dtc = new DocTableCell();
            dtc.IsHeader = true;
            dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = "Name and Description" });
            dtc.Paragraphs.Add(dp);
            dtr.Cells.Add(dtc);
            dt.Rows.Add(dtr);
        }

        void IApiDocTemplateProcessor.Process(MethodListTemplate template)
        {
            //((IApiDocTemplateProcessor)this).Process((MemberListTemplate)template);
            if (this.currentClassifier == null) return;
            List<DoxMember> members = this.currentClassifier.Members.ToList();
            members.RemoveAll(m => !this.SelectMember(m, template));
            members = members.OrderBy(m => m.Kind).ThenBy(m => m.Name).ToList();
            List<DocTable> tables = new List<DocTable>();
            DocPara dpara = new DocPara();
            DocTable dt = new DocTable();
            header_row(dt);
            foreach (var m in members)
            {
                DocTable desdt = new DocTable(); //! DocTable to the description
                desdt.RowCount = 2;
                desdt.ColCount = 1;
                DocTableRow dtr = new DocTableRow();
                if (members.Count > 0)
                {
                    dt.RowCount = members.Count+1;
                }
                else
                {
                    break;
                }
                dt.ColCount = 2;
                this.currentMember = m;
                GetTruePropertys(m, dtr,desdt);
                dt.Rows.Add(dtr);
                if (desdt.ColCount != 0)
                {
                    tables.Add(desdt);
                }
                template.ProcessItems(this);
                this.currentMember = null;

            }
            if (members.Count > 0)
            {
                generator.PrintDocCmd(dt);
                dpara.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = "" });
                generator.PrintDocCmd(dpara);
                System.Threading.Thread.Sleep(100);
                for (int i = 0; i < tables.Count; i++)
                {
                    generator.PrintDocCmd(tables[i]);
                    generator.PrintDocCmd(dpara);
                }
            }
        }

        void IApiDocTemplateProcessor.Process(UmlClassDiagramTemplate template)
        {

        }

        void IApiDocTemplateProcessor.Process(DescriptionTemplate template)
        {
            Description description = null;
            if (this.currentMember != null)
            {
                description = this.currentMember.Description;
            }
            else if (this.currentClassifier != null)
            {
                description = this.currentClassifier.Description;
            }
            else if (this.currentNamespace != null)
            {
                description = this.currentNamespace.Description;
            }
            if (description != null)
            {
                //this.generator.PrintDocCmd(description);
            }
        }

        void IApiDocTemplateProcessor.Process(BriefDescriptionTemplate template)
        {
            Description description = null;
            if (this.currentMember != null)
            {
                description = this.currentMember.BriefDescription;
            }
            else if (this.currentClassifier != null)
            {
                description = this.currentClassifier.BriefDescription;
            }
            else if (this.currentNamespace != null)
            {
                description = this.currentNamespace.BriefDescription;
            }
            if (description != null)
            {
                //this.generator.PrintDocCmd(description);
            }
        }

        void IApiDocTemplateProcessor.Process(TextTemplate template)
        {
            if (template.Text != null)
            {
                this.generator.PrintDocCmd(template.Text);
            }
        }
    }
}
