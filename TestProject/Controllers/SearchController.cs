using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using TestProject.Core;

namespace TestProject.Controllers
{
    public class SearchController : ApiController
    {
        [HttpGet]
        [Route("api/search/getBooks")]
        public IHttpActionResult GetBooks(string sub)
        {
            if (!(HttpContext.Current.Application[IndexingHelper.AppPropertyName] is IndexingHelper indexer))
                return NotFound();

            var path = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, ConfigurationManager.AppSettings["BooksForIndexingPath"]);
            return Json(indexer.FindWordsWith(sub)
                .Select(e => PathUtil.GetRelativePath(path, e.FileName))
                .Distinct()
                .OrderBy(l => l)
                );
        }

        [HttpGet]
        [Route("api/search/getLocations")]
        public IHttpActionResult GetLocations([FromUri]string sub, string fileName)
        {
            if (!(HttpContext.Current.Application[IndexingHelper.AppPropertyName] is IndexingHelper indexer))
                return NotFound();

            var path = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, ConfigurationManager.AppSettings["BooksForIndexingPath"]);
            return Json(indexer.FindWordsWith(sub)
                .Select(e => new WordLocation(e.Location, PathUtil.GetRelativePath(path, e.FileName)))
                .OrderBy(l => l.FileName + l.Location.ToString().PadLeft(10, '0'))
                );
        }


        [HttpGet]
        [Route("api/search/getTextFragment")]
        public IHttpActionResult getTextFragment(string fileName, int location)
        {
            fileName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, 
                ConfigurationManager.AppSettings["BooksForIndexingPath"],
                "." + fileName);
            var numberOfCharsToRead = 500;
            var start = Math.Max(0, location - numberOfCharsToRead/2);
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(start, SeekOrigin.Begin);

                byte[] b = new byte[numberOfCharsToRead];
                fs.Read(b, 0, numberOfCharsToRead);

                string s = System.Text.Encoding.UTF8.GetString(b);
                return Ok(s);
            }
        }
    }
 
}
