using AutoIt;
using AutoItX3Lib;
using SoftwareEngineeringTools.Documentation;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SoftwareEngineeringTools.Testing
{
    public class AutoItController
    {
        static AutoItX3 au3;         //our au3 class that gives us au3 functionality
        static Thread thread;                           //our thread
        static bool threadshouldexecute = true;         //only execute thread 2 while this equals true
        static int i = 0;                               //our incrementer
        string hWnd;
        bool writeAfterRead;

        public AutoItController()
        {
            au3 = new AutoItX3();                              //initialize our au3 class library
            au3.AutoItSetOption("WinTitleMatchMode", 2);                        //advanced window matching
            hWnd = "";                                                   //let's use a window handle
            writeAfterRead = true;
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

            hWnd = au3.WinGetHandle(className, "");
        }

        public void write(string title, string controll, string text)
        {
            au3.WinActivate(title, "");
            au3.WinWaitActive(title, "");
            hWnd = au3.WinGetHandle(title, "");
            au3.ControlSend(title, "", controll, text,1);
        }

        public void keyCommand(string title, string text)
        {
            au3.WinActivate(title, "");
            au3.WinWaitActive(title, "");
            hWnd = au3.WinGetHandle(title, "");
            au3.Send(text);
        }

        public void click(string title, string controll)
        {
            au3.WinActivate(title, "");
            au3.WinWaitActive(title, "",5);
            hWnd = au3.WinGetHandle(title, "");
           au3.ControlClick(title, "", controll);
        }

        public bool exist(string title)
        {
            au3.WinWaitActive(title, "", 5);
            Thread.Sleep(500);
            return au3.WinExists(title) != 0;           
        }

        public void waitActive(string title)
        {
            au3.WinActivate(title, "");
            au3.WinWaitActive(title, "");
            hWnd = au3.WinGetHandle(title, "");
        }

        public void close(string title)
        {
            au3.WinClose(title,"");
        }

        public string read(string title, string controll, IDocumentGenerator dg)
        {
            throw new NotImplementedException();
        }

        public void testIfEqual(string title, string controll, string testValue, IDocumentGenerator dg)
        {
            writeAfterRead = false;
            string result = read(title, controll, dg);
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
                dg.BeginMarkup(DocumentMarkupKind.Emphasis);
                dg.PrintText(" " + result);
                dg.EndMarkup(DocumentMarkupKind.Emphasis);
                dg.EndMarkup(DocumentMarkupKind.Fail);
            }

        }

        public void save (string filePath)
        {
            au3.Send("!{PRINTSCREEN}");
            Thread.Sleep(400);      
            saveImage(filePath);
        }

        
        public void saveImage(string filePath)
        {
            if (Clipboard.GetDataObject() != null)
            {
                IDataObject data = Clipboard.GetDataObject();

                if (data.GetDataPresent(DataFormats.Bitmap))
                {
                    Image image = (Image)data.GetData(DataFormats.Bitmap, true);

                    switch (filePath.Split('.')[1])
                    {
                        case "Jpeg":
                            {
                                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                break;
                            }
                        case "Bmp":
                            {
                                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);
                                break;
                            }
                        case "Gif":
                            {
                                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Gif);
                                break;
                            }
                        case "Png":
                            {
                                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                                break;
                            }
                        case "Tiff":
                            {
                                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Tiff);
                                break;
                            }
                        case "Exif":
                            {
                                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Exif);
                                break;
                            }
                        case "Wmf":
                            {
                                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Wmf);
                                break;
                            }
                    }
                }
                else
                {
                    
                }
            }
            else
            {
               
            } 
        }

        

        public static void NotepadTest()
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

        public static void WinRARTest()
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
        public static void test()
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
