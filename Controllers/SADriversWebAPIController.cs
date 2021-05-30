using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Assignment2.Controllers
{
    public class SADriversWebAPIController : ApiController
    {
        private Models.SADriverEntities db = new Models.SADriverEntities();

        /* Use this request to get a list of Suburbs  */

        // GET: /api/SADriversWebAPI
        [HttpGet]
        public object GetSADriversWebAPI()
        {
            // var postcodes = (from db.)
            var suburbSummary = (from p in db.SAPostcodes
                                 group p by p.Postcode into newGroup
                                 orderby newGroup.Key
                                 select new
                                 {
                                     postcode = newGroup.Key,
                                     years = db.Drivers
                                     .Where(x => x.Postcode == newGroup.Key)
                                     .GroupBy(x => x.Year)
                                     .Select(x => new
                                     {
                                         year = x.Key,
                                         records = x.Count()
                                     }).ToList(),
                                     suburbs = db.SAPostcodes
                                     .Where(x => x.Postcode == newGroup.Key)
                                     .OrderBy(x => x.Locality)
                                     .Select(x => x.Locality)
                                     .ToList()

                                 });
            return suburbSummary.ToList();
        }

        // GET: /api/SADriversWebAPI?postcode=5000&year=2019
        [HttpGet]
        public object GetSADriversWebAPI(string postcode, int year)
        {
            var data = (from d in db.Drivers
                        where d.Postcode == postcode
                        && d.Year == year
                        group d by d.Quarter into newGroup
                        orderby newGroup.Key
                        select new QuaterCount
                        {
                            quater = newGroup.Key,
                            count = newGroup.Count(),
                            genderCounts = new List<GenderCount>() {
                            new GenderCount {gender = "Female",
                                count = newGroup.Count(p => p.Sex == "Female") },
                              new GenderCount{gender = "Male",
                                  count = newGroup.Count(p => p.Sex == "Male")},
                               new GenderCount{gender = "Gender X",
                                   count = newGroup.Count(p => p.Sex == "Gender X")},
                            new GenderCount{gender = "Not Listed",
                                count = newGroup.Count(p => p.Sex == "Not listed") }
                    }
                        }).ToList();

            data = data.Select(p =>
            {
                p.maxCount = p.genderCounts.Max(p2 => p2.count);
                return p;
            }).ToList();

            return data;
        }

        // GET: /api/SADriversWebAPI?postcode=5000&year=2019&gender=female&quater=q1
        [HttpGet]
        public object GetSADriversWebAPI(string postcode, int year, string gender, string quater)
        {
            var records = (from d in db.Drivers
                           where d.Postcode == postcode
                           && d.Year == year
                           && d.Sex == gender
                           && d.Quarter == quater
                           select d).OrderBy(p => p.Age).ToList();

            var finalRecords = records.GroupBy(p => GetAgeGroup(p.Age)).Select(p => new
            {
                ageGroup = p.Key,
                count = p.Count()
            }).ToList();

            return finalRecords;
        }

        public string GetAgeGroup(int? age)
        {
            if (!age.HasValue) return "unknown";
            else if (age < 20)
                return "<20";
            else if (age <= 30)
                return "21-30";
            else if (age <= 40)
                return "31-40";
            else if (age <= 50)
                return "41-50";
            else if (age <= 60)
                return "51-60";
            else if (age <= 70)
                return "61-70";
            else if (age < 80)
                return "71-79";
            return "80+";
        }

    }

    public class QuaterCount
    {
        public string quater { get; set; }
        public int count { get; set; }
        public List<GenderCount> genderCounts { get; set; }
        public int maxCount { get; set; }
    }

    public class GenderCount
    {
        public string gender { get; set; }
        public int count { get; set; }
    }
}
