using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;

namespace ConsoleApp2
{
    class Program
    {
        static Random rnd = new Random();

        static void SurfingThread()
        {
            Thread_Count++;
            Browser b = new Browser();
            try
            {
                b.DoSurf(rnd);
            }
            catch(Exception e) {
                ;
            }
            b.Quit();
            Thread_Count--;
        }
        
        static void Main(string[] args)
        {
            while (true)
            {
                if(Thread_Count < 50)
                {
                    new Thread(() => SurfingThread()).Start();
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        static int thread_cnt = 0;
        static readonly object o_thread_cnt = new object();
        static int Thread_Count
        {
            get
            {
                lock (o_thread_cnt)
                {
                    return thread_cnt;
                }
            }
            set
            {
                lock (o_thread_cnt)
                {
                    thread_cnt = value;
                }
            }
        }
    }

    public class Browser
    {
        IWebDriver webDriver;

        public Browser()
        {
            ChromeOptions options = new ChromeOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();

            //options.AddArgument("--no-sandbox");

            options.AddArgument("--headless");

            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-first-run");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-notifications");

            options.AddArgument("--log-level=3");
            service.HideCommandPromptWindow = true;
            service.SuppressInitialDiagnosticInformation = true;

            options.AddArgument("--blink-settings=imagesEnabled=false");

            webDriver = new ChromeDriver(service, options, TimeSpan.FromSeconds(30));
        }

        public void DoSurf(Random rnd)
        {
            webDriver.Navigate().GoToUrl("http://bbc.com");

            for (int i = 0; i < 30; ++i)
            {
                if (WaitForElementLoad(By.Id("orb-footer")))
                {
                    List<IWebElement> links = webDriver.FindElements(By.TagName("a")).ToList();
                    if (links.Count > 0)
                    {
                        IWebElement link = links[rnd.Next(links.Count)];
                        if (link.GetAttribute("href").Contains("/news/"))
                        {
                            try
                            {
                                link.Click();
                            }
                            catch { }
                        }
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        public void Quit()
        {
            try
            {
                ((IJavaScriptExecutor)webDriver).ExecuteScript("return window.stop");
            }
            catch { }

            try
            {
                webDriver.Close();
            }
            catch { }

            try
            {
                webDriver.Quit();
            }
            catch { }

            webDriver = null;
        }

        bool WaitForElementLoad(By elementLocator, bool check_visibility = false, int timeout = 10000)
        {
            bool result = false;

            int i = 0;
            int t = timeout / 10;

            while (i < timeout)
            {
                System.Threading.Thread.Sleep(t);

                try
                {
                    if (ElementExists(elementLocator, check_visibility))
                    {
                        result = true;
                        break;
                    }
                }
                catch { }

                i += t;
            }

            return result;
        }

        bool ElementExists(By locator, bool check_visibility = false)
        {
            IWebElement el = webDriver.FindElement(locator);

            if (el == null)
            {
                return false;
            }

            if (check_visibility)
            {
                return el.Displayed;
            }

            return true;
        }
    }
}
