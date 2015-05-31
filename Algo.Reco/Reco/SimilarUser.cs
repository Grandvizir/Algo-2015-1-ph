using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo
{
    public struct SimilarUser
    {
        public readonly User User;
        public readonly double Similarity;

        public SimilarUser( User u, double s )
        {
            User = u;
            Similarity = s;
        }
    }
}
