using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftwareEngineeringTools.Documentation
{
    public enum DocumentMarkupKind
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

    public interface IDocumentGenerator
    {
        void BeginSectionTitle(int level, string title, string label);
        void EndSectionTitle();
        void SetContent(bool enabled, int depth);
        void NewParagraph();
        void PrintText(string text, bool insertSpace = true);
        void PrintVerbatimText(string text);
        void BeginMarkup(DocumentMarkupKind markupKind);
        void EndMarkup(DocumentMarkupKind markupKind);
        void NewLabel(string id);
        void BeginReference(string id, bool url);
        void EndReference();
        void BeginList();
        void EndList();
        void BeginListItem(int index, string title);
        void EndListItem(int index);
        void BeginTable(int rowCount, int colCount);
        void BeginTableRow(int rowIndex);
        void EndTableRow(int index);
        void BeginTableCell(int rowIndex, int colIndex, bool head);
        void EndTableCell(int rowIndex, int colIndex, bool head);
        void EndTable();
        void AddImage(string path, string Width= null, string Height=null);
    }

    public abstract class DocumentGenerator : IDocumentGenerator, IDisposable
    {
        public abstract void Dispose();

        protected abstract void BeginDocument();
        protected abstract void EndDocument();
        public abstract void BeginSectionTitle(int level, string title, string label);
        public abstract void SetContent(bool enabled, int depth);
        public abstract void EndSectionTitle();
        public abstract void NewParagraph();
        public abstract void PrintText(string text, bool insertSpace = true);
        public abstract void PrintVerbatimText(string text);
        public abstract void BeginMarkup(DocumentMarkupKind markupKind);
        public abstract void EndMarkup(DocumentMarkupKind markupKind);
        public abstract void NewLabel(string id);
        public abstract void BeginReference(string id, bool url);
        public abstract void EndReference();
        public abstract void BeginList();
        public abstract void EndList();
        public abstract void BeginListItem(int index, string title);
        public abstract void EndListItem(int index);
        public abstract void BeginTable(int rowCount, int colCount);
        public abstract void BeginTableRow(int rowIndex);
        public abstract void EndTableRow(int index);
        public abstract void BeginTableCell(int rowIndex, int colIndex, bool head);
        public abstract void EndTableCell(int rowIndex, int colIndex, bool head);
        public abstract void EndTable();
        public abstract void AddImage(string path, string Width = null, string Height = null);
    }
}
