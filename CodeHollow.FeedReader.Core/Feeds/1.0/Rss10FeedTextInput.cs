﻿namespace CodeHollow.FeedReader.Feeds
{
    using System.Xml.Linq;

    /// <summary>
    /// Rss 1.0 Feed textinput according to specification: http://web.resource.org/rss/1.0/spec
    /// </summary>
    public class Rss10FeedTextInput : FeedTextInput
    {
        /// <summary>
        /// The "about" attribute of the element
        /// </summary>
        public string About { get; set; }

        /// <summary>
        /// default constructor (for serialization)
        /// </summary>
        public Rss10FeedTextInput() : base()
        { }

        /// <summary>
        /// Reads a rss 1.0 textInput element based on the xml given in item
        /// </summary>
        /// <param name="element"></param>
        public Rss10FeedTextInput(XElement element)
            : base(element)
        {
            this.About = element.GetAttribute("rdf:about").GetValue();
        }
    }
}
