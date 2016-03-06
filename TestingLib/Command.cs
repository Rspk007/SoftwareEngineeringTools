using System;
using System.Collections.Generic;
using System.Linq;

using SoftwareEngineeringTools.Documentation;
using System.Reflection;

namespace SoftwareEngineeringTools.Testing
{
    public class Command : DocCmd
    {
        public commandType CommandName;
        public static Dictionary<string, string> variables;
        public Dictionary<string, object> paramteters;
        public static string Title;
        public static controllerType Controller;
        protected static AutoItController aic;
        protected static SeleniumController sc;
        public string commandName { get; set; }
        public string parameter { get; set; }

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

        public void addParamteter(string name, string value)
        {
            variables.Add(name, value);
        }

        public Command()
        {
            variables = new Dictionary<string, string>();
            Kind = DocKind.Command;
            CommandName = commandType.NONE;

        }

        public override void print(IDocumentGenerator dg, int sectionLevel)
        {            
            execute(dg,sectionLevel);
        }

        public virtual void execute(IDocumentGenerator dg, int sectionLevel)
        {
            Type type;
            if (sc == null && aic == null)
            {
                type = typeof(Command);
            }
            else if(sc == null)
            {
                type = typeof(AutoItController);
            }
            else
            {
                type = typeof(SeleniumController);
            }            
            MethodInfo method = type.GetMethod(commandName);
            if(method == null)
            {
                Console.WriteLine("Invalid commandname: "+commandName +". This command will be skipped.");
                return;
            }
            paramteters = new Dictionary<string, object>();
            paramteters.Add("dg", dg);
            foreach (var variable in parameter.Split(',').Where(p => !string.IsNullOrEmpty(p)))
            {
                string paramName = variable.Split('=')[0];
                string paramValue = variable.Split('=')[1];
                if(paramValue.StartsWith("$"))
                {
                    variables.TryGetValue(paramValue.Substring(1), out paramValue);
                }
                paramteters.Add(paramName, paramValue);
            }
            var arguments = method.GetParameters().Select(p => paramteters[p.Name]).ToArray();
            string result;
            if (sc == null && aic == null)
            {
                 result = (string)method.Invoke(this, arguments);
            }
            else if (sc == null)
            {
                 result = (string)method.Invoke(aic, arguments);
            }
            else
            {
                 result = (string)method.Invoke(sc, arguments);
            }
            Console.WriteLine(result);
        }

        public void init(string type, string browserType)
        {
            if (type.ToUpper() == "SELENIUM")
            {
                try
                {
                    sc = new SeleniumController((SeleniumController.BrowserType)Enum.Parse(typeof(SeleniumController.BrowserType), browserType));
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Invalid browser type:" + browserType);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public void open(string path, string className)
        {
            Uri uriResult;
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
        public class DecisionCommand : Command
        {
            public DecisionCommand()
            {
                CommandName = commandType.EXIST;
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
                    variables.TryGetValue("title", out title);
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