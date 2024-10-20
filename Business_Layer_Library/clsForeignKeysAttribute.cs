using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer_Library
{
    public class clsForeignKeysAttribute : clsAttributes
    {

        public clsForeignKeysAttribute(string Name, bool isNullable, string DataType, bool IsForeignKey, bool IsPrimaryKey) { }
    }
}
