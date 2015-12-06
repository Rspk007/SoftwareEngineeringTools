using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SoftwareEngineeringTools.Documentation;

namespace SoftwareEngineeringTools.Testing
{
    public class Command : DocCmd
    {
        public commandType CommandName;
        public Dictionary<string, string> paramters;
        static public controllerType Controller;
        static protected AutoItController aic;
        static protected SeleniumController sc;
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
            SAVE,
            SCREENSHOT,
            WAITACTIVE,
            WRITE
        }

        public Command()
        {
            this.paramters = new Dictionary<string, string>();
            this.Kind = DocKind.Command;

        }
        public void execute()
        {
            switch (this.CommandName)
            {
                case commandType.CLICK:
                    string title = "";                        
                    string controll = "";

                    paramters.TryGetValue("title", out title);
                    paramters.TryGetValue("controll", out controll);
                    if (Controller == controllerType.SELENIUM)
                    {
                        
                        sc.click(title, controll);
                    }
                    else
                    {
                        aic.click(title, controll);
                    }
                    break;
                case commandType.CLOSE:
                    title = "";
                    paramters.TryGetValue("title", out title);
                    if (Controller == controllerType.SELENIUM)
                    {
                        sc.winClose(title);
                    }
                    else
                    {
                        aic.winClose(title);
                    }
                    break;
                case  commandType.INIT:
                    string type="";
                    string browserType = "";

                    paramters.TryGetValue("type", out type);
                    paramters.TryGetValue("browserType", out browserType);
                    if (type.ToUpper() == "SELENIUM")
                    {
                        try
                        {
                            sc = new SeleniumController((SoftwareEngineeringTools.Testing.SeleniumController.BrowserType)Enum.Parse(typeof(SoftwareEngineeringTools.Testing.SeleniumController.BrowserType),browserType));
                        }
                        catch(ArgumentException)
                        {
                            Console.WriteLine("Invalid browser type:");
                        }
                    }
                    else
                    {

                    }
                    break;
                case commandType.KEYCOMMAND:
                    title = "";
                    string text = "";
                    paramters.TryGetValue("title", out title);
                    paramters.TryGetValue("text", out text);
                    if (Controller == controllerType.SELENIUM)
                    {
                        sc.keyCommand("", text);
                    }
                    else
                    {
                        aic.keyCommand(title, text);
                    }
                    break;
                case commandType.OPEN:
                    Uri uriResult;
                    string path;
                    string className;

                    paramters.TryGetValue("path", out path);
                    paramters.TryGetValue("className", out className);
                    bool result = Uri.TryCreate(path, UriKind.Absolute, out uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp
                        || uriResult.Scheme == Uri.UriSchemeHttps);
                    if(result)
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
                    break;
                case commandType.SAVE:
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
                    break;
                case commandType.WAITACTIVE:
                    title = "";

                    paramters.TryGetValue("title", out title);
                    if (Controller == controllerType.SELENIUM)
                    {
                        sc.waitActive(title);
                    }
                    else
                    {
                        aic.waitActive(title);
                    }
                    break;   
                case commandType.WRITE:
                     title = "";
                     controll = "";
                     text = "";

                     paramters.TryGetValue("title", out title);
                     paramters.TryGetValue("controll", out controll);
                     paramters.TryGetValue("text", out text);
                    if(Controller == controllerType.SELENIUM)
                    {
                        sc.write("", controll, text);
                    }
                    else
                    {
                        aic.write(title, controll, text);
                    }
                    break;
                default:
                    break;
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
            if(Controller == controllerType.SELENIUM)
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

    public class IfCommand :Command
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

        public void execute() 
        { 
            if(decisionCommand.execute())
            {
                foreach (var command in trueCommand)
                {
                    command.execute();
                }
                
            }
            else
            {
                foreach (var command in falseCommand)
                {
                    command.execute();
                }
            }
        }
    }
}
