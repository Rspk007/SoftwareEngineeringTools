using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;

// Requires reference to WebDriver.Support.dll
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Chrome;
using System.Drawing.Imaging;

namespace SoftwareEngineeringTools.Testing
{
    public class SeleniumController
    {

        public enum BrowserType
        {
            HtmlUnit,
            Firefox,
            InternetExplorer,
            Chrome,
            Edge
        }
        BrowserType currentBrowserType;
        IWebDriver webdriver;

        public SeleniumController()
        {
            currentBrowserType = BrowserType.HtmlUnit;
        }

        public SeleniumController(BrowserType type)
        {
            currentBrowserType = type;
            switch (type)
            {
                case BrowserType.HtmlUnit: 
                    break;
                case BrowserType.Firefox:
                    webdriver = new FirefoxDriver();
                    break;
                case BrowserType.InternetExplorer:
                    webdriver = new InternetExplorerDriver();
                    break;
                case BrowserType.Chrome:
                    webdriver = new ChromeDriver();
                    break;
                case BrowserType.Edge:
                    webdriver = new EdgeDriver();
                    break;
                default:
                    break;
            }
        }

        public void open(string path, string className="")
        {
            if(webdriver == null)
            {
                webdriver = new RemoteWebDriver(new Uri(path),
                                        DesiredCapabilities.HtmlUnit());

            }            
            webdriver.Navigate().GoToUrl(path);     
       
        }

        public void write(string title, string controll, string text)
        {
            string[] findBy = controll.Split(':', ';');
            IWebElement query;
            IList<IWebElement> queries = new List<IWebElement>(); 
            for(int i = 0; i<findBy.Count(); i = i+2)
            {
                string type = findBy[i];
                string searchsequence = findBy[i + 1];
                if(type.ToLower() == "id")
                {
                    query = webdriver.FindElement(By.Id(searchsequence));
                    break;
                }
                else if (type.ToLower() == "classname")
                {
                    if (queries.Count == 0)
                    {
                        queries = webdriver.FindElements(By.ClassName(searchsequence));
                    }
                    else
                    {
                        queries = queries.Where(webelement => webelement.GetAttribute("class") == searchsequence).ToList();
                    }

                }
                else if(type.ToLower() == "tagname")
                {
                    if (queries.Count == 0)
                    {
                        queries = webdriver.FindElements(By.TagName(searchsequence));
                    }
                    else
                    {
                        queries = queries.Where(webelement => webelement.TagName == searchsequence).ToList();
                    }
                }
                else if(type.ToLower() == "name")
                {
                    if (queries.Count == 0)
                    {
                        queries = webdriver.FindElements(By.Name(searchsequence));
                    }
                    else
                    {
                        queries = queries.Where(webelement => webelement.GetAttribute("name") == searchsequence).ToList();
                    }
                }
            }
            if(queries.Count == 1)
            {
                query = queries.First();
            }
            else if(queries.Count > 0)
            {
                Console.WriteLine("Controll isn't enought specific! The first of the matching elements will be selected");
                query = queries.First();
            }
            else
            {
                return;
            }
            query.SendKeys(text);            
        }

        public void keyCommand(string title, string text)
        {
            IWebElement query = webdriver.FindElement(By.TagName("html"));
            query.SendKeys(text);
        }

