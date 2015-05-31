using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo
{
    public struct SimilarMovie
    {
        public readonly Movie Movie;
        public readonly double Similarity;

        public SimilarMovie(Movie m, double s)
        {
            Movie = m;
            Similarity = s;
        }
    }
}