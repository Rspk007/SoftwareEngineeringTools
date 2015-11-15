using AutoIt;
using AutoItX3Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftwareEngineeringTools.Testing
{
    public class AutoItController
    {
        static AutoItX3 au3;         //our au3 class that gives us au3 functionality
        static Thread thread;                           //our thread
        static bool threadshouldexecute = true;         //only execute thread 2 while this equals true
        static int i = 0;                               //our incrementer
        string hWnd;

        public AutoItController()
        {
            au3 = new AutoItX3();                              //initialize our au3 class library
            au3.AutoItSetOption("WinTitleMatchMode", 2);                        //advanced window matching
            hWnd = "";                                                   //let's use a window handle
        }
        /// <summary>
        /// Use AutoIt to open the given program. If it exist, than only activate it
        /// </summary>
        /// <param name="path">Path of the program</param>
        /// <param name="className">Class of the program</param>
        public void open(string path, string className)
        {
            if (au3.WinExists(className, "") == 0)
                au3.Run(path, "", au3.SW_SHOW);   
            else
                au3.WinActivate(className, "");           
        }

        public void write(string title, string controll, string text)
        {
            au3.WinActivate(title, "");
            au3.WinWaitActive(title, "");
            au3.ControlSend(title, "", controll, text,1);
        }

        public void keyCommand(string title, string text)
        {
            au3.WinActivate(title, "");
            au3.WinWaitActive(title, "");
            au3.Send(text);
        }

        public void click(string title, string controll)
        {
            au3.WinActivate(title, "");
            au3.WinWaitActive(title, "",10);
           au3.ControlClick(title, "", controll);
        }

        public bool exist(string title)
        {
            return au3.WinExists(title) != 0;
        }

        public void waitActive(string title)
        {
            au3.WinActivate(title, "");
            au3.WinWaitActive(title, "");
        }

        public void winClose(string title)
        {
            au3.WinClose(title,"");
        }

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
            AutoItX.Run(@"C:\Program Files\WinRAR\WinRAR.exe", "");
            AutoItX.WinWaitActive("[CLASS:WinRarWindow]", "", 10);
            int expWindow = AutoItX.WinExists("[CLASS:RarReminder]");
            if(expWindow!=0)
            {
                AutoItX.ControlClick("[CLASS:RarReminder]", "", "[CLASS:Button; INSTANCE:1]");
            }
            var buyWindow = AutoItX.WinExists("[CLASS:#32770]");
            if(buyWindow!=0)
            {
                AutoItX.ControlClick("[CLASS:#32770]", "", "[ID:1]");
            }
        }

        /// <summary>
        /// The entry point, or main thread / main loop, of our program
        /// </summary>
        public void test()
        {
            au3 = new AutoItX3();                              //initialize our au3 class library

            au3.AutoItSetOption("WinTitleMatchMode", 4);                        //advanced window matching

            thread = new Thread(new ThreadStart(threadtest));                   //initialize and start our thread
            thread.Start();

            if (au3.WinExists("Névtelen - Jegyzettömb", "") == 0)                   //if an Untitled - Notepad document doesn't exist
                au3.Run(@"C:\WINDOWS\SYSTEM32\notepad.exe", "", au3.SW_SHOW);   //run notepad
            else
                au3.WinActivate("Névtelen - Jegyzettömb", "");                      //otherwise activate the window

            string hWnd = "";                                                   //let's use a window handle

            while (hWnd.Length == 0)                                            //try to get a handle to notepad until it succeeds
                hWnd = au3.WinGetHandle("Névtelen - Jegyzettömb", "");

            while (au3.WinActive("handle=" + hWnd, "") == 0)                    //loop while it's not active
            {
                au3.WinActivate("handle=" + hWnd, "");                          //and activate it
                Thread.Sleep(100);
            }

            while (au3.WinExists("handle=" + hWnd, "") != 0)                    //while the window exists, loop
            {
                //send our incrementing variable, i, to notepad, with a trailing |
                au3.ControlSend("handle=" + hWnd, "", "Edit1", i.ToString() + "|", 0);

                i++;                                                            //increment i

                Thread.Sleep(100);                                              //short sleep so we don't burn CPU
            }

            //if the while loop exited--because there's no Untitled - Notepad--make the other thread stop executing
            threadshouldexecute = false;

            Console.Write("Press [ENTER] to continue...");                      //tell the user to press ENTER to quit
            Console.ReadLine();                                                 //pause until enter is pressed
        }

        /// <summary>
        /// our void function to execute thread #2
        /// </summary>
        static void threadtest()
        {
            while (threadshouldexecute)                             //loop while this thread should execute
            {
                au3.ToolTip("Thread 2\ni: " + i.ToString(), 0, 0);  //display a tooltip with the incrementing variable i in it

                Thread.Sleep(50);                                   //sleep to free up CPU
            }

            au3.ToolTip("", 0, 0);                                  //clear the tooltip after loop is done
        }

    }
}
