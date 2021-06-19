using System;
using System.Collections.Generic;
using System.Text;

namespace AdvertAPI.Models
{
    public class AdvertResponse
    {
        public string Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
    }
}
