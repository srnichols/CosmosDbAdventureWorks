using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BlazorServerApp.Models
{
    public class ProductMeta
    {

        public string id { get; set; }
        [Required, MinLength(3), MaxLength(75)]
        public string name { get; set; }
        public string type { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }

    }
}
