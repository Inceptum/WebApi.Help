using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Inceptum.WebApi.Help.Builders
{
    /// <summary>
    /// Builds help items from markdown (*.md) files, stored in app's resources
    /// </summary>
    public class MarkdownHelpBuilder : IHelpBuilder
    {
        private readonly IDictionary<string, HelpItem[]> m_ResourceCache;
        private static readonly Regex m_HeaderExtractingRegex = new Regex("^\\s*#+(.*?)($|\n|\r)(.*)", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex m_ExtractCodeLanguageRegex = new Regex("^({{(.+)}}\\s*?[\r\n]+)", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private const string TEMPLATE_NAME = @"markdown";

        public MarkdownHelpBuilder(Assembly resourcesAssembly, string resourcesNamespaceRoot)
        {
            if (resourcesAssembly == null) throw new ArgumentNullException("resourcesAssembly");
            if (string.IsNullOrWhiteSpace(resourcesNamespaceRoot)) throw new ArgumentNullException("resourcesNamespaceRoot");

            m_ResourceCache = initializeResourceCache(resourcesAssembly, resourcesNamespaceRoot);
        }

        public IEnumerable<HelpItem> BuildHelp()
        {
            var language = detectLanguage();
            HelpItem[] helpItems;
            return m_ResourceCache.TryGetValue(language, out helpItems) ? helpItems : new HelpItem[0];
        }

        private static string detectLanguage()
        {
            var language = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            if (language != "ru" && language != "en")
                language = "ru";
            return language;
        }

        private static IDictionary<string, HelpItem[]> initializeResourceCache(Assembly assembly, string nsRoot)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (string.IsNullOrWhiteSpace(nsRoot)) throw new ArgumentNullException("nsRoot");

            // Match: [ru|en].[filename].md
            var r = new Regex(@"^(ru|en)\.(.*)\.md$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var localizedHelp = from res in assembly.GetManifestResourceNames()
                                 where res.StartsWith(nsRoot, StringComparison.OrdinalIgnoreCase)
                                 let match = r.Match(res.Substring(nsRoot.Length))
                                 where match.Success
                                 let page = new { locale = match.Groups[1].ToString(), name = match.Groups[2].ToString(), resource = res }
                                 group page by page.locale
                                     into g
                                     select g;

            return localizedHelp.ToDictionary(g => g.Key,
                g => g.Select(page => ReadItem(page.name, assembly.GetManifestResourceStream(page.resource))).ToArray());
        }

        public static HelpItem ReadItem(string path, Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            var codeBlocks = new Dictionary<string, string>();
            var markdown = new MarkdownDeep.Markdown
                {
                    ExtraMode = true,
                    FormatCodeBlock = (md, code) =>
                        {
                            // Try to extract the language from the first line
                            var match = m_ExtractCodeLanguageRegex.Match(code);
                            string codeLanguage = null;

                            if (match.Success)
                            {
                                // Save the language
                                var g = match.Groups[2];
                                codeLanguage = g.ToString();

                                // Remove the first line
                                code = code.Substring(match.Groups[1].Length);
                            }

                            // If not specified - just print out keeping formatting
                            if (codeLanguage == null)
                            {
                                return string.Format("<pre><code>{0}</code></pre>\n\n", code);
                            }

                            codeBlocks[codeLanguage] = code;
                            return "";
                        }
                };

            path = path.Replace(".", "/");

            using (var sr = new StreamReader(stream))
            {
                string text, displayName = null;
                var input = sr.ReadToEnd();
                var match = m_HeaderExtractingRegex.Match(input);
                
                if (match.Success)
                {
                    displayName = match.Groups[1].ToString();
                    text = markdown.Transform(match.Groups[3].ToString());
                }
                else
                {
                    text = markdown.Transform(input);
                }

                return new HelpItem(path)
                {
                    Data = new { text, codeBlocks = codeBlocks.ToArray() },
                    Title = displayName,
                    Template = TEMPLATE_NAME
                };
            }
        }

    }
}