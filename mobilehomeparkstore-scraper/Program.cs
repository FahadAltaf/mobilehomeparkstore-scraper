using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace mobilehomeparkstore_scraper
{
    public class DataModel
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Area { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<DataModel> entries = new List<DataModel>();

            string state = "washington";
            HtmlWeb web = new HtmlWeb();
            var startingUrl = $"https://www.mobilehomeparkstore.com/mobile-home-park-directory/{state}/a";
            var doc = web.Load(startingUrl);

            List<string> urls = new List<string>();
            urls.Add(startingUrl);
            var alphabets = doc.DocumentNode.SelectSingleNode("//*[@id=\"main\"]/div[1]/div[4]/h3");
            if (alphabets != null)
            {
                var nodes = alphabets.ChildNodes.Where(x => x.Name == "span").ToList();

                Console.WriteLine(startingUrl);
                for (int i = 1; i < nodes.Count; i++)
                {
                    string link = "https://www.mobilehomeparkstore.com" + nodes[i].ChildNodes[0].Attributes.FirstOrDefault(x => x.Name == "href").Value;
                    urls.Add(link);
                    Console.WriteLine(link);
                }
            }



            if (urls.Count > 0)
            {
                foreach (var url in urls)
                {
                    Console.WriteLine($"Extracting data from {url}");
                    int pages = 1;
                    doc = web.Load(url);
                    var pagesNode = doc.DocumentNode.SelectSingleNode("//ul[@class='pagination']");
                    if (pagesNode != null)
                    {
                        pages = pagesNode.ChildNodes.Where(x => x.Name == "li").Count() - 2;
                    }

                    Console.WriteLine($"Total Pages: {pages}");

                    Console.WriteLine($"Extracting data from Page: 1");
                    var allNodes = doc.DocumentNode.SelectNodes("//div[@class='gray-border-top']");
                    foreach (var node in allNodes)
                    {
                        var subDoc = new HtmlDocument();
                        subDoc.LoadHtml(node.InnerHtml);

                        var entry = new DataModel();
                        var nameNode = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[1]");
                        if (nameNode != null)
                            entry.Name =HttpUtility.HtmlDecode( nameNode.InnerText.Replace("\n", ""));

                        var x1 = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[2]");
                        if (x1 != null)
                            entry.Address = x1.InnerText.Replace("\n", "");

                        var nameNx2ode = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[3]");
                        if (nameNx2ode != null)
                            entry.Area = nameNx2ode.InnerText.Replace("\n", "");

                        entries.Add(entry);
                    }

                    if (pages > 1)
                        for (int i = 2; i <= pages; i++)
                        {
                            
                            string newUrl = $"{url}/page/{i}";
                            doc = web.Load(newUrl);
                            Console.WriteLine($"Extracting data from Page: {i}");
                            allNodes = doc.DocumentNode.SelectNodes("//div[@class='gray-border-top']");
                            foreach (var node in allNodes)
                            {
                                var subDoc = new HtmlDocument();
                                subDoc.LoadHtml(node.InnerHtml);

                                var entry = new DataModel();
                                var nameNode = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[1]");
                                if (nameNode != null)
                                    entry.Name = HttpUtility.HtmlDecode(nameNode.InnerText.Replace("\n", ""));

                                var x1 = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[2]");
                                if (x1 != null)
                                    entry.Address = x1.InnerText.Replace("\n", "");

                                var nameNx2ode = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[3]");
                                if (nameNx2ode != null)
                                    entry.Area = nameNx2ode.InnerText.Replace("\n", "");

                                entries.Add(entry);
                            }
                        }

                }
            }
            else
                Console.WriteLine("No alphabet url found");

            using (var writer = new StreamWriter($"{state}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(entries);
            }

            Console.WriteLine("Completed");
        }
    }
}
