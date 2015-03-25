using System;
using System.Collections.Generic;

namespace Inceptum.WebApi.Help.Builders
{
    /// <summary>
    /// Contributor which passes a task of composing <see cref="T:Inceptum.WebApi.Help.HelpPageModel"/> help page model to a delegate, provided by consumer.
    /// </summary>
    public class DelegatingBuilder : IHelpBuilder
    {
        private readonly Func<IEnumerable<HelpItem>> m_Delegated;

        public DelegatingBuilder(Func<IEnumerable<HelpItem>> delegated)
        {
            if (delegated == null) throw new ArgumentNullException("delegated");
            m_Delegated = delegated;
        }

        public IEnumerable<HelpItem> BuildHelp()
        {
            return m_Delegated();
        }
    }
}