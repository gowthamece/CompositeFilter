using CompositeFilter.Models;
using CompositeFilter.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;

namespace CompositeFilter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public async Task<IActionResult> Get()
        {
            dynamic sessions = new ExpandoObject();
            string filter = HttpContext.Request.Query["filter"];
            var filterResult = new RootFilter();
            if (!string.IsNullOrEmpty(filter))
            {
                filterResult = JsonConvert.DeserializeObject<RootFilter>(filter);
            }
            else
            {
                filterResult = new RootFilter();
            }
            var productList = new List<Product>();
            for (int i = 0; i < 100; i++)
            {
                productList.Add(new Product { ProductID = i, Name = "Product " + i, Price = i * 10, Description = "Description " + i });
            }
            var sessionsQuery = productList.AsQueryable();
            if(filterResult.Filters != null)
            {
                sessionsQuery = CompositeFilter<Product>.ApplyFilter(sessionsQuery, filterResult);
            }
            
            //  sessions.totalNoOfRecords = sessionsQuery.Count();
            //   sessionsQuery = ApplyBinding(sessionsQuery);
            sessions.records = sessionsQuery.ToList();
            //var sessions = sessionsQuery.ToList();
            //  return Ok(sessions.records);
            return Ok(sessions.records);
           
        }
    }
}
