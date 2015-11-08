using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareEngineeringTools.Documentation
{
    public class DocPrinter
    {
        private IDocumentGenerator dg;
        private int sectionLevel;        
        public DoxygenModel Model { get; private set; }
        private int rowindex;
        private DocTable currentTable = null;
        private DocSect currentsection = null;
        public DocPrinter(IDocumentGenerator generator)
        {
            this.dg = generator;
            this.sectionLevel = 0;
        }
        public DocPrinter (IDocumentGenerator generator, DoxygenModel model)
        {
            this.dg = generator;
            this.Model = model;
            this.sectionLevel = 0;
        }

        public void print()
        {
            this.printNameScapes(true);
        }

        public void printNameScapes(Boolean detailed, List<Compound> inList = null)
        {
            List<Compound> NameSpaces;
            if (inList == null)
            {
                NameSpaces = Model.Compounds.Where(comp => comp.Kind == CompoundKind.Namespace).OrderBy(ns => ns.Name).ToList();

            }
            else
            {
                NameSpaces = inList;
            }
            DocSect ds = new DocSect();
            DocTable dt = new DocTable();
            this.currentsection = ds;
            if (detailed == false)
            {                
                ds.Title = "Namespaces: ";
                ds.Identifier = "";
                DocPara dp = new DocPara();
                ds.Paragraphs.Add(dp);
                dt.ColCount = 2;
                dt.RowCount = NameSpaces.Count + 1;
                this.header_row(dt);
                this.currentTable = dt;
            }            
            foreach (DoxNamespace Namespace in NameSpaces)
            {
                if (Namespace.Classifiers.Count > 0)
                {
                    this.printNameSpace(Namespace, detailed);
                }
            }
            if(detailed == false)
            {                
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                ds.Paragraphs.Add(dp);                
                this.currentTable = null;
            }
            this.PrintDocCmd(ds);
        }

        public void printNameSpace(DoxNamespace ns, Boolean detailed)
        {
            if (detailed == true)
            {
                string name = this.NormalizeName(ns.Name);
                DocSect parentsec = this.currentsection;
                DocSect ds = new DocSect();
                ds.Title = "Namespace: " + name;
                ds.Identifier = ns.Identifier;                
                if (ns.BriefDescription != null && ns.BriefDescription.Paragraphs.Count > 0)
                {
                    ds.Paragraphs.AddRange(ns.BriefDescription.Paragraphs);                    
                }
                else if(ns.Description != null)
                {
                    ds.Paragraphs.AddRange(ns.Description.Paragraphs);
                }
                this.currentsection = ds;

                this.printClasses(detailed, ns);
                this.printInterfaces(detailed, ns);
                this.printStructs(detailed, ns);
                this.printEnums(detailed, ns);

                parentsec.Sections.Add(ds);                
            }
            else
            {
                DocTableRow dtr = new DocTableRow();
                DocTableCell dtc = new DocTableCell();
                DocPara dp = new DocPara();
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = ns.Name });
                dtc.Paragraphs.Add(dp);
                dtr.Cells.Add(dtc);
                dp = new DocPara();
                dtc = new DocTableCell();
                if (ns.BriefDescription != null && ns.BriefDescription.Paragraphs.Count > 0)
                {
                    dtc.Paragraphs.AddRange(ns.BriefDescription.Paragraphs);
                }
                else if(ns.Description != null)
                {
                    dtc.Paragraphs.AddRange(ns.Description.Paragraphs);
                }
                dtc.Paragraphs.Add(dp);
                dtr.Cells.Add(dtc);
                this.currentTable.Rows.Add(dtr);
            }
        }

        public void printClassifiers(bool detailed, DoxNamespace ns = null, List<DoxClassifier> Inclassifiers = null)
        {
            List<DoxClassifier> classifiers = null;
            if (Inclassifiers == null && ns != null)
            {
                classifiers = ns.Classifiers.OrderBy(c => c.Name).ToList();
            }
            else if(Inclassifiers != null)
            {
                classifiers = Inclassifiers;
            }
            else
            {
                this.Log("Error DPrint-0: printClassifiers method should get a Namespace or list of classifiers");
            }
            DocSect parentSect = this.currentsection;
            DocSect ds = new DocSect();
            DocTable dt = new DocTable();
            if (detailed == false)
            {
                ds.Title = "Classifiers: ";
                ds.Identifier = "";
                DocPara dp = new DocPara();
                ds.Paragraphs.Add(dp);
                dt.ColCount = 2;
                dt.RowCount = classifiers.Count + 1;
                this.header_row(dt);
                this.currentTable = dt;
            }     
            foreach (DoxClassifier classifier in classifiers.Where(classifier => classifier.Members.Count > 0))
            {
                printClassifier(classifier, detailed);

            }
            if (detailed == false)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                ds.Paragraphs.Add(dp);
                if(parentSect != null)
                {
                    parentSect.Sections.Add(ds);
                    this.currentsection = parentSect;
                }
                else
                {
                    this.PrintDocCmd(ds);
                }
                this.currentTable = null;
            }

        }

        public void printClassifier (DoxClassifier dc, bool detailed)
        {
           if (detailed == true)
            {
                DocSect parentSect = this.currentsection;
                string name = this.NormalizeName(dc.Name);
                DocSect ds = new DocSect();
                ds.Title = dc.Kind + ": " + name;
                ds.Identifier = dc.Identifier;
                
                // Leírás         
                if (dc.BriefDescription != null && dc.BriefDescription.Paragraphs.Count > 0)
                {
                    ds.Paragraphs.AddRange(dc.BriefDescription.Paragraphs);                    
                }
                else if(dc.Description != null)
                {
                    ds.Paragraphs.AddRange(dc.Description.Paragraphs);
                }
                this.currentsection = ds;
                printFields(detailed,dc);
                printProperties(detailed, dc);
                printMethods(detailed, dc);
                printEnumValues(detailed, dc);
                if (parentSect != null)
                {
                    parentSect.Sections.Add(ds);
                    this.currentsection = parentSect;
                }
                else
                {
                    this.PrintDocCmd(ds);
                }
            }
            else
            {
                DocTableRow dtr = new DocTableRow();
                DocTableCell dtc = new DocTableCell();
                DocPara dp = new DocPara();
                dp.Commands.Add(new DocText() { TextKind = DocTextKind.Plain, Text = dc.Name });
                dtc.Paragraphs.Add(dp);
                dtr.Cells.Add(dtc);
                dp = new DocPara();
                dtc = new DocTableCell();
                if (dc.BriefDescription != null && dc.BriefDescription.Paragraphs.Count > 0)
                {
                    dtc.Paragraphs.AddRange(dc.BriefDescription.Paragraphs);
                }
                else if( dc.Description != null)
                {
                    dtc.Paragraphs.AddRange(dc.Description.Paragraphs);
                }
                dtc.Paragraphs.Add(dp);
                dtr.Cells.Add(dtc);
                this.currentTable.Rows.Add(dtr);
            }
        }

        public void printClasses(bool detailed, DoxNamespace ns = null, List<DoxClassifier> Inclasses = null)
        {
            List<DoxClassifier> classes = null;
            if (Inclasses == null && ns != null)
            {
                classes = ns.Classifiers.Where(dc => dc.Kind == CompoundKind.Class && dc.Members.Count > 0).OrderBy(c => c.Name).ToList();
            }
            else if (Inclasses != null)
            {
                classes = Inclasses;
            }
            else
            {
                this.Log("Error DPrint-1: printClasses method should get a Namespace or list of classifiers");
            }
            DocSect parentSect = this.currentsection;
            DocSect ds = new DocSect();
            DocTable dt = new DocTable();
            if (detailed == false)
            {
                ds.Title = "Classes: ";
                ds.Identifier = "";
                DocPara dp = new DocPara();
                ds.Paragraphs.Add(dp);
                dt.ColCount = 2;
                dt.RowCount = classes.Count + 1;
                this.header_row(dt);
                this.currentTable = dt;
            }
            foreach (DoxClass dc in classes)
            {
                if (dc.Members.Count > 0)
                {
                    printClassifier(dc, detailed);
                }
            }
            if (detailed == false)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                ds.Paragraphs.Add(dp);
                if (parentSect != null)
                {
                    parentSect.Sections.Add(ds);
                    this.currentsection = parentSect;
                }
                else
                {
                    this.PrintDocCmd(ds);
                }
                this.currentTable = null;
            }
        }

        public void printClass(DoxClass dc, bool detailed)
        {
            DocSect dsec = new DocSect();
            DocTable dt = new DocTable();
            bool endOfTable = true;
            if (detailed == false)
            {
                if (this.currentTable == null)
                {
                    dt.ColCount = 2;
                    dt.RowCount = 2;
                    this.header_row(dt);
                }
                else
                {
                    dt = this.currentTable;
                    endOfTable = false;
                }
                DocPara dp = new DocPara();
                dsec.Paragraphs.Add(dp);
                this.currentTable = dt;
            }
            printClassifier(dc, detailed);
            if (detailed == false && endOfTable == true)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                dsec.Paragraphs.Add(dp);
                this.PrintDocCmd(dsec);
                this.currentTable = null;

            }            
           
        }

        public void printInterfaces(bool detailed, DoxNamespace ns=null, List<DoxClassifier> Ininterfaces=null)
        {
            List<DoxClassifier> interfaces = null;
            if (Ininterfaces == null && ns != null)
            {
                interfaces = ns.Classifiers.Where(dc => dc.Kind == CompoundKind.Interface && dc.Members.Count > 0).OrderBy(c => c.Name).ToList();
            }
            else if(Ininterfaces != null)
            {
                interfaces = Ininterfaces;
            }
            else
            {
                this.Log("Error DPrint-2: printInterfaces method should get a Namespace or list of classifiers");
            }
            DocSect parentSect = this.currentsection;
            DocSect ds = new DocSect();
            DocTable dt = new DocTable();
            if (detailed == false)
            {
                ds.Title = "Interfaces: ";
                ds.Identifier = "";
                DocPara dp = new DocPara();
                ds.Paragraphs.Add(dp);
                dt.ColCount = 2;
                dt.RowCount = interfaces.Count + 1;
                this.header_row(dt);
                this.currentTable = dt;
            }
            foreach (DoxInterface di in interfaces)
            {
                if (di.Members.Count > 0)
                {
                    printClassifier(di, detailed);
                }
            }
            if (detailed == false)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                ds.Paragraphs.Add(dp);
                if (parentSect != null)
                {
                    parentSect.Sections.Add(ds);
                    this.currentsection = parentSect;
                }
                else
                {
                    this.PrintDocCmd(ds);
                }
                this.currentTable = null;
            }
            
        }

        public void printInterface(DoxInterface di, bool detailed)
        {
            DocSect dsec = new DocSect();
            DocTable dt = new DocTable();
            bool endOfTable = true;
            if (detailed == false)
            {
                if (this.currentTable == null)
                {
                    dt.ColCount = 2;
                    dt.RowCount = 2;
                    this.header_row(dt);
                }
                else
                {
                    dt = this.currentTable;
                    endOfTable = false;
                }
                DocPara dp = new DocPara();
                dsec.Paragraphs.Add(dp);
                this.currentTable = dt;
            }
            printClassifier(di, detailed);
            if (detailed == false && endOfTable == true)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                dsec.Paragraphs.Add(dp);
                this.PrintDocCmd(dsec);
                this.currentTable = null;

            }  
            
        }

        public void printStructs(bool detailed, DoxNamespace ns=null, List<DoxClassifier> Instructs=null)
        {
            List<DoxClassifier> structs = null;
            if (Instructs == null && ns != null)
            {
                structs = ns.Classifiers.Where(dc => dc.Kind == CompoundKind.Struct && dc.Members.Count > 0).OrderBy(c => c.Name).ToList();
            }
            else if (Instructs != null)
            {
                structs = Instructs;
            }
            else
            {
                this.Log("Error DPrint-3: printStructs method should get a Namespace or list of classifiers");
            }            
            DocSect parentSect = this.currentsection;
            DocSect ds = new DocSect();
            DocTable dt = new DocTable();
            if (detailed == false)
            {
                ds.Title = "Structs: ";
                ds.Identifier = "";
                DocPara dp = new DocPara();
                ds.Paragraphs.Add(dp);
                dt.ColCount = 2;
                dt.RowCount = structs.Count + 1;
                this.header_row(dt);
                this.currentTable = dt;
            }
            foreach (DoxStruct dst in structs)
            {
                if (dst.Members.Count > 0)
                {
                    printClassifier(dst, detailed);
                }
            }
            if (detailed == false)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                ds.Paragraphs.Add(dp);
                if (parentSect != null)
                {
                    parentSect.Sections.Add(ds);
                    this.currentsection = parentSect;
                }
                else
                {
                    this.PrintDocCmd(ds);
                }
                this.currentTable = null;
            }
        }

        public void printStruct(DoxStruct ds, bool detailed)
        {
            DocSect dsec = new DocSect();
            DocTable dt = new DocTable();
            bool endOfTable = true;
            if (detailed == false)
            {
                if (this.currentTable == null)
                {
                    dt.ColCount = 2;
                    dt.RowCount = 2;
                    this.header_row(dt);
                }
                else
                {
                    dt = this.currentTable;
                    endOfTable = false;
                }
                DocPara dp = new DocPara();
                dsec.Paragraphs.Add(dp);
                this.currentTable = dt;
            }
            printClassifier(ds, detailed);
            if (detailed == false && endOfTable == true)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                dsec.Paragraphs.Add(dp);
                this.PrintDocCmd(dsec);
                this.currentTable = null;

            }  
            
             
            
        }

        public void printEnums(bool detailed, DoxNamespace dn=null, List<DoxClassifier> inenums=null)
        {
            List<DoxClassifier> enums = null;
            if (inenums == null && dn != null)
            {
                enums = dn.Classifiers.Where(dc => dc.Kind == CompoundKind.Enum && dc.Members.Count > 0).OrderBy(c => c.Name).ToList();
            }
            else if(inenums != null)
            {
                enums = inenums;
            }
            else
            {
                this.Log("Error DPrint-4: printEnums method should get a Namespace or list of classifiers");
            }            
            DocSect parentSect = this.currentsection;
            DocSect ds = new DocSect();
            DocTable dt = new DocTable();
            if (detailed == false)
            {
                ds.Title = "Enums: ";
                ds.Identifier = "";
                DocPara dp = new DocPara();
                ds.Paragraphs.Add(dp);
                dt.ColCount = 2;
                dt.RowCount = enums.Count + 1;
                this.header_row(dt);
                this.currentTable = dt;
            }
            foreach (DoxEnum dc in enums)
            {
                if (dc.Members.Count > 0)
                {
                    printClassifier(dc, detailed);
                }
            }
            if (detailed == false)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                ds.Paragraphs.Add(dp);
                if (parentSect != null)
                {
                    parentSect.Sections.Add(ds);
                    this.currentsection = parentSect;
                }
                else
                {
                    this.PrintDocCmd(ds);
                }
                this.currentTable = null;
            }
            
        }

        public void printEnum(DoxEnum de, bool detailed)
        {
            DocSect dsec = new DocSect();
            DocTable dt = new DocTable();
            bool endOfTable = true;
            if (detailed == false)
            {
                if (this.currentTable == null)
                {
                    dt.ColCount = 2;
                    dt.RowCount = 2;
                    this.header_row(dt);
                }
                else
                {
                    dt = this.currentTable;
                    endOfTable = false;
                }
                DocPara dp = new DocPara();
                dsec.Paragraphs.Add(dp);
                this.currentTable = dt;
            }
            printClassifier(de, detailed);
            if (detailed == false && endOfTable == true)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                dsec.Paragraphs.Add(dp);
                this.PrintDocCmd(dsec);
                this.currentTable = null;

            }  
        }



        public void printMembers(bool detailed, DoxClassifier dc=null, List<DoxMember> InMembers =null)
        {
            List<DoxMember> members = null;
            if (InMembers == null && dc != null)
            {
                members = dc.Members.OrderBy(c => c.Name).ToList();
            }
            else if (InMembers != null)
            {
                members = InMembers;
            }            
            else
            {
                this.Log("Error DPrint-5: printMembers method should get a DoxClassifier or list of DoxMembers");
            }

            if(members.Count == 0)
            {
                return;
            }
            DocSect dsec = this.currentsection;
            DocSect ds = new DocSect();
            ds.Identifier = "";
            ds.Title = "Members: ";            
            DocTable dt = new DocTable();
            dt.ColCount = 2;
            dt.RowCount = members.Count+1;
            rowindex = 1;
            this.header_row(dt);
            this.currentTable = dt;
            this.currentsection = ds;
            foreach (DoxMember member in members)
            {
                printMember(member,detailed);
                rowindex++;
            }
            if(dsec != null)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                ds.Paragraphs.Add(dp);
                dsec.Sections.Add(ds);
                this.currentsection = dsec;
            }
            else
            {
                this.PrintDocCmd(ds);
            }
        }

        public void printMember(DoxMember dm, bool detailed)
        {
            DocTable dtable = null;
            if(detailed == true && dm.DetailedDescription == null)
            {
                return;
            }
            else if (detailed == true && dm.DetailedDescription.Paragraphs.Count == 0)
            {
                return;
            }

            if(detailed == true)
            {
                dtable = new DocTable();
                dtable.ColCount = 1;
                dtable.RowCount = 2;
            }
                DocTableRow dtr = new DocTableRow();
                DocTableCell dtc = new DocTableCell();
                DocPara dp = new DocPara();
                if (dm.ProtectionKind != null)
                {
                    DocText dt = new DocText();
                    dt.Text = dm.ProtectionKind.Value.ToString() + ' ';
                    dt.TextKind = DocTextKind.Plain;
                    dp.Commands.Add(dt);
                }
                if (dm.New == true)
                {
                    DocText dt = new DocText();
                    dt.Text = "new ";
                    dt.TextKind = DocTextKind.Plain;
                    dp.Commands.Add(dt);
                }
                if (dm.Final == true)
                {
                    DocText dt = new DocText();
                    dt.Text = "final ";
                    dt.TextKind = DocTextKind.Plain;
                    dp.Commands.Add(dt);
                }
                if (dm.Volatile == true)
                {
                    DocText dt = new DocText();
                    dt.Text = "volatile ";
                    dt.TextKind = DocTextKind.Plain;
                    dp.Commands.Add(dt);
                }
                if (dm.VirtualKind != null && dm.VirtualKind != VirtualKind.NonVirtual && dm.Kind != MemberKind.Function)
                {
                    DocText dt = new DocText();
                    dt.Text = dm.VirtualKind.ToString();
                    dt.TextKind = DocTextKind.Plain;
                    dp.Commands.Add(dt);
                }
                if (dm.Inline == true && dm.Kind != MemberKind.Function)
                {
                    DocText dt = new DocText();
                    dt.Text = "inline ";
                    dt.TextKind = DocTextKind.Plain;
                    dp.Commands.Add(dt);
                }
                if (dm.Static == true && dm.Kind != MemberKind.Function)
                {
                    DocText dt = new DocText();
                    dt.Text = "static ";
                    dt.TextKind = DocTextKind.Plain;
                    dp.Commands.Add(dt);
                }
                if (dm.Const == true)
                {
                    DocText dt = new DocText();
                    dt.Text = "contant ";
                    dt.TextKind = DocTextKind.Plain;
                    dp.Commands.Add(dt);
                }

                if (dm.Explicit == true)
                {
                    DocText dt = new DocText();
                    dt.Text = "explicit ";
                    dt.TextKind = DocTextKind.Plain;
                    dp.Commands.Add(dt);
                }

                if (dm.Definition != null)
                {
                    String[] texts = dm.Definition.Split(' ');
                    if (texts.Length > 1)
                    {
                        texts = texts.Take(texts.Count() - 1).ToArray();
                    }
                    for (int i = 0; i < texts.Length; i++ )
                    {
                        String item = texts[i];
                        item = item.Split('.').Last();
                        texts[i] = item;
                    }
                    String text = String.Join(" ", texts);
                    DocText dt = new DocText();
                    dt.Text = text;
                    dt.TextKind = DocTextKind.Plain;
                    dp.Commands.Add(dt);
                }

                if (detailed == false)
                {
                    dtc.Paragraphs.Add(dp);
                    dtr.Cells.Add(dtc);
                    dtc = new DocTableCell();
                    dp = new DocPara();
                }

                if (dm.Params.Count != 0)
                {
                    DocText dt = new DocText();
                    dt.TextKind = DocTextKind.Plain;
                    dt.Text = dm.Name + "(";
                    dp.Commands.Add(dt);
                    DocAnchor da = new DocAnchor();
                    da.Id = dm.Identifier;
                    dp.Commands.Add(da);
                    dt = new DocText();
                    dt.TextKind = DocTextKind.Plain;
                    for (int i = 0; i < dm.Params.Count; i++)
                    {
                        dt.Text = dm.Params[i].Type.Items[0].Text + " ";
                        dt.Text += dm.Params[i].DeclarationName + " ";
                        if (i != dm.Params.Count - 1)
                        {
                            dt.Text += ", ";
                        }
                    }
                    dt.Text += ")";
                    dp.Commands.Add(dt);
                }
                else if (dm.Kind == MemberKind.Function)
                {
                    DocText dt = new DocText();
                    dt.TextKind = DocTextKind.Plain;
                    dt.Text = dm.Name;
                    dp.Commands.Add(dt);
                    dt = new DocText();
                    dt.TextKind = DocTextKind.Plain;
                    dt.Text = "()";
                    dp.Commands.Add(dt);
                    DocAnchor da = new DocAnchor();
                    da.Id = dm.Identifier;
                    dp.Commands.Add(da);
                }
                else
                {
                    DocText dt = new DocText();
                    dt.TextKind = DocTextKind.Plain;
                    dt.Text = dm.Name;
                    dp.Commands.Add(dt);
                    DocAnchor da = new DocAnchor();
                    da.Id = dm.Identifier;
                    dp.Commands.Add(da);
                }
                if (detailed == false)
                {
                    if (dm.BriefDescription != null && dm.BriefDescription.Paragraphs.Count > 0)
                    {
                        dp.Commands.Add(new DocPara());
                        dp.Commands.AddRange(dm.BriefDescription.Paragraphs);
                    }
                    dtc.Paragraphs.Add(dp);
                    dtr.Cells.Add(dtc);
                    this.currentTable.Rows.Add(dtr);
                }
                else
                {
                    dtc.IsHeader = true;
                    dtc.Paragraphs.Add(dp);
                    dtr.Cells.Add(dtc);
                    dtable.Rows.Add(dtr);
                    dp = new DocPara();
                    dp.Commands.AddRange(dm.DetailedDescription.Paragraphs);
                    dtr = new DocTableRow();
                    dtc = new DocTableCell();
                    dtc.Paragraphs.Add(dp);
                    dtr.Cells.Add(dtc);
                    dtable.Rows.Add(dtr);
                    dp = new DocPara();
                    dp.Commands.Add(dtable);
                    DocSect ds = new DocSect();
                    ds.Paragraphs.Add(dp);
                    this.currentsection.Sections.Add(ds);
                }
            
            
        }

        /// <summary>
        /// Method, that responsible for the output of the fields
        /// </summary>
        /// <param name="detailed">Type of output</param>
        /// <param name="dc">If not null, than the method use the fields of this Classifier</param>
        /// <param name="InFileds">If isn't null, than the method use these fields</param>
        public void printFields(bool detailed, DoxClassifier dc=null, List<DoxMember> InFileds = null)
        {
            List<DoxMember> fields = null;
            if (InFileds == null && dc != null)
            {
                fields = dc.Members.Where(dm => dm.Kind == MemberKind.Variable).OrderBy(c => c.Name).ToList();
            }
            else if (InFileds != null)
            {
                fields = InFileds;
            }
            else
            {
                this.Log("Error DPrint-6: printFields method should get a DoxClassifier or list of DoxMembers");
            }
            if (fields.Count == 0)
            {
                return;
            }
            DocSect dsec = this.currentsection;
            DocSect ds = new DocSect();
            ds.Identifier = "";
            ds.Title = "Fields: ";
            DocTable dt = new DocTable();
            dt.ColCount = 2;
            dt.RowCount = fields.Count+1;
            rowindex = 1;
            this.header_row(dt);
            this.currentTable = dt;
            foreach (DoxField member in fields)
            {
                printField(member, false);
                rowindex++;
            }            
            if (dsec != null)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                ds.Paragraphs.Add(dp);
                dsec.Sections.Add(ds);
            }
            else
            {
                this.PrintDocCmd(ds);
            }
            if (detailed == true)
            {
                foreach (DoxField member in fields)
                {
                    printField(member, true);
                } 
            }
        }

        public void printField(DoxField df, bool detailed)
        {
            this.printMember(df, detailed);
        }

        public void printMethods(bool detailed,DoxClassifier dc = null, List<DoxMember> Inmethods = null)
        {
            List<DoxMember> methods = null;
            if (Inmethods == null && dc != null)
            {
                methods = dc.Members.Where(dm => dm.Kind == MemberKind.Function).OrderBy(c => c.Name).ToList();
            }
            else if (Inmethods != null)
            {
                methods = Inmethods;
            }
            else
            {
                this.Log("Error DPrint-7: printMethods method should get a DoxClassifier or list of DoxMembers");
            }

            if (methods.Count == 0)
            {
                return;
            }
            DocSect dsec = this.currentsection;
            DocSect ds = new DocSect();
            ds.Identifier = "";
            ds.Title = "Methods: ";
            DocTable dt = new DocTable();
            dt.ColCount = 2;
            dt.RowCount = methods.Count + 1;
            rowindex = 1;
            this.header_row(dt);
            this.currentTable = dt;
            foreach (DoxMethod member in methods)
            {
                printMethod(member, false);
                rowindex++;
            }
            if (dsec != null)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                ds.Paragraphs.Add(dp);
                dsec.Sections.Add(ds);
            }
            else
            {
                this.PrintDocCmd(ds);
            }
            if(detailed == true)
            {
                foreach (DoxMethod member in methods)
                {
                    printMethod(member, detailed);
                }
            }
        }

        public void printMethod(DoxMethod dm, bool detailed)
        {
            this.printMember(dm, detailed);
        }

        public void printEnumValues(bool detailed, DoxClassifier dev = null, List<DoxMember> InEnumVales = null)
        {
            List<DoxMember> enumValues = null;
            if (InEnumVales == null && dev != null)
            {
                enumValues = dev.Members.Where(dm => dm.Kind == MemberKind.EnumValue).OrderBy(c => c.Name).ToList();
            }
            else if (InEnumVales != null)
            {
                enumValues = InEnumVales;
            }
            else
            {
                this.Log("Error DPrint-8: printEnumValues method should get a DoxClassifier or list of DoxMembers");
            }
            if (enumValues.Count == 0)
            {
                return;
            }
            DocSect dsec = this.currentsection;
            DocSect ds = new DocSect();
            ds.Identifier = "";
            ds.Title = "Enum values: ";
            DocTable dt = new DocTable();
            dt.ColCount = 2;
            dt.RowCount = enumValues.Count + 1;
            rowindex = 1;
            this.header_row(dt);
            this.currentTable = dt;
            foreach (DoxEnumValue member in enumValues)
            {
                printEnumValue(member, false);
                rowindex++;
            }
            if (dsec != null)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                ds.Paragraphs.Add(dp);
                dsec.Sections.Add(ds);
            }
            else
            {
                this.PrintDocCmd(ds);
            }       
            if(detailed == true)
            {
                foreach (DoxEnumValue member in enumValues)
                {
                    printEnumValue(member, detailed);
                }
            }
        }

        public void printEnumValue(DoxEnumValue dev, bool detailed)
        {
            printMember(dev, detailed);
        }

        public void printProperties(bool detailed, DoxClassifier dc = null, List<DoxMember> InProperties = null)
        {
            List<DoxMember> properties = null;
            if (InProperties == null && dc != null)
            {
                properties = dc.Members.Where(dm => dm.Kind == MemberKind.Property).OrderBy(c => c.Name).ToList();
            }
            else if (InProperties != null)
            {
                properties = InProperties;
            }
            else
            {
                this.Log("Error DPrint-8: printEnumValues method should get a DoxClassifier or list of DoxMembers");
            }
            if (properties.Count == 0)
            {
                return;
            }
            DocSect dsec = this.currentsection;
            DocSect ds = new DocSect();
            ds.Identifier = "";
            ds.Title = "Properties: ";
            DocTable dt = new DocTable();
            dt.ColCount = 2;
            dt.RowCount = properties.Count + 1;
            rowindex = 1;
            this.header_row(dt);
            this.currentTable = dt;
            foreach (DoxProperty member in properties)
            {
                printProperty(member, false);
                rowindex++;
            }
            if (dsec != null)
            {
                DocPara dp = new DocPara();
                dp.Commands.Add(dt);
                ds.Paragraphs.Add(dp);
                dsec.Sections.Add(ds);
            }
            else
            {
                this.PrintDocCmd(ds);
            }  
            if(detailed == true)
            {
                foreach (DoxProperty member in properties)
                {
                    printProperty(member, detailed);
                }
            }
        }

        public void printProperty(DoxProperty dp, bool detailed)
        {
            printMember(dp, detailed);
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
        public string GetPackage(string name)
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

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        private void header_row(DocTable dt)
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

        int depth = 0;
        public void PrintDocCmd(DocCmd cmd)
        {
            depth++;
            if(depth > 20)
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
                                dg.BeginReference(docReference.referenceID, false);
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
