# WebApi.Help package

This package is intended to provide a help page for self hosted ASP.NET Web API projects.
Being added to application it works by scanning available Web API routes and composing a help page out of them.
This page is published at **/help** uri (configurable). 
Methods documentation is taken from Visual Studio XML documentation files with extra support for custom documentation providers.
The package currently supports Russian and English languages.

**Usage sample**
```C#
internal void ConfigureApiHelpPage(HttpSelfHostConfiguration config) {
  config.UseHelpPage(
    help =>
    {
        // help page will at http://localhost/api/help route
        help.UriPrefix = "/api/help"; 
        // This uri will be used for samples generation
        help.SamplesBaseUri = new Uri("http://api.mydomain.org:9999");
        help
          // Set documentation provide (XML-doc files are expected to be under ./help directory
          .WithDocumentationProvider(new XmlDocumentationProvider(Environment.CurrentDirectory, "help"))
          // You may set custom content provider to handle content (css, html, js) request
          .WithContentProvider(help.DefaultContentProvider)
          // You may register custom help builder to participate in help page generation process
          .RegisterHelpBuilder(new MarkdownHelpBuilder(typeof(Program).Assembly, "Sandbox.Help."))
          // This add a custom help builder which automatically generates documentation for given types
          .AutoDocumentedTypes(typeof(Contact), typeof(User), typeof(Gender), typeof(ICollection<User>), typeof(IDictionary<string, Contact>));
    }
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
 
