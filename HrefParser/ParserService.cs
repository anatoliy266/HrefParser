using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace HrefParser
{
    internal static class ParserService
    {
        private static HttpClient client = new HttpClient();
        public static async Task<List<HrefDataModel>> Parse(Uri url, CancellationToken token)
        {
            var hrefsData = new List<HrefDataModel>();
            var page = await client.GetStringAsync(url, token);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(page);
            foreach (var node in htmlDoc.DocumentNode.SelectNodes("//a[@href]"))
            {
                var href = node.GetAttributeValue("href", "").Trim();
                try
                {
                    var uri = new Uri(url, href);
                    if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                    {
                        //var uri = new Uri(node.Attributes["href"].Value);
                        hrefsData.Add(new HrefDataModel
                        {
                            Href = uri,
                            SiteName = "",
                            Status = Status.Pending,
                        });
                    }
                }
                catch
                {
                    continue;
                }
            }

            return hrefsData;

        }

        public static async Task<string> ParseTitle(Uri? url, CancellationToken cts)
        {
            var hrefsData = new List<HrefDataModel>();
            var page = await client.GetStringAsync(url, cts);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(page);
            return htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText ?? "";
        }
    }
}
