
namespace wem
{


    class MainClass
    {

        //Test funkcion to WikiReader
        public static void Main(string[] args)
        {
            //Converter.MediaWiki2XhtmlFragmentTest();
            //Converter.MediaWiki2XhtmlFileTest();
            string text = Converter.MediaWikiToXHTML(@"{|
|Food complements || Test
|-
|Orange
|Apple
|-
|Bread
|Pie
|-
|Butter
|Ice cream 
|}");

            System.Console.WriteLine(System.Environment.NewLine);
            System.Console.WriteLine(" --- Press any key to continue --- ");
            System.Console.ReadKey();
        }


    }


}
