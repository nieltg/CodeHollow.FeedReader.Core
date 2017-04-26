﻿namespace CodeHollow.FeedReader.Feeds
{
    using System.Collections.Generic;

    /// <summary>
    /// BaseFeed object which contains the basic properties that each feed has.
    /// </summary>
    public abstract class BaseFeed
    {
        /// <summary>
        /// creates the generic <see cref="Feed"/> object.
        /// </summary>
        /// <returns>Feed</returns>
        public abstract Feed ToFeed();

        /// <summary>
        /// The title of the feed
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The link (url) to the feed
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// The items that are in the feed
        /// </summary>
        public ICollection<BaseFeedItem> Items { get; set; }

        /// <summary>
        /// Gets the whole, original feed as string
        /// </summary>
        public string OriginalDocument { get; private set; }

        /// <summary>
        /// default constructor (for serialization)
        /// </summary>
        public BaseFeed()
        {
            this.Items = new List<BaseFeedItem>();
        }

        /// <summary>
        /// Reads a base feed based on the xml given in element
        /// </summary>
        /// <param name="feedXml"></param>
        /// <param name="xelement"></param>
        public BaseFeed(string feedXml, System.Xml.Linq.XElement xelement)
            : this()
        {
            this.OriginalDocument = feedXml;

            this.Title = xelement.GetValue("title");
            this.Link = xelement.GetValue("link");
        }
    }
}
