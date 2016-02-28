using System;
using System.Collections.Generic;

using SoftwareEngineeringTools.Documentation;

namespace SoftwareEngineeringTools.Testing
{
    public class Command : DocCmd
    {
        public commandType CommandName;
        public Dictionary<string, string> paramters;
        public static string Title;
        public static controllerType Controller;
        protected static AutoItController aic;
        protected static SeleniumController sc;
        public enum controllerType
        {
            AUTOIT,
            SELENIUM
        }

        public enum commandType
        {
            CLICK,
            CLOSE,
            ELSE,
            ENDIF,
            EXIST,
            IF,
            INIT,
            INSERT,
            KEYCOMMAND,
            NONE,
            OPEN,
            OUTPUT,
            READ,
            SAVE,
            SCREENSHOT,
            TESTIFEQUAL,
            WAITACTIVE,
            WRITE
        }

        public Command()
        {
            paramters = new Dictionary<string, string>();
            Kind = DocKind.Command;
            CommandName = commandType.NONE;

        }

        public override void print(IDocumentGenerator dg, int sectionLevel)
        {            
            execute(dg,sectionLevel);
        }

        public virtual void execute(IDocumentGenerator dg, int sectionLevel)
        {
            throw new InvalidOperationException();
        }
    }
    public class ClickCommand : Command
    {
        public ClickCommand() : base()
        {
            CommandName = commandType.CLICK;            
        }
        public override void execute(IDocumentGenerator dg, int sectionLevel)
        {
            string title = "";
            string controll = "";

            paramters.TryGetValue("title", out title);
            paramters.TryGetValue("controll", out controll);

            if (string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(Title))
            {
                title = Title;
            }
            else
            {
                Title = title;
            }
            if (Controller == controllerType.SELENIUM)
            {

                sc.click(title, controll);
            }
            else
            {
                aic.click(title, controll);
            }
        }
    }

    public class ReadCommand : Command
    {
        public ReadCommand(): base()
        {
            CommandName = commandType.READ;
        }

        public override void execute(IDocumentGenerator dg, int sectionLevel)
        {
            string title = "";
            string controll = "";
            paramters.TryGetValue("title", out title);
            paramters.TryGetValue("controll", out controll);

            if (string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(Title))
            {
                title = Title;
            }
            else
            {
                Title = title;
            }

            if (Controller == controllerType.SELENIUM)
            {
                sc.read(controll);
            }
            else
            {
                aic.read(title, controll);
            }
        }
    }

    public class TestIfEqualCommand : Command
    {
        public string testValue { get; set; }
        public TestIfEqualCommand() : base()
        {
            CommandName = commandType.TESTIFEQUAL;
        }

        public override void execute(IDocumentGenerator dg, int sectionLevel)
        {
            string title = "";
            string controll = "";
            paramters.TryGetValue("title", out title);
            paramters.TryGetValue("controll", out controll);
            if (string.IsNullOrEmpty(testValue))
            {
                string testValue;
                paramters.TryGetValue("value", out testValue);
                this.testValue = testValue;
            }

            if (string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(Title))
            {
                title = Title;
            }
            else
            {
                Title = title;
            }
            string result;
            if (Controller == controllerType.SELENIUM)
            {
                result = sc.read(controll);                
            }
            else
            {
                result = aic.read(title, controll);
            }

            if (result.Equals(testValue))
            {
                dg.BeginMarkup(DocumentMarkupKind.Success);
                dg.PrintText(testValue);
                dg.EndMarkup(DocumentMarkupKind.Success);
            }
            else
            {
                dg.BeginMarkup(DocumentMarkupKind.Fail);
                dg.PrintText(testValue);
                dg.EndMarkup(DocumentMarkupKind.Fail);
            }
        }
    }

        public class CloseCommand : Command
        {
            public CloseCommand() : base()
            {
                CommandName = commandType.CLOSE;
            }
            public override void execute(IDocumentGenerator dg, int sectionLevel)
            {
                string title = "";
                paramters.TryGetValue("title", out title);

                if (string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(Title))
                {
                    title = Title;
                }
                else
                {
                    Title = title;
                }

                if (Controller == controllerType.SELENIUM)
                {
                    sc.winClose(title);
                }
                else
                {
                    aic.winClose(title);
                }
            }
        }

