using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Inceptum.WebApi.Help.Tests
{
    [TestFixture, Ignore]
    public class InvestigationTests
    {
        [Test]
        public void UriTests()
        {
            var uri1 = new Uri(@"http:\\somedomain.com:80/api/test", UriKind.RelativeOrAbsolute);
            var uri2 = new Uri(@"\api\test", UriKind.Relative);
            var uri3 = new Uri(@"api\test", UriKind.Relative);


            Debug.WriteLine("URI 1: " + uri1.IsAbsoluteUri);
            Debug.WriteLine("URI 2: " + uri2.IsAbsoluteUri);
            Debug.WriteLine("URI 3: " + uri3.IsAbsoluteUri);

            Debug.WriteLine("Local path URI 1 : " + uri1.LocalPath);
            Debug.WriteLine("Local path URI 2: " + uri2.LocalPath);
            Debug.WriteLine("Local path URI 3: " + uri3.LocalPath);

        }
    }
}
