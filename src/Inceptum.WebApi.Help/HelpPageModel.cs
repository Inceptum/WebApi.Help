﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Inceptum.WebApi.Help.Extensions;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// Describes a structure of the help page.
    /// </summary>
    public class HelpPageModel
    {
        public HelpPageModel()
        {
            Items = new List<HelpItem>();            
        }

        public List<HelpItem> Items { get; private set; }
        public TableOfContentItem TableOfContent { get; set; }
    }

    /// <summary>
    /// Represents an article on the help page.
    /// </summary>
    public class HelpItem
    {
        private string m_Title;

        public HelpItem(string tableOfContentPath)
        {
            if(string.IsNullOrWhiteSpace(tableOfContentPath))
                throw new ArgumentException(@"tableOfContentPath should be not whitespace string", "tableOfContentPath");
            TableOfContentPath = tableOfContentPath.Split('/').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            //TableOfContentId = TableOfContentPath.Last();
            TableOfContentId = string.Join("-", TableOfContentPath.Select(x => x.ToSlug()));
        }

        /// <summary>
        /// An identifier used to reference this help item in table of content tree.
        /// </summary>
        public string TableOfContentId { get; private set; }

        /// <summary>
        /// Text to display in for this help item in table of content tree.
        /// </summary>
        public string Title
        {
            get { return m_Title ?? TableOfContentId; }
            set { m_Title = value; }
        }

        /// <summary>
        /// Item's link path in table of content tree, e.g. /api/help/my-item
        /// </summary>
        [JsonIgnore]
        public string[] TableOfContentPath { get; set; }

        /// <summary>
        /// Data, associated with the help item.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// A template, used to render the data.
        /// </summary>
        public string Template { get; set; }

        public int Level { get; internal set; }
    }

    /// <summary>
    /// Represents an item in the table of contents on the help page.
    /// </summary>
    public class TableOfContentItem
    {
        public TableOfContentItem()
        {
            Children = new List<TableOfContentItem>();
        }

        public string Text { get; set; }
        public string ReferenceId { get; set; }
        public List<TableOfContentItem> Children { get; set; }
    }
}