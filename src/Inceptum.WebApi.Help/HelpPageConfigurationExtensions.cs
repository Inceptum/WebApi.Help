using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.SelfHost;
using Inceptum.WebApi.Help.ModelDescriptions;
using Inceptum.WebApi.Help.SampleGeneration;

namespace Inceptum.WebApi.Help
{
    public static class HelpPageConfigurationExtensions
    {
        /// <summary>
        /// Plugs help pages infrastructure into given <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration" />.</param>
        /// <param name="setup">Optional delegate, used to futher configure help pages infastructure.</param>
        public static HttpConfiguration UseHelpPage(this HttpConfiguration configuration, Action<HelpPageConfiguration> setup = null)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            configuration.EnsureInitialized();

            var helpPageConfiguration = new HelpPageConfiguration(configuration);

            if (setup != null)
            {
                setup(helpPageConfiguration);
            }

            helpPageConfiguration.Configure();

            return configuration;
        }

        /// <summary>
        /// Sets the objects that will be used by the formatters to produce sample requests/responses.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration" />.</param>
        /// <param name="sampleObjects">The sample objects.</param>
        public static void SetSampleObjects(this HttpConfiguration config, IDictionary<Type, object> sampleObjects)
        {
            config.GetSamplesGenerator().SampleObjects = sampleObjects;
        }

        /// <summary>
        ///  Sets the sample request directly for the specified media type and action.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration" />.</param>
        /// <param name="sample">The sample request.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        public static void SetSampleRequest(this HttpConfiguration config, object sample, MediaTypeHeaderValue mediaType, string controllerName, string actionName)
        {
            config.GetSamplesGenerator()
                .ActionSamples.Add(new HelpPageSampleKey(mediaType, SampleDirection.Request, controllerName, actionName, new[] { "*" }), sample);
        }

        /// <summary>
        /// Sets the sample request directly for the specified media type and action with parameters.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration" />.</param>
        /// <param name="sample">The sample request.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameterNames">The parameter names.</param>
        public static void SetSampleRequest(this HttpConfiguration config, object sample, MediaTypeHeaderValue mediaType, string controllerName, string actionName, params string[] parameterNames)
        {
            config.GetSamplesGenerator()
                  .ActionSamples.Add(new HelpPageSampleKey(mediaType, SampleDirection.Request, controllerName, actionName, parameterNames), sample);
        }

        /// <summary>
        /// Sets the sample request directly for the specified media type of the action.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration" />.</param>
        /// <param name="sample">The sample response.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        public static void SetSampleResponse(this HttpConfiguration config, object sample, MediaTypeHeaderValue mediaType, string controllerName, string actionName)
        {
            config.GetSamplesGenerator()
                  .ActionSamples.Add(new HelpPageSampleKey(mediaType, SampleDirection.Response, controllerName, actionName, new[] { "*" }), sample);
        }

        /// <summary>
        /// Sets the sample response directly for the specified media type of the action with specific parameters.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration" />.</param>
        /// <param name="sample">The sample response.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameterNames">The parameter names.</param>
        public static void SetSampleResponse(this HttpConfiguration config, object sample, MediaTypeHeaderValue mediaType, string controllerName, string actionName, params string[] parameterNames)
        {
            config.GetSamplesGenerator()
                  .ActionSamples.Add(new HelpPageSampleKey(mediaType, SampleDirection.Response, controllerName, actionName, parameterNames), sample);
        }

        /// <summary>
        /// Sets the sample directly for all actions with the specified media type.
        /// </summary>        
        /// <param name="config">The <see cref="HttpConfiguration" />.</param>        
        /// <param name="sample">The sample.</param>
        /// <param name="mediaType">The media type.</param>
        public static void SetSampleForMediaType(this HttpConfiguration config, object sample, MediaTypeHeaderValue mediaType)
        {
            config.GetSamplesGenerator().ActionSamples.Add(new HelpPageSampleKey(mediaType), sample);
        }

        /// <summary>
        /// Sets the sample directly for all actions with the specified type and media type.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration" />.</param>
        /// <param name="sample">The sample.</param>
        /// <param name="mediaType">The media type.</param>
        /// <param name="type">The parameter type or return type of an action.</param>
        public static void SetSampleForType(this HttpConfiguration config, object sample, MediaTypeHeaderValue mediaType, Type type)
        {
            config.GetSamplesGenerator().ActionSamples.Add(new HelpPageSampleKey(mediaType, type), sample);
        }

