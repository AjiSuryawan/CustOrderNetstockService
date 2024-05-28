using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustOrderNetstockService
{
    public class FormatSpecification
    {
        public string FieldName { get; set; }
        public string Format { get; set; }
        public string DataType { get; set; }
    }

    public class FormatConfig
    {
        public List<FormatSpecification> Formats { get; set; }
    }
}
