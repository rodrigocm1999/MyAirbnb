using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAirbnb.Data
{
    public static class DataClassesHelper
    {
        public class ValueName
        {
            public int Value { get; set; }
            public string Name { get; set; }

            public ValueName(int value, string name)
            {
                Value = value;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public static List<ValueName> BuildTypesList(IEnumerable<string> elements)
        {
            List<ValueName> list = new List<ValueName>();
            int i = 0;
            foreach (var e in elements)
            {
                list.Add(new ValueName(i,e));
                i++;
            }
            return list;
        }
    }
}
