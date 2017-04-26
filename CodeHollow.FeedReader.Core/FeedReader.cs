﻿namespace CodeHollow.FeedReader
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Parser;
    using System.Threading.Tasks;

    /// <summary>
    /// The static FeedReader class which allows to read feeds from a given url. Use it to 
    /// parse a feed from an url <see cref="Read(string)"/>, a file <see cref="ReadFromFile(string)"/>  
    /// or a string <see cref="ReadFromString(string)"/>. If the feed url is not known, <see cref="ParseFeedUrlsFromHtml(string)"/> 
    /// returns all feed links on a given page. 
    /// </summary>
    /// <example>
    /// var links = FeedReader.ParseFeedUrlsFromHtml("https://codehollow.com");
    /// var firstLink = links.First();
    /// var feed = FeedReader.Read(firstLink.Url);
    /// Console.WriteLine(feed.Title);
    /// </example>
    public static class FeedReader
    {
        /// <summary>
        /// gets a url (with or without http) and returns the full url
        /// </summary>
        /// <param name="url">url with or without http</param>
        /// <returns>full url</returns>
        /// <example>GetUrl("codehollow.com"); => returns https://codehollow.com</example>
        public static string GetAbsoluteUrl(string url)
        {
            return new UriBuilder(url).ToString();
        }

        /// <summary>
        /// Returns the absolute url of a link on a page. If you got the feed links via
        /// GetFeedUrlsFromUrl(url) and the url is relative, you can use this method to get the full url.
        /// </summary>
        /// <param name="pageUrl">the original url to the page</param>
        /// <param name="feedLink">a referenced feed (link)</param>
        /// <returns>a feed link</returns>
        /// <example>GetAbsoluteFeedUrl("codehollow.com", myRelativeFeedLink);</example>
        public static HtmlFeedLink GetAbsoluteFeedUrl(string pageUrl, HtmlFeedLink feedLink)
        {
            string tmpUrl = feedLink.Url.HtmlDecode();
            pageUrl = GetAbsoluteUrl(pageUrl);

            if (tmpUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || tmpUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return feedLink;

            if (tmpUrl.StartsWith("//", StringComparison.OrdinalIgnoreCase)) // special case
                tmpUrl = "http:" + tmpUrl;

            Uri finalUri;
            if (Uri.TryCreate(tmpUrl, UriKind.RelativeOrAbsolute, out finalUri))
            {
                if (finalUri.IsAbsoluteUri)
                {
                    return new HtmlFeedLink(feedLink.Title.HtmlDecode(), finalUri.ToString(), feedLink.FeedType);
                }
                else if (Uri.TryCreate(pageUrl + '/' + tmpUrl.TrimStart('/'), UriKind.Absolute, out finalUri))
                    return new HtmlFeedLink(feedLink.Title.HtmlDecode(), finalUri.ToString(), feedLink.FeedType);
            }

            throw new Exception($"Could not get the absolute url out of {pageUrl} and {feedLink.Url}");
        }

        /// <summary>
        /// Opens a webpage and reads all feed urls from it (link rel="alternate" type="application/...")
        /// </summary>
        /// <param name="url">the url of the page</param>
        /// <returns>a list of links including the type and title, an empty list if no links are found</returns>
        /// <example>FeedReader.GetFeedUrlsFromUrl("codehollow.com"); // returns a list of all available feeds at 
        /// https://codehollow.com </example>
        public static async Task<IEnumerable<HtmlFeedLink>> GetFeedUrlsFromUrlAsync(string url)
        {
            url = GetAbsoluteUrl(url);
            string pageContent = await Helpers.DownloadAsync(url);
            return ParseFeedUrlsFromHtml(pageContent);
        }

        /// <summary>
        /// Opens a webpage and reads all feed urls from it (link rel="alternate" type="application/...")
        /// </summary>
        /// <param name="url">the url of the page</param>
        /// <returns>a list of links, an empty list if no links are found</returns>
        public static async Task<string[]> ParseFeedUrlsAsStringAsync(string url)
        {
            IEnumerable<HtmlFeedLink> links = await GetFeedUrlsFromUrlAsync(url);

            return links.Select(x => x.Url).ToArray();
        }
        
        /// <summary>
        /// Parses RSS links from html page and returns all links
        /// </summary>
        /// <param name="htmlContent">the content of the html page</param>
        /// <returns>all RSS/feed links</returns>
        public static IEnumerable<HtmlFeedLink> ParseFeedUrlsFromHtml(string htmlContent)
        {
            // sample link:
            // <link rel="alternate" type="application/rss+xml" title="Microsoft Bot Framework Blog" href="http://blog.botframework.com/feed.xml">
            // <link rel="alternate" type="application/atom+xml" title="Aktuelle News von heise online" href="https://www.heise.de/newsticker/heise-atom.xml">
            var htmlDoc = new HtmlAgilityPack.HtmlDocument()
            {
                OptionAutoCloseOnEnd = true,
                OptionFixNestedTags = true
            };

            htmlDoc.LoadHtml(htmlContent);

            if (htmlDoc.DocumentNode != null)
            {
                var nodes = htmlDoc.DocumentNode.SelectNodes("//link").Where(
                    x => x.Attributes["type"] != null &&
                    (x.Attributes["type"].Value.Contains("application/rss") || x.Attributes["type"].Value.Contains("application/atom")));

                foreach (var node in nodes)
                {
                    yield return new HtmlFeedLink()
                    {
                        Title = node.Attributes["title"]?.Value?.HtmlDecode(),
                        Url = node.Attributes["href"]?.Value.HtmlDecode(),
                        FeedType = GetFeedTypeFromLinkType(node.Attributes["type"].Value.HtmlDecode())
                    };
                }
            }
        }

        /// <summary>
        /// reads a feed from an url. the url must be a feed. Use ParseFeedUrlsFromHtml to
        /// parse the feeds from a url which is not a feed.
        /// </summary>
        /// <param name="url">the url to a feed</param>
        /// <returns>parsed feed</returns>
        public static async Task<Feed> ReadAsync(string url)
        {
            string feedContent = await Helpers.DownloadAsync(GetAbsoluteUrl(url));
            return ReadFromString(feedContent);
        }

        /// <summary>
        /// reads a feed from a file
        /// </summary>
        /// <param name="filePath">the path to the feed file</param>
        /// <returns>parsed feed</returns>
        public static Feed ReadFromFile(string filePath)
        {
            string feedContent = System.IO.File.ReadAllText(filePath);
            return ReadFromString(feedContent);
        }

        /// <summary>
        /// reads a feed from the <paramref name="feedContent" />
        /// </summary>
        /// <param name="feedContent">the feed content (xml)</param>
        /// <returns>parsed feed</returns>
        public static Feed ReadFromString(string feedContent)
        {
            var feed = FeedParser.GetFeed(feedContent);
            return feed;
        }
        
        /// <summary>
        /// read the rss feed type from the type statement of an html link
        /// </summary>
        /// <param name="linkType">application/rss+xml or application/atom+xml or ...</param>
        /// <returns>the feed type</returns>
        private static FeedType GetFeedTypeFromLinkType(string linkType)
        {
            if (linkType.Contains("application/rss"))
                return FeedType.Rss;
            
            if (linkType.Contains("application/atom"))
                return FeedType.Atom;

            throw new Exception($"The link type '{linkType}' is not a valid feed link!");
        }
    }
}
