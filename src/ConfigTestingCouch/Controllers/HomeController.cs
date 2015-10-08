using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.OptionsModel;
using Couchbase.Authentication.SASL;
using Couchbase.Configuration;
using Couchbase.Configuration.Client;
using Couchbase.Configuration.Server.Serialization;
using Couchbase.Core.Transcoders;
using Couchbase.IO;
using Couchbase.IO.Converters;
using Couchbase.IO.Strategies;
using Newtonsoft.Json;
using Couchbase.Linq;
using ConfigTestingCouch.Models;
using Couchbase;

namespace ConfigTestingCouch.Controllers
{
    public class HomeController : Microsoft.AspNet.Mvc.Controller
    {
        private CouchSettings _couch;
        public HomeController(IOptions<CouchSettings> couch)
        {
            _couch = couch.Options;

            ClusterHelper.Initialize(
            new ClientConfiguration
            {
                Servers = new List<Uri>
                {
                    new Uri(_couch.Server)
                },
                UseSsl = false,
                DefaultOperationLifespan = 2500,
                EnableTcpKeepAlives = true,
                TcpKeepAliveTime = 1000 * 60 * 60,
                TcpKeepAliveInterval = 5000,
                BucketConfigs = new Dictionary<string, BucketConfiguration>
                {
                    {
                        _couch.Bucket,
                        new BucketConfiguration
                        {
                            BucketName = _couch.Bucket,
                            UseSsl = false,
                            Password = "",
                            PoolConfiguration = new PoolConfiguration
                            {
                                MaxSize = 50,
                                MinSize = 10
                            }
                        }
                    }
                }
            });

        }
        public IActionResult Index()
        {
            
            return View();
        }

        public IActionResult About()
        {
            var bucket = ClusterHelper.GetBucket(_couch.Bucket);
            var query = bucket.CreateQuery("beer", "brewery_beers", false).Limit(5);

            var result = bucket.Query<Beer>(query);
            var beers = new List<string>();
            foreach (var row in result.Rows)
            {
                beers.Add(row.Value.Name);
                Console.WriteLine(row.Value.Name);
            }

            ViewBag.Beers = beers;
            ViewData["Message"] = bucket.Name;
            
            //    var query = from b in db.Query<Beer>()
            //                select b;

            //ViewData["Message"] = query.Select(a => a.Name).First();

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
