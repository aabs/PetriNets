using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PetriNetCore;

namespace TestProject1
{
    public static class UtilityExtensions
    {
        public static void Assert(this Marking m, 
            Dictionary<int, string> names, 
            Dictionary<string, int> expected)
        {
            foreach (var kvp in expected)
            {
                int idx = names.Where(x => x.Value == kvp.Key).Select(x => x.Key).Single();
                NUnit.Framework.Assert.AreEqual(kvp.Value, m[idx]);
            }
        }

        public static void Set(this Marking m,
            Dictionary<int, string> names,
            string name,
            int value)
        {
            int idx = names.Where(x => x.Value == name).Select(x => x.Key).Single();
            m[idx] = value;
        }
        public static int Get(this Marking m,
            Dictionary<int, string> names,
            string name)
        {
            int idx = names.Where(x => x.Value == name).Select(x => x.Key).Single();
            return (int)m[idx];
        }
    }
}
