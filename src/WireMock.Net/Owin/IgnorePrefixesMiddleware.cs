using System.Collections.Generic;
using System.Threading.Tasks;
using WireMock.Matchers;
using WireMock.Validation;
#if !USE_ASPNETCORE
using Microsoft.Owin;
using IContext = Microsoft.Owin.IOwinContext;
using OwinMiddleware = Microsoft.Owin.OwinMiddleware;
using Next = Microsoft.Owin.OwinMiddleware;
#else
using OwinMiddleware = System.Object;
using IContext = Microsoft.AspNetCore.Http.HttpContext;
using Next = Microsoft.AspNetCore.Http.RequestDelegate;
#endif

namespace WireMock.Owin
{
  internal class IgnorePrefixesMiddleware : OwinMiddleware
  {
    private readonly IWireMockMiddlewareOptions m_options;

    private List<WildcardMatcher> m_matchers;
    
#if !USE_ASPNETCORE
    public IgnorePrefixesMiddleware(OwinMiddleware p_next, IWireMockMiddlewareOptions p_options) : base(p_next)
    {
      Check.NotNull(p_options, nameof(p_options));

      m_matchers = new List<WildcardMatcher>();
      m_options = p_options;
      if (m_options.IgnorePrefixURLs != null)
      {
        foreach (string prefix in m_options.IgnorePrefixURLs)
        {
          m_matchers.Add(new WildcardMatcher(prefix, true));
        }
      }
    }
#endif

#if USE_ASPNETCORE
        public Next Next { get; }
#endif

#if !USE_ASPNETCORE
    public override Task Invoke(IContext p_ctx)
#else
        public Task Invoke(IContext p_ctx)
#endif
    {
      return InvokeInternal(p_ctx);
    }

    private async Task InvokeInternal(IContext p_ctx)
    {
      if (m_options.IgnorePrefixURLs != null)
      {
        foreach (WildcardMatcher matcher in m_matchers)
        {
          if (matcher.IsMatch(p_ctx.Request.Path.Value) >= 0.99d)
          {
            // skip owin and go on
#if !USE_ASPNETCORE
            p_ctx.Set("gotoNext", true);
#else
#endif
          }
        }
      }
      
      await Next?.Invoke(p_ctx);
    }
  }
}