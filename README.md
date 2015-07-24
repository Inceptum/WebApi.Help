# WebApi.Help package

This package is intended to provide a help page for self-hosted ASP.NET Web API projects.
When added to an application it works by scanning available Web API routes and composing a help page out of them.
This page is then published at **/help** uri (configurable). 
API methods documentation is taken from Visual Studio XML documentation files with extra support for custom documentation providers.
The package currently supports Russian and English languages.

**Usage sample**
```C#
internal void ConfigureApiHelpPage(HttpConfiguration config) {
  config.UseHelpPage(
	help => help.Route("api/help/{*resource}")	// help page will listen on http://yourmachine/api/help
				// This base uri will be used for generation API endpoints uri examples
				.SamplesUri(new Uri("http://myapi.mycompany.com:8080"))
				// Replace default documentation provider
				.WithDocumentationProvider(new XmlDocumentationProvider(Environment.CurrentDirectory))
				// Configure default help provider
				.ConfigureHelpProvider(hp =>
						// Reregister help builders
						hp.ClearBuilders()
							.RegisterBuilder(new ErrorsDocumentationBuilder("API/Errors"))	// Add http error codes documentation
							.RegisterBuilder(new ApiDocumentationBuilder(help, "API"))		// Add API methods documentation
							.RegisterBuilder(new DelegatingBuilder(addDynamicContent))		// Add some dynamic content
							.RegisterBuilder(new MarkdownHelpBuilder(typeof(Program).Assembly, "Sandbox.Help.")))	// Adds markdown pages from resources
				// Builds documentation for given types
				.AutoDocumentedTypes(new[] { typeof(Contact), typeof(User), typeof(Gender), typeof(ICollection<User>), typeof(IDictionary<string, Contact>) }, "API/DataTypes"));
  );
  
  // Global sample data configuration: whenever the specified type is requested 
  // (doesn't metter if it's a top level type, or a part of complex model), the provided object will be used
  config.SetSampleObjects(new Dictionary<Type, object>()
  {
    { typeof(Contact), new Contact { Phone = "0123456789", Email = "test@example.com" } },
    { typeof(ExtraData), new ExtraData { Data = "Some additional data" } }
  });
  
  // Per-request samples configuration. Note the value here is expected be a string, not an object.
  config.SetSampleResponse(JObject.FromObject(new
    {
       name = "Just created user",
       age = 30
    }).ToString(Formatting.Indented), new MediaTypeHeaderValue("application/json"), "Users", "CreateUser");
}

 private static HelpItem[] addDynamicContent()
{
	return new[] {
		new HelpItem("/some/path/TestPage") { Title = "TestPage",   Data = "<p id='TestPage'><b>Динамический контент</b><br/>Это нелокализованный динамический контент, добавленный через кастомный <b>HelpBuilder</b></p>", Template = null }
	};
}
 
