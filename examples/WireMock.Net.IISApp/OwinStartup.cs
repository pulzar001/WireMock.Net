using System.Web;
using Microsoft.Owin;
using Owin;
using WireMock.Handlers;
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
      m_settings.IgnorePrefixURLs = new string[] { "/Home" };

      m_wiremockServer = WireMockServer.StartForOwin(app, m_settings);
    }
  }
}