using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;
using WireMock.Server;

namespace WireMock.Net.IISApp.Controllers
{
  public class HomeController : Controller
  {
    public ActionResult Index()
    {
      return View();
    }

    public ActionResult About()
    {
      ViewBag.Message = "Your application description page.";
      return View();
    }

    public ActionResult Contact()
    {
      ViewBag.Message = "Your contact page.";
      return View();
    }

    public ActionResult Test()
    {
      WireMockServer wiremock;
      
      wiremock = Request.GetOwinContext().Get<WireMockServer>("wiremockServer");

      wiremock
        .Given(WireMock.RequestBuilders.Request.Create()
          .WithPath("/test/list")
          .UsingGet())
        .RespondWith(WireMock.ResponseBuilders.Response.Create()
          .WithHeader("Content-Type", "application/json")
          .WithBodyAsJson(new {
            Text = "{{Random Type=\"Text\" Min=8 Max=20}}"
          })
          .WithTransformer());

      return View();
    }
  }
}