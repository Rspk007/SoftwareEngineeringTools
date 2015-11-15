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
        public List<string> paramters;
        static public controllerType Controller;
        static protected AutoItController aic;
        public enum controllerType
        {
            AUTOIT,
            SELENIUM
        }

        public enum commandType
        {
            NONE,
            OPEN,
            WRITE,
            CLICK,
            WAITACTIVE,
            KEYCOMMAND,
            EXIST,
            IF,
            ELSE,
            ENDIF,
            CLOSE,
            WINRARTEST
        }

        public Command()
        {
            this.paramters = new List<string>();
            this.Kind = DocKind.Command;

        }
        public void execute()
        {
            switch (this.CommandName)
            {
                case commandType.OPEN:
                    Uri uriResult;
                    bool result = Uri.TryCreate(paramters[0], UriKind.Absolute, out uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp
                        || uriResult.Scheme == Uri.UriSchemeHttps);
                    if(result)
                    {

                    }
                    else
                    {
                        Controller = controllerType.AUTOIT;
                        aic = new AutoItController();
                        aic.open(paramters[0], paramters[1]);
                    }
                    break;
                case commandType.WRITE:
                    if(Controller == controllerType.SELENIUM)
                    {

                    }
                    else
                    {
                        aic.write(paramters[0], paramters[1], paramters[2]);
                    }
                    break;
                case commandType.CLICK:
                    if(Controller == controllerType.SELENIUM)
                    {

                    }
                    else
                    {
                        aic.click(paramters[0], paramters[1]);
                    }                    
                    break;
                case commandType.CLOSE:
                    if (Controller == controllerType.SELENIUM)
                    {

                    }
                    else
                    {
                        aic.winClose(paramters[0]);
                    }
                    break;
                case commandType.KEYCOMMAND:
                    if (Controller == controllerType.SELENIUM)
                    {

                    }
                    else
                    {
                        aic.keyCommand(paramters[0], paramters[1]);
                    }
                    break;
                case commandType.WAITACTIVE:
                    if (Controller == controllerType.SELENIUM)
                    {

                    }
                    else
                    {
                        aic.waitActive(paramters[0]);
                    }
                    break;
                case commandType.WINRARTEST:
                    aic = new AutoItController();
                    aic.WinRARTest();
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
                return aic.exist(paramters[0]);
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
