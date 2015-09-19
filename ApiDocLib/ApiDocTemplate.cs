using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoftwareEngineeringTools.Documentation;

namespace SoftwareEngineeringTools.Documentation
{
    public enum ProgrammingLanguage
    {
        CSharp,
        Java,
        Cpp
    }

    public interface IApiDocTemplateProcessor
    {
        void Process(DocumentTemplate template);
        void Process(SectionTemplate template);
        void Process(NamespaceListTemplate template);
        void Process(ClassifierListTemplate template);
        void Process(InterfaceListTemplate template);
        void Process(ClassListTemplate template);
        void Process(StructListTemplate template);
        void Process(EnumListTemplate template);
        void Process(MemberListTemplate template);
        void Process(EnumValueListTemplate template);
        void Process(FieldListTemplate template);
        void Process(PropertyListTemplate template);
        void Process(MethodListTemplate template);
        void Process(UmlClassDiagramTemplate template);
        void Process(DescriptionTemplate template);
        void Process(BriefDescriptionTemplate template);
        void Process(TextTemplate template);
    }

    public class DocumentTemplate : SectionTemplate
    {
        public ProgrammingLanguage ProgrammingLanguage { get; set; }

        public DocumentTemplate()
            : base(null)
        {

        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }


        private static DocumentTemplate groupByNamespaceTemplate;
        public static DocumentTemplate GroupByNamespaceTemplate
        {
            get
            {
                if (groupByNamespaceTemplate == null)
                {
                    Template t, t1;
                    groupByNamespaceTemplate = new DocumentTemplate();
                    t = GroupByNamespaceTemplate.AddSection("Documentation of the types").AddNamespaceList();
                    t.AddBriefDescription();
                    t = t.AddClassifierList();
                    t.AddBriefDescription();
                    t1 = t.AddSection("Fields").AddFieldList().AddBriefDescription();
                    t1 = t.AddSection("Properties").AddPropertyList().AddBriefDescription();
                    t1 = t.AddSection("Methods").AddMethodList().AddBriefDescription();
                    t.AddDescription();
                }
                return groupByNamespaceTemplate;
            }
        }

        private static DocumentTemplate groupByKindTemplate;
        public static DocumentTemplate GroupByKindTemplate 
        { 
            get
            {
                if (groupByKindTemplate == null)
                {
                    Template t, t1;
                    groupByKindTemplate = new DocumentTemplate();
                    t = GroupByKindTemplate.AddSection("Documentation of the enums").AddEnumList();
                    t.AddBriefDescription();
                    t1 = t.AddSection("Enum values").AddEnumValueList().AddBriefDescription();
                    t1 = t.AddSection("Fields").AddFieldList().AddBriefDescription();
                    t1 = t.AddSection("Properties").AddPropertyList().AddBriefDescription();
                    t1 = t.AddSection("Methods").AddMethodList().AddBriefDescription();
                    t.AddDescription();
                    t = GroupByKindTemplate.AddSection("Documentation of the interfaces").AddInterfaceList();
                    t.AddBriefDescription();
                    t1 = t.AddSection("Fields").AddFieldList().AddBriefDescription();
                    t1 = t.AddSection("Properties").AddPropertyList().AddBriefDescription();
                    t1 = t.AddSection("Methods").AddMethodList().AddBriefDescription();
                    t.AddDescription();
                    t = GroupByKindTemplate.AddSection("Documentation of the classes").AddClassList();
                    t.AddBriefDescription();
                    t1 = t.AddSection("Fields").AddFieldList().AddBriefDescription();
                    t1 = t.AddSection("Properties").AddPropertyList().AddBriefDescription();
                    t1 = t.AddSection("Methods").AddMethodList().AddBriefDescription();
                    t.AddDescription();
                    t = GroupByKindTemplate.AddSection("Documentation of the structs").AddStructList();
                    t.AddBriefDescription();
                    t1 = t.AddSection("Fields").AddFieldList().AddBriefDescription();
                    t1 = t.AddSection("Properties").AddPropertyList().AddBriefDescription();
                    t1 = t.AddSection("Methods").AddMethodList().AddBriefDescription();
                    t.AddDescription();
                }
                return groupByKindTemplate;
            }
        }
    }

    public abstract class Template
    {
        public bool Leaf { get; set; }
        public Template Parent { get; set; }
        public List<Template> Items { get; private set; }

