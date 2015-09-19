﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftwareEngineeringTools.Documentation
{
    public class DoxygenIndex
    {
        public Dictionary<string,CompoundIndex> CompoundIndexRefs { get; private set; }
        public Dictionary<string,MemberIndex> MemberIndexRefs { get; private set; }
        public string Version { get; set; }
        public List<CompoundIndex> Compounds { get; private set; }
        public DoxygenIndex()
        {
            this.Compounds = new List<CompoundIndex>();
            this.CompoundIndexRefs = new Dictionary<string, CompoundIndex>();
            this.MemberIndexRefs = new Dictionary<string, MemberIndex>();
        }
    }

    public class CompoundIndex
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public CompoundKind Kind { get; set; }
        public List<MemberIndex> Members { get; private set; }
        public Compound Compound { get; internal set; }
        public CompoundIndex()
        {
            this.Members = new List<MemberIndex>();
        }
    }

    public class MemberIndex
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public MemberKind Kind { get; set; }
        public DoxMember Member { get; internal set; }
    }

    public enum CompoundKind
    {
        Class,
        Struct,
        Union,
        Interface,
        Enum,
        Protocol,
        Category,
        Exception,
        File,
        Namespace,
        Group,
        Page,
        Example,
        Dir
    }

    public enum MemberKind
    {
        Define,
        Property,
        Event,
        Variable,
        TypeDef,
        Enum,
        EnumValue,
        Function,
        Signal,
        Prototype,
        Friend,
        Dcop,
        Slot
    }

    public class DoxygenModel
    {
        public Dictionary<string, Compound> CompoundRefs { get; private set; }
        public Dictionary<string, DoxMember> MemberRefs { get; private set; }
        public ModelVersion Version { get; set; }
        public List<Compound> Compounds { get; private set; }
        public DoxygenModel()
        {
            this.Compounds = new List<Compound>();
            this.CompoundRefs = new Dictionary<string, Compound>();
            this.MemberRefs = new Dictionary<string, DoxMember>();
        }
    }

    public struct ModelVersion
    {
        public string Major { get; set; }
        public string Minor { get; set; }
    }

    public class Compound
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public CompoundKind Kind { get; set; }
        public Description BriefDescription { get; set; }
        public Description Description { get; set; }
        public DoxProgramListing ProgramListing { get; set; }
        public Location Location { get; set; }

        public Compound()
        {
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} ({2})", this.Kind, this.Name, this.Identifier);
        }
    }

    public class Location
    {
        public string File { get; set; }
        public int Line { get; set; }
        public string BodyFile { get; set; }
        public int BodyStart { get; set; }
        public int BodyEnd { get; set; }
    }

    public class DoxNamespace : Compound
    {
        public List<DoxNamespace> Namespaces { get; private set; }
        public List<DoxClassifier> Classifiers { get; private set; }

        public DoxNamespace()
        {
            this.Kind = CompoundKind.Namespace;
            this.Namespaces = new List<DoxNamespace>();
            this.Classifiers = new List<DoxClassifier>();
        }
    }

    public class DoxClassifier : Compound
    {
        public List<DoxMember> Members { get; private set; }
        public List<DoxClassifier> InnerClassifiers { get; private set; }
        public List<DoxReference> BaseClassifiers { get; private set; }
        public List<DoxReference> DerivedClassifiers { get; private set; }
        public List<DoxParam> TemplateParams { get; private set; }
        public ProtectionKind ProtectionKind { get; set; }
        public bool? Final { get; set; }
        public bool? Sealed { get; set; }
        public bool? Abstract { get; set; }

        public DoxClassifier()
        {
            this.Members = new List<DoxMember>();
            this.InnerClassifiers = new List<DoxClassifier>();
            this.BaseClassifiers = new List<DoxReference>();
            this.DerivedClassifiers = new List<DoxReference>();
            this.TemplateParams = new List<DoxParam>();
        }
    }

    public enum ProtectionKind
    {
        Public,
        Protected,
        Private,
        Package
    }

    public enum VirtualKind
    {
        NonVirtual,
        Virtual,
        Abstract
    }

    public class DoxClass : DoxClassifier
    {
        public DoxClass()
        {
            this.Kind = CompoundKind.Class;
        }
    }

    public class DoxInterface : DoxClassifier
    {
        public DoxInterface()
        {
            this.Kind = CompoundKind.Interface;
        }
    }

    public class DoxStruct : DoxClassifier
    {
        public DoxStruct()
        {
            this.Kind = CompoundKind.Struct;
        }
    }

    public class DoxEnum : DoxClassifier
    {
        public DoxEnum()
        {
            this.Kind = CompoundKind.Enum;
        }
    }

    public class DoxDirectory : Compound
    {
        public List<DoxDirectory> Directories { get; private set; }
        public List<DoxFile> Files { get; private set; }

        public DoxDirectory()
        {
            this.Kind = CompoundKind.Dir;
            this.Directories = new List<DoxDirectory>();
            this.Files = new List<DoxFile>();
        }
    }

    public class DoxFile : Compound
    {
        public List<DoxClassifier> Classifiers { get; private set; }
        public List<DoxNamespace> Namespaces { get; private set; }

        public DoxFile()
        {
            this.Kind = CompoundKind.File;
            this.Classifiers = new List<DoxClassifier>();
            this.Namespaces = new List<DoxNamespace>();
        }
    }

    public class DoxMember
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public MemberKind Kind { get; set; }
        public List<DoxParam> TemplateParams { get; private set; }
        public DocReference Type { get; set; }
        public string Definition { get; set; }
        public string ArgsString { get; set; }
        public string Read { get; set; }
        public string Write { get; set; }
        public string BitField { get; set; }
        public List<DoxMember> Reimplements { get; private set; }
        public List<DoxMember> ReimplementedBy { get; private set; }
        public List<DoxParam> Params { get; private set; }
        public DoxLinkedText Initializer { get; set; }
        public DoxLinkedText Exceptions { get; set; }
        public Description BriefDescription { get; set; }
        public Description Description { get; set; }
        public Description InBodyDescription { get; set; }
        public Description DetailedDescription { get; set; }
        public Location Location { get; set; }
        public List<DoxReference> References { get; private set; }
        public List<DoxReference> ReferencedBy { get; private set; }
        public ProtectionKind? ProtectionKind { get; set; }
        public bool? Static { get; set; }
        public bool? Const { get; set; }
        public bool? Explicit { get; set; }
        public bool? Inline { get; set; }
        public VirtualKind? VirtualKind { get; set; }
        public bool? Volatile { get; set; }
        public bool? Mutable { get; set; }
        public bool? Initonly { get; set; }
        public bool? Settable { get; set; }
        public bool? Gettable { get; set; }
        public bool? Final { get; set; }
        public bool? Sealed { get; set; }
        public bool? New { get; set; }
        public bool? Add { get; set; }
        public bool? Remove { get; set; }
        public bool? Raise { get; set; }

        public DoxMember()
        {
            this.TemplateParams = new List<DoxParam>();
            this.Reimplements = new List<DoxMember>();
            this.ReimplementedBy = new List<DoxMember>();
            this.Params = new List<DoxParam>();
            this.References = new List<DoxReference>();
            this.ReferencedBy = new List<DoxReference>();
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} ({2})", this.Kind, this.Name, this.Identifier);
        }
    }

    public class DoxField : DoxMember
    {
        public DoxField()
        {
            this.Kind = MemberKind.Variable;
        }
    }

    public class DoxProperty : DoxMember
    {
        public DoxProperty()
        {
            this.Kind = MemberKind.Property;           
        }
    }

    public class DoxMethod : DoxMember
    {
        public DoxMethod()
        {
            this.Kind = MemberKind.Function;
        }
    }

    public class DoxFriend : DoxMember
    {
        public DoxFriend()
        {
            this.Kind = MemberKind.Friend;
        }
    }

    public class DoxDefine : DoxMember
    {
        public DoxDefine()
        {
            this.Kind = MemberKind.Define;
        }
    }

    public class DoxEnumValue : DoxMember
    {
        public DoxEnumValue()
        {
            this.Kind = MemberKind.EnumValue;
        }
    }

    public class MemberEnum : DoxMember
    {
        public MemberEnum()
        {
            this.Kind = MemberKind.Enum;
        }
    }

    public class DoxParam
    {
        public DoxLinkedText Type { get; set; }
        public string DeclarationName { get; set; }
        public string DefinitionName { get; set; }
        public string Array { get; set; }
        public DoxLinkedText DefaultValue { get; set; }
        public Description Description { get; set; }
    }

    public class DoxLinkedTextItem
    {
        public string Text { get; set; }
    }

    public class DoxText : DoxLinkedTextItem
    {
    }

    public class DoxReference : DoxLinkedTextItem
    {
        public Compound Compound { get; set; }
        public DoxMember Member { get; set; }
        public ProtectionKind? ProtectionKind { get; set; }
        public VirtualKind? VirtualKind { get; set; }
        public bool? External { get; set; }
        public string Tooltip { get; set; }
    }

    public class DoxLinkedText
    {
        public List<DoxLinkedTextItem> Items { get; private set; }

        public DoxLinkedText()
        {
            this.Items = new List<DoxLinkedTextItem>();
        }
    }

    public class DoxProgramListing
    {
        public List<DoxCodeLine> CodeLines { get; private set; }

        public DoxProgramListing()
        {
            this.CodeLines = new List<DoxCodeLine>();
        }
    }

    public class DoxCodeLine : DoxReference
    {
        public int LineNumber { get; set; }
        public List<DoxHighlight> Tokens { get; private set; }

        public DoxCodeLine()
        {
            this.Tokens = new List<DoxHighlight>();
        }
    }

    public class DoxHighlight
    {
        public DoxHighlightKind Kind { get; set; }
        public List<DoxHighlightText> Text { get; set; }

        public DoxHighlight()
        {
            this.Text = new List<DoxHighlightText>();
        }
    }

    public class DoxHighlightText
    {
        public bool Space { get; set; }
        public string Text { get; set; }
        public DoxReference Reference { get; set; }
    }

    public enum DoxHighlightKind
    {
        Comment,
        Normal,
        Preprocessor,
        Keyword,
        KeywordType,
        KeywordFlow,
        StringLiteral,
        CharLiteral
    }

    public class Description : DocCmd
    {
        public string Title { get; set; }
        public List<DocPara> Paragraphs { get; private set; }
        public List<DocSect> Sections { get; private set; }
        public Description Internal { get; set; }

        public Description()
        {
            this.Kind = DocKind.Description;
            this.Paragraphs = new List<DocPara>();
            this.Sections = new List<DocSect>();
        }
    }

    public class DocSect : Description
    {
        public string Identifier { get; set; }

        public DocSect()
        {
            this.Kind = DocKind.Sect;
        }
    }

    public enum DocKind
    {
        Description,
        Cmd,
        CmdGroup,
        Para,
        ParaList,
        Markup,
        Text,
        Char,
        Empty,
        UrlLink,
        Anchor,
        Formula,
        Reference,
        IndexEntry,
        List,
        ListItem,
        Sect,
        SimpleSect,
        SimpleSectItem,
        Title,
        Caption,
        Heading,
        VariableList,
        Table,
        Image,
        DotFile,
        TocList,
        TocItem,
        Language,
        ParamList,
        XRefSec,
        Copy,
        BlockQuote
    }

    public class DocCmd
    {
        public DocKind Kind { get; set; }

        public DocCmd()
        {
            this.Kind = DocKind.Cmd;
        }
    }

    public class DocCmdGroup : DocCmd
    {
        public List<DocCmd> Commands { get; private set; }

        public DocCmdGroup() 
        {
            this.Kind = DocKind.CmdGroup;
            this.Commands = new List<DocCmd>();
        }
    }

    public class DocPara : DocCmdGroup
    {
        public DocPara()
        {
            this.Kind = DocKind.Para;
        }
    }

    public class DocParaList : DocCmd
    {
        public List<DocPara> Paragraphs { get; private set; }

        public DocParaList()
        {
            this.Kind = DocKind.ParaList;
            this.Paragraphs = new List<DocPara>();
        }
    }

    public class DocMarkup : DocCmdGroup
    {
        public DocMarkupKind MarkupKind { get; set; }

        public DocMarkup()
        {
            this.Kind = DocKind.Markup;
        }
    }

    public class DocText : DocCmd
    {
        public DocTextKind TextKind { get; set; }
        public string Text { get; set; }

        public DocText()
        {
            this.Kind = DocKind.Text;
        }
    }

    public class DocChar : DocCmd
    {
        public DocCharKind CharKind { get; set; }

        public DocChar()
        {
            this.Kind = DocKind.Char;
        }
    }

    public class DocEmpty : DocCmd
    {
        public DocEmptyKind EmptyKind { get; set; }

        public DocEmpty()
        {
            this.Kind = DocKind.Empty;
        }
    }

    public enum DocMarkupKind
    {
        Bold,
        Emphasis,
        ComputerOutput,
        SubScript,
        SuperScript,
        Center,
        Small,
        Preformatted
    }

    public enum DocTextKind
    {
        Plain,
        Verbatim,
        HtmlOnly,
        ManOnly,
        XmlOnly,
        RtfOnly,
        LatexOnly,
        Dot
    }

    public enum DocCharKind
    {
        Umlaut,
        Acute,
        Grave,
        Circ,
        Slash,
        Tilde,
        Cedil,
        Ring
    }

    public enum DocEmptyKind
    {
        simplesectsep,
        linebreak,
        hruler,
        copy,
        trademark,
        registered,
        lsquo,
        rsquo,
        ldquo,
        rdquo,
        ndash,
        mdash,
        szlig,
        nonbreakablespace,
        aelig,
        AElig,
        Gamma,
        Delta,
        Theta,
        Lambda,
        Xi,
        Pi,
        Sigma,
        Upsilon,
        Phi,
        Psi,
        Omega,
        alpha,
        beta,
        gamma,
        delta,
        epsilon,
        zeta,
        eta,
        theta,
        iota,
        kappa,
        lambda,
        mu,
        nu,
        xi,
        pi,
        rho,
        sigma,
        tau,
        upsilon,
        phi,
        chi,
        psi,
        omega,
        sigmaf,
        sect,
        deg,
        prime,
        Prime,
        infin,
        empty,
        plusmn,
        times,
        minus,
        sdot,
        part,
        nabla,
        radic,
        perp,
        sum,
        @int,
        prod,
        sim,
        asymp,
        ne,
        equiv,
        prop,
        le,
        ge,
        larr,
        rarr,
        isin,
        notin,
        lceil,
        rceil,
        lfloor,
        rfloor,
    }

    public class DocUrlLink : DocCmdGroup
    {
        public string Url { get; set; }

        public DocUrlLink()
        {
            this.Kind = DocKind.UrlLink;
        }
    }

    public class DocAnchor : DocCmdGroup
    {
        public string Id { get; set; }

        public DocAnchor()
        {
            this.Kind = DocKind.Anchor;
        }
    }

    public class DocFormula : DocCmdGroup
    {
        public string Id { get; set; }

        public DocFormula()
        {
            this.Kind = DocKind.Formula;
        }
    }

    public class DocReference : DocCmdGroup
    {
        public Compound Compound { get; set; }
        public DoxMember Member { get; set; }
        public DocRefKind RefKind { get; set; }
        public bool? External { get; set; }

        public DocReference()
        {
            this.Kind = DocKind.Reference;
        }
    }

    public enum DocRefKind
    {
        Compound,
        Member
    }

    public class DocIndexEntry : DocCmd
    {
        public string PrimaryEntry { get; set; }
        public string SecondaryEntry { get; set; }

        public DocIndexEntry()
        {
            this.Kind = DocKind.IndexEntry;
        }
    }

    public class DocList : DocCmd
    {
        public List<DocListItem> Items { get; private set; }

        public DocList()
        {
            this.Kind = DocKind.List;
            this.Items = new List<DocListItem>();
        }
    }

    public class DocListItem : DocParaList
    {
        public DocListItem()
        {
            this.Kind = DocKind.ListItem;
        }
    }

    public class DocSimpleSect : DocCmd
    {
        public DocSimpleSectKind SimpleSectKind { get; set; }
        public string Title { get; set; }
        public List<DocSimpleSectItem> Items { get; private set; }

        public DocSimpleSect()
        {
            this.Kind = DocKind.SimpleSect;
        }
    }

    public enum DocSimpleSectKind
    {
        See,
        Return,
        Author,
        Authors,
        Version,
        Since,
        Date,
        Note,
        Warning,
        Pre,
        Post,
        Copyright,
        Invariant,
        Remark,
        Attention,
        Par,
        Rcs
    }

    public class DocSimpleSectItem : DocParaList
    {
        public DocEmpty Separator { get; set; }

        public DocSimpleSectItem()
        {
            this.Kind = DocKind.SimpleSectItem;
        }
    }

    public class DocTitle : DocCmdGroup
    {
        public DocTitle()
        {
            this.Kind = DocKind.Title;
        }
    }

    public class DocCaption : DocCmdGroup
    {
        public DocCaption()
        {
            this.Kind = DocKind.Caption;
        }
    }

    public class DocHeading : DocCmdGroup
    {
        public int Level { get; set; }

        public DocHeading()
        {
            this.Kind = DocKind.Heading;
        }
    }

    public class DocVariableList : DocCmd
    {
        public List<DocVariable> Variables { get; private set; }

        public DocVariableList()
        {
            this.Kind = DocKind.VariableList;
            this.Variables = new List<DocVariable>();
        }
    }

    public class DocVariable
    {
        public DocTitle Entry { get; set; }
        public DocListItem Item { get; set; }
    }

    public class DocTable : DocCmd
    {
        public int RowCount { get; set; }
        public int ColCount { get; set; }
        public DocCaption Caption { get; set; }
        public List<DocTableRow> Rows { get; private set; }

        public DocTable()
        {
            this.Kind = DocKind.Table;
            this.Rows = new List<DocTableRow>();
        }
    }

    public class DocTableRow
    {
        public List<DocTableCell> Cells { get; private set; }

        public DocTableRow()
        {
            this.Cells = new List<DocTableCell>();
        }
    }

    public class DocTableCell : DocParaList
    {
        public bool IsHeader { get; set; }
    }

    public class DocImage : DocCmdGroup
    {
        public DocImageKind ImageKind { get; set; }
        public string Name { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }

        public DocImage()
        {
            this.Kind = DocKind.Image;
        }
    }

    public enum DocImageKind
    {
        Html,
        Latex,
        Rtf
    }

    public class DocDotFile : DocCmdGroup
    {
        public string Id { get; set; }

        public DocDotFile()
        {
            this.Kind = DocKind.DotFile;
        }
    }

    public class DocTocList : DocCmd
    {
        public List<DocTocItem> Items { get; private set; }

        public DocTocList()
        {
            this.Kind = DocKind.TocList;
            this.Items = new List<DocTocItem>();
        }
    }

    public class DocTocItem : DocCmdGroup
    {
        public string Id { get; set; }

        public DocTocItem()
        {
            this.Kind = DocKind.TocItem;
        }
    }

    public class DocLanguage : DocParaList
    {
        public string LanguageId { get; set; }

        public DocLanguage()
        {
            this.Kind = DocKind.Language;
        }
    }

    public class DocParamList : DocCmd
    {
        public DocParamListKind ParamListKind { get; set; }
        public List<DocParamListItem> Params { get; private set; }

        public DocParamList()
        {
            this.Kind = DocKind.ParamList;
            this.Params = new List<DocParamListItem>();
        }
    }

    public enum DocParamListKind
    {
        Parameter,
        ReturnValue,
        Exception,
        TemplateParam
    }

    public class DocParamListItem
    {
        public List<DocParamNameList> ParamNames { get; private set; }
        public Description Description { get; set; }

        public DocParamListItem()
        {
            this.ParamNames = new List<DocParamNameList>();
        }
    }

    public class DocParamNameList
    {
        public List<DocReference> Types { get; private set; }
        public List<DocParamName> Names { get; private set; }

        public DocParamNameList()
        {
            this.Types = new List<DocReference>();
            this.Names = new List<DocParamName>();
        }
    }

    public class DocParamName
    {
        public DocReference Name { get; set; }
        public DocParamDir Direction { get; set; }
    }

    public enum DocParamDir
    {
        In,
        Out,
        InOut
    }

    public class DocXRefSec : DocCmd
    {
        public List<string> Titles { get; private set; }
        public Description Description { get; set; }
        public string Id { get; set; }

        public DocXRefSec()
        {
            this.Kind = DocKind.XRefSec;
            this.Titles = new List<string>();
        }
    }

    public class DocCopy : DocSect
    {
        public DocCopy()
        {
            this.Kind = DocKind.Copy;
        }
    }

    public class DocBlockQuote : DocParaList
    {
        public DocBlockQuote()
        {
            this.Kind = DocKind.BlockQuote;
        }
    }
}
