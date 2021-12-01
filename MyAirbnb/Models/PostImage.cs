using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Models
{
    public class PostImage
    {

        public int Id { get; set; }
        public int PostId { get; set; }
        public string FilePath { get; set; }

    }
}
