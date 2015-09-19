using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftwareEngineeringTools.Documentation
{

    public enum Language
    {
        Hungarian,
        English
    }

    public class ApiDocConfig
    {
        public Language Language { get; set; }
        public int ChapterLevel { get; set; }

        public bool NamespaceChapter { get; set; }
        public bool ClassifierChapter { get; set; }

        public bool InheritanceHierarchyText { get; set; }
        public bool InheritanceHierarchyUml { get; set; }
        public bool Syntax { get; set; }
        public ProgrammingLanguage ProgrammingLanguage { get; set; }


        public ApiDocConfig()
        {
            this.ChapterLevel = 1;
            this.NamespaceChapter = true;
            this.ClassifierChapter = true;
        }
    }
}
