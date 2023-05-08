using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageScraper.Extensions
{
    public static class ListExtensions
    {
        public static T GetRandomElement<T>(this List<T> source)
        {
            if (source.Count == 0)
            {
                throw new InvalidOperationException("The source list is empty.");
            }

            Random random = new Random();
            int randomIndex = random.Next(source.Count);
            return source[randomIndex];
        }
    }
}
