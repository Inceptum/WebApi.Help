using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Inceptum.WebApi.Help.ModelDescriptions;

namespace Inceptum.WebApi.Help.Builders
{
    /// <summary>
    /// Adds API methods documentation to help page.
    /// </summary>
    public class ApiDocumentationBuilder : IHelpBuilder
    {
        private readonly IServiceLocator m_ServiceLocator;
        private readonly Uri m_SamplesBaseUri;
        private readonly string m_TocRoot;
        private const string GROUP_TEMPLATE_NAME = "apiMethodGroup";
        private const string METHOD_TEMPLATE_NAME = "apiMethod";

        public ApiDocumentationBuilder(HelpPageConfiguration configurationConfig, string tocRoot = null)
        {
            m_ServiceLocator = configurationConfig.ServiceLocator;
            m_SamplesBaseUri = configurationConfig.SamplesUriInternal;
            m_TocRoot = tocRoot ?? string.Empty;
        }

        public IEnumerable<HelpItem> BuildHelp()
        {
            return CreateMethodGroupsHelp().Union(CreateMethodsHelp());
        }

        protected virtual IEnumerable<HelpItem> CreateMethodGroupsHelp()
        {
            return from @group in m_ServiceLocator.Get<IExtendedApiExplorer>().ExtendedApiDescriptions.GroupBy(x => x.Controller)
                   let controllerDescriptor = @group.First().ActionDescriptor.ControllerDescriptor
                   select new HelpItem(string.Format("{0}/{1}", m_TocRoot, controllerDescriptor.ControllerName))
                       {
                           Title = m_ServiceLocator.Get<IExtendedDocumentationProvider>().GetName(controllerDescriptor),
                           Data = new
                               {
                                   name = @group.Key,
                                   documentation = m_ServiceLocator.Get<IExtendedDocumentationProvider>().GetDocumentation(controllerDescriptor)
                               },
                           Template = GROUP_TEMPLATE_NAME
                       };
        }

        protected virtual IEnumerable<HelpItem> CreateMethodsHelp()
        {
            var builder = new DtoBuilder();
            return m_ServiceLocator.Get<IExtendedApiExplorer>().ExtendedApiDescriptions.Select(api => new HelpItem(string.Format("{0}/{1}/{2}", m_TocRoot, api.ActionDescriptor.ControllerDescriptor.ControllerName, api.ActionDescriptor.ActionName))
                {
                    Title = api.DisplayName,
                    Template = METHOD_TEMPLATE_NAME,
                    Data = builder.BuildDTO(m_SamplesBaseUri, api)
                });
        }

        #region DTO

        /// <summary>
        /// An API documentation DTO
        /// </summary>
        public class ApiDescriptionDto
        {
            public string DisplayName;

            public string Controller;

            public string HttpMethod;

            public string RelativePath;

            public string FullPath;

            public string Documentation;

            public PayloadDescriptorDto RequestBody;

            public PayloadDescriptorDto ResponseBody;

            public ParameterDescriptionDto[] UriParameters;

            public SampleDto SampleRequest;

            public SampleDto SampleResponse;
        }

        /// <summary>
        /// Sample payload DTO (JSON, XML, etc).
        /// </summary>
        public class SampleDto
        {
            public string MediaType;
            public string Content;
        }

        /// <summary>
        /// Request/response body payload DTO.
        /// </summary>
        public class PayloadDescriptorDto
        {
            public string TypeName;
            public string Documentation;
            public ParameterDescriptionDto[] Properties;
        }

        /// <summary>
        /// DTO for parameter
        /// </summary>
        public class ParameterDescriptionDto
        {
            public string Name;
            public string TypeName;
            public string TypeDocumentation;
            public string Documentation;
            public string[] Annotations;
        }

        /// <summary>
        /// This class builds DTO objects from domain models
        /// </summary>
        public sealed class DtoBuilder
        {
            public ApiDescriptionDto BuildDTO(Uri baseUri, ExtendedApiDescription apiDoc)
            {
                if (apiDoc == null) throw new ArgumentNullException("apiDoc");

                var dto = new ApiDescriptionDto
                {
                    DisplayName = apiDoc.DisplayName,
                    Controller = apiDoc.Controller,
                    HttpMethod = apiDoc.HttpMethod.Method,
                    RelativePath = apiDoc.RelativePath,
                    FullPath = new Uri(baseUri, apiDoc.RelativePath).ToString(),
                    Documentation = apiDoc.Documentation,
                    UriParameters = apiDoc.UriParameters.Select(buildDTO).ToArray(),
                };

                if (apiDoc.RequestModelDescription != null)
                {
                    dto.RequestBody = new PayloadDescriptorDto
                    {
                        Documentation = apiDoc.RequestDocumentation ?? apiDoc.RequestModelDescription.Documentation,
                        TypeName = apiDoc.RequestModelDescription.Name,
                        Properties = apiDoc.RequestModelParameters.Select(buildDTO).ToArray()
                    };
                }

                if (apiDoc.ResponseModelDescription != null)
                {
                    dto.ResponseBody = new PayloadDescriptorDto
                    {
                        Documentation = apiDoc.ResponseDocumentation ?? apiDoc.ResponseModelDescription.Documentation,
                        TypeName = apiDoc.ResponseModelDescription.Name,
                        Properties = apiDoc.ResponseModelParameters.Select(buildDTO).ToArray()
                    };
                }

                // For now we are only interested in JSON samples
                var jsonMediaType = new MediaTypeHeaderValue("application/json");

                object jsonSample;
                apiDoc.SampleRequests.TryGetValue(jsonMediaType, out jsonSample);
                if (jsonSample != null)
                {
                    dto.SampleRequest = new SampleDto { Content = jsonSample.ToString(), MediaType = jsonMediaType.MediaType };
                }

                apiDoc.SampleResponses.TryGetValue(jsonMediaType, out jsonSample);
                if (jsonSample != null)
                {
                    dto.SampleResponse = new SampleDto { Content = jsonSample.ToString(), MediaType = jsonMediaType.MediaType };
                }
                return dto;
            }

            private static ParameterDescriptionDto buildDTO(ParameterDescription description)
            {
                if (description == null) throw new ArgumentNullException("description");

                return new ParameterDescriptionDto
                {
                    Name = description.Name,
                    Documentation = description.Documentation,
                    TypeName = description.TypeDescription.Name,
                    TypeDocumentation = description.TypeDescription.Documentation,
                    Annotations = description.Annotations.Select(a => a.Documentation).ToArray()
                };
            }
        }

        #endregion
    }
}