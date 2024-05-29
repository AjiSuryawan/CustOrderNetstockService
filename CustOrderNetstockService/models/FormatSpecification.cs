using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustOrderNetstockService.models
{
    internal class FormatSpecification
    {
        public string FieldName { get; set; }
        public string Format { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }
        public string DefaultValue { get; set; }
    }
}
