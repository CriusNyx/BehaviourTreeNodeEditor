using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Extensions
{
    public static class ListExtensions
    {
        public static IEnumerable<(int index, T value)> Foreach<T>(this IList<T> list)
        {
            for(int i = 0; i < list.Count; i++)
            {
                yield return (i, list[i]);
            }
        }
    }
}
