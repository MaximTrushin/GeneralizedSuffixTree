using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CodeRepository.Web;
using TestProject.Core;

namespace TestProject
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Register Web API routing support before anything else
            GlobalConfiguration.Configure(WebApiConfig.Register);

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            var path = System.IO.Path.Combine(HostingEnvironment.ApplicationPhysicalPath, ConfigurationManager.AppSettings["BooksForIndexingPath"]);
            var app = HttpContext.Current.Application;
            var indexingHelper = new IndexingHelper(path, app);
            indexingHelper.Index();
                
            
        }
    }
}
