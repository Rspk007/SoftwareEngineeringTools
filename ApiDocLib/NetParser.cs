using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SoftwareEngineeringTools.Documentation
{
    public class NetParser : Parser
    {
        private string currentFileName = null;
        private StreamWriter LogFile;
        private string projectname;

        /// <summary>
        /// Current path
        /// </summary>
        public string IndexFile { get; private set; }
        private List<string> DllFiles;
        private List<string> XmlFiles;
        public DoxygenIndex Index { get; private set; }
        public DoxygenModel Model { get; private set; }
        XElement members;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Path of the folder, where are the dll, xml files.</param>
        /// <param name="logFile">Path of the logfile</param>
        public NetParser(string path, string logFile = null)
        {
            this.IndexFile = path;             
            if (logFile != null) this.LogFile = new StreamWriter(logFile);
            this.Log(LogKind.Info, "Parse started.");
            this.Parse();
            this.Log(LogKind.Info, "Parse done.");
            if (this.LogFile != null)
            {
                this.LogFile.Dispose();
            }

        }

        /// <summary>
        /// Kind of the log
        /// </summary>
        private enum LogKind
        {
            Info,
            Warning,
            Error
        }

        /// <summary>
        /// Method, what makes the log
        /// </summary>
        /// <param name="kind">Kind of the log</param>
        /// <param name="text">Message of the log</param>
        /// <param name="xobj">XML object, where i the problem</param>
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
        /// <summary>
        /// Read the XML file and return XDocument of it.
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <returns>Return the XDocument representation of the xml file.</returns>
        private static 
            XDocument LoadXml(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                string xml = reader.ReadToEnd();
                XDocument xdoc = XDocument.Parse(xml, LoadOptions.SetLineInfo);
                return xdoc;
            }
        }

        private enum DescriptionType
        {
            Brief,
            Detailed
        }

        private enum AttributeType
        {
            Method,
            Field,
            Type,
            Property
        }
        private string getDescription(string name, DescriptionType type)
        {
            XElement[] elements = this.members.Elements().Where(elem => elem.Attributes().First().Value.Substring(2).Equals(name)).ToArray();
            if(elements.Length>0)
            {
                if(type == DescriptionType.Detailed)
                {
                    string returnString = "";
                    XElement[] insideElements = elements.Elements().ToArray();
                    foreach (var item in insideElements)
                    {
                        if (item.Name == "summary" && !String.IsNullOrEmpty(item.Value))
                        {
                            returnString += item.Value.Replace("\n",string.Empty).Replace("  ",string.Empty) + '\n';
                        }
                        else if (item.Name == "param" && !String.IsNullOrEmpty(item.Value))
                        {
                            returnString += item.FirstAttribute.Value.Replace("\n", string.Empty).Replace("  ", string.Empty) + ": ";
                            returnString += item.Value.Replace("\n", string.Empty).Replace("  ", string.Empty) + '\n';
                        }
                        else if (item.Name == "returns" && !String.IsNullOrEmpty(item.Value))
                        {
                            returnString += "Return: \n";
                            returnString += item.Value.Replace("\n", string.Empty).Replace("  ", string.Empty) + '\n';
                        }
                    }
                    return returnString;
                }
                else
                {
                    XElement[] insideElements = elements.Elements().ToArray();
                    foreach (var item in insideElements)
                    {
                        if(item.Name == "summary")
                        {
                            return item.Value.Replace("\n", string.Empty).Replace("  ", string.Empty);
                        }
                    }
                }
            }
            else if (elements.Length>1)
            {
                Log(LogKind.Error, "More than 1 line found.");
            }
            
            return null;
        }

        private void Parse()
        {
            this.DllFiles = System.IO.Directory.GetFiles(this.IndexFile, "*.dll").ToList();
            this.DllFiles.AddRange(System.IO.Directory.GetFiles(this.IndexFile, "*.exe").ToList());
            this.XmlFiles = System.IO.Directory.GetFiles(this.IndexFile, "*.xml").ToList();
            
            //Exe fileok is dll-ként beolvshatóak
            this.Index = new DoxygenIndex();
            this.Model = new DoxygenModel();
            foreach (string path in DllFiles)
            {
                ParseDll(path);
            }
        }


        /// <summary>
        /// Open the dll and get all parameter, enum and method in it.
        /// </summary>
        /// <param name="path">Path of the dll</param>
        private void ParseDll(string path)
        {
            XDocument xdoc = null;
            try
            {
                xdoc = LoadXml(path.Substring(0, path.Length - 3) + "xml");
            }
            catch (Exception)
            {

            }
            if (xdoc != null)
            {
                XElement root = xdoc.Element("doc");
                this.members = root.Element("members");
            }
            else
            {
                this.members = null;
            }
            Assembly ass = Assembly.LoadFrom(path);
            Type[] Classes = ass.GetTypes().Where(t => t.IsClass == true).ToArray();
            this.projectname = Classes[0].Namespace.Split('.')[0];
            foreach (Type Class in Classes)
            {                
                ParseClass(Class);
            }
            Type[] Interfaces = ass.GetTypes().Where(t => t.IsInterface == true && t.Namespace.Split('.')[0] == this.projectname).ToArray();
            foreach (Type Interface in Interfaces)
            {
                parseInterface(Interface);
            }
            Type[] Enums = ass.GetTypes().Where(t => t.IsEnum == true && t.Namespace.Split('.')[0] == this.projectname).ToArray();
            foreach (Type Enum in Enums)
            {
                parseEnum(Enum);
            }
        }

        private void ParseClass(Type Class)
        {
            if(Class.Namespace == null || Class.Name.Any(c => c == '<' || c == '>'))
            {
                //this.Log(LogKind.Warning,Class.ToString()+ " isn't in any namespace.");
                return;               
            }
            FieldInfo[] field = Class.GetFields(BindingFlags.NonPublic | BindingFlags.Public
         | BindingFlags.Instance | BindingFlags.Static);
            PropertyInfo[] propertys = Class.GetProperties(
        BindingFlags.NonPublic | BindingFlags.Public
        | BindingFlags.Instance | BindingFlags.Static);
            MethodInfo[] methods = Class.GetMethods(BindingFlags.NonPublic | BindingFlags.Public
        | BindingFlags.Instance | BindingFlags.Static);
            ConstructorInfo[] constructors = Class.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public
        | BindingFlags.Instance | BindingFlags.Static);
            Type[] NestedTypes = Class.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public
        | BindingFlags.Instance | BindingFlags.Static);
 
            DoxNamespace dns = (DoxNamespace)Model.Compounds.FirstOrDefault(c => c.Identifier.Equals(Class.Namespace));
            if(dns == null)
            {
                CompoundIndex ci = new CompoundIndex();
                this.Index.Compounds.Add(ci);
                ci.Name = Class.Namespace;
                ci.Identifier = ci.Name;
                ci.Kind = CompoundKind.Namespace;

                DoxNamespace c = new DoxNamespace();
                c.Identifier = ci.Identifier;
                c.Name = ci.Name;
                c.Kind = ci.Kind;

                dns = c;

                ci.Compound = c;
                this.Model.Compounds.Add(c);
                this.Index.CompoundIndexRefs.Add(ci.Identifier, ci);
                this.Model.CompoundRefs.Add(ci.Identifier, ci.Compound); 
            }
            DoxClass dc = new DoxClass();
            dc.Abstract = Class.IsAbstract;
            dc.Identifier = Class.FullName;
            dc.Kind = CompoundKind.Class;
            dc.Name = Class.FullName;
            dc.Sealed = Class.IsSealed;

            if (propertys.Length > 0)
            {
                parseProperty(propertys, dc);
            }
            if (field.Length > 0)
            {
                parseField(field, dc);
            }
            if (constructors.Length > 0)
            {
                parseCtor(constructors, dc);
            }
            if(methods.Length > 0)
            {
                parseMethod(methods, dc);
            }


            dns.Classifiers.Add(dc);
            CompoundIndex compi = new CompoundIndex();
            this.Index.Compounds.Add(compi);
            compi.Name = dc.Name;
            compi.Identifier = dc.Identifier;
            compi.Kind = CompoundKind.Namespace;
            compi.Compound = dc;
            this.Model.Compounds.Add(dc);
            this.Index.CompoundIndexRefs.Add(compi.Identifier, compi);
            this.Model.CompoundRefs.Add(compi.Identifier, compi.Compound); 
        }

        private void parseInterface(Type Interface)
        {
            if (Interface.Namespace == null || Interface.Name.Any(c => c == '<' || c == '>'))
            {
                //this.Log(LogKind.Warning,Class.ToString()+ " isn't in any namespace.");
                return;               
            }
            FieldInfo[] field = Interface.GetFields(BindingFlags.NonPublic | BindingFlags.Public
        | BindingFlags.Instance | BindingFlags.Static);
            PropertyInfo[] propertys = Interface.GetProperties(
        BindingFlags.NonPublic | BindingFlags.Public
        | BindingFlags.Instance | BindingFlags.Static);
            MethodInfo[] methods = Interface.GetMethods(BindingFlags.NonPublic | BindingFlags.Public
        | BindingFlags.Instance | BindingFlags.Static);
            ConstructorInfo[] constructors = Interface.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public
        | BindingFlags.Instance | BindingFlags.Static);
            Type[] NestedTypes = Interface.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public
        | BindingFlags.Instance | BindingFlags.Static);

            DoxNamespace dns = (DoxNamespace)Model.Compounds.FirstOrDefault(c => c.Identifier.Equals(Interface.Namespace));
            if(dns == null)
            {
                CompoundIndex ci = new CompoundIndex();
                this.Index.Compounds.Add(ci);
                ci.Name = Interface.Namespace;
                ci.Identifier = ci.Name;
                ci.Kind = CompoundKind.Namespace;

                DoxNamespace c = new DoxNamespace();
                c.Identifier = ci.Identifier;
                c.Name = ci.Name;
                c.Kind = ci.Kind;

                dns = c;

                ci.Compound = c;
                this.Model.Compounds.Add(c);
                this.Index.CompoundIndexRefs.Add(ci.Identifier, ci);
                this.Model.CompoundRefs.Add(ci.Identifier, ci.Compound); 
            }
            DoxInterface dc = new DoxInterface();
            dc.Abstract = Interface.IsAbstract;
            dc.Identifier = Interface.FullName;
            dc.Kind = CompoundKind.Interface;
            dc.Name = Interface.FullName;
            dc.Sealed = Interface.IsSealed;

            if (propertys.Length > 0)
            {
                parseProperty(propertys, dc);
            }
            if (field.Length > 0)
            {
                parseField(field, dc);
            }
            if (constructors.Length > 0)
            {
                parseCtor(constructors, dc);
            }
            if(methods.Length > 0)
            {
                parseMethod(methods, dc);
            }
            

            dns.Classifiers.Add(dc);
            CompoundIndex compi = new CompoundIndex();
            this.Index.Compounds.Add(compi);
            compi.Name = dc.Name;
            compi.Identifier = dc.Identifier;
            compi.Kind = CompoundKind.Namespace;
            compi.Compound = dc;
            this.Model.Compounds.Add(dc);
            this.Index.CompoundIndexRefs.Add(compi.Identifier, compi);
            this.Model.CompoundRefs.Add(compi.Identifier, compi.Compound);         
        }

        private void parseEnum(Type Enum)
        {
            if (Enum.Namespace == null || Enum.Name.Any(c => c == '<' || c == '>'))
            {
                //this.Log(LogKind.Warning,Class.ToString()+ " isn't in any namespace.");
                return;
            }
            FieldInfo[] field = Enum.GetFields().Where(c => !c.Name.Equals("value__")).ToArray();
            PropertyInfo[] propertys = Enum.GetProperties();
            MethodInfo[] methods = Enum.GetMethods();
            ConstructorInfo[] constructors = Enum.GetConstructors();
            Type[] NestedTypes = Enum.GetNestedTypes();

            DoxNamespace dns = (DoxNamespace)Model.Compounds.FirstOrDefault(c => c.Identifier.Equals(Enum.Namespace));
            if (dns == null)
            {
                CompoundIndex ci = new CompoundIndex();
                this.Index.Compounds.Add(ci);
                ci.Name = Enum.Namespace;
                ci.Identifier = ci.Name;
                ci.Kind = CompoundKind.Namespace;

                DoxNamespace c = new DoxNamespace();
                c.Identifier = ci.Identifier;
                c.Name = ci.Name;
                c.Kind = ci.Kind;

                dns = c; 

                ci.Compound = c;
                this.Model.Compounds.Add(c);
                this.Index.CompoundIndexRefs.Add(ci.Identifier, ci);
                this.Model.CompoundRefs.Add(ci.Identifier, ci.Compound);
            }
            DoxEnum dc = new DoxEnum();
            dc.Abstract = Enum.IsAbstract;
            dc.Identifier = Enum.FullName.Replace('+', '.');
            dc.Kind = CompoundKind.Enum;
            dc.Name = Enum.FullName;
            dc.Sealed = Enum.IsSealed;

            if (propertys.Length > 0)
            {
                parseProperty(propertys, dc);
            }
            if (field.Length > 0)
            {
                parseField(field, dc);
            }
            if (methods.Length > 0)
            {
                parseMethod(methods, dc);
            }
            if (constructors.Length > 0)
            {
                parseCtor(constructors, dc);
            }

            dns.Classifiers.Add(dc);
            CompoundIndex compi = new CompoundIndex();
            this.Index.Compounds.Add(compi);
            compi.Name = dc.Name;
            compi.Identifier = dc.Identifier;
            compi.Kind = CompoundKind.Namespace;
            compi.Compound = dc;
            this.Model.Compounds.Add(dc);
            this.Index.CompoundIndexRefs.Add(compi.Identifier, compi);
            this.Model.CompoundRefs.Add(compi.Identifier, compi.Compound); 
        }

        /// <summary>
        /// Parse the properties.
        /// </summary>
        /// <param name="propertys">Array of the propoerties</param>
        /// <param name="dc">The base classifier, where the properties are.</param>
        private void parseProperty(PropertyInfo[] propertys, DoxClassifier dc)
        {
            foreach (PropertyInfo property in propertys)
            {
                DoxProperty dp = new DoxProperty();
                if(property.Name.First().Equals('<')|| property.Name.Contains("<>"))
                {
                    continue;
                }
                dp.Name = property.Name;
                dp.Identifier = dc.Name.Replace('+','.') + '.' + dp.Name;
                dp.Sealed = property.PropertyType.IsSealed;
                if (property.PropertyType.GenericTypeArguments.Length > 0)
                {
                    String type = property.PropertyType.Name.Substring(0, property.PropertyType.Name.Length - 2);
                    type = type + '<';
                    bool first = true;
                    foreach (var item in property.PropertyType.GenericTypeArguments)
                    {
                        if (first)
                        {
                            type = type + item.Name;
                            first = false;
                        }
                        else
                        {
                            type = type + ", " + item.Name;
                        }
                    }
                    type = type + '>';
                    dp.Definition = type;
                }
                else
                {
                    dp.Definition = property.PropertyType.Name;
                }
                String[] attributes = property.PropertyType.Attributes.ToString().Replace(" ", string.Empty).Split(',');
                foreach (string attr in attributes)
                {
                    switch (attr)
                    {
                        case "Public":
                        case "NestedPublic":
                            dp.ProtectionKind = ProtectionKind.Public;
                            break;
                        case "Protected":
                            dp.ProtectionKind = ProtectionKind.Protected;
                            break;
                        case "NestedPrivate":
                        case "Private":
                            dp.ProtectionKind = ProtectionKind.Private;
                            break;
                        case "Abstract":
                            dp.VirtualKind = VirtualKind.Abstract;                            
                            break;
                        case "Serializable":
                        case "Sealed":
                        case "AutoLayout":
                        case "AnsiClass":
                        case "Class":
                        case "SequentialLayout":
                        case "ClassSemanticsMask":
                        case "HasSecurity":
                        case "BeforeFieldInit":
                            break;
                        default:
                            this.Log(LogKind.Warning, attr + " can't be processed as attribute of attributes.");
                            break;
                    }
                }
                dc.Members.Add(dp);
                //String briefDescription = getDescription(dp.Identifier, DescriptionType.Brief);
                //String detailedDescription = getDescription(dp.Identifier, DescriptionType.Detailed);
                MemberIndex mi = new MemberIndex();
                mi.Identifier = dp.Identifier;
                mi.Kind = dp.Kind;
                mi.Member = dp;
                mi.Name = dp.Name;
                try
                {
                    this.Model.MemberRefs.Add(dp.Identifier, dp);
                    this.Index.MemberIndexRefs.Add(dp.Identifier, mi);
                }
                catch(ArgumentException ae)
                {
                    this.Log(LogKind.Error, ae.Message + " " + mi.Identifier);
                }

            }
        }

        private void parseField(FieldInfo[] fields, DoxClassifier dc)
        {
            foreach (FieldInfo field in fields)
            {
                if (field.Name.First().Equals('<') || field.Name.Contains("<>"))
                {
                    continue;
                }
                DoxField dp = new DoxField();
                dp.Name = field.Name;
                dp.Identifier = dc.Name.Replace('+','.') + '.' + dp.Name;
                dp.Sealed = field.FieldType.IsSealed;
                if (field.FieldType.GenericTypeArguments.Length > 0)
                {
                    String type = field.FieldType.Name.Substring(0, field.FieldType.Name.Length - 2);
                    type = type + '<';
                    bool first = true;
                    foreach (var item in field.FieldType.GenericTypeArguments)
                    {
                        if (first)
                        {
                            type = type + item.Name;
                            first = false;
                        }
                        else
                        {
                            type = type + ", " + item.Name;
                        }
                    }
                    type = type + '>';
                    dp.Definition = type;
                }
                else
                {
                    dp.Definition = field.FieldType.Name;
                }
                String[] attributes = field.FieldType.Attributes.ToString().Replace(" ", string.Empty).Split(',');
                foreach (string attr in attributes)
                {
                    switch (attr)
                    {
                        case "Public":
                        case "NestedPublic":
                            dp.ProtectionKind = ProtectionKind.Public;
                            break;
                        case "Protected":
                            dp.ProtectionKind = ProtectionKind.Protected;
                            break;
                        case "NestedPrivate":
                        case "Private":
                            dp.ProtectionKind = ProtectionKind.Private;
                            break;
                        case "Abstract":
                            dp.VirtualKind = VirtualKind.Abstract;
                            break;
                        case "Serializable":
                        case "Sealed":
                        case "AutoLayout":
                        case "AnsiClass":
                        case "Class":
                        case "SequentialLayout":
                        case "BeforeFieldInit":
                        case "ClassSemanticsMask":
                        case "Import":
                            break;
                        default:
                            this.Log(LogKind.Warning, attr + " can't be processed as attribute of fields.");
                            break;
                    }
                }
                dc.Members.Add(dp);
                getDescription(dp.Identifier, DescriptionType.Detailed);
                MemberIndex mi = new MemberIndex();
                mi.Identifier = dp.Identifier;
                mi.Kind = dp.Kind;
                mi.Member = dp;
                mi.Name = dp.Name;
                try
                {
                    this.Model.MemberRefs.Add(dp.Identifier, dp);
                    this.Index.MemberIndexRefs.Add(dp.Identifier, mi);
                }
                catch(ArgumentException ae)
                {
                    this.Log(LogKind.Error, ae.Message + " " + mi.Name);
                }

            }
        }
        private void parseMethod(MethodInfo[] methods, DoxClassifier dc)
        {
            foreach (MethodInfo method in methods)
            {
                if(method.Name.Length > 4)
                {
                    if(method.Name.Substring(0,4).Equals("get_") || method.Name.Substring(0,4).Equals("set_"))
                    {
                        continue;
                    }
                    else if (method.Name.First().Equals('<') || method.Name.Contains("<>"))
                    {
                        continue;
                    }
                }
                DoxMethod dp = new DoxMethod();
                dp.Name = method.Name;                
                dp.Identifier = dc.Identifier.Replace('+','.') + "." + dp.Name;
                String xmlName = dp.Identifier;
                ParameterInfo[] parameters = method.GetParameters();

                if (method.ReturnType.GenericTypeArguments.Length > 0)
                {
                    String type = method.ReturnType.Name.Substring(0, method.ReturnType.Name.Length - 2);
                    type = type + '<';
                    bool first = true;
                    foreach (var item in method.ReturnType.GenericTypeArguments)
                    {
                        if (first)
                        {
                            type = type + item.Name;
                            first = false;
                        }
                        else
                        {
                            type = type + ", " + item.Name;
                        }
                    }
                    type = type + '>';
                    dp.Definition = type;
                }
                else
                {
                    dp.Definition = method.ReturnType.Name;
                }

                if(parameters.Length > 0)
                {
                    xmlName += "(";
                }
                foreach (ParameterInfo param in parameters)
                {
                    DoxLinkedTextItem dlti = new DoxLinkedTextItem(); 
                    if (param.ParameterType.GenericTypeArguments.Length > 0)
                    {
                        String type = param.ParameterType.Name.Substring(0, param.ParameterType.Name.Length - 2);
                        type = type + '<';
                        bool first = true;
                        foreach (var item in param.ParameterType.GenericTypeArguments)
                        {
                            if (first)
                            {
                                type = type + item.Name;
                                first = false;
                            }
                            else
                            {
                                type = type + ", " + item.Name;
                            }
                        }
                        type = type + '>';
                        dlti.Text = type;
                        dp.Identifier = dp.Identifier + '_' + type;                        
                    }
                    else
                    {
                        dlti.Text = param.ParameterType.Name;
                        dp.Identifier = dp.Identifier + '_' + dlti.Text;
                    }
                    if (param != parameters.Last())
                    {
                        if (param.ParameterType.FullName != null)
                        {
                            xmlName += param.ParameterType.FullName.Replace('+', '.') + ",";
                        }
                        else
                        {
                            xmlName += param.ParameterType.Name.Replace('+', '.') + ",";
                        }

                    }
                    else
                    {
                        if (param.ParameterType.FullName != null)
                        {
                            xmlName += param.ParameterType.FullName.Replace('+', '.') + ")";
                        }
                        else
                        {
                            xmlName += param.ParameterType.Name.Replace('+', '.') + ")";
                        }
                       
                    }
                    DoxLinkedText dlt = new DoxLinkedText();
                    dlt.Items.Add(dlti);
                    
                    DoxParam DPara = new DoxParam();
                    DPara.DeclarationName = param.Name;
                    DPara.Type = dlt;
                    dp.Params.Add(DPara);
                }

                
                String[] attributes = method.Attributes.ToString().Replace(" ", string.Empty).Split(',');
                foreach (string attr in attributes)
                {
                    switch (attr)
                    {
                        case "Abstract":
                            dp.VirtualKind = VirtualKind.Abstract;
                            break;
                        case "Final":
                            dp.Final = true;
                            break;
                        case "Public":
                            dp.ProtectionKind = ProtectionKind.Public;
                            break;
                        case "Protected":
                        case "Family":
                            dp.ProtectionKind = ProtectionKind.Protected;
                            break;
                        case "Private":
                            dp.ProtectionKind = ProtectionKind.Private;
                            break;
                        case "Static":
                            dp.Static = true;
                            break;
                        case "Virtual":
                            dp.VirtualKind = VirtualKind.Virtual;
                            break;
                        case "Serializable":
                        case "Sealed":
                        case "AutoLayout":
                        case "AnsiClass":
                        case "Class":
                        case "SequentialLayout":
                        case "BeforeFieldInit":
                        case "PrivateScope":
                        case "HideBySig":
                        case "SpecialName":
                        case "VtableLayoutMask":
                        case "Assembly":
                        case "CheckAccessOnOverride":
                            break;
                        default:
                            this.Log(LogKind.Warning, attr + " can't be processed as attribute of porpoerty.");
                            break;
                    }
                }
                dc.Members.Add(dp);                
                getDescription(xmlName, DescriptionType.Detailed);
                getDescription(xmlName, DescriptionType.Brief);
                MemberIndex mi = new MemberIndex();
                mi.Identifier = dp.Identifier;
                mi.Kind = dp.Kind;
                mi.Member = dp;
                mi.Name = dp.Name;
                try
                {
                    this.Model.MemberRefs.Add(dp.Identifier, dp);
                }
                catch (Exception)
                {
                    continue;
                }
                this.Index.MemberIndexRefs.Add(dp.Identifier, mi);

            }

        }
        private void parseCtor(ConstructorInfo[] constructors, DoxClassifier dc)
        {
            foreach (ConstructorInfo constructor in constructors)
            {
                DoxMethod dp = new DoxMethod();
                dp.Name = dc.Identifier.Substring(dc.Identifier.LastIndexOf('.')+1);
                dp.Identifier = dc.Identifier + "." + dp.Name;
                String xmlName = dc.Identifier + "." + "#ctor";
                ParameterInfo[] parameters = constructor.GetParameters();              

                if(parameters.Length > 0)
                {
                    xmlName += "(";
                }
                foreach (ParameterInfo param in parameters)
                {
                    DoxLinkedTextItem dlti = new DoxLinkedTextItem();
                    if (param.ParameterType.GenericTypeArguments.Length > 0)
                    {
                        String type = param.ParameterType.Name.Substring(0, param.ParameterType.Name.Length - 2);
                        type = type + '<';
                        bool first = true;
                        foreach (var item in param.ParameterType.GenericTypeArguments)
                        {
                            if (first)
                            {
                                type = type + item.Name;
                                first = false;
                            }
                            else
                            {
                                type = type + ", " + item.Name;
                            }
                        }
                        type = type + '>';
                        dlti.Text = type;
                        dp.Identifier = dp.Identifier + '_' + type;
                    }
                    else
                    {
                        dlti.Text = param.ParameterType.Name;
                        dp.Identifier = dp.Identifier + '_' + dlti.Text;
                    }
                    if(param != parameters.Last())
                    {
                        xmlName += param.ParameterType.FullName.Replace('+','.') + ",";
                    }
                    else
                    {
                        xmlName += param.ParameterType.FullName.Replace('+', '.') + ")";
                    }
                    DoxLinkedText dlt = new DoxLinkedText();
                    dlt.Items.Add(dlti);

                    DoxParam DPara = new DoxParam();
                    DPara.DeclarationName = param.Name;
                    DPara.Type = dlt;
                    dp.Params.Add(DPara);
                }


                String[] attributes = constructor.Attributes.ToString().Replace(" ", string.Empty).Split(',');
                foreach (string attr in attributes)
                {
                    switch (attr)
                    {
                        case "Abstract":
                            dp.VirtualKind = VirtualKind.Abstract;
                            break;
                        case "Final":
                            dp.Final = true;
                            break;
                        case "Public":
                            dp.ProtectionKind = ProtectionKind.Public;
                            break;
                        case "Protected":
                        case "Family":
                            dp.ProtectionKind = ProtectionKind.Protected;
                            break;
                        case "Private":
                            dp.ProtectionKind = ProtectionKind.Private;
                            break;
                        case "Static":
                            dp.Static = true;
                            break;
                        case "Virtual":
                            dp.VirtualKind = VirtualKind.Virtual;
                            break;
                        case "Serializable":
                        case "Sealed":
                        case "AutoLayout":
                        case "AnsiClass":
                        case "Class":
                        case "SequentialLayout":
                        case "BeforeFieldInit":
                        case "PrivateScope":
                        case "HideBySig":
                        case "SpecialName":
                        case "VtableLayoutMask":
                        case "RTSpecialName":
                            break;
                        default:
                            this.Log(LogKind.Warning, attr + " can't be processed as attribute of porpoerty.");
                            break;
                    }
                }
                dc.Members.Add(dp);
                getDescription(xmlName, DescriptionType.Detailed);
                MemberIndex mi = new MemberIndex();
                mi.Identifier = dp.Identifier;
                mi.Kind = dp.Kind;
                mi.Member = dp;
                mi.Name = dp.Name;
                try
                {
                    this.Model.MemberRefs.Add(dp.Identifier, dp);
                }
                catch (Exception)
                {
                    continue;
                }
                this.Index.MemberIndexRefs.Add(dp.Identifier, mi);

            }
        }
    }
}
