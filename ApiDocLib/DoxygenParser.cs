using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Xml;

namespace SoftwareEngineeringTools.Documentation
{
    public class DoxygenParser : Parser
    {
        private static readonly List<string> DocEmptyTags = new List<string>(Enum.GetNames(typeof(DocEmptyKind)));
        private static readonly List<string> DocMarkupTags = new List<string>(Enum.GetNames(typeof(DocMarkupKind)).Select(e => e.ToLower()));
        private static readonly List<string> DocReferenceTags = new List<string>(Enum.GetNames(typeof(DocRefKind)).Select(e => e.ToLower()));

        public DoxygenParser(string indexFile, string logFile = null)
        {
            this.IndexFile = indexFile;
            if (logFile != null) this.LogFile = new StreamWriter(logFile);
            this.Parse();
            if (this.LogFile != null)
            {
                this.LogFile.Dispose();
            }
        }

        private string currentFileName = null;
        private StreamWriter LogFile;

        public string IndexFile { get; private set; }
        public DoxygenIndex Index { get; private set; }
        public DoxygenModel Model { get; private set; }

        private void Log(LogKind kind, string text, XObject xobj = null)
        {
            string message = null;
            IXmlLineInfo li = xobj as IXmlLineInfo;
            if (li != null && li.HasLineInfo())
            {
                if (this.currentFileName != null)
                {
                    message = string.Format("{0} in {2} at ({3},{4}): {1}", kind.ToString().ToUpper(), text, this.currentFileName, li.LineNumber, li.LinePosition);
                }
                else
                {
                    message = string.Format("{0} at ({3},{4}): {1}", kind.ToString().ToUpper(), text, li.LineNumber, li.LinePosition);
                }
            }
            else
            {
                message = string.Format("{0}: {1}", kind.ToString().ToUpper(), text);
            }
            if (LogFile != null)
            {
                LogFile.WriteLine(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        private XDocument LoadXml(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                string xml = reader.ReadToEnd();
                XDocument xdoc = XDocument.Parse(xml, LoadOptions.SetLineInfo);
                return xdoc;
            }
        }

        private void Parse()
        {
            this.Index = new DoxygenIndex();
            this.Model = new DoxygenModel();
            this.ParseIndex();
            this.CreateModelElements();
            this.ParseModel();
        }

        private void ParseIndex()
        {
            this.currentFileName = this.IndexFile;
            XDocument xdoc = this.LoadXml(this.IndexFile);
            XElement root = xdoc.Element("doxygenindex");
            this.Index.Version = root.Attribute("version").Value;
            foreach (var cie in root.Elements())
            {
                if (cie.Name == "compound")
                {
                    CompoundIndex ci = new CompoundIndex();
                    this.Index.Compounds.Add(ci);
                    ci.Identifier = cie.Attribute("refid").Value;
                    ci.Name = cie.Element("name").Value;
                    XAttribute cka = cie.Attribute("kind");
                    CompoundKind ck;
                    if (this.FromEnum<CompoundKind>(cka.Value, out ck, true, cka))
                    {
                        ci.Kind = ck;
                        foreach (var mie in cie.Elements())
                        {
                            if (mie.Name == "name")
                            {
                            }
                            else if (mie.Name == "member")
                            {
                                MemberIndex mi = new MemberIndex();
                                ci.Members.Add(mi);
                                mi.Identifier = mie.Attribute("refid").Value;
                                mi.Name = mie.Element("name").Value;
                                XAttribute mka = mie.Attribute("kind");
                                MemberKind mk;
                                if (this.FromEnum<MemberKind>(mka.Value, out mk, true, mka))
                                {
                                    mi.Kind = mk;
                                    foreach (var miee in mie.Elements())
                                    {
                                        if (miee.Name == "name")
                                        {
                                        }
                                        else
                                        {
                                            this.Log(LogKind.Warning, "Unknown element: " + miee.Name, miee);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                this.Log(LogKind.Warning, "Unknown element: " + mie.Name, mie);
                            }
                        }
                    }
                }
                else
                {
                    this.Log(LogKind.Warning, "Unknown element: "+cie.Name, cie);
                }
            }
        }

        private void CreateModelElements()
        {
            foreach (var ci in this.Index.Compounds)
            {
                Compound c = this.CreateCompound(ci);
                if (c != null)
                {
                    this.Model.Compounds.Add(c);
                    foreach (var mi in ci.Members)
                    {
                        DoxMember m = this.CreateMember(mi);
                    }
                    int j = 0;
                    while (j < ci.Members.Count)
                    {
                        MemberIndex mi = ci.Members[j];
                        if (mi.Member == null)
                        {
                            ci.Members.Remove(mi);
                            this.Log(LogKind.Warning, string.Format("Unsupported member (kind={0},name={1},id={2}) removed from compound (kind={3},name={4},id={5}).", mi.Kind, mi.Name, mi.Identifier, ci.Kind, ci.Name, ci.Identifier));
                        }
                        else
                        {
                            ++j;
                        }
                    }
                }
            }
            int i = 0;
            while (i < this.Index.Compounds.Count)
            {
                CompoundIndex ci = this.Index.Compounds[i];
                if (ci.Compound == null)
                {
                    this.Index.Compounds.Remove(ci);
                    this.Log(LogKind.Warning, string.Format("Unsupported compound (kind={0},name={1},id={2}) removed.", ci.Kind, ci.Name, ci.Identifier));
                }
                else
                {
                    ++i;
                }
            }
            foreach (var ci in this.Index.Compounds)
            {
                this.Index.CompoundIndexRefs.Add(ci.Identifier, ci);
                this.Model.CompoundRefs.Add(ci.Identifier, ci.Compound);
                foreach (var mi in ci.Members)
                {
                    this.Index.MemberIndexRefs.Add(mi.Identifier, mi);
                    this.Model.MemberRefs.Add(mi.Identifier, mi.Member);
                }
            }
        }

        private Compound CreateCompound(CompoundIndex ci)
        {
            Compound c = null;
            switch (ci.Kind)
            {
                case CompoundKind.Class:
                    c = new DoxClass();
                    break;
                case CompoundKind.Struct:
                    c = new DoxStruct();
                    break;
                case CompoundKind.Union:
                    break;
                case CompoundKind.Interface:
                    c = new DoxInterface();
                    break;
                case CompoundKind.Enum:
                    c = new DoxEnum();
                    break;
                case CompoundKind.Protocol:
                    break;
                case CompoundKind.Category:
                    break;
                case CompoundKind.Exception:
                    break;
                case CompoundKind.File:
                    c = new DoxFile();
                    break;
                case CompoundKind.Namespace:
                    c = new DoxNamespace();
                    break;
                case CompoundKind.Group:
                    break;
                case CompoundKind.Page:
                    break;
                case CompoundKind.Example:
                    break;
                case CompoundKind.Dir:
                    c = new DoxDirectory();
                    break;
                default:
                    break;
            }
            ci.Compound = c;
            if (c != null)
            {
                c.Identifier = ci.Identifier;
                c.Name = ci.Name;
            }
            return c;
        }

        private DoxMember CreateMember(MemberIndex mi)
        {
            DoxMember m = null;
            switch (mi.Kind)
            {
                case MemberKind.Define:
                    m = new DoxDefine();
                    break;
                case MemberKind.Property:
                    m = new DoxProperty();
                    break;
                case MemberKind.Event:
                    break;
                case MemberKind.Variable:
                    m = new DoxField();
                    break;
                case MemberKind.TypeDef:
                    break;
                case MemberKind.EnumValue:
                    m = new DoxEnumValue();
                    break;
                case MemberKind.Enum:
                    m = new MemberEnum();
                    break;
                case MemberKind.Function:
                    m = new DoxMethod();
                    break;
                case MemberKind.Signal:
                    break;
                case MemberKind.Prototype:
                    break;
                case MemberKind.Friend:
                    m = new DoxFriend();
                    break;
                case MemberKind.Dcop:
                    break;
                case MemberKind.Slot:
                    break;
                default:
                    break;
            }
            mi.Member = m;
            if (m != null)
            {
                m.Name = mi.Name;
                m.Identifier = mi.Identifier;
                
            }
            return m;
        }

        private void ParseModel()
        {
            foreach (var c in this.Model.Compounds)
            {
                this.ParseCompound(c);
            }
        }

        //! XML reader and processing.
        /*!
          Read the XML file of the specified class and than process it.
        */
        /// <summary>
        /// Read the XML file of the specified class and than process it.
        /// </summary>
        /// <param name="c"></param>
        private void ParseCompound(Compound c)
        {
            string fileName = Path.Combine(Path.GetDirectoryName(this.IndexFile), c.Identifier + ".xml");
            this.currentFileName = fileName;
            try
            {
                XDocument xdoc = this.LoadXml(fileName);
             XElement de = xdoc.Element("doxygen");
            XElement cde = de.Element("compounddef");
            if (this.ParseCompound(c, cde))
            {
                switch (c.Kind)
                {
                    case CompoundKind.Class:
                        this.ParseClass((DoxClass)c, cde);
                        break;
                    case CompoundKind.Struct:
                        this.ParseStruct((DoxStruct)c, cde);
                        break;
                    case CompoundKind.Union:
                        break;
                    case CompoundKind.Interface:
                        this.ParseInterface((DoxInterface)c, cde);
                        break;
                    case CompoundKind.Enum:
                        this.ParseEnum((DoxEnum)c, cde);
                        break;
                    case CompoundKind.Protocol:
                        break;
                    case CompoundKind.Category:
                        break;
                    case CompoundKind.Exception:
                        break;
                    case CompoundKind.File:
                        this.ParseFile((DoxFile)c, cde);
                        break;
                    case CompoundKind.Namespace:
                        this.ParseNamespace((DoxNamespace)c, cde);
                        break;
                    case CompoundKind.Group:
                        break;
                    case CompoundKind.Page:
                        break;
                    case CompoundKind.Example:
                        break;
                    case CompoundKind.Dir:
                        this.ParseDirectory((DoxDirectory)c, cde);
                        break;
                    default:
                        break;
                }
            }
            this.currentFileName = null;
            }
            catch (Exception)
            {

                Console.WriteLine(fileName + " can't be open.");
            }
           
        }

        /** Compound Parser
        * Get info from the Compound
        */
        private bool ParseCompound(Compound c, XElement cde)
        {
            XAttribute ida = cde.Attribute("id");
            if (ida.Value != c.Identifier)
            {
                this.Log(LogKind.Error, string.Format("Identifier mismatch: {0} != {1}", ida.Value, c.Identifier), ida);
                return false;
            }
            XElement ne = cde.Element("compoundname");
            if (ne.Value != c.Name)
            {
                this.Log(LogKind.Error, string.Format("Compound name mismatch: {0} != {1}", ne.Value, c.Name), ne);
                return false;
            }
            XElement te = cde.Element("title");
            if (te != null)
            {
                c.Title = te.Value;
            }
            XElement bde = cde.Element("briefdescription");
            if (bde != null)
            {
                c.BriefDescription = new Description();
                this.ParseDescription(c.BriefDescription, bde);
            }
            XElement de = cde.Element("description");
            if (de != null)
            {
                c.Description = new Description();
                this.ParseDescription(c.Description, de);
            }
            XElement dde = cde.Element("detaileddescription");
            if (dde != null)
            {
                c.Description = new Description();
                this.ParseDescription(c.Description, dde);
            }
            XElement ple = cde.Element("programlisting");
            if (ple != null)
            {
                c.ProgramListing = new DoxProgramListing();
                this.ParseListing(c.ProgramListing, ple);
            }
            XElement le = cde.Element("location");
            if (le != null)
            {
                c.Location = new Location();
                this.ParseLocation(c.Location, le);
            }
            return true;
        }

        private bool ParseLocation(Location l, XElement le)
        {
            XAttribute a;
            a = le.Attribute("file");
            l.File = a != null ? a.Value : null;
            a = le.Attribute("line");
            l.Line = a != null ? int.Parse(le.Attribute("line").Value) : -1;
            a = le.Attribute("bodyfile");
            l.BodyFile = a != null ? le.Attribute("bodyfile").Value : null;
            a = le.Attribute("bodystart");
            l.BodyStart = a != null ? int.Parse(le.Attribute("bodystart").Value) : -1;
            a = le.Attribute("bodyend");
            l.BodyEnd = a != null ? int.Parse(le.Attribute("bodyend").Value) : -1;
            return true;
        }

        private bool ParseClassifier(DoxClassifier c, XElement cde)
        {
            XAttribute pka = cde.Attribute("prot");
            ProtectionKind pk;
            if (!this.FromEnum<ProtectionKind>(pka.Value, out pk, true, pka)) return false;
            c.ProtectionKind = pk;
            c.Final = this.GetBoolAttributeValue(cde, "final");
            c.Sealed = this.GetBoolAttributeValue(cde, "sealed");
            c.Abstract = this.GetBoolAttributeValue(cde, "abstract");

            foreach (var bce in cde.Elements("basecompoundref"))
            {
                DoxReference r = new DoxReference();
                c.BaseClassifiers.Add(r);
                if (!this.ParseCompoundRef(r, bce)) return false;
            }
            foreach (var dce in cde.Elements("derivedcompoundref"))
            {
                DoxReference r = new DoxReference();
                c.DerivedClassifiers.Add(r);
                if (!this.ParseCompoundRef(r, dce)) return false;
            }
            if (!this.ParseInnerClasses(c.InnerClassifiers, cde.Elements("innerclass"))) return false;
            XElement tple = cde.Element("templateparamlist");
            if (tple != null)
            {
                if (!this.ParseTemplateParamList(c.TemplateParams, tple)) return false;
            }
            foreach (var se in cde.Elements("sectiondef"))
            {
                if (!this.ParseSection(c, se)) return false;
            }
            return true;
        }

        private bool ParseCompoundRef(DoxReference r, XElement re)
        {
            r.Text = re.Value;
            XAttribute pka = re.Attribute("prot");
            ProtectionKind pk;
            if (!this.FromEnum<ProtectionKind>(pka.Value, out pk, true, pka)) return false;
            r.ProtectionKind = pk;
            XAttribute va = re.Attribute("virt");
            VirtualKind vk;
            if (!GetVirtualKind(va.Value, out vk, va)) return false;
            r.VirtualKind = vk;
            XAttribute ida = re.Attribute("refid");
            if (ida != null)
            {
                Compound rc;
                if (this.Model.CompoundRefs.TryGetValue(ida.Value, out rc))
                {
                    r.Compound = rc;
                }
                if (r.Compound == null)
                {
                    this.Log(LogKind.Error, "Cannot resolve id: " + ida.Value, ida);
                    return false;
                }
            }
            return true;
        }

        private bool ParseRef(DoxReference r, XElement re)
        {
            r.Text = re.Value;
            XAttribute pka = re.Attribute("prot");
            if (pka != null)
            {
                ProtectionKind pk;
                if (!this.FromEnum<ProtectionKind>(pka.Value, out pk, true, pka)) return false;
                r.ProtectionKind = pk;
            }
            XAttribute ida = re.Attribute("refid");
            if (ida != null)
            {
                Compound rc;
                DoxMember rm;
                if (this.Model.CompoundRefs.TryGetValue(ida.Value, out rc))
                {
                    r.Compound = rc;
                }
                if (this.Model.MemberRefs.TryGetValue(ida.Value, out rm))
                {
                    r.Member = rm;
                }
                if (r.Compound == null && r.Member == null)
                {
                    this.Log(LogKind.Error, "Cannot resolve id: " + ida.Value, ida);
                    return false;
                }
            }
            return true;
        }

        private bool GetVirtualKind(string value, out VirtualKind vk, XObject xobj)
        {
            switch (value)
            {
                case "non-virtual":
                    vk = VirtualKind.NonVirtual;
                    return true;
                case "virtual":
                    vk = VirtualKind.Virtual;
                    return true;
                case "pure-virtual":
                    vk = VirtualKind.Abstract;
                    return true;
                default:
                    vk = VirtualKind.NonVirtual;
                    this.Log(LogKind.Error, "Invalid enum value: " + value, xobj);
                    return false;
            }
        }

        private bool ParseTemplateParamList(List<DoxParam> tpl, XElement tple)
        {
            foreach (var pe in tple.Elements("param"))
            {
                DoxParam p = new DoxParam();
                tpl.Add(p);
                if (!this.ParseParam(p, pe)) return false;
            }
            return true;
        }

        private bool ParseParam(DoxParam p, XElement pe)
        {
            XElement dce = pe.Element("declname");
            if (dce != null) p.DeclarationName = dce.Value;
            XElement dfe = pe.Element("defname");
            if (dfe != null) p.DeclarationName = dfe.Value;
            XElement ae = pe.Element("array");
            if (ae != null) p.Array = ae.Value;
            XElement te = pe.Element("type");
            if (te != null)
            {
                p.Type = new DoxLinkedText();
                if (!this.ParseLinkedText(p.Type, te)) return false;
            }
            XElement dve = pe.Element("defval");
            if (dve != null)
            {
                p.DefaultValue = new DoxLinkedText();
                if (!this.ParseLinkedText(p.DefaultValue, dve)) return false;
            }
            XElement bde = pe.Element("briefdescription");
            if (bde != null)
            {
                p.Description = new Description();
                if (!this.ParseDescription(p.Description, bde)) return false;
            }
            return true;
        }

        private bool ParseLinkedText(DoxLinkedText r, XElement re)
        {
            foreach (var n in re.Nodes())
            {
                if (n.NodeType == XmlNodeType.Text)
                {
                    DoxText dt = new DoxText();
                    r.Items.Add(dt);
                    dt.Text = ((XText)n).Value;
                }
                else if (n.NodeType == XmlNodeType.Element)
                {
                    XElement e = (XElement)n;
                    if (e.Name == "ref")
                    {
                        DoxReference tr = new DoxReference();
                        r.Items.Add(tr);
                        if (!this.ParseRefText(tr, e)) return false;
                    }
                    else
                    {
                        this.Log(LogKind.Error, "Unexpected element: " + e.Name, e);
                        return false;
                    }
                }
                else
                {
                    this.Log(LogKind.Warning, "Unexpected node: " + n.NodeType, n);
                    return false;
                }
            }
            return true;
        }

        private bool ParseRefText(DoxReference r, XElement re)
        {
            r.Text = re.Value;
            r.External = this.GetBoolAttributeValue(re, "external");
            XAttribute tta = re.Attribute("tooltip");
            if (tta != null) r.Tooltip = tta.Value;
            XAttribute rka = re.Attribute("kindref");
            XAttribute ida = re.Attribute("refid");
            if (ida != null)
            {
                Compound rc;
                DoxMember rm;
                if (rka.Value == "compound" && this.Model.CompoundRefs.TryGetValue(ida.Value, out rc))
                {
                    r.Compound = rc;
                }
                if (rka.Value == "member" && this.Model.MemberRefs.TryGetValue(ida.Value, out rm))
                {
                    r.Member = rm;
                }
                if (r.Compound == null && r.Member == null)
                {
                    this.Log(LogKind.Error, "Cannot resolve id: " + ida.Value, ida);
                    return false;
                }
            }
            return true;
        }

        private bool ParseClass(DoxClass c, XElement cde)
        {
            if (!this.ParseClassifier(c, cde)) return false;
            return true;
        }

        private bool ParseStruct(DoxStruct c, XElement cde)
        {
            if (!this.ParseClassifier(c, cde)) return false;
            return true;
        }

        private bool ParseInterface(DoxInterface c, XElement cde)
        {
            if (!this.ParseClassifier(c, cde)) return false;
            return true;
        }

        private bool ParseEnum(DoxEnum c, XElement cde)
        {
            if (!this.ParseClassifier(c, cde)) return false;
            return true;
        }

        private bool ParseInnerClasses(List<DoxClassifier> cl, IEnumerable<XElement> ices)
        {
            foreach (var ice in ices)
            {
                DoxReference r = new DoxReference();
                if (!this.ParseRef(r, ice)) return false;
                if (r.Compound is DoxClassifier)
                {
                    cl.Add((DoxClassifier)r.Compound);
                }
                else
                {
                    this.Log(LogKind.Error, "Not a classifier: " + r.Compound.Identifier, ice);
                    return false;
                }
            }
            return true;
        }

        private bool ParseInnerNamespaces(List<DoxNamespace> nl, IEnumerable<XElement> ices)
        {
            foreach (var ice in ices)
            {
                DoxReference r = new DoxReference();
                if (!this.ParseRef(r, ice)) return false;
                if (r.Compound is DoxNamespace)
                {
                    nl.Add((DoxNamespace)r.Compound);
                }
                else
                {
                    this.Log(LogKind.Error, "Not a namespace: " + r.Compound.Identifier, ice);
                    return false;
                }
            }
            return true;
        }

        private bool ParseFile(DoxFile c, XElement cde)
        {
            if (!this.ParseInnerClasses(c.Classifiers, cde.Elements("innerclass"))) return false;
            if (!this.ParseInnerNamespaces(c.Namespaces, cde.Elements("innernamespace"))) return false;
            return true;
        }

        private bool ParseNamespace(DoxNamespace c, XElement cde)
        {
            if (!this.ParseInnerClasses(c.Classifiers, cde.Elements("innerclass"))) return false;
            if (!this.ParseInnerNamespaces(c.Namespaces, cde.Elements("innernamespace"))) return false;
            return true;
        }

        private bool ParseDirectory(DoxDirectory c, XElement cde)
        {
            if (!this.ParseDirectories(c.Directories, cde.Elements("innerdir"))) return false;
            if (!this.ParseFiles(c.Files, cde.Elements("innerfile"))) return false;
            return true;
        }

        private bool ParseFiles(List<DoxFile> fl, IEnumerable<XElement> fle)
        {
            foreach (var fe in fle)
            {
                DoxReference r = new DoxReference();
                if (!this.ParseRef(r, fe)) return false;
                if (r.Compound is DoxFile)
                {
                    fl.Add((DoxFile)r.Compound);
                }
                else
                {
                    this.Log(LogKind.Error, "Not a file: " + r.Compound.Identifier, fe);
                    return false;
                }
            }
            return true;
        }

        private bool ParseDirectories(List<DoxDirectory> dl, IEnumerable<XElement> dle)
        {
            foreach (var de in dle)
            {
                DoxReference r = new DoxReference();
                if (!this.ParseRef(r, de)) return false;
                if (r.Compound is DoxDirectory)
                {
                    dl.Add((DoxDirectory)r.Compound);
                }
                else
                {
                    this.Log(LogKind.Error, "Not a directory: " + r.Compound.Identifier, de);
                    return false;
                }
            }
            return true;
        }

        private bool ParseSection(DoxClassifier c, XElement se)
        {
            foreach (var me in se.Elements("memberdef"))
            {
                XAttribute ida = me.Attribute("id");
                string id = ida.Value;
                DoxMember m = null;
                if (this.Model.MemberRefs.TryGetValue(id, out m))
                {
                    c.Members.Add(m);
                    if (!this.ParseMember(m, me)) return false;
                }
                else
                {
                    this.Log(LogKind.Error, "Cannot find member in model: " + id, ida);
                    return false;
                }
            }
            return true;
        }

        private bool? GetBoolAttributeValue(XElement e, string name)
        {
            XAttribute a = e.Attribute(name);
            return a != null ? a.Value == "yes" : (bool?)null;
        }

        private bool CheckMember(DoxMember m, XElement me)
        {
            XAttribute ida = me.Attribute("id");
            if (ida.Value != m.Identifier)
            {
                this.Log(LogKind.Error, string.Format("Identifier mismatch: {0} != {1}", ida.Value, m.Identifier), ida);
                return false;
            }
            XElement ne = me.Element("name");
            if (ne.Value != m.Name)
            {
                this.Log(LogKind.Error, string.Format("Member name mismatch: {0} != {1}", ne.Value, m.Name), ne);
                return false;
            }
            return true;
        }

        private bool ParseMember(DoxMember m, XElement me)
        {
            if (!CheckMember(m, me)) return false;
            XAttribute pka = me.Attribute("prot");
            ProtectionKind pk;
            if (!this.FromEnum<ProtectionKind>(pka.Value, out pk, true, pka)) return false;
            m.ProtectionKind = pk;
            XAttribute vka = me.Attribute("virt");
            if (vka != null)
            {
                VirtualKind vk;
                if (!this.GetVirtualKind(vka.Value, out vk, vka)) return false;
                m.VirtualKind = vk;
            }
            m.Static = this.GetBoolAttributeValue(me, "static");
            m.Const = this.GetBoolAttributeValue(me, "const");
            m.Explicit = this.GetBoolAttributeValue(me, "explicit");
            m.Inline = this.GetBoolAttributeValue(me, "inline");
            m.Volatile = this.GetBoolAttributeValue(me, "volatile");
            m.Mutable = this.GetBoolAttributeValue(me, "mutable");
            m.Initonly = this.GetBoolAttributeValue(me, "initonly");
            m.Settable = this.GetBoolAttributeValue(me, "settable");
            m.Gettable = this.GetBoolAttributeValue(me, "gettable");
            m.Final = this.GetBoolAttributeValue(me, "final");
            m.Sealed = this.GetBoolAttributeValue(me, "sealed");
            m.New = this.GetBoolAttributeValue(me, "new");
            m.Add = this.GetBoolAttributeValue(me, "add");
            m.Remove = this.GetBoolAttributeValue(me, "remove");
            m.Raise = this.GetBoolAttributeValue(me, "raise");
            XElement tple = me.Element("templateparamlist");
            if (tple != null)
            {
                if (!this.ParseTemplateParamList(m.TemplateParams, tple)) return false;
            }
            XElement dfe = me.Element("definition");
            m.Definition = dfe != null ? dfe.Value : null;
            XElement ase = me.Element("argsstring");
            m.ArgsString = ase != null ? ase.Value : null;
            XElement re = me.Element("read");
            m.Read = re != null ? re.Value : null;
            XElement we = me.Element("write");
            m.Write = we != null ? we.Value : null;
            XElement bfe = me.Element("bitfield");
            m.BitField = bfe != null ? bfe.Value : null;
            foreach (var pe in me.Elements("param"))
            {
                DoxParam p = new DoxParam();
                m.Params.Add(p);
                if (!this.ParseParam(p, pe)) return false;
            }
            if (me.Element("enumvalue") != null)
            {
                m.Kind = MemberKind.Enum;
                //this.Log(LogKind.Warning, "enumvalue in member: " + m.Identifier, me);
            }
            XElement ie = me.Element("initializer");
            if (ie != null)
            {
                m.Initializer = new DoxLinkedText();
                if (!this.ParseLinkedText(m.Initializer, ie)) return false;
            }
            XElement exe = me.Element("exceptions");
            if (exe != null)
            {
                m.Exceptions = new DoxLinkedText();
                if (!this.ParseLinkedText(m.Exceptions, exe)) return false;
            }
            XElement bde = me.Element("briefdescription");
            if (bde != null)
            {
                m.BriefDescription = new Description();
                this.ParseDescription(m.BriefDescription, bde);
            }
            XElement de = me.Element("description");
            if (de != null)
            {
                m.Description = new Description();
                this.ParseDescription(m.Description, de);
            }
            XElement ibde = me.Element("inbodydescription");
            if (ibde != null)
            {
                m.InBodyDescription = new Description();
                this.ParseDescription(m.InBodyDescription, ibde);
            }
            XElement dde = me.Element("detaileddescription");
            if (dde != null)
            {
                m.DetailedDescription = new Description();
                this.ParseDescription(m.DetailedDescription, dde);
            }
            XElement le = me.Element("location");
            if (le != null)
            {
                m.Location = new Location();
                this.ParseLocation(m.Location, le);
            }
            foreach (var rfe in me.Elements("references"))
            {
                DoxReference rf = new DoxReference();
                m.References.Add(rf);
                if (!this.ParseRef(rf, rfe)) return false;
            }
            foreach (var rfe in me.Elements("referencedby"))
            {
                DoxReference rf = new DoxReference();
                m.ReferencedBy.Add(rf);
                if (!this.ParseRef(rf, rfe)) return false;
            }
            foreach (var rie in me.Elements("reimplements"))
            {
                XAttribute ra = rie.Attribute("refid");
                DoxMember rm = null;
                if (this.Model.MemberRefs.TryGetValue(ra.Value, out rm))
                {
                    m.Reimplements.Add(rm);
                }
                else
                {
                    this.Log(LogKind.Error, "Cannot find reimplemented member: " + ra.Value, rie);
                    return false;
                }
            }
            foreach (var rie in me.Elements("reimplementedby"))
            {
                XAttribute ra = rie.Attribute("refid");
                DoxMember rm = null;
                if (this.Model.MemberRefs.TryGetValue(ra.Value, out rm))
                {
                    m.ReimplementedBy.Add(rm);
                }
                else
                {
                    this.Log(LogKind.Error, "Cannot find reimplemented member: " + ra.Value, rie);
                    return false;
                }
            }
            return true;
        }

        private bool ParseListing(DoxProgramListing pl, XElement ple)
        {
            foreach (var cle in ple.Elements("codeline"))
            {
                DoxCodeLine cl = new DoxCodeLine();
                pl.CodeLines.Add(cl);
                XAttribute lnoa = cle.Attribute("lineno");
                cl.LineNumber = lnoa != null ? int.Parse(lnoa.Value) : -1;
                cl.External = this.GetBoolAttributeValue(cle, "external");
                XAttribute rka = cle.Attribute("refkind");
                XAttribute ida = cle.Attribute("refid");
                if (ida != null)
                {
                    Compound rc;
                    DoxMember rm;
                    if (rka.Value == "compound" && this.Model.CompoundRefs.TryGetValue(ida.Value, out rc))
                    {
                        cl.Compound = rc;
                    }
                    if (rka.Value == "member" && this.Model.MemberRefs.TryGetValue(ida.Value, out rm))
                    {
                        cl.Member = rm;
                    }
                    if (cl.Compound == null && cl.Member == null)
                    {
                        this.Log(LogKind.Error, "Cannot resolve id: " + ida.Value, ida);
                        return false;
                    }
                }
                foreach (var hle in cle.Elements("highlight"))
                {
                    DoxHighlight h = new DoxHighlight();
                    cl.Tokens.Add(h);
                    foreach (var n in hle.Nodes())
                    {
                        if (n.NodeType == XmlNodeType.Text)
                        {
                            DoxHighlightText ht = new DoxHighlightText();
                            h.Text.Add(ht);
                            ht.Text = ((XText)n).Value;
                        }
                        else if (n.NodeType == XmlNodeType.Element)
                        {
                            XElement ne = ((XElement)n);
                            if (ne.Name == "sp")
                            {
                                DoxHighlightText ht = new DoxHighlightText();
                                h.Text.Add(ht);
                                ht.Text = ne.Value;
                                ht.Space = true;
                            }
                            else if (ne.Name == "ref")
                            {
                                DoxHighlightText ht = new DoxHighlightText();
                                h.Text.Add(ht);
                                ht.Text = ne.Value;
                                ht.Reference = new DoxReference();
                                if (!this.ParseRefText(ht.Reference, ne)) return false;
                            }
                            else
                            {
                                this.Log(LogKind.Warning, "Unexpected element: " + ne.Name, ne);
                            }
                        }
                        else
                        {
                            this.Log(LogKind.Warning, "Unexpected node", n);
                        }
                    }
                }
            }
            return true;
        }

        private bool ParseDescription(Description d, XElement de, int level = 1)
        {
            XElement te = de.Element("title");
            d.Title = te != null ? te.Value : null;
            foreach (var pe in de.Elements("para"))
            {
                DocPara dp = new DocPara();
                d.Paragraphs.Add(dp);
                if (!this.ParseDocCmdGroup(dp, pe)) return false;
            }
            string sectionName = string.Format("sect{0}", level);
            foreach (var se in de.Elements(sectionName))
            {
                DocSect ds = new DocSect();
                d.Sections.Add(ds);
                if (!this.ParseDescription(ds, se, level + 1)) return false;
            }
            XElement ie = de.Element("internal");
            if (ie != null)
            {
                d.Internal = new Description();
                if (!this.ParseDescription(d.Internal, ie)) return false;
            }
            return true;
        }

        private bool ParseDocCmdGroup(DocCmdGroup g, XElement ge)
        {
            foreach (var n in ge.Nodes())
            {
                if (n.NodeType == XmlNodeType.Text)
                {
                    DocText dt = new DocText();
                    g.Commands.Add(dt);
                    dt.TextKind = DocTextKind.Plain;
                    dt.Text = ((XText)n).Value;
                }
                else if (n.NodeType == XmlNodeType.Element)
                {
                    XElement e = ((XElement)n);
                    string name = e.Name.LocalName;
                    if (DoxygenParser.DocEmptyTags.Contains(name))
                    {
                        DocEmpty de = new DocEmpty();
                        g.Commands.Add(de);
                        DocEmptyKind dek;
                        if (!Enum.TryParse<DocEmptyKind>(name, out dek))
                        {
                            this.Log(LogKind.Error, "Cannot resolve DocEmptyKind: " + name, e);
                            return false;
                        }
                        de.EmptyKind = dek;
                    }
                    else if (DoxygenParser.DocMarkupTags.Contains(name))
                    {
                        DocMarkup dm = new DocMarkup();
                        g.Commands.Add(dm);
                        DocMarkupKind dmk;
                        if (!Enum.TryParse<DocMarkupKind>(name, out dmk))
                        {
                            this.Log(LogKind.Error, "Cannot resolve DocMarkupKind: " + name, e);
                            return false;
                        }
                        dm.MarkupKind = dmk;
                        if (!this.ParseDocCmdGroup(dm, e)) return false;
                    }
                    else if (DoxygenParser.DocReferenceTags.Contains(name))
                    {
                        DocReference dr = new DocReference();
                        g.Commands.Add(dr);
                        DocRefKind drk;
                        if (!Enum.TryParse<DocRefKind>(name, out drk))
                        {
                            this.Log(LogKind.Error, "Cannot resolve DocRefKind: " + name, e);
                            return false;
                        }
                        dr.RefKind = drk;
                        if (!this.ParseDocCmdGroup(dr, e)) return false;
                    }
                    else
                    {
                        DocCmd dc;
                        if (this.ParseDocCmd(out dc, e, g))
                        {
                            g.Commands.Add(dc);
                        }
                    }
                }
                else
                {
                    this.Log(LogKind.Error, "Unexpected node", n);
                    return false;
                }
            }
            return true;
        }

        private bool ParseDocCmd(out DocCmd c, XElement ce, DocCmdGroup g)
        {
            c = null;
            if (ce.Name == "ref")
            {
                DocReference r = new DocReference();
                c = r;
                return this.ParseDocReference(r, ce);
            }
            else if (ce.Name.LocalName == "para")//Paraméterlista visszaküldése feldolgozásra
            {
                ParseDocCmdGroup(g, ce);
                return false;
            }
            else if (ce.Name == "simplesect")
            {
                if (ce.LastAttribute.Value == "return")
                {
                    DocText dt = new DocText();
                    DocPara dp = new DocPara();
                    foreach (var command in g.Commands)
                    {
                        if (command.Kind != DocKind.Para)
                        {
                            dp.Commands.Add(command);
                        }
                    }
                    g.Commands.RemoveAll(item => item.Kind != DocKind.Para);
                    g.Commands.Add(dp);
                    g.Commands.Add(dt);
                    dt.TextKind = DocTextKind.Verbatim;
                    dt.Text = " Return:";
                }
                else
                {
                    this.Log(LogKind.Warning, "Unsupported element in Description: " + ce.Name, ce);
                }
                ParseDocCmdGroup(g, ce);
                return false;
            }
            else if (ce.Name.LocalName == "parameterlist" && ce.LastAttribute.Value == "param")//Paraméterlista visszaküldése feldolgozásra
            {
                ParseDocCmdGroup(g, ce);
                return false;
            }
            else if (ce.Name.LocalName == "parameteritem")//Paraméterlista egy elemének visszaküldése feldolgozásra
            {
                ParseDocCmdGroup(g, ce);
                return false;
            }
            else if (ce.Name.LocalName == "parameternamelist")
            {
                ParseDocCmdGroup(g, ce);
                return false;
            }
            else if (ce.Name.LocalName == "parametername")
            {
                DocText dt = new DocText();
                DocPara dp = new DocPara();
                foreach (var command in g.Commands)
                {
                    if (command.Kind != DocKind.Para)
                    {
                        dp.Commands.Add(command);                        
                    }
                }
                g.Commands.RemoveAll(item => item.Kind != DocKind.Para);
                g.Commands.Add(dp);
                g.Commands.Add(dt);
                dt.TextKind = DocTextKind.Verbatim;
                dt.Text = " " + ce.Value + ": ";
                return false;
            }
            else if (ce.Name.LocalName == "parameterdescription")
            {
                DocText dt = new DocText();
                g.Commands.Add(dt);
                dt.TextKind = DocTextKind.Plain;
                dt.Text = ce.Value;
                return false;
            }
            else
            {
                this.Log(LogKind.Warning, "Unsupported element in Description: " + ce.Name, ce);
            }
            return true;
        }

        private bool ParseDocReference(DocReference r, XElement re)
        {
            r.External = this.GetBoolAttributeValue(re, "external");
            XAttribute rka = re.Attribute("kindref");
            XAttribute ida = re.Attribute("refid");
            if (ida != null)
            {
                Compound rc;
                DoxMember rm;
                if (rka.Value == "compound" && this.Model.CompoundRefs.TryGetValue(ida.Value, out rc))
                {
                    r.Compound = rc;
                }
                if (rka.Value == "member" && this.Model.MemberRefs.TryGetValue(ida.Value, out rm))
                {
                    r.Member = rm;
                }
                if (r.Compound == null && r.Member == null)
                {
                    this.Log(LogKind.Error, "Cannot resolve id: " + ida.Value, ida);
                    return false;
                }
            }
            return true;
        }

        private enum LogKind
        {
            Info,
            Warning,
            Error
        }
        /// <summary>
        ///  Get the enum value from xml
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <param name="result">returned enum type</param>
        /// <param name="ignoreCase"></param>
        /// <param name="xobj"></param>
        /// <returns>False, if there aren't so enum type</returns>
        private bool FromEnum<TEnum>(string value, out TEnum result, bool ignoreCase, XObject xobj) where TEnum : struct
        {
            if (Enum.TryParse<TEnum>(value, ignoreCase, out result))
            {
                return true;
            }
            else
            {
                this.Log(LogKind.Error, "Unknown enum value: " + value, xobj);
                return false;
            }
        }
    }
}
