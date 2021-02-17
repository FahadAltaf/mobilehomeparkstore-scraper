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
        public string StateName { get; set; }
        public string Name { get; set; }
        public string FullAddress { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Area { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<DataModel> allentries = new List<DataModel>();
            var lines = File.ReadAllLines("run.csv");
            foreach (var line in lines)
            {
                List<DataModel> entries = new List<DataModel>();
                var parts = line.Split(',');
                if (parts.Count() == 2)
                {
                    HtmlWeb web = new HtmlWeb();
                    var startingUrl = parts[1];
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
                            var container = doc.DocumentNode.SelectSingleNode("//*[@id=\"main\"]/div[1]");
                            if (container != null)
                            {
                                string city = "";
                                foreach (var child in container.ChildNodes.Where(x=>x.Name!= "#text"))
                                {
                                    var nodeClass = child.Attributes.FirstOrDefault(x=>x.Name=="class");
                                    if (nodeClass != null)
                                    {
                                        switch (nodeClass.Value)
                                        {
                                            case "mt-5":
                                                city = child.InnerText.Replace("\n\n","").Split('\n')[0];
                                                break;
                                            case "gray-border-top":
                                                var subDoc = new HtmlDocument();
                                                subDoc.LoadHtml(child.InnerHtml);

                                                var entry = new DataModel() { Url = url, StateName = parts[0] };
                                                var nameNode = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[1]");
                                                if (nameNode != null)
                                                    entry.Name = HttpUtility.HtmlDecode(nameNode.InnerText.Replace("\n", ""));

                                                var x1 = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[2]");
                                                if (x1 != null)
                                                    entry.FullAddress = x1.InnerText.Replace("\n", "");

                                                var nameNx2ode = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[3]");
                                                if (nameNx2ode != null)
                                                    entry.Area = nameNx2ode.InnerText.Replace("\n", "");

                                                SplitAddress(entry);
                                                entry.City = city;
                                                entries.Add(entry);
                                                break;
                                            default:
                                                break;
                                        }
                                    }

                                }
                            }
              

                            if (pages > 1)
                                for (int i = 2; i <= pages; i++)
                                {

                                    string newUrl = $"{url}/page/{i}";
                                    doc = web.Load(newUrl);
                                    Console.WriteLine($"Extracting data from Page: {i}");
                                     container = doc.DocumentNode.SelectSingleNode("//*[@id=\"main\"]/div[1]");
                                    if (container != null)
                                    {
                                        string city = "";
                                        foreach (var child in container.ChildNodes.Where(x => x.Name != "#text"))
                                        {
                                            var nodeClass = child.Attributes.FirstOrDefault(x => x.Name == "class");
                                            if (nodeClass != null)
                                            {
                                                switch (nodeClass.Value)
                                                {
                                                    case "mt-5":
                                                        city = child.InnerText.Replace("\n\n", "").Split('\n')[0];
                                                        break;
                                                    case "gray-border-top":
                                                        var subDoc = new HtmlDocument();
                                                        subDoc.LoadHtml(child.InnerHtml);

                                                        var entry = new DataModel() { Url = url, StateName = parts[0] };
                                                        var nameNode = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[1]");
                                                        if (nameNode != null)
                                                            entry.Name = HttpUtility.HtmlDecode(nameNode.InnerText.Replace("\n", ""));

                                                        var x1 = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[2]");
                                                        if (x1 != null)
                                                            entry.FullAddress = x1.InnerText.Replace("\n", "");

                                                        var nameNx2ode = subDoc.DocumentNode.SelectSingleNode("/div[1]/div[3]");
                                                        if (nameNx2ode != null)
                                                            entry.Area = nameNx2ode.InnerText.Replace("\n", "");

                                                        SplitAddress(entry);
                                                        entry.City = city;
                                                        entries.Add(entry);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }

                                        }
                                    }
                                }

                        }
                    }
                    else
                        Console.WriteLine("No alphabet url found");

                    using (var writer = new StreamWriter($"{parts[0]}.csv"))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(entries);
                    }

                    allentries.AddRange(entries);
                }
            }

            using (var writer = new StreamWriter($"Combined.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(allentries);
            }



            Console.WriteLine("Completed");
        }

        private static void SplitAddress(DataModel entry)
        {
            if (!string.IsNullOrEmpty(entry.FullAddress))
            {
                var parts = entry.FullAddress.Split(',');

                switch (parts.Count())
                {
                    case 5:
                        entry.Address = parts[0];
                        entry.City = parts[1];
                        entry.State = parts[3];
                        entry.Zip = parts[4];
                        break;
                    case 4:
                        entry.Address = parts[0];
                        entry.City = parts[1];
                        entry.State = parts[2];
                        entry.Zip = parts[3];
                        break;
                    case 3:
                        if (parts[1].Length == 2)
                        {
                            entry.City = parts[0];
                            entry.State = parts[1];
                            entry.Zip = parts[2];
                        }
                        else
                        {
                            entry.Address = parts[0];
                            entry.State = parts[1];
                            entry.Zip = parts[2];
                        }
                        break;
                    case 2:
                        if (parts[1].Trim().Length == 2)
                        {
                            entry.City = parts[0];
                            entry.State = parts[1];
                        }
                        else
                        {
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
