using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI_Multipart_Formatter.Models
{
    public class ModelWithFile
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public File MyFile { get; set; }
    }

    public class File
    {
        public byte[] Contents { get; set; }
        public string Filename { get; set; }
        public string MediaType { get; set; }
    }
}
