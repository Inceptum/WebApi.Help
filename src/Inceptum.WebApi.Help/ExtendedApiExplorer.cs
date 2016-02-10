using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using Inceptum.WebApi.Help.Description;
using Inceptum.WebApi.Help.ModelDescriptions;
using Inceptum.WebApi.Help.SampleGeneration;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// Default implementation of the <see cref="T:Inceptum.WebApi.Help.IExtendedApiExplorer"/> interface.
    /// </summary>
    internal class ExtendedApiExplorer : IExtendedApiExplorer
    {
        private readonly HttpConfiguration m_HttpConfiguration;
        private readonly IApiExplorer m_InnerExplorer;
        private Lazy<IExtendedDocumentationProvider> m_DocumentationProvider;
        private Lazy<HelpPageSampleGenerator> m_SamplesGenerator;

        public ExtendedApiExplorer(HttpConfiguration httpConfiguration)
        {
            if (httpConfiguration == null) throw new ArgumentNullException("httpConfiguration");
            m_HttpConfiguration = httpConfiguration;
            m_InnerExplorer = new CultureAwareApiExplorer(httpConfiguration);
            initializeDependencies();
        }

        /// <summary>
        /// When set to <c>true</c> will ignore query string parameters that appears from route template and doesn't map to any of action parameters.
        /// <code>
        /// // Consider the route configured as: 
        /// config.Routes.MapHttpRoute("TestRoute", "api/{version}/users/{id}", new { controller = "Users", action = "Get" }, new { version = "v[12]" /* Version constraint */ });
        /// 
        /// // And corresponding action is:
        /// public User Get(int id /* No version parameter here! */) {
        ///     // ...
        /// }
        /// // Then the {version} parameter will not appear in the documentation.
        /// </code>
        /// </summary>
        public bool IgnoreUndeclaredRouteParameters { get; set; }

        private void initializeDependencies()
        {
            m_DocumentationProvider = new Lazy<IExtendedDocumentationProvider>(() =>
            {
                var docProvider = m_HttpConfiguration.Services.GetDocumentationProvider() as IExtendedDocumentationProvider;
                if (docProvider == null)
                {
                    throw new ConfigurationErrorsException(string.Format("The type {0} requires {1} to be registered in http configuration.", typeof(ExtendedApiExplorer).Name, typeof(IExtendedDocumentationProvider).Name));
                }
                return docProvider;
            });
            m_SamplesGenerator = new Lazy<HelpPageSampleGenerator>(() => m_HttpConfiguration.GetSamplesGenerator());
        }

        protected IExtendedDocumentationProvider DocumentationProvider
        {
            get { return m_DocumentationProvider.Value; }
        }

        protected HelpPageSampleGenerator SamplesGenerator
        {
            get { return m_SamplesGenerator.Value; }
        }

        protected ModelDescriptionGenerator ModelDescriptionGenerator
        {
            get { return m_HttpConfiguration.GetModelDescriptionGenerator(); }
        }

        public Collection<ApiDescription> ApiDescriptions
        {
            get
            {
                var descriptions = m_InnerExplorer.ApiDescriptions
                    .Select((ad, i) => new { Index = i, ApiDescription = ad })
                    .GroupBy(ad => ad.ApiDescription.ActionDescriptor.ControllerDescriptor)
                    .OrderBy(g => getOrderStr(g.Key.GetCustomAttributes<ApiExplorerOrderAttribute>().FirstOrDefault(), g.First().Index))
                    .SelectMany(g => g.OrderBy(ad => getOrderStr(ad.ApiDescription.ActionDescriptor.GetCustomAttributes<ApiExplorerOrderAttribute>().FirstOrDefault(), ad.Index)))
                    .Select(g => g.ApiDescription)
                    .ToList();

                return new Collection<ApiDescription>(descriptions);
            }
        }

        public ICollection<ExtendedApiDescription> ExtendedApiDescriptions
        {
            get { return ApiDescriptions.Select(ExtendDescription).ToArray(); }
        }

        protected virtual ExtendedApiDescription ExtendDescription(ApiDescription apiDescription)
        {
            if (apiDescription == null) throw new ArgumentNullException("apiDescription");

            var apiDescriptionEx = new ExtendedApiDescription(apiDescription)
                {
                    Controller = DocumentationProvider.GetName(apiDescription.ActionDescriptor.ControllerDescriptor) ?? apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName,
                    DisplayName = DocumentationProvider.GetName(apiDescription.ActionDescriptor) ?? string.Format("{0} {1}", apiDescription.HttpMethod.Method, apiDescription.RelativePath)
                };

            GenerateUriParameters(apiDescriptionEx);
            GenerateRequestDescription(apiDescriptionEx);
            GenerateResponseDescription(apiDescriptionEx);
            GenerateSamples(apiDescriptionEx);

            return apiDescriptionEx;
        }

        protected virtual void GenerateUriParameters(ExtendedApiDescription apiDescription)
        {
            foreach (ApiParameterDescription apiParameter in apiDescription.InnerDescription.ParameterDescriptions.Where(x => x.Source == ApiParameterSource.FromUri))
            {
                HttpParameterDescriptor parameterDescriptor = apiParameter.ParameterDescriptor;
                ModelDescription typeDescription = null;
                ComplexTypeModelDescription complexTypeDescription = null;
                if (parameterDescriptor != null)
                {
                    Type parameterType = parameterDescriptor.ParameterType;
                    typeDescription = ModelDescriptionGenerator.GetOrCreateModelDescription(parameterType);
                    complexTypeDescription = typeDescription as ComplexTypeModelDescription;
                }

                if (complexTypeDescription != null)
                {
                    foreach (ParameterDescription uriParameter in complexTypeDescription.Properties)
                    {
                        apiDescription.UriParameters.Add(uriParameter);
                    }
                }
                else if (parameterDescriptor != null)
                {
                    ParameterDescription uriParameter = AddParameterDescription(apiDescription, apiParameter, typeDescription);

                    if (!parameterDescriptor.IsOptional)
                    {
                        uriParameter.Annotations.Add(new ParameterAnnotation { Documentation = Strings.Required });
                    }

                    object defaultValue = parameterDescriptor.DefaultValue;
                    if (defaultValue != null)
                    {
                        uriParameter.Annotations.Add(new ParameterAnnotation
                            {
                                Documentation = string.Format(Strings.DefaultValue, Convert.ToString(defaultValue, CultureInfo.InvariantCulture))
                            });
                    }
                }
                else
                {
                    Debug.Assert(parameterDescriptor == null);
                    // If parameterDescriptor is null, this is an undeclared route parameter which only occurs
                    // when source is FromUri. Ignored in request model and among resource parameters but listed
                    // as a simple string here.
                    if (!IgnoreUndeclaredRouteParameters)
                    {
                        ModelDescription modelDescription = ModelDescriptionGenerator.GetOrCreateModelDescription(typeof(string));
                        AddParameterDescription(apiDescription, apiParameter, modelDescription);
                    }
                }

            }
        }

        protected virtual ParameterDescription AddParameterDescription(ExtendedApiDescription apiModel, ApiParameterDescription apiParameter, ModelDescription typeDescription)
        {
            if (apiModel == null) throw new ArgumentNullException("apiModel");
            if (apiParameter == null) throw new ArgumentNullException("apiParameter");
            if (typeDescription == null) throw new ArgumentNullException("typeDescription");

            var paramDesc = new ParameterDescription
                {
                    Name = apiParameter.Name,
                    Documentation = apiParameter.Documentation,
                    TypeDescription = typeDescription,
                };
            apiModel.UriParameters.Add(paramDesc);
            return paramDesc;
        }

        protected virtual void GenerateRequestDescription(ExtendedApiDescription apiDescription)
        {
            if (apiDescription == null) throw new ArgumentNullException("apiDescription");

            foreach (ApiParameterDescription apiParameter in apiDescription.InnerDescription.ParameterDescriptions)
            {
                if (apiParameter.Source == ApiParameterSource.FromBody)
                {
                    Type parameterType = apiParameter.ParameterDescriptor.ParameterType;
                    apiDescription.RequestModelDescription = ModelDescriptionGenerator.GetOrCreateModelDescription(parameterType);
                    apiDescription.RequestDocumentation = apiParameter.Documentation;
                }
                else if (apiParameter.ParameterDescriptor != null && apiParameter.ParameterDescriptor.ParameterType == typeof(HttpRequestMessage))
                {
                    Type parameterType = SamplesGenerator.ResolveHttpRequestMessageType(apiDescription.InnerDescription);
                    if (parameterType != null)
                    {
                        apiDescription.RequestModelDescription = ModelDescriptionGenerator.GetOrCreateModelDescription(parameterType);
                        apiDescription.RequestDocumentation = apiParameter.Documentation;
                    }
                }
            }
        }

        protected virtual void GenerateResponseDescription(ExtendedApiDescription apiDescription)
        {
            if (apiDescription == null) throw new ArgumentNullException("apiDescription");

            ResponseDescription response = apiDescription.InnerDescription.ResponseDescription;
            Type responseType = response.ResponseType ?? response.DeclaredType;
            if (responseType != null && responseType != typeof(void))
            {
                apiDescription.ResponseModelDescription = ModelDescriptionGenerator.GetOrCreateModelDescription(responseType);
            }
        }

        protected virtual void GenerateSamples(ExtendedApiDescription apiDescription)
        {
            if (apiDescription == null) throw new ArgumentNullException("apiDescription");

            foreach (var item in SamplesGenerator.GetSampleRequests(apiDescription.InnerDescription))
            {
                apiDescription.SampleRequests.Add(item.Key, item.Value);
            }

            foreach (var item in SamplesGenerator.GetSampleResponses(apiDescription.InnerDescription))
            {
                apiDescription.SampleResponses.Add(item.Key, item.Value);
            }
        }

        static Int64 getOrderStr(ApiExplorerOrderAttribute orderAttribute, Int64 originalOrder)
        {
            return orderAttribute == null ? (Int32.MaxValue + originalOrder + 1) : orderAttribute.Order;
        }
    }
}