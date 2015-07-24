using System;
using System.Collections.Generic;
using System.Linq;
using Inceptum.WebApi.Help.Builders;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// Default implementation of the <see cref="T:Inceptum.WebApi.Help.IHelpProvider"/> interface.
    /// </summary>
    public class HelpProvider : IHelpProvider
    {
        private readonly List<IHelpBuilder> m_Builders = new List<IHelpBuilder>();

        public HelpPageModel GetHelp()
        {
            var helpItems = m_Builders.SelectMany(x => x.BuildHelp());

            var model = BuildHelpPage(SortItems(helpItems));

            return model;
        }

        /// <summary>
        /// This method gives an appotunity to sort help items before creating a navigation tree.
        /// Sorting means that items at the same level of the navigation tree will be placed in order, defined herein.
        /// </summary>        
        protected virtual IEnumerable<HelpItem> SortItems(IEnumerable<HelpItem> helpItems)
        {
            if (helpItems == null) throw new ArgumentNullException("helpItems");
            return helpItems;
        }

        protected virtual HelpPageModel BuildHelpPage(IEnumerable<HelpItem> helpItems)
        {
            if (helpItems == null) throw new ArgumentNullException("helpItems");

            var model = new HelpPageModel();

            // Build help tree
            var helpTree = new TreeNode { Id = "ROOT" };
            foreach (var item in helpItems)
            {
                helpTree.AddByPath(item.TableOfContentPath, item);
            }

            // Build table of content
            model.TableOfContent = processTree(model, helpTree);
            return model;
        }

        private static TableOfContentItem processTree(HelpPageModel helpPage, TreeNode node, int level = 0)
        {
            if (helpPage == null) throw new ArgumentNullException("helpPage");
            if (node == null) throw new ArgumentNullException("node");

            if (node.Data != null)
            {
                node.Data.Level = level;
                helpPage.Items.Add(node.Data);
            }

            var tocItem = new TableOfContentItem()
                {
                    Text = node.Data != null ? node.Data.Title : node.Id,
                    ReferenceId = node.Data != null ? node.Data.TableOfContentId : null
                };

            foreach (var child in node.Children)
            {
                tocItem.Children.Add(processTree(helpPage, child, level + 1));
            }

            return tocItem;
        }

        sealed class TreeNode
        {
            public string Id;
            public HelpItem Data;
            public readonly IList<TreeNode> Children = new List<TreeNode>();

            public void AddByPath(IEnumerable<string> path, HelpItem data)
            {
                if (path == null) throw new ArgumentNullException("path");

                TreeNode current = this;

                foreach (var segment in path)
                {
                    var found = current.Children.FirstOrDefault(x => x.Id == segment);
                    if (found == null)
                    {
                        found = new TreeNode() { Id = segment };
                        current.Children.Add(found);
                    }
                    current = found;
                }
                current.Data = data;
            }
        }

        /// <summary>
        /// Registers a <paramref name="builder"/> for participation in help page building process.
        /// </summary>
        /// <param name="builder">Builder instance</param>        
        public HelpProvider RegisterBuilder(IHelpBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            m_Builders.Add(builder);

            return this;
        }

        /// <summary>
        /// Clears builders chain
        /// </summary>
        /// <returns></returns>
        public HelpProvider ClearBuilders()
        {
            m_Builders.Clear();
            return this;
        }
    }
}