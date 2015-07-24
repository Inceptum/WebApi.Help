using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Inceptum.WebApi.Help.ModelDescriptions;

namespace Inceptum.WebApi.Help.Builders
{
    /// <summary>
    /// Adds documentation about data types, used in API, to the help page.
    /// </summary>
    internal sealed class TypesDocumentationBuilder : IHelpBuilder
    {
        private readonly ModelDescriptionGenerator m_ModelDescriptionGenerator;
        private readonly string m_TocPath;
        private readonly Type[] m_TypesToDocument;
        private const string TEMPLATE_NAME = "dataType";
        internal const string DEFAULT_TOC_PATH = "APIDataTypes";

        public TypesDocumentationBuilder(ModelDescriptionGenerator modelDescriptionGenerator, IEnumerable<Type> typesToDocument, string tocPath = DEFAULT_TOC_PATH)
        {
            if (modelDescriptionGenerator == null) throw new ArgumentNullException("modelDescriptionGenerator");
            if (typesToDocument == null) throw new ArgumentNullException("typesToDocument");
            if (string.IsNullOrWhiteSpace(tocPath)) throw new ArgumentNullException("tocPath");

            m_ModelDescriptionGenerator = modelDescriptionGenerator;
            m_TocPath = tocPath;
            m_TypesToDocument = typesToDocument.Distinct().ToArray();
        }

        public IEnumerable<HelpItem> BuildHelp()
        {
            return new[] { new HelpItem(m_TocPath) { Title = Strings.DataTypes_Title } }
                .Concat(
                    m_TypesToDocument.Select(x => m_ModelDescriptionGenerator.GetOrCreateModelDescription(x))
                        .Select(x => new HelpItem(string.Format("{0}/{1}", m_TocPath, x.ModelType.Name))
                              {
                                  Title = x.Name,
                                  Template = TEMPLATE_NAME,
                                  Data = x
                              }));
        }
    }
}