using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using Inceptum.WebApi.Help.Extensions;
using Inceptum.WebApi.Help.ModelDescriptions;

namespace Inceptum.WebApi.Help
{
    /// <summary>
    /// Extends an API definition with extra api related info (request/response samples and some, display name, category, etc).
    /// </summary>
    public class ExtendedApiDescription
    {        
        public ExtendedApiDescription(ApiDescription inner)
        {
            if (inner == null) throw new ArgumentNullException("inner");

            InnerDescription = inner;
            ResponseDocumentation = inner.ResponseDescription.Documentation;
            UriParameters = new Collection<ParameterDescription>();
            SampleRequests = new Dictionary<MediaTypeHeaderValue, object>();
            SampleResponses = new Dictionary<MediaTypeHeaderValue, object>();
        }

        public ApiDescription InnerDescription { get; set; }

        /// <summary>
        ///  Gets or sets the HTTP method for the API.
        /// </summary>
        public HttpMethod HttpMethod
        {
            get { return InnerDescription.HttpMethod; }
            set { InnerDescription.HttpMethod = value; }
        }

        /// <summary>
        /// Gets or sets the relative path for the API.
        /// </summary>
        public string RelativePath
        {
            get { return InnerDescription.RelativePath; }
            set { InnerDescription.RelativePath = value; }
        }

        /// <summary>
        /// Gets or sets the base path for the API.
        /// </summary>
        public string BaseAddress { get; set; }        

        /// <summary>
        /// Gets or sets the action descriptor that will handle the API.
        /// </summary>       
        public HttpActionDescriptor ActionDescriptor
        {
            get { return InnerDescription.ActionDescriptor; }
            set { InnerDescription.ActionDescriptor = value; }
        }

        /// <summary>
        /// Gets or sets the documentation of the API.
        /// </summary>
        public string Documentation
        {
            get { return InnerDescription.Documentation; }
            set { InnerDescription.Documentation = value; }
        }        

        /// <summary>
        /// Gets or sets a display name for the API.        
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// A controller's name, this API belongs to.
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Gets the <see cref="ModelDescriptions.ParameterDescription" /> collection that describes the URI parameters for the API.
        /// </summary>
        public Collection<ParameterDescription> UriParameters { get; private set; }

        /// <summary>
        /// Gets or sets the documentation for the request.
        /// </summary>
        public string RequestDocumentation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ModelDescription" /> that describes the request body.
        /// </summary>
        public ModelDescription RequestModelDescription { get; set; }

        /// <summary>
        /// Gets the request body parameter descriptions.
        /// </summary>
        public IList<ParameterDescription> RequestModelParameters
        {
            get { return RequestModelDescription.GetParameterDescriptions(); }
        }

        /// <summary>
        /// Gets or sets the documentation for the response.
        /// </summary>
        public string ResponseDocumentation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ModelDescription" /> that describes the response.
        /// </summary>
        public ModelDescription ResponseModelDescription { get; set; }

        /// <summary>
        /// Gets the response parameters descriptions.
        /// </summary>
        public IList<ParameterDescription> ResponseModelParameters
        {
            get { return ResponseModelDescription.GetParameterDescriptions(); }
        }

        /// <summary>
        /// Gets the sample requests associated with the API.
        /// </summary>
        public IDictionary<MediaTypeHeaderValue, object> SampleRequests { get; private set; }

        /// <summary>
        /// Gets the sample responses associated with the API.
        /// </summary>
        public IDictionary<MediaTypeHeaderValue, object> SampleResponses { get; private set; }
    }
}