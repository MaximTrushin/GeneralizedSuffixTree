using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using TestProject.Core;

namespace TestProject.Controllers
{
    public class SearchController : ApiController
    {

        // GET api/Search/{id}
        public IHttpActionResult Get([FromUri]string id)
        {
            if (!(HttpContext.Current.Application[IndexingHelper.AppPropertyName] is IndexingHelper indexer))
                return Ok("Indexing is not finished!");

            var path = System.IO.Path.Combine(HostingEnvironment.ApplicationPhysicalPath, ConfigurationManager.AppSettings["BooksForIndexingPath"]);
            //var sb = new StringBuilder();
            //foreach (var location in indexer.FindWordsWith(id))
            //{
            //    sb.AppendLine( $"{PathUtil.GetRelativePath(path, location.FileName)} ({location.Location})");
            //}

            //return Json(sb.ToString());
            return Json(indexer.FindWordsWith(id).Select(e => new WordLocation(e.Location, PathUtil.GetRelativePath(path, e.FileName))));
        }


        public IHttpActionResult Post([FromBody]string id)
        {
            return Ok("It works! Your id is " + id);
        }


        [HttpGet]
        [Route("api/Sample/Custom")]
        public IHttpActionResult Custom()
        {
            // sample custom action method using attribute-based routing
            // TODO: my code here
            throw new NotImplementedException();
        }
    }

    //public class SampleController : ApiController
    //{
    //    // GET: api/Sample
    //    public IEnumerable<string> Get()
    //    {
    //        return new string[] { "value1", "value2" };
    //    }

    //    // GET: api/Sample/5
    //    public string Get(int id)
    //    {
    //        return "value";
    //    }

    //    // POST: api/Sample
    //    public void Post([FromBody]string value)
    //    {
    //    }

    //    // PUT: api/Sample/5
    //    public void Put(int id, [FromBody]string value)
    //    {
    //    }

    //    // DELETE: api/Sample/5
    //    public void Delete(int id)
    //    {
    //    }
    //}
}
