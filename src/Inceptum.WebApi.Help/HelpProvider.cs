using System;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json.Linq;
using Inceptum.WebApi.Help.Builders;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// Default implementation of the <see cref="T:Inceptum.WebApi.Help.IHelpProvider"/> interface.
    /// </summary>
    public class HelpProvider : IHelpProvider
    {
        private readonly List<Tuple<IHelpBuilder, int>> m_Builders = new List<Tuple<IHelpBuilder, int>>();

        public HelpPageModel GetHelp()
        {
            // Sort builders by rank: higher ranked builders will produce results later and therefore can override low-ranked results.
            var helpItems = m_Builders.OrderBy(x => x.Item2).SelectMany(x => x.Item1.BuildHelp());
           
            var model = buildHelpPage(helpItems);

            return model;
        }

        private static HelpPageModel buildHelpPage(IEnumerable<HelpItem> helpItems)
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

        public sealed class TreeNode
        {
            public string Id;
            public HelpItem Data;
            public IList<TreeNode> Children = new List<TreeNode>();

            public void AddByPath(string[] path, HelpItem data)
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



        public void RegisterBuilder(IHelpBuilder builder, int rank = 0)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            if (m_Builders.Any(x => x.GetType() == builder.GetType()))
            {
                throw new InvalidOperationException(string.Format("Builder of type {0} is already registered.", builder.GetType()));
            }

            m_Builders.Add(Tuple.Create(builder, rank));
        }

        public void UnregisterBuilder(Type concreteType)
        {
            if (concreteType == null) throw new ArgumentNullException("concreteType");

            var builder = findBuilder(concreteType);
            if (builder != null)
            {
                m_Builders.Remove(builder);
            }
        }

        public T FindBuilder<T>() where T : IHelpBuilder
        {
            var tuple = findBuilder(typeof(T));

            return tuple != null ? (T)tuple.Item1 : default(T);
        }

        private Tuple<IHelpBuilder, int> findBuilder(Type actualType)
        {
            if (actualType == null) throw new ArgumentNullException("actualType");

            return m_Builders.FirstOrDefault(x => x.Item1.GetType() == actualType);
        }
    }

    public interface IPdfTemplateProvider
    {
        Paragraph GetTemplate(string name, object data, BaseFont defaultFont);
    }
}