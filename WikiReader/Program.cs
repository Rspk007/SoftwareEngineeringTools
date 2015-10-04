
using System;
using System.IO;
namespace wem
{


    class MainClass
    {

        //Test funkcion to WikiReader
        public static void Main(string[] args)
        {
            //Converter.MediaWiki2XhtmlFragmentTest();
            //Converter.MediaWiki2XhtmlFileTest();
            String line;
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(@"..\..\..\SoftwareEngineeringTools\ApiDoc.wiki"))
                {
	            // Read the stream to a string, and write the string to the console.
                    line = sr.ReadToEnd();
                    string text = Converter.MediaWikiToXHTML(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            

            System.Console.WriteLine(System.Environment.NewLine);
            System.Console.WriteLine(" --- Press any key to continue --- ");
            System.Console.ReadKey();
        }


    }


}
