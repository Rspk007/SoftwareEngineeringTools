using AutoIt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareEngineeringTools.Testing
{
    public class AutoItController
    {
        public void NotepadTest()
        {            
            if(AutoItX.WinExists("Névtelen") == 0)
            {
                AutoItX.Run("notepad.exe", "");
                AutoItX.WinWaitActive("Névtelen");
            }
            else
            {
                AutoItX.WinActive("Névtelen");
            }                        
            AutoItX.Send("I'm in notepad");
            AutoItX.WinClose("Névtelen");
            AutoItX.WinWaitActive("Jegyzettömb", "Ne mentsen");
            AutoItX.Send("!n");            
        }

        public void WinRARTest()
        {
            AutoItX.Run("winrar.exe",@"C:\Users\Zsolt\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\WinRAR");
            AutoItX.WinWaitActive("[CLASS:WinRarWindow]", "", 10);
            int expWindow = AutoItX.WinExists("[CLASS:RarReminder]");
            if(expWindow!=0)
            {
                AutoItX.ControlClick("[CLASS:RarReminder]", "", "[CLASS:Button; INSTANCE:1]");
            }
            var buyWindow = AutoItX.WinExists("[CLASS:#32770]");
            if(buyWindow!=0)
            {
                AutoItX.ControlClick("[CLASS:#32770]", "", "Bezár");
            }
        }

    }
}