        public Template(Template parent)
        {
            this.Leaf = false;
            this.Parent = parent;
            this.Items = new List<Template>();
        }

        public abstract void Process(IApiDocTemplateProcessor processor);

        public void ProcessItems(IApiDocTemplateProcessor processor)
        {
            foreach (var item in this.Items)
            {
                item.Process(processor);
            }
        }

        private void CheckParentOfType<TParent>(string templateName)
            where TParent : class
        {
            bool found = false;
            Template parent = this;
            while (parent != null)
            {
                TParent tparent = parent as TParent;
                if (tparent != null)
                {
                    found = true;
                    break;
                }
                parent = parent.Parent;
            }
            if (!found)
            {
                throw new DocumentException(string.Format("A {0} can only be inserted under a {1}.", templateName, typeof(TParent).Name));
            }
        }

        private void CheckNoParentOfType<TParent>(string templateName)
            where TParent : class
        {
            Template parent = this;
            while (parent != null)
            {
                TParent tparent = parent as TParent;
                if (tparent != null)
                {
                    throw new DocumentException(string.Format("Cannot insert a {0} under a {1}.", templateName, typeof(TParent).Name));
                }
                parent = parent.Parent;
            }
        }

        private TItem Add<TItem>(TItem item)
            where TItem : Template
        {
            if (this.Leaf)
            {
                string.Format("Cannot insert a {0} under a {1}.", item.GetType().Name, this.GetType().Name);
            }
            this.Items.Add(item);
            return item;
        }

        public Template AddSection(string title)
        {
            SectionTemplate result = new SectionTemplate(this);
            result.Title = title;
            return this.Add(result);
        }

        public Template AddNamespaceList()
        {
            this.CheckNoParentOfType<NamespaceListTemplate>("NamespaceListTemplate");
            NamespaceListTemplate result = new NamespaceListTemplate(this);
            return this.Add(result);
        }

        public Template AddClassifierList()
        {
            this.CheckNoParentOfType<ClassifierListTemplate>("ClassifierListTemplate");
            ClassifierListTemplate result = new ClassifierListTemplate(this);
            return this.Add(result);
        }

        public Template AddInterfaceList()
        {
            this.CheckNoParentOfType<ClassifierListTemplate>("InterfaceListTemplate");
            InterfaceListTemplate result = new InterfaceListTemplate(this);
            return this.Add(result);
        }

        public Template AddClassList()
        {
            this.CheckNoParentOfType<ClassifierListTemplate>("ClassListTemplate");
            ClassListTemplate result = new ClassListTemplate(this);
            return this.Add(result);
        }

        public Template AddStructList()
        {
            this.CheckNoParentOfType<ClassifierListTemplate>("StructListTemplate");
            StructListTemplate result = new StructListTemplate(this);
            return this.Add(result);
        }

        public Template AddEnumList()
        {
            this.CheckNoParentOfType<ClassifierListTemplate>("EnumListTemplate");
            EnumListTemplate result = new EnumListTemplate(this);
            return this.Add(result);
        }

        public Template AddMemberList(bool groupByVisiblity = true)
        {
            this.CheckParentOfType<ClassifierListTemplate>("MemberListTemplate");
            this.CheckNoParentOfType<MemberListTemplate>("MemberListTemplate");
            MemberListTemplate result = new MemberListTemplate(this);
            result.GroupByVisibility = groupByVisiblity;
            return this.Add(result);
        }

        public Template AddEnumValueList(bool groupByVisiblity = true)
        {
            this.CheckParentOfType<ClassifierListTemplate>("EnumValueListTemplate");
            this.CheckNoParentOfType<MemberListTemplate>("EnumValueListTemplate");
            EnumValueListTemplate result = new EnumValueListTemplate(this);
            result.GroupByVisibility = groupByVisiblity;
            return this.Add(result);
        }

        public Template AddFieldList(bool groupByVisiblity = true)
        {
            this.CheckParentOfType<ClassifierListTemplate>("FieldListTemplate");
            this.CheckNoParentOfType<MemberListTemplate>("FieldListTemplate");
            FieldListTemplate result = new FieldListTemplate(this);
            result.GroupByVisibility = groupByVisiblity;
            return this.Add(result);
        }