        public class InitCommand : Command
        {
            public InitCommand() : base()
            {
                CommandName = commandType.INIT;
            }
            public override void execute(IDocumentGenerator dg, int sectionLevel)
            {
                string type = "";
                string browserType = "";

                paramters.TryGetValue("type", out type);
                paramters.TryGetValue("browserType", out browserType);
                if (type.ToUpper() == "SELENIUM")
                {
                    try
                    {
                        sc = new SeleniumController((SeleniumController.BrowserType)Enum.Parse(typeof(SeleniumController.BrowserType), browserType));
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Invalid browser type:");
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class KeyCommand : Command
        {
            public KeyCommand() : base()
            {
                CommandName = commandType.KEYCOMMAND;
            }
            public override void execute(IDocumentGenerator dg, int sectionLevel)
            {
                string title = "";
                string text = "";
                paramters.TryGetValue("title", out title);
                paramters.TryGetValue("text", out text);

                if (string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(Title))
                {
                    title = Title;
                }
                else
                {
                    Title = title;
                }

                if (Controller == controllerType.SELENIUM)
                {
                    sc.keyCommand("", text);
                }
                else
                {
                    aic.keyCommand(title, text);
                }
            }
        }

        public class OpenCommand : Command
        {
            public OpenCommand() : base()
            {
                CommandName = commandType.OPEN;
            }
            public override void execute(IDocumentGenerator dg, int sectionLevel)
            {
                Uri uriResult;
                string path;
                string className;

                paramters.TryGetValue("path", out path);
                paramters.TryGetValue("className", out className);
                bool result = Uri.TryCreate(path, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp
                    || uriResult.Scheme == Uri.UriSchemeHttps);
                if (result)
                {
                    Controller = controllerType.SELENIUM;
                    if (sc == null)
                    {
                        sc = new SeleniumController();
                    }
                    sc.open(path);
                }
                else
                {
                    Controller = controllerType.AUTOIT;
                    aic = new AutoItController();
                    aic.open(path, className);
                }
            }
        }

        public class SaveCommand : Command
        {
            public SaveCommand() : base()
            {
                CommandName = commandType.SAVE;
            }
            public override void execute(IDocumentGenerator dg, int sectionLevel)
            {
                string filePath;

                paramters.TryGetValue("filePath", out filePath);
                if (Controller == controllerType.SELENIUM)
                {
                    sc.save(filePath);
                }
                else
                {
                    aic.save(filePath);
                }
            }
        }

        public class WaitActiveCommand : Command
        {
            public WaitActiveCommand() : base()
            {
                CommandName = commandType.WAITACTIVE;
            }
            public override void execute(IDocumentGenerator dg, int sectionLevel)
            {
                string title = "";
                paramters.TryGetValue("title", out title);

                if (string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(Title))
                {
                    title = Title;
                }
                else
                {
                    Title = title;
                }

                if (Controller == controllerType.SELENIUM)
                {
                    sc.waitActive(title);
                }
                else
                {
                    aic.waitActive(title);
                }
            }
        }
        public class WriteCommand : Command
        {
            public WriteCommand() : base()
            {
                CommandName = commandType.WRITE;
            }
            public override void execute(IDocumentGenerator dg, int sectionLevel)
            {
                string title = "";
                string controll = "";
                string text = "";

                paramters.TryGetValue("title", out title);
                paramters.TryGetValue("controll", out controll);
                paramters.TryGetValue("text", out text);

                if (string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(Title))
                {
                    title = Title;
                }
                else
                {
                    Title = title;
                }

                if (Controller == controllerType.SELENIUM)
                {
                    sc.write("", controll, text);
                }
                else
                {
                    aic.write(title, controll, text);
                }
            }
        }

        public class DecisionCommand : Command
        {
            public DecisionCommand()
            {
                this.CommandName = commandType.EXIST;
            }

            public bool execute()
            {
                if (Controller == controllerType.SELENIUM)
                {
                    return true;
                }
                else
                {
                    string title;
                    paramters.TryGetValue("title", out title);
                    return aic.exist(title);
                }
            }
        }

        public class IfCommand : Command
        {
            public IfCommand parent;
            public DecisionCommand decisionCommand;
            public List<Command> trueCommand;
            public List<Command> falseCommand;

            public IfCommand()
            {
                this.trueCommand = new List<Command>();
                this.falseCommand = new List<Command>();
                this.CommandName = commandType.IF;
                Kind = DocKind.IfCommand;
            }

            public override void execute(IDocumentGenerator dg, int sectionLevel)
            {
                if (decisionCommand.execute())
                {
                    foreach (var command in trueCommand)
                    {
                        command.execute(dg,sectionLevel);
                    }

                }
                else
                {
                    foreach (var command in falseCommand)
                    {
                        command.execute(dg, sectionLevel);
                    }
                }
            }
        } }