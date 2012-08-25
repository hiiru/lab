using System;
using System.Collections.Generic;
using Awesomium.Core;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Threading;

namespace AS24SPI
{
    internal class Program
    {
        private const int TIMEOUT_REQUEST = 30;
        private const int TIMEOUT_LASTACTION = 5;

        private static bool finishedLoading;
        private static DateTime lastAction;
        private static bool debug = false;

        private static void Main(string[] args)
        {
            PrintMessage("Starting spider test.", true);
            var config = new WebCoreConfig { CustomCSS = "::-webkit-scrollbar { visibility: hidden; }" };
            WebCore.Initialize(config);

            using (WebView webView = WebCore.CreateWebView(1800, 3600))
            {
                webView.LoadCompleted += OnFinishLoading;
                webView.BeginLoading += OnBeginLoading;
                webView.ResourceRequest += OnRessourceRequest;

                webView.LoadURL("http://www.autoscout24.ch/default.aspx?lng=de");
                WebCore.SetCookie("http://www.autoscout24.ch", ".listPageSize=50; domain=autoscout24.ch; path=/Search/", false, false);

                WaitForRequest();
                string idModelDDL = "DdlMakes";
                string idSearchButton = "BtnSearch";
                JSValue value = webView.ExecuteJavascriptWithResult(String.Format("document.getElementById('{0}').value='{1}'", idModelDDL, 12894));  //If you want simulate KeyStrokes instead you should use KeyEvent

                webView.Render().SaveToPNG("pre-submitsearch.png", true);
                Process.Start("pre-submitsearch.png");
                JSValue valueSelectText = webView.ExecuteJavascriptWithResult(String.Format("document.getElementById('{0}').click()", idSearchButton)); //With This you take focus of textbox

                WaitForRequest();
                webView.ExecuteJavascriptWithResult("document.getElementsByTagName('html')[0].outerHTML;");
                JSValue innerHtml = webView.ExecuteJavascriptWithResult("document.getElementById('aspnetForm').getElementsByTagName('fieldset')[0].innerHTML");
                var list = parseList(innerHtml);
                if (list != null && list.Count > 0)
                {
                    printList(list);
                }

                webView.Render().SaveToPNG("result.png", true);
                Process.Start("result.png");
            }

            Console.ReadKey(true);

            // Shut down Awesomium before exiting.
            WebCore.Shutdown();
        }

        private static void printList(List<CarListItem> list)
        {
            foreach (var carListItem in list)
            {
                Console.WriteLine(string.Format("#{0} - {1} - {2}", carListItem.Id, carListItem.Title, carListItem.Price));
            }
            Console.WriteLine("-----------------------");
            Console.WriteLine("Total Objects:" + list.Count);
        }

        private static List<CarListItem> parseList(JSValue innerHtml)
        {
            if (innerHtml == null) return null;
            string html = innerHtml.ToString();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            List<CarListItem> retVal = new List<CarListItem>();
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class='car']"))
            {
                var item = new CarListItem();
                item.Id = node.SelectSingleNode(".//input[@type='checkbox']").GetAttributeValue("value", 0);
                item.UrlImage = node.SelectSingleNode(".//img").GetAttributeValue("src", "");
                item.Title = node.SelectSingleNode(".//h3[@class='car-title show-both']/a").GetAttributeValue("title", "");
                item.Abstract = node.SelectSingleNode(".//p[@class='description']").InnerText;

                item.BuildYear = node.SelectSingleNode(".//li[@class='prop prop-date']").InnerText;
                item.Milage = node.SelectSingleNode(".//li[@class='prop prop-milage']").InnerText;
                item.Price = node.SelectSingleNode(".//li[@class='prop prop-price']").InnerText;
                retVal.Add(item);
            }
            return retVal;
        }

        private static HashSet<string> blockedHosts = new HashSet<string>
		                                	{
		                                		"at06.alenty.com",
														"aka-cdn-ns.adtech.de",
														"ad.dc2.adtech.de",
														"autosct.wemfbox.ch",
														"www.google-analytics.com",
														"www.admanagement.ch",
														"qs.wemfbox.ch",
		                                	};

        private static ResourceResponse OnRessourceRequest(object sender, ResourceRequestEventArgs e)
        {
            string host = GetHost(e.Request.Url);
            if (host == null || blockedHosts.Contains(host))
            {
                PrintMessage("blocked: " + host);

                //ignore request from this domain
                e.Request.Cancel();
                return null;
            }
            lastAction = DateTime.Now;
            PrintMessage("ressource: " + e.Request.Url);
            return new ResourceResponse(e.Request.Url);
        }

        private static string GetHost(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("http")) return null;
            string[] split = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length >= 2) return split[1];
            return null;
        }

        private static void OnBeginLoading(object sender, BeginLoadingEventArgs e)
        {
            PrintMessage("loading: " + e.Url);
        }

        private static void OnFinishLoading(object sender, EventArgs e)
        {
            finishedLoading = true;
        }

        private static void WaitForRequest()
        {
            DateTime startTime = DateTime.Now;
            lastAction = startTime;
            while (!finishedLoading && lastAction.AddSeconds(5) > DateTime.Now && startTime.AddSeconds(20) > DateTime.Now)//(!finishedLoading)
            {
                Thread.Sleep(100);

                // A Console application does not have a synchronization
                // context, thus auto-update won't be enabled on WebCore.
                // We need to manually call Update here.
                WebCore.Update();
            }
            finishedLoading = false;
        }

        private static void PrintMessage(string msg, bool force = false)
        {
            if (force || debug)
                Console.WriteLine(msg);
        }

        public class CarListItem
        {
            public int Id;
            public string Title;
            public string Abstract;
            public string BuildYear;
            public string Milage;
            public string Price;
            public string UrlImage;
            public string UrlDetail;
        }
    }
}