        public void click(string title, string controll)
        {
            string[] findBy = controll.Split(':', ';');
            IWebElement query;
            IList<IWebElement> queries = new List<IWebElement>();
            for (int i = 0; i < findBy.Count(); i = i + 2)
            {
                string type = findBy[i].Replace(" ",string.Empty);
                string searchsequence = findBy[i + 1];
                if (type.ToLower() == "id")
                {
                    query = webdriver.FindElement(By.Id(searchsequence));
                    break;
                }
                else if (type.ToLower() == "classname")
                {
                    if (queries.Count == 0)
                    {
                        queries = webdriver.FindElements(By.ClassName(searchsequence));
                    }
                    else
                    {
                        queries = queries.Where(webelement => webelement.GetAttribute("class") == searchsequence).ToList();
                    }

                }
                else if (type.ToLower() == "tagname")
                {
                    if (queries.Count == 0)
                    {
                        queries = webdriver.FindElements(By.TagName(searchsequence));
                    }
                    else
                    {
                        queries = queries.Where(webelement => webelement.TagName == searchsequence).ToList();
                    }
                }
                else if (type.ToLower() == "name")
                {
                    if (queries.Count == 0)
                    {
                        queries = webdriver.FindElements(By.Name(searchsequence));
                    }
                    else
                    {
                        queries = queries.Where(webelement => webelement.GetAttribute("name") == searchsequence).ToList();
                    }
                }
            }
            if (queries.Count == 1)
            {
                query = queries.First();
            }
            else if (queries.Count > 0)
            {
                Console.WriteLine("Controll isn't enought specific! The first of the matching elements will be selected");
                query = queries.First();
            }
            else
            {
                return;
            }
            query.Click();
        }

        public bool exist(string title)
        {
            WebDriverWait wait = new WebDriverWait(webdriver, TimeSpan.FromSeconds(10));
            return wait.Until((d) => { return d.Title.ToLower().Contains(title); });
        }

        public void waitActive(string title)
        {
            WebDriverWait wait = new WebDriverWait(webdriver, TimeSpan.FromSeconds(60));
            wait.Until((d) => { return d.Title.ToLower().Contains(title.ToLower()); });
        }
        public void winClose(string title)
        {
            webdriver.Close();            
        }

        public void save(string filePath)
        {
            try
            {
                Screenshot ss = ((ITakesScreenshot)webdriver).GetScreenshot();
                string extension = filePath.Split('.')[1];
                ss.SaveAsFile(filePath, getFormat(extension));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public ImageFormat getFormat(string extension)
        {
            ImageFormat returnImageFormat;
            switch(extension.ToLower())
            {
                case "bmp":
                    returnImageFormat = ImageFormat.Bmp;
                    break;
                case "emf":
                    returnImageFormat = ImageFormat.Emf;
                    break;
                case "exif":
                    returnImageFormat = ImageFormat.Exif;
                    break;
                case "gif":
                    returnImageFormat = ImageFormat.Gif;
                    break;
                case "icon":
                    returnImageFormat = ImageFormat.Icon;
                    break;
                case "jpeg":
                    returnImageFormat = ImageFormat.Jpeg;
                    break;
                default:
                case "png":
                    returnImageFormat = ImageFormat.Png;
                    break;
                case "tiff":
                    returnImageFormat = ImageFormat.Tiff;
                    break;
                case "wmf":
                    returnImageFormat = ImageFormat.Wmf;
                    break;
            }
            return returnImageFormat;
        }

        public void test()
        {
            // Create a new instance of the Firefox driver.

            // Notice that the remainder of the code relies on the interface, 
            // not the implementation.

            // Further note that other drivers (InternetExplorerDriver,
            // ChromeDriver, etc.) will require further configuration 
            // before this example will work. See the wiki pages for the
            // individual drivers at http://code.google.com/p/selenium/wiki
            // for further information.
            IWebDriver driver = new FirefoxDriver();

            //Notice navigation is slightly different than the Java version
            //This is because 'get' is a keyword in C#        
            driver.Navigate().GoToUrl("http://www.google.com/");

            // Find the text input element by its name
            IWebElement query = driver.FindElement(By.Name("q"));

            // Enter something to search for
            query.SendKeys("Cheese");

            // Now submit the form. WebDriver will find the form for us from the element
            query.Submit();

            // Google's search is rendered dynamically with JavaScript.
            // Wait for the page to load, timeout after 10 seconds
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until((d) => { return d.Title.ToLower().StartsWith("cheese"); });

            // Should see: "Cheese - Google Search"
            System.Console.WriteLine("Page title is: " + driver.Title);

            //Close the browser
            driver.Quit();
        }
        
    }
    
   
}
