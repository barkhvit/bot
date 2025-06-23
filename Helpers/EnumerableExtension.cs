using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Helpers
{
    public static class EnumerableExtension
    {
        public static IEnumerable<T> GetBatchByNumber<T>(this IEnumerable<T> source,int batchSize, int batchNumber)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (batchSize <= 0)
                throw new ArgumentException("Размер должен быть больше 0", nameof(batchSize));

            if (batchNumber < 0)
                throw new ArgumentException("Номер стопки не должен быть меньше 0", nameof(batchNumber));

            return source.Skip(batchNumber * batchSize).Take(batchSize);
        }
    }
}
