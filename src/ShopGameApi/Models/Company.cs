using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace ShopGameApi.Models
{
    public class Company
    {
        public int CompanyId { get; set; }

        [StringLength(255, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(255, MinimumLength = 3)]
        public string Country { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public ICollection<Game> Games { get; set; }
    }
}