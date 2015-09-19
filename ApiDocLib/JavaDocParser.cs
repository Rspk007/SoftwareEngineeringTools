using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;


namespace SoftwareEngineeringTools.Documentation
{
    public class JavaDocParser
    {
        private string currentFileName = null;
        private StreamWriter LogFile;

        public string IndexFile { get; private set; }
        private List<string> ProcessPackages;
        private List<string> ProcessCompounds;
        public DoxygenIndex Index { get; private set; }
        public DoxygenModel Model { get; private set; }

        public JavaDocParser(string indexFile, string logFile = null)
        {
            this.IndexFile = indexFile;
            ProcessPackages = new List<string>();
            ProcessCompounds = new List<string>();
            if (logFile != null) this.LogFile = new StreamWriter(logFile);
            this.Parse();
            if (this.LogFile != null)
            {
                this.LogFile.Dispose();
            }

        }

        private enum LogKind
        {
            Info,
            Warning,
            Error
        }

        private void Log(LogKind kind, string text, HtmlNode tnode = null)
        {
            string message = null;
            if (tnode != null)
            {
                if (this.currentFileName != null)
                {
                    message = string.Format("{0} in {2} at ({3},{4}): {1}", kind.ToString().ToUpper(), text, this.currentFileName, tnode.Line, tnode.LinePosition);
                }
                else
                {
                    message = string.Format("{0} at ({3},{4}): {1}", kind.ToString().ToUpper(), text, tnode.Line, tnode.LinePosition);
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

       private string normalizeHtml (String input)
        {
            String output = input.Replace("&nbsp;", " ");
            output = output.Replace("&lt;", "<");
            output = output.Replace("&gt;", ">");
            return output;
           
        }

        private string getId (string args)
       {
           string[] output = args.Replace("(", string.Empty).Replace(")", string.Empty).Replace("[", "_").Replace("]", string.Empty).Split(' ');
           output = output.Where(c => !String.IsNullOrEmpty(c)).ToArray();
            for (int i = 0; i< output.Length; i++)
            {
              string[] outp = output[i].Replace("...",string.Empty).Split('.');
              output[i] = outp[outp.Length - 1];
            }          
           string ret = "";
           for (int i = 0; i < output.Length; i = i+2 )
           {
               if(!String.IsNullOrEmpty(output[i]))
               {
                   ret += output[i] + "_";
               }
           }
           return ret;
       }
        private Description getDescription(HtmlNode descNode)
        {
            Description retDescription = new Description();
            DocPara dp = new DocPara();
            DocText dt = new DocText();
            DocPara newPara;
            bool newParagraph = false;
            foreach (HtmlNode childnode in descNode.ChildNodes.Where(c => c.InnerText != "\r\n"))
            {
                switch (childnode.Name)
                {
                    case "#text":
                        //if (newParagraph == true)
                        //{
                        //    newPara = new DocPara();
                        //    dt = new DocText();
                        //    dt.TextKind = DocTextKind.Plain;
                        //    dt.Text = childnode.InnerText.Replace("\r\n \r\n", "\n");
                        //    newPara.Commands.Add(dt);
                        //    dp.Commands.Add(newPara);
                        //    newParagraph = false;
                        //}
                        //else
                        //{
                            dt = new DocText();
                            dt.TextKind = DocTextKind.Plain;
                            dt.Text = childnode.InnerText.Replace("\r\n \r\n", "\n");
                            dp.Commands.Add(dt);
                        //}
                        break;
                    case "dt":
                        newPara = new DocPara();
                        dt = new DocText();
                        dt.TextKind = DocTextKind.Verbatim;
                        dt.Text = childnode.InnerText.Replace("\r\n \r\n", "\n");
                        newPara.Commands.Add(dt);
                        dp.Commands.Add(newPara);
                        break;
                    case "span":
                        dt = new DocText();
                        dt.TextKind = DocTextKind.Verbatim;
                        dt.Text = childnode.InnerText.Replace("\r\n \r\n", "\n");
                        dp.Commands.Add(dt);
                        break;                        
                    case "code":
                    case "i":
                        DocMarkup dm = new DocMarkup();
                        dm.MarkupKind = DocMarkupKind.Emphasis;
                        dt = new DocText();
                        dt.TextKind = DocTextKind.Plain;
                        dt.Text = childnode.InnerText.Replace("\r\n \r\n", "\n");
                        dm.Commands.Add(dt);
                        dp.Commands.Add(dm);
                        break;
                    case "dd":
                    case "div":
                        Description partialDescription = this.getDescription(childnode);
                        DocPara partialPara = new DocPara();
                        partialPara.Commands.AddRange(partialDescription.Paragraphs[0].Commands);
                        dp.Commands.Add(partialPara);
                        break;
                    case "p":
                        newPara = new DocPara();
                        dp.Commands.Add(newPara);
                        //newParagraph = true;
                        break;
                    case "a":
                        String path = childnode.Attributes[0].Value.Replace("../", string.Empty);
                        String package = path.Split('/')[0];
                        String obj = path.Replace(".html", string.Empty).Split('/')[1];
                        DocReference dref = new DocReference();
                        DocText dtx = new DocText();
                        newPara = new DocPara();
                        dtx.TextKind = DocTextKind.Plain;
                        if (path.Split('/').Length == 3)
                        {
                            String member = path.Split('/')[2];
                            dref.Member = this.Model.MemberRefs.Single(c => c.Key == package + "_" + obj + "_" + member).Value;
                            dtx.Text = member;
                        }
                        else
                        {
                            dref.Compound = this.Model.Compounds.Single(c => c.Identifier == package + "_" + obj);
                            dtx.Text = obj;
                        }
                        dref.Commands.Add(dtx);
                        dp.Commands.Add(dref);
                        break;
                    case "pre":
                        dm = new DocMarkup();
                        dm.MarkupKind = DocMarkupKind.Emphasis;
                        dt = new DocText();
                        dt.TextKind = DocTextKind.Plain;                        
                        dt.Text = WebUtility.HtmlDecode(childnode.InnerText);
                        dm.Commands.Add(dt);
                        dp.Commands.Add(dm);
                        break;
                    default:
                        break;
                }
            }            
            retDescription.Paragraphs.Add(dp);
            return retDescription;
        }

        private void Parse()
        {
            this.Index = new DoxygenIndex();
            this.Model = new DoxygenModel();
            this.ParsePackages();
            foreach (var package in this.ProcessPackages)
            {
                this.ParseCompounds(package);
            }
            foreach (var compound in this.ProcessCompounds)
            {
                this.ParseCompoundsInside(compound);
            }
            //this.CreateModelElements();
            //this.ParseModel();
        }

        private void ParsePackages()
        {
            this.currentFileName = this.IndexFile;
            HtmlDocument document = new HtmlDocument();
            try
            {
                document.Load(currentFileName);
            }
            catch (Exception)
            {
                this.Log(LogKind.Error, "Can't open " + this.currentFileName + ".");
                return;
            }
            HtmlNode root = document.DocumentNode;

            // Get the HtmlNode, what represents the html tag
            HtmlNode htmlnode = root.ChildNodes.First(node => node.Name == "html");
            // Get the node, what represents the head tag
            HtmlNode headNode = null;
            try
            {
                headNode = htmlnode.ChildNodes.First(node => node.Name == "head");
            }
            // If the aren't any head tag, than the html is invalid or corrupted
            catch(InvalidOperationException)
            {
                this.Log(LogKind.Error, "Missing head tag in the html. Program is terminating.");
                return;
            }
            // Find the comment, where the version of the javadoc was writen
            HtmlNode versionnode = headNode.ChildNodes.First(node => node.Name == "#comment");
            String version = versionnode.InnerHtml;
            version = version.Split('(', ')')[1];
            this.Index.Version = version;

            HtmlNode bodyNode = null;
            try
            {
                bodyNode = htmlnode.ChildNodes.First(node => node.Name == "body");
            }
            // If the aren't any body tag, than the html is invalid or corrupted
            catch (InvalidOperationException)
            {
                this.Log(LogKind.Error, "Missing body tag in the html. Program is terminating.");
                return;
            }
            // Get the div tag, what contains the informations about the packages
            HtmlNode packageDivNode = null;
            try
            {
                packageDivNode = bodyNode.ChildNodes.First(node => node.Name == "div" && node.Attributes.FirstOrDefault(attr => attr.Name == "class" && attr.Value =="contentContainer")!=null);
            }
            // If the program can't find the package, than it exit with error.
            catch (InvalidOperationException iox)
            {
                this.Log(LogKind.Error, "Can't find the packages. Program is terminating.");
                return;
            }
            // Get the table, where is the data of the packages.
            HtmlNode packageTableNode = null;
            try
            {
                packageTableNode = packageDivNode.ChildNodes.First(node => node.Name == "table").ChildNodes.First(node => node.Name == "tbody");
            }
            catch (InvalidOperationException iox)
            {
                this.Log(LogKind.Error, "Can't find the data of the packages. Program is terminating.");
                return;
            }
            foreach (var packagerow in packageTableNode.ChildNodes.Where(node => node.Name == "tr"))
            {
                HtmlNode[] datas = packagerow.ChildNodes.Where(node => node.Name == "td").ToArray();
                CompoundIndex ci = new CompoundIndex();
                this.Index.Compounds.Add(ci);
                ci.Name = datas[0].ChildNodes.ElementAt(0).InnerHtml;
                ci.Identifier = ci.Name;
                ci.Kind = CompoundKind.Namespace;

                DoxNamespace c = new DoxNamespace();
                c.Identifier = ci.Identifier;
                c.Name = ci.Name;
                c.Kind = ci.Kind;
                
                
                this.ProcessPackages.Add(datas[0].ChildNodes.ElementAt(0).Attributes.ElementAt(0).Value);
                // NEED TO CHANGE!
                HtmlNode desc = datas[1].ChildNodes.FirstOrDefault(node => node.Name == "div" && node.Attributes.FirstOrDefault(attr => attr.Name == "class" && attr.Value == "block") != null);
                if(desc != null)
                {
                    String Description = normalizeHtml(desc.InnerText);
                }
                

                ci.Compound = c;

                this.Model.Compounds.Add(c);
                this.Index.CompoundIndexRefs.Add(ci.Identifier, ci);
                this.Model.CompoundRefs.Add(ci.Identifier, ci.Compound);   
            }
            this.Log(LogKind.Info, "Processing of " + this.currentFileName + " has ended.");
        }

        private void ParseCompounds (string path)
        {
            this.currentFileName = @"..\..\javadoc\html\" + path;
            HtmlDocument document = new HtmlDocument();
            try
            {
                document.Load(currentFileName);
            }
            catch(Exception)
            {
                this.Log(LogKind.Warning, "Can't open "+path);
                return;
            }
            HtmlNode root = document.DocumentNode;

            String packagename = path.Split('/')[0];
            DoxNamespace ns = (DoxNamespace) Model.Compounds.First(c => c.Name == packagename);

            // Get the HtmlNode, what represents the html tag
            HtmlNode htmlnode = root.ChildNodes.First(node => node.Name == "html");
            HtmlNode bodyNode = null;
            try
            {
                bodyNode = htmlnode.ChildNodes.First(node => node.Name == "body");
            }
            // If the aren't any body tag, than the html is invalid or corrupted
            catch (InvalidOperationException)
            {
                this.Log(LogKind.Error, "Missing body tag in the html. In " + packagename);
                return;
            }

            // Get the div tag, within the compound informations are.
            HtmlNode compoundDivNode = null;
            try
            {
                compoundDivNode = bodyNode.ChildNodes.First(node => node.Name == "div" && node.Attributes.FirstOrDefault(attr => attr.Name == "class" && attr.Value == "contentContainer") != null);
            }
            // If the program can't find the package, than it exit with error.
            catch (InvalidOperationException)
            {
                this.Log(LogKind.Error, "Can't find the compounds in the package. ");
                return;
            }

            //Get the list of the compoundinformations
            HtmlNode compoundListNode = null;
            try
            {
                compoundListNode = compoundDivNode.ChildNodes.First(node => node.Name == "ul");
            }
            // If the program can't find the list of the compounds, than it exit with error.
            catch (InvalidOperationException iox)
            {
                this.Log(LogKind.Error, "Can't find the list of the compounds in the package.");
                return;
            }

            foreach (HtmlNode CompoundTypeList in compoundListNode.ChildNodes.Where(node => node.Name == "li"))
            {
                HtmlNode CompoundTypeTable = CompoundTypeList.ChildNodes.First(node => node.Name == "table");
                string Type = normalizeHtml(CompoundTypeTable.ChildNodes.First(node => node.Name == "tr").ChildNodes.First(node => node.Name == "th").InnerText);

                foreach (HtmlNode CompoundNode in CompoundTypeTable.ChildNodes.First(node => node.Name=="tbody").ChildNodes.Where(node => node.Name=="tr"))
                {
                    HtmlNode[] CompoundRows = CompoundNode.ChildNodes.Where(node => node.Name == "td").ToArray();
                    CompoundIndex ci = new CompoundIndex();
                    this.Index.Compounds.Add(ci);
                    ParseCompound(ci, Type);
                    ci.Name = CompoundRows[0].InnerText;
                    ci.Identifier = ns.Name + "_" + ci.Name;

                    DoxClassifier c;
                    if (Type == "Class")
                    {
                        ci.Kind = CompoundKind.Class;
                        c = new DoxClass();
                    }
                    else if (Type == "Interface")
                    {
                        ci.Kind = CompoundKind.Interface;
                        c = new DoxInterface();
                    }
                    else if (Type == "Exception" || Type == "Error")
                    {
                        ci.Kind = CompoundKind.Exception;
                        c = new DoxClass();
                        
                    }
                    else if (Type == "Enum")
                    {
                        ci.Kind = CompoundKind.Enum;
                        c = new DoxEnum();
                    }
                    else
                    {
                        this.Log(LogKind.Warning, Type + " isn't a valid compound type.");
                        continue;
                    }
                                                   
                    c.Identifier = ci.Identifier;
                    c.Name = ci.Name;
                    c.Kind = ci.Kind;

                    this.ProcessCompounds.Add(CompoundRows[0].ChildNodes.First(node => node.Name == "a").Attributes.First(attr => attr.Name == "href").Value);

                    // NEED TO CHANGE!
                    String description = normalizeHtml(CompoundRows[1].InnerText);

                    ci.Compound = c;
                    ns.Classifiers.Add(c);
                    this.Model.Compounds.Add(c);
                    this.Index.CompoundIndexRefs.Add(ci.Identifier, ci);
                    this.Model.CompoundRefs.Add(ci.Identifier, ci.Compound);   
                }
                
            }
            HtmlNode briefDescriptionNode = compoundDivNode.ChildNodes.FirstOrDefault(node => node.Name == "div" && node.Attributes.First(attr => attr.Name == "class" && attr.Value == "block")!=null);
            if(briefDescriptionNode != null)
            {
                String briefDescription = normalizeHtml(briefDescriptionNode.InnerText);
            }
            this.Log(LogKind.Info, "Processing of " + this.currentFileName + " has ended.");
        }

        void ParseCompound(CompoundIndex ci, string type)
        {
            if(type == "Class")
            {
                ci.Kind = CompoundKind.Class;
            }
            else if(type == "Interface")
            {
                ci.Kind = CompoundKind.Interface;
            }
            else if(type == "Exception" || type == "Error")
            {
                ci.Kind = CompoundKind.Exception;
            }
            else if (type == "Enum")
            {
                ci.Kind = CompoundKind.Enum;
            }
            else
            {
                this.Log(LogKind.Warning, type + " isn't a valid compound type.");
            }
        }

        private void ParseCompoundsInside (string path)
        {
            this.currentFileName = @"..\..\javadoc\html\" + path.Substring(2);
            HtmlDocument document = new HtmlDocument();
            try
            {
                document.Load(currentFileName);
            }
            catch (Exception)
            {
                this.Log(LogKind.Warning, "Can't open " + path);
                return;
            }
            HtmlNode root = document.DocumentNode;



            // Get the HtmlNode, what represents the html tag
            HtmlNode htmlnode = root.ChildNodes.First(node => node.Name == "html");
            HtmlNode bodyNode = null;
            try
            {
                bodyNode = htmlnode.ChildNodes.First(node => node.Name == "body");
            }
            // If the aren't any body tag, than the html is invalid or corrupted
            catch (InvalidOperationException iox)
            {
                this.Log(LogKind.Error, "Missing body tag in the html.");
                return;
            }

            //Get the node, what contains the class data
            HtmlNode compoundDivNode = null;
            try
            {
                compoundDivNode = bodyNode.ChildNodes.First(node => node.Name == "div" && node.Attributes.FirstOrDefault(attr => attr.Name == "class" && attr.Value == "contentContainer") != null);
            }
            catch (InvalidOperationException iox)
            {
                this.Log(LogKind.Error, "Missing data of the coumpound can't find.");
                return;
            }

            String compoundname = path.Split('/')[2];
            compoundname = compoundname.Remove(compoundname.Length - 5);
            String packagename = path.Split('/')[1];
            DoxNamespace ns = (DoxNamespace)Model.Compounds.First(c => c.Name == packagename);
            Compound comp = (Compound)Model.Compounds.First(c => c.Identifier == ns.Name + "_" + compoundname);
            string Type = comp.Kind.ToString();
            DoxClassifier dc;
                    if (Type == "Class")
                    {
                        dc = (DoxClass)comp;
                    }
                    else if (Type == "Interface")
                    {
                        dc = (DoxInterface)comp;
                    }
                    else if (Type == "Exception" || Type == "Error")
                    {
                        dc = comp as DoxClass;                    
                        
                    }
                    else if (Type == "Enum")
                    {
                        dc = (DoxEnum)comp;
                    }
                    else
                    {
                        this.Log(LogKind.Warning, Type + " isn't a valid compound type.");
                        return;
                    }
            
            //Read description of the compound
            HtmlNode description = null;
            HtmlNode compoundtypeparameters = null;
            try
            {
                description = compoundDivNode.ChildNodes.First(node => node.Name == "div" && node.Attributes.FirstOrDefault(attr => attr.Name == "class" && attr.Value == "description") != null);
            }
            catch (InvalidOperationException iox)
            { }
            if (description != null)
            {
                try
                {
                    compoundtypeparameters = description.ChildNodes.First(node => node.Name == "ul").ChildNodes.First(node => node.Name == "li");
                    foreach (HtmlNode typeparam in compoundtypeparameters.ChildNodes.Where(node => node.Name == "dl"))
                    {
                        // Get all implemented interface
                        HtmlNode type = typeparam.ChildNodes.First(node => node.Name == "dt");
                        if (type.InnerText == "All Implemented Interfaces:")
                        {
                            HtmlNode interfaces = typeparam.ChildNodes.First(node => node.Name == "dd");
                            foreach (HtmlNode interf in interfaces.ChildNodes.Where(node => node.Name == "a"))
                            {
                                // Save a reference of the interface to the compound
                                String interfacepath = interf.Attributes.First(attr => attr.Name == "href").Value;
                                String package = interfacepath.Split('/')[1];
                                String name = interfacepath.Split('/')[2];
                                name = name.Remove(name.Length - 5);
                                DoxReference dr = new DoxReference();
                                dr.Compound = (Compound)Model.Compounds.First(c => c.Identifier == package + "_" + name);
                                dr.External = true;
                                dr.Text = normalizeHtml(interf.InnerText);
                                dc.BaseClassifiers.Add(dr);
                            }
                        }
                        //Save a reference of the know subclasses
                        else if (type.InnerText == "Direct Known Subclasses:")
                        {
                            HtmlNode interfaces = typeparam.ChildNodes.First(node => node.Name == "dd");
                            foreach (HtmlNode interf in interfaces.ChildNodes.Where(node => node.Name == "a"))
                            {
                                String subclasspath = interf.Attributes.First(attr => attr.Name == "href").Value;
                                String package = subclasspath.Split('/')[1];
                                String name = subclasspath.Split('/')[2];
                                name = name.Remove(name.Length - 5);
                                DoxReference dr = new DoxReference();
                                dr.Compound = (Compound)Model.Compounds.First(c => c.Identifier == package + "_" + name);
                                dr.External = true;
                                dr.Text = normalizeHtml(interf.InnerText);
                                dc.DerivedClassifiers.Add(dr);
                            }
                        }
                    }
                }
                catch (InvalidOperationException iox)
                { }

                try
                {
                    //Get direct data of the compound
                    HtmlNode compoundproperty = compoundtypeparameters.ChildNodes.First(node => node.Name == "pre");
                    String compoundData = normalizeHtml(compoundproperty.ChildNodes.First(node => node.Name == "#text").InnerText);
                    String[] compoundDatas = compoundData.Split(' ');
                    dc.Abstract = compoundDatas.Any(str => str == "abstact");
                    dc.Final = compoundDatas.Any(str => str == "final");
                    if (compoundDatas.Any(str => str == "public"))
                    {
                        dc.ProtectionKind = ProtectionKind.Public;
                    }
                    else if (compoundDatas.Any(str => str == "protected"))
                    {
                        dc.ProtectionKind = ProtectionKind.Protected;
                    }
                    else if (compoundDatas.Any(str => str == "private"))
                    {
                        dc.ProtectionKind = ProtectionKind.Private;
                    }
                    dc.Sealed = compoundDatas.Any(str => str == "sealed");


                    // Get extends and implemets compounds
                    bool extend = false;
                    foreach (var Node in compoundproperty.ChildNodes)
                    {
                        if (Node.Name == "#text" && Node.InnerText.Equals("extends "))
                        {
                            extend = true;
                        }
                        else if (Node.Name == "#text" && Node.InnerText.Equals("implements "))
                        {
                            extend = false;
                            break;
                        }
                        else if (Node.Name == "a")
                        {
                            String subclasspath = Node.Attributes.First(attr => attr.Name == "href").Value;
                            String package = subclasspath.Split('/')[1];
                            String name = subclasspath.Split('/')[2];
                            name = name.Remove(name.Length - 5);
                            if (extend)
                            {
                                // Avoid duplication, try to get the base compound
                                Compound oldCOmpound = dc.BaseClassifiers.FirstOrDefault(c => c.Compound.Identifier == package + "_" + name).Compound;
                                if (oldCOmpound == null)
                                {
                                    DoxReference dr = new DoxReference();
                                    dr.Compound = (Compound)Model.Compounds.First(c => c.Identifier == package + "_" + name);
                                    dr.External = true;
                                    dr.Text = normalizeHtml(Node.InnerText);
                                    dc.DerivedClassifiers.Add(dr);
                                }
                            }
                        }
                    }
                }

                catch (InvalidOperationException iox)
                {
                    this.Log(LogKind.Error, "There isn't any data of the compound.");
                    return;
                }

               

                HtmlNode[] MemberTableLiNode = null;
                try
                {
                    MemberTableLiNode = compoundDivNode.ChildNodes.First(node => node.Name == "div" && node.Attributes.FirstOrDefault(attr => attr.Name == "class" && attr.Value == "summary") != null).ChildNodes.First(node => node.Name == "ul").ChildNodes.First(node => node.Name == "li").ChildNodes.Where(node => node.Name == "ul").ToArray();
                    foreach (HtmlNode MemberTable in MemberTableLiNode)
                    {
                        HtmlNode TypeTable = MemberTable.ChildNodes.First(node => node.Name == "li");
                        // Get the type of the short description (method, field or constructor).
                        String type = TypeTable.ChildNodes.First(node => node.Name == "a").Attributes.First(attr => attr.Name == "name").Value;
                        // Get the HtmlNode, where the table of the member is.
                        if (type == "nested.class.summary" || TypeTable.ChildNodes.FirstOrDefault(node => node.Name == "table")==null)
                        {
                            continue;
                        }
                        HtmlNode MemTable = TypeTable.ChildNodes.First(node => node.Name == "table");
                        foreach (HtmlNode MemberRow in MemTable.ChildNodes.Where(node => node.Name == "tr"))
                        {
                            HtmlNode[] rowData = MemberRow.ChildNodes.Where(node => node.Name != "th" && node.Name != "#text").ToArray();
                            if(rowData.Length == 0)
                            {
                                continue;
                            }
                            DoxMember dm = null;
                            switch (type)
                            {
                                    // Summary of the fields
                                case "field.summary": dm = new DoxField();                                  
                                    // Every line should be parsed diferrently and the '\r' character should deleted.
                                    String[] nameAndDescription = rowData[1].InnerText.Replace("\r", string.Empty).Split('\n');
                                    // Int he first col, there is the name of the field
                                    dm.Name = normalizeHtml(nameAndDescription[0]).Replace(" ", string.Empty);
                                    HtmlNode descNode = rowData[1].ChildNodes.FirstOrDefault(node => node.Name == "div");
                                    if(descNode != null)
                                    {
                                        //BriefDescription
                                        dm.BriefDescription = this.getDescription(descNode);
                                    }
                                    
                                    dm.DetailedDescription = new Description();                                    
                                    dm.Identifier = packagename + "_" + compoundname + "_" + dm.Name;                                    
                                    String[] properties = normalizeHtml(rowData[0].InnerText).Split(' ');
                                    DoxReference dr = new DoxReference();                                    
                                    foreach (String item in properties)
                                    {
                                        switch (item)
                                        {
                                            case "const":
                                                dm.Const = true;
                                                break;
                                            case "final":
                                                dm.Final = true;
                                                break;
                                            case "public":
                                                dm.ProtectionKind = ProtectionKind.Public;
                                                break;
                                            case "protected":
                                                dm.ProtectionKind = ProtectionKind.Protected;
                                                break;
                                            case "private":
                                                dm.ProtectionKind = ProtectionKind.Private;
                                                break;
                                            case "sealed":
                                                dm.Sealed = true;
                                                break;
                                            case "static":
                                                dm.Sealed = true;
                                                break;
                                            default:
                                                 String[] typeRoute = item.Replace("...","…").Split('.');
                                                dm.Definition = typeRoute[typeRoute.Length-1].Replace(dm.Name,string.Empty);
                                                //this.Log(LogKind.Info, item + " used as type.");
                                                //Here need to be implented the set of the type.
                                                break;
                                        }

                                    }
                                    if(dm.VirtualKind == null)
                                    {
                                        dm.ProtectionKind = ProtectionKind.Package;
                                    }


                                    dc.Members.Add(dm);
                                    MemberIndex mi = new MemberIndex();
                                    mi.Identifier = dm.Identifier;
                                    mi.Kind = dm.Kind;
                                    mi.Name = dm.Name;
                                    mi.Member = dm;                                    
                                    this.Model.MemberRefs.Add(dm.Identifier, dm);
                                    this.Index.MemberIndexRefs.Add(dm.Identifier, mi);
                                    break;

                                case "constructor.summary":
                                    dm = new DoxMethod();
                                    nameAndDescription = normalizeHtml(rowData[0].InnerText).Split('\n');
                                    dm.Name = normalizeHtml(rowData[0].FirstChild.FirstChild.FirstChild.InnerText);
                                    dm.ArgsString = normalizeHtml(rowData[0].ChildNodes.First(node => node.Name == "code").InnerText);
                                    dm.ArgsString = dm.ArgsString.Replace(dm.Name, string.Empty);
                                    descNode = rowData[0].ChildNodes.FirstOrDefault(node => node.Name == "div");
                                        if(descNode != null)
                                        {
                                            //String desc = descNode.InnerText;
                                            dm.BriefDescription = this.getDescription(descNode);
                                                                                       
                                        }
                                     
                                    dm.DetailedDescription = new Description();
                                    
                                    dm.Identifier = packagename + "_" + compoundname + "_" + dm.Name + "_" + getId(dm.ArgsString);       
                                    dc.Members.Add(dm);
                                    mi = new MemberIndex();
                                    mi.Identifier = dm.Identifier;
                                    mi.Kind = dm.Kind;
                                    mi.Name = dm.Name;
                                    mi.Member = dm;                                    
                                    this.Model.MemberRefs.Add(dm.Identifier, dm);
                                    this.Index.MemberIndexRefs.Add(dm.Identifier, mi);
                                    break;
                                    
                                case "method.summary": dm = new DoxMethod();
                                    dm = new DoxMethod();
                                    dm.Name = normalizeHtml(rowData[1].FirstChild.FirstChild.FirstChild.InnerText);
                                    string[] returnTypeAndVisibility = normalizeHtml(rowData[0].InnerText).Split(' ');

                                    foreach (String item in returnTypeAndVisibility)
                                    {
                                        switch (item)
                                        {
                                            case "const":
                                                dm.Const = true;
                                                break;
                                            case "final":
                                                dm.Final = true;
                                                break;
                                            case "public":
                                                dm.ProtectionKind = ProtectionKind.Public;
                                                break;
                                            case "protected":
                                                dm.ProtectionKind = ProtectionKind.Protected;
                                                break;
                                            case "private":
                                                dm.ProtectionKind = ProtectionKind.Private;
                                                break;
                                            case "sealed":
                                                dm.Sealed = true;
                                                break;
                                            case "static":
                                                dm.Static = true;
                                                break;

                                            default:
                                                String[] typeRoute = item.Replace("...","…").Split('.');
                                                dm.Definition = typeRoute[typeRoute.Length-1].Replace(dm.Name, string.Empty);
                                                //this.Log(LogKind.Info, item + " used as method type.");
                                                //Here need to be implented the set of the type.
                                                break;
                                        }

                                    }

                                    nameAndDescription = normalizeHtml(rowData[1].InnerText).Replace("\r",string.Empty).Split('\n');
                                    dm.ArgsString = normalizeHtml(rowData[1].ChildNodes.First(node => node.Name == "code").InnerText);
                                    dm.ArgsString = dm.ArgsString.Replace(dm.Name, string.Empty);
                                    descNode = rowData[1].ChildNodes.FirstOrDefault(node => node.Name == "div");
                                        if(descNode != null)
                                        {
                                            dm.BriefDescription = this.getDescription(descNode);                                            
                                                                                       
                                        }
                                    dm.DetailedDescription = new Description();
                                    
                                    dm.Identifier = packagename + "_" + compoundname + "_" + dm.Name + "_" + getId(dm.ArgsString);       
                                    dc.Members.Add(dm);
                                    mi = new MemberIndex();
                                    mi.Identifier = dm.Identifier;
                                    mi.Kind = dm.Kind;
                                    mi.Name = dm.Name;
                                    mi.Member = dm;                                    
                                    this.Model.MemberRefs.Add(dm.Identifier, dm);
                                    this.Index.MemberIndexRefs.Add(dm.Identifier, mi);
                                    break;
                                case "enum.constant.summary":
                                    DoxEnumValue dev = new DoxEnumValue();
                                    HtmlNode nameNode = rowData[0].ChildNodes.FirstOrDefault(node => node.Name == "code");
                                    if (rowData[0].ChildNodes.FirstOrDefault(node => node.Name == "div") != null)
                                    {
                                        dev.Name = normalizeHtml(nameNode.InnerText);
                                        dm.BriefDescription = this.getDescription(rowData[0].ChildNodes.First(node => node.Name == "div"));                                        
                                    }
                                    else
                                    {
                                        dev.Name = normalizeHtml(rowData[0].InnerText);
                                    }
                                    dev.Kind = MemberKind.EnumValue;
                                    dev.Identifier = packagename + "_" + compoundname + "_" + dev.Name.Replace(" ",string.Empty);
                                    dc.Members.Add(dev);
                                    mi = new MemberIndex();
                                    mi.Identifier = dev.Identifier;
                                    mi.Kind = dev.Kind;
                                    mi.Name = dev.Name;
                                    mi.Member = dev;
                                    this.Model.MemberRefs.Add(dev.Identifier, dev);
                                    this.Index.MemberIndexRefs.Add(dev.Identifier, mi);
                                    break;
                                default:
                                    this.Log(LogKind.Error, "Not implemented member type: " + type);
                                    break;
                            }

                        }

                    }


                }
                catch (InvalidOperationException iox)
                {
                    this.Log(LogKind.Warning, iox.Message);
                }
                // Get the brief description of the compound.
                try
                {
                    HtmlNode BriefDescription = compoundtypeparameters.ChildNodes.First(node => node.Name == "div" && node.Attributes.FirstOrDefault(attr => attr.Name == "class" && attr.Value == "block") != null);
                    dc.BriefDescription = this.getDescription(BriefDescription);
                    //!! Change in the future. <code> -> Emphasis; <a> -> reference inside of the description
                }
                catch (InvalidOperationException iox)
                { // There is no brief description
                }
                try
                {
                    HtmlNode details = compoundDivNode.ChildNodes.First(Node => Node.Name == "div" && Node.Attributes.FirstOrDefault(Attr => Attr.Name=="class" && Attr.Value == "details")!= null);
                    HtmlNode detailsList = details.ChildNodes.First(Node => Node.Name == "ul").ChildNodes.First(Node => Node.Name == "li");
                    foreach (HtmlNode memberDetailTable in detailsList.ChildNodes.Where(Node => Node.Name == "ul"))
                    {
                        HtmlNode detailsData = memberDetailTable.ChildNodes.First(Node => Node.Name == "li");
                        String type = detailsData.ChildNodes.First(Node => Node.Name == "a").Attributes.First(Attr => Attr.Name=="name").Value;

                        foreach (HtmlNode memberDetail in detailsData.ChildNodes.Where(Node => Node.Name == "ul"))
                        {
                            HtmlNode detailListItem = memberDetail.ChildNodes.First(Node => Node.Name == "li");
                            // Name of the member
                            String name = normalizeHtml(detailListItem.ChildNodes.First(Node => Node.Name == "h4").InnerText);

                            switch (type)
                            {
                                case "field.detail":
                                    String[] datas = normalizeHtml(detailListItem.ChildNodes.First(Node => Node.Name == "pre").InnerText).Split(' ');
                                    datas = datas.Where(s => s != name).ToArray();
                                    DoxField currentMember = (DoxField)dc.Members.FirstOrDefault(c => c.Identifier == packagename + "_" + compoundname + "_" + name);
                                    foreach (String data in datas)
                                    {
                                        switch (data)
                                        {
                                            case "public":
                                                currentMember.ProtectionKind = ProtectionKind.Public;
                                                break;
                                            case "private":
                                                currentMember.ProtectionKind = ProtectionKind.Private;
                                                break;
                                            case "protected":
                                                currentMember.ProtectionKind = ProtectionKind.Protected;
                                                break;
                                            case "constant":
                                                currentMember.Const = true;
                                                break;
                                            case "static":
                                                currentMember.Static = true;
                                                break;
                                            case "volatile":
                                                currentMember.Volatile = true;
                                                break;
                                            case "final":
                                                currentMember.Final = true;
                                                break;
                                            default:
                                                
                                                break;
                                        }

                                    }
                                    if (currentMember.ProtectionKind == null)
                                    {
                                        currentMember.ProtectionKind = ProtectionKind.Package;
                                    }
                                    if (detailListItem.ChildNodes.FirstOrDefault(Node => Node.Name == "div" && Node.Attributes.FirstOrDefault(Attr => Attr.Name == "class" && Attr.Value == "block") != null) != null)
                                    {
                                       currentMember.DetailedDescription =this.getDescription(detailListItem.ChildNodes.FirstOrDefault(Node => Node.Name == "div" && Node.Attributes.FirstOrDefault(Attr => Attr.Name == "class" && Attr.Value == "block") != null));
                                    }
                                    break;
                                case "constructor.detail":
                                case "method.detail":
                                    datas = normalizeHtml(detailListItem.ChildNodes.First(Node => Node.Name == "pre").InnerText).Split('(');
                                    String argsList = datas[datas.Length-1].Split(')')[0];
                                    String ID = getId(argsList);
                                    DoxMethod currentMethod = (DoxMethod)dc.Members.FirstOrDefault(c => c.Identifier == packagename + "_" + compoundname + "_" + name + "_" +  ID);
                                   
                                    String[] visibilitydatas = datas[0].Split(' ').Where(c => c != name).ToArray();                                    
                                    foreach (String data in visibilitydatas)
                                    {
                                        switch (data)
                                        {
                                            case "public":
                                                currentMethod.ProtectionKind = ProtectionKind.Public;
                                                break;
                                            case "private":
                                                currentMethod.ProtectionKind = ProtectionKind.Private;
                                                break;
                                            case "protected":
                                                currentMethod.ProtectionKind = ProtectionKind.Protected;
                                                break;
                                            case "constant":
                                                currentMethod.Const = true;
                                                break;
                                            case "static":
                                                currentMethod.Static = true;
                                                break;
                                            case "volatile":
                                                currentMethod.Volatile = true;
                                                break;
                                            case "final":
                                                currentMethod.Final = true;
                                                break;
                                            default:
                                                break;
                                        }

                                    }
                                    foreach (string arg in argsList.Replace("  ",string.Empty).Split(','))
                                    {
                                        if(String.IsNullOrEmpty(arg))
                                        {
                                            continue;
                                        }
                                        DoxParam dp = new DoxParam();
                                        DoxLinkedText dlt = new DoxLinkedText();
                                        DoxLinkedTextItem dlti = new DoxLinkedTextItem();
                                        String[] relativeroute =  arg.Replace("\n",string.Empty).Replace("\r",string.Empty).Replace("...","…").Split('.');
                                        dlti.Text = relativeroute[relativeroute.Length-1].Split(' ')[0];
                                        dlt.Items.Add(dlti);
                                        dp.Type = dlt;
                                        dp.DeclarationName = relativeroute[relativeroute.Length-1].Split(' ')[1];
                                        currentMethod.Params.Add(dp);      
                                    }    
                                    if(currentMethod.ProtectionKind == null)
                                    {
                                        currentMethod.ProtectionKind = ProtectionKind.Package;
                                    }
                                    HtmlNode briefDescriptionNode = detailListItem.ChildNodes.FirstOrDefault(Node => Node.Name == "div" && Node.Attributes.FirstOrDefault(Attr => Attr.Name == "class" && Attr.Value == "block") != null);
                                    HtmlNode extraDetails = detailListItem.ChildNodes.FirstOrDefault(Node => Node.Name == "dl");
                                    if (briefDescriptionNode != null && extraDetails != null)
                                    {
                                        currentMethod.DetailedDescription = this.getDescription(briefDescriptionNode);
                                        currentMethod.DetailedDescription.Paragraphs.AddRange(this.getDescription(extraDetails).Paragraphs);
                                        
                                    }
                                    else if (briefDescriptionNode != null)
                                    {
                                        currentMethod.DetailedDescription = this.getDescription(briefDescriptionNode);
                                    }
                                    else if(extraDetails != null)
                                    {
                                        currentMethod.DetailedDescription = this.getDescription(extraDetails);
                                    }
                                    break;
                                case "enum.constant.detail":
                                     datas = normalizeHtml(detailListItem.ChildNodes.First(Node => Node.Name == "pre").InnerText).Split('(');
                                    DoxEnumValue currentEnum = (DoxEnumValue)dc.Members.FirstOrDefault(c => c.Identifier == packagename + "_" + compoundname + "_" + name);
                                    visibilitydatas = datas[0].Split(' ').Where(c => c != name).ToArray();
                                    foreach (String data in visibilitydatas)
                                    {
                                        switch (data)
                                        {
                                            case "public":
                                                currentEnum.ProtectionKind = ProtectionKind.Public;
                                                break;
                                            case "private":
                                                currentEnum.ProtectionKind = ProtectionKind.Private;
                                                break;
                                            case "protected":
                                                currentEnum.ProtectionKind = ProtectionKind.Protected;
                                                break;
                                            case "constant":
                                                currentEnum.Const = true;
                                                break;
                                            case "static":
                                                currentEnum.Static = true;
                                                break;
                                            case "volatile":
                                                currentEnum.Volatile = true;
                                                break;
                                            case "final":
                                                currentEnum.Final = true;
                                                break;
                                            default:
                                                currentEnum.Definition = data;
                                                break;
                                        }

                                    }
                                    if (currentEnum.ProtectionKind == null)
                                    {
                                        currentEnum.ProtectionKind = ProtectionKind.Package;
                                    }
                                    briefDescriptionNode = detailListItem.ChildNodes.FirstOrDefault(Node => Node.Name == "div" && Node.Attributes.FirstOrDefault(Attr => Attr.Name == "class" && Attr.Value == "block") != null);
                                    extraDetails = detailListItem.ChildNodes.FirstOrDefault(Node => Node.Name == "dl");
                                   if (briefDescriptionNode != null && extraDetails != null)
                                    {
                                        currentEnum.DetailedDescription = this.getDescription(briefDescriptionNode);
                                        currentEnum.DetailedDescription.Paragraphs.AddRange(this.getDescription(extraDetails).Paragraphs);
                                        
                                    }
                                    else if (briefDescriptionNode != null)
                                    {
                                        currentEnum.DetailedDescription = this.getDescription(briefDescriptionNode);
                                    }
                                    else if(extraDetails != null)
                                    {
                                        currentEnum.DetailedDescription = this.getDescription(extraDetails);
                                    }
                                    break;                                    
                                default:
                                    this.Log(LogKind.Error, type + "isn't processed");
                                    break;
                            }


                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Log(LogKind.Warning, ex.Message);
                }

            }     
        }
  
    }
}
