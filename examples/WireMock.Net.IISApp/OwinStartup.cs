using System.Web;
using Microsoft.Owin;
using Owin;
using WireMock.Handlers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

[assembly: OwinStartup(typeof(WireMock.Net.IISApp.OwinStartup))]
namespace WireMock.Net.IISApp
{
  public class OwinStartup
  {
    private IWireMockServerSettings m_settings;
    private WireMockServer m_wiremockServer;
    
    public void Configuration(IAppBuilder app)
    {
      m_settings = new WireMockServerSettings();
      m_settings.StartAdminInterface = true;
      m_settings.ReadStaticMappings = true;
      m_settings.WatchStaticMappings = true;
      m_settings.WatchStaticMappingsInSubdirectories = true;
      m_settings.FileSystemHandler = new LocalFileSystemHandler(HttpRuntime.AppDomainAppPath);
      m_settings.IgnorePrefixURLs = new string[] { "/Hom*" };
      /*m_settings.ProxyAndRecordSettings = new ProxyAndRecordSettings()
      {
        Url = "http://www.google.com",
        SaveMapping = true,
        SaveMappingToFile = true
      };*/

      m_wiremockServer = WireMockServer.StartForOwin(app, m_settings);

      app.Use(async (ctx, next) =>
      {
        ctx.Set("wiremockServer", m_wiremockServer);
        await next();
      });

      BuildTestScenario(m_wiremockServer);
    }

    private void BuildTestScenario(WireMockServer p_server)
    {
      p_server
        .Given(Request.Create()
          .WithPath("/todo/items")
          .UsingGet())
        .InScenario("To do list")
        .WillSetStateTo("TodoList State Started")
        .RespondWith(Response.Create()
          .WithBody("Buy milk"));

      p_server
        .Given(Request.Create()
          .WithPath("/todo/items")
          .UsingPost())
        .InScenario("To do list")
        .WhenStateIs("TodoList State Started")
        .WillSetStateTo("Cancel newspaper item added")
        .RespondWith(Response.Create()
          .WithStatusCode(201));

      p_server
        .Given(Request.Create()
          .WithPath("/todo/items")
          .UsingGet())
        .InScenario("To do list")
        .WhenStateIs("Cancel newspaper item added")
        .RespondWith(Response.Create()
          .WithBody("Buy milk;Cancel newspaper subscription"));
    }
  }
}