        /// <summary>
        /// Specifies the actual type of <see cref="System.Net.Http.ObjectContent{T}" /> passed to the <see cref="System.Net.Http.HttpRequestMessage" /> in an action.
        /// The help page will use this information to produce more accurate request samples.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration" />.</param>
        /// <param name="type">The type.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        public static void SetActualRequestType(this HttpConfiguration config, Type type, string controllerName, string actionName)
        {
            config.GetSamplesGenerator().ActualHttpMessageTypes.Add(new HelpPageSampleKey(SampleDirection.Request, controllerName, actionName, new[] { "*" }), type);
        }

        /// <summary>
        /// Specifies the actual type of <see cref="System.Net.Http.ObjectContent{T}" /> passed to the <see cref="System.Net.Http.HttpRequestMessage" /> in an action.
        /// The help page will use this information to produce more accurate request samples.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration" />.</param>
        /// <param name="type">The type.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameterNames">The parameter names.</param>
        public static void SetActualRequestType(this HttpConfiguration config, Type type, string controllerName, string actionName, params string[] parameterNames)
        {
            config.GetSamplesGenerator().ActualHttpMessageTypes.Add(new HelpPageSampleKey(SampleDirection.Request, controllerName, actionName, parameterNames), type);
        }

        /// <summary>
        /// Specifies the actual type of <see cref="System.Net.Http.ObjectContent{T}" /> returned as part of the <see cref="System.Net.Http.HttpRequestMessage" /> in an action.
        /// The help page will use this information to produce more accurate response samples.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration" />.</param>
        /// <param name="type">The type.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        public static void SetActualResponseType(this HttpConfiguration config, Type type, string controllerName, string actionName)
        {
            config.GetSamplesGenerator().ActualHttpMessageTypes.Add(new HelpPageSampleKey(SampleDirection.Response, controllerName, actionName, new[] { "*" }), type);
        }

        /// <summary>
        /// Specifies the actual type of <see cref="System.Net.Http.ObjectContent{T}" /> returned as part of the <see cref="System.Net.Http.HttpRequestMessage" /> in an action.
        /// The help page will use this information to produce more accurate response samples.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration" />.</param>
        /// <param name="type">The type.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameterNames">The parameter names.</param>
        public static void SetActualResponseType(this HttpConfiguration config, Type type, string controllerName, string actionName, params string[] parameterNames)
        {
            config.GetSamplesGenerator().ActualHttpMessageTypes.Add(new HelpPageSampleKey(SampleDirection.Response, controllerName, actionName, parameterNames), type);
        }

        internal static Uri GetBaseAddress(this HttpConfiguration config)
        {
            var sfConfig = config as HttpSelfHostConfiguration;
            return sfConfig != null ? sfConfig.BaseAddress : null;
        }

        internal static HelpPageSampleGenerator GetSamplesGenerator(this HttpConfiguration config)
        {
            return (HelpPageSampleGenerator)config.Properties.GetOrAdd(typeof(HelpPageSampleGenerator), k => new HelpPageSampleGenerator());
        }           
    
        internal static ModelDescriptionGenerator GetModelDescriptionGenerator(this HttpConfiguration config)
        {
            // NOTE[tv]: ModelDescriptionGenerator caches generated models (and their documentation which is culture-specific),
            //           so we'll have it's own generator for every culture.
            var key = Tuple.Create(typeof(ModelDescriptionGenerator), Thread.CurrentThread.CurrentUICulture);
            return (ModelDescriptionGenerator) config.Properties.GetOrAdd(key, _ => initializeModelDescriptionGenerator(config));
        }

        private static ModelDescriptionGenerator initializeModelDescriptionGenerator(HttpConfiguration config)
        {
            var generator = new ModelDescriptionGenerator(config);
            foreach (ApiDescription api in config.Services.GetApiExplorer().ApiDescriptions)
            {
                ApiParameterDescription parameterDescription;
                Type parameterType;
                if (tryGetResourceParameter(api, config, out parameterDescription, out parameterType))
                {
                    generator.GetOrCreateModelDescription(parameterType);
                }
            }
            return generator;
        }

        private static bool tryGetResourceParameter(ApiDescription apiDescription, HttpConfiguration config, out ApiParameterDescription parameterDescription, out Type resourceType)
        {
            parameterDescription = apiDescription.ParameterDescriptions.FirstOrDefault(
                p => p.Source == ApiParameterSource.FromBody ||
                    (p.ParameterDescriptor != null && p.ParameterDescriptor.ParameterType == typeof(HttpRequestMessage)));

            if (parameterDescription == null)
            {
                resourceType = null;
                return false;
            }

            resourceType = parameterDescription.ParameterDescriptor.ParameterType;

            if (resourceType == typeof(HttpRequestMessage))
            {
                resourceType = config.GetSamplesGenerator().ResolveHttpRequestMessageType(apiDescription);
            }

            if (resourceType == null)
            {
                parameterDescription = null;
                return false;
            }

            return true;
        }
    }
}