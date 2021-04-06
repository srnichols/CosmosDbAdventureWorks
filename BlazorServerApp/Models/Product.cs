using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BlazorServerApp.Models
{
    public class Product
    {
        [Required]
        public string id { get; set; }
        [Required]
        public string categoryId { get; set; }
        public string categoryName { get; set; }
        [Required, MinLength(3), MaxLength(25)]
        public string sku { get; set; }
        [Required, MinLength(3), MaxLength(75)]
        public string name { get; set; }
        [Required, MinLength(5), MaxLength(250)]
        public string description { get; set; }
        [Required]
        public double price { get; set; }
        public List<Tag> tags { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }
    }

    public class Tag
    {
        public string id { get; set; }
        public string name { get; set; }

    }
}
