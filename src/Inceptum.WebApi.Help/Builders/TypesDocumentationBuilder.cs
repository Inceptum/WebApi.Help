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
    public sealed class TypesDocumentationBuilder : IHelpBuilder
    {
        private readonly HttpConfiguration m_HttpConfiguration;
        private readonly string m_TocPath;
        private readonly Type[] m_TypesToDocument;
        private const string TEMPLATE_NAME = "dataType";

        public TypesDocumentationBuilder(HttpConfiguration httpConfiguration, IEnumerable<Type> typesToDocument, string tocPath = null)
        {
            if (httpConfiguration == null) throw new ArgumentNullException("httpConfiguration");
            if (typesToDocument == null) throw new ArgumentNullException("typesToDocument");

            m_HttpConfiguration = httpConfiguration;
            m_TocPath = tocPath ?? "APIDataTypes";
            m_TypesToDocument = typesToDocument.Distinct().ToArray();
        }

        private ModelDescriptionGenerator ModelDescriptionGenerator
        {
            get { return m_HttpConfiguration.GetModelDescriptionGenerator(); }
        }

        public IEnumerable<HelpItem> BuildHelp()
        {
            return new[] { new HelpItem(m_TocPath) { Title = Strings.DataTypes_Title } }
                .Concat(
                    m_TypesToDocument.Select(x => ModelDescriptionGenerator.GetOrCreateModelDescription(x))
                        .Select(x => new HelpItem(string.Format("{0}/{1}", m_TocPath, x.ModelType.Name))
                              {
                                  Title = x.Name,
                                  Template = TEMPLATE_NAME,
                                  Data = x
                              }));
        }
    }
}