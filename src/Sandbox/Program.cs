using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Inceptum.WebApi.Help;
using System.Threading;
using Inceptum.WebApi.Help.Builders;

namespace Sandbox
{
    class Program
    {
        internal static void Main()
        {
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            const string baseUri = "http://localhost:7777";
            var server = new HttpSelfHostServer(createApiConfiguration(baseUri));
            Console.WriteLine(@"Starting host at {0}...", baseUri);
            server.OpenAsync().Wait();
            Console.WriteLine(@"Started. Press ENTER to stop");
            Console.ReadLine();
            server.CloseAsync().Wait();
        }

        private static HttpSelfHostConfiguration createApiConfiguration(string baseUrl)
        {
            var config = new HttpSelfHostConfiguration(baseUrl);

            JsonMediaTypeFormatter jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            jsonFormatter.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            jsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.Routes.MapHttpRoute("Default", "api/{controller}", new { controller = "Test" });

            config.MapHttpAttributeRoutes();

            config.UseHelpPage(
                help => help.Route("/api/help")
                            .SamplesUri(new Uri("http://myapi.mycompany.com:8080"))
                            .WithDocumentationProvider(new XmlDocumentationProvider(Environment.CurrentDirectory))
                            .ConfigureHelpProvider(hp =>
                                    hp.ClearBuilders()
                                        .RegisterBuilder(new ErrorsDocumentationBuilder("API/Errors"))
                                        .RegisterBuilder(new ApiDocumentationBuilder(help, "API"))
                                        .RegisterBuilder(new DelegatingBuilder(addDynamicContent))
                                        .RegisterBuilder(new MarkdownHelpBuilder(typeof(Program).Assembly, "Sandbox.Help.")))
                            .AutoDocumentedTypes(new[] { typeof(Contact), typeof(User), typeof(Gender), typeof(ICollection<User>), typeof(IDictionary<string, Contact>), typeof(ExtraValuesList) }, "API/DataTypes"));

            // Global sample data configuration: whenever the specified type is requested (doesn't metter if it's a top level type, or a part of complex model), the provided data will be used 
            config.SetSampleObjects(new Dictionary<Type, object>
                {
                    //{ typeof(User),new User { Age = 25, Name = "Martin Luter King" } },
                    { typeof(Contact),new Contact { Phone = "0123456789", Email = "test@example.com" } },
                    { typeof(ExtraData),new ExtraData { Data = "Some additional data" } }
                });

            //config.SetSampleObject();

            // Per-request samples configuration. Note the value here is expected be a string.
            config.SetSampleResponse(() =>
            {
                return JObject.FromObject(new
                {
                    name = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "Только что созданный пользователь" : "Just created user",
                    age = 1
                }).ToString(Formatting.Indented);
            }, new MediaTypeHeaderValue("application/json"), "Echo", "CreateUser");

            return config;
        }

        private static HelpItem[] addDynamicContent()
        {
            return new[] {
                new HelpItem("/some/path/TestPage") { Title = "TestPage",   Data = "<p id='TestPage'><b>Динамический контент</b><br>Это нелокализованный динамический контент, добавленный через кастомный <b>HelpBuilder</b></p>", Template = null }
            };
        }
    }
}