        public Template AddPropertyList(bool groupByVisiblity = true)
        {
            this.CheckParentOfType<ClassifierListTemplate>("PropertyListTemplate");
            this.CheckNoParentOfType<MemberListTemplate>("PropertyListTemplate");
            PropertyListTemplate result = new PropertyListTemplate(this);
            result.GroupByVisibility = groupByVisiblity;
            return this.Add(result);
        }

        public Template AddMethodList(bool groupByVisiblity = true)
        {
            this.CheckParentOfType<ClassifierListTemplate>("MethodListTemplate");
            this.CheckNoParentOfType<MemberListTemplate>("MethodListTemplate");
            MethodListTemplate result = new MethodListTemplate(this);
            result.GroupByVisibility = groupByVisiblity;
            return this.Add(result);
        }

        public Template AddDescription()
        {
            this.CheckParentOfType<DoxygenObjectTemplate>("DescriptionTemplate");
            this.CheckNoParentOfType<DescriptionTemplate>("DescriptionTemplate");
            DescriptionTemplate result = new DescriptionTemplate(this);
            return this.Add(result);
        }

        public Template AddBriefDescription()
        {
            this.CheckParentOfType<DoxygenObjectTemplate>("BriefDescriptionTemplate");
            this.CheckNoParentOfType<DescriptionTemplate>("BriefDescriptionTemplate");
            BriefDescriptionTemplate result = new BriefDescriptionTemplate(this);
            return this.Add(result);
        }

        public Template AddUmlClassDiagram()
        {
            this.CheckParentOfType<ClassifierListTemplate>("UmlClassDiagramTemplate");
            UmlClassDiagramTemplate result = new UmlClassDiagramTemplate(this);
            return this.Add(result);
        }

        public Template AddText(Description text)
        {
            TextTemplate result = new TextTemplate(this);
            result.Text = text;
            return this.Add(result);
        }
    }

    public class SectionTemplate : Template
    {
        public string Title { get; set; }

        public SectionTemplate(Template parent) 
            : base(parent)
        {
            
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public abstract class DoxygenObjectTemplate : Template
    {
        public DoxygenObjectTemplate(Template parent)
            : base(parent)
        {
        }
    }

    public class NamespaceListTemplate : DoxygenObjectTemplate
    {
        public NamespaceListTemplate(Template parent)
            : base(parent)
        {
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class ClassifierListTemplate : DoxygenObjectTemplate
    {
        public ClassifierListTemplate(Template parent)
            : base(parent)
        {
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class InterfaceListTemplate : ClassifierListTemplate
    {
        public InterfaceListTemplate(Template parent)
            : base(parent)
        {
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class ClassListTemplate : ClassifierListTemplate
    {
        public ClassListTemplate(Template parent)
            : base(parent)
        {
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class StructListTemplate : ClassifierListTemplate
    {
        public StructListTemplate(Template parent)
            : base(parent)
        {
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class EnumListTemplate : ClassifierListTemplate
    {
        public EnumListTemplate(Template parent)
            : base(parent)
        {
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class MemberListTemplate : DoxygenObjectTemplate
    {
        public bool GroupByVisibility { get; set; }

        public MemberListTemplate(Template parent)
            : base(parent)
        {
            this.GroupByVisibility = true;
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class EnumValueListTemplate : MemberListTemplate
    {
        public EnumValueListTemplate(Template parent)
            : base(parent)
        {
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class FieldListTemplate : MemberListTemplate
    {
        public FieldListTemplate(Template parent)
            : base(parent)
        {
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class PropertyListTemplate : MemberListTemplate
    {
        public PropertyListTemplate(Template parent)
            : base(parent)
        {
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class MethodListTemplate : MemberListTemplate
    {
        public MethodListTemplate(Template parent)
            : base(parent)
        {
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class DescriptionTemplate : Template
    {
        public DescriptionTemplate(Template parent)
            : base(parent)
        {
            this.Leaf = true;
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class BriefDescriptionTemplate : DescriptionTemplate
    {
        public BriefDescriptionTemplate(Template parent)
            : base(parent)
        {

        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class UmlClassDiagramTemplate : Template
    {
        public UmlClassDiagramTemplate(Template parent)
            : base(parent)
        {
            this.Leaf = true;
        }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

    public class TextTemplate : Template
    {
        public TextTemplate(Template parent)
            : base(parent)
        {
            this.Leaf = true;
        }

        public Description Text { get; set; }

        public override void Process(IApiDocTemplateProcessor processor)
        {
            processor.Process(this);
        }
    }

}
