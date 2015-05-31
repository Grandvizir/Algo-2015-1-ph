using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Algo
{
    public class RecoContext
    {
        public User[] Users { get; private set; }
        public Movie[] Movies { get; private set; }

        public void LoadFrom( string folder )
        {
            Users = User.ReadUsers( Path.Combine( folder, "users.dat" ) );
            Movies = Movie.ReadMovies( Path.Combine( folder, "movies.dat" ) );
            User.ReadRatings( Users, Movies, Path.Combine( folder, "ratings.dat" ) );
        }

        public void computingMovieAverage()
        {
            foreach ( User user in Users ) {
                foreach ( KeyValuePair<Movie, int> hash in user.Ratings) {
                     Movie m = (from s in Movies where s.MovieID == hash.Key.MovieID select s).First();
                    if (m != null) m.AddRating(user, hash.Value);
                } 
            }
            

        }


        public double SimilarityPearson( Movie m1, Movie m2 )
        {
            IEnumerable<User> common = m1.RatingAverage.Keys.Intersect( m2.RatingAverage.Keys );
            double count = common.Count();
            if (count == 0) return 0;
            // TODO
            if (count == 1) return 0;

            double sumProd = 0;
            double sumSquare1 = 0;
            double sumSquare2 = 0;
            double sum1 = 0;
            double sum2 = 0;


            foreach (User u in common)
            {
                int r1 = m1.RatingAverage[u];
                int r2 = m2.RatingAverage[u];
                sum1 += r1;
                sum2 += r2;
                sumSquare1 += r1 * r1;
                sumSquare2 += r2 * r2;
                sumProd += r1 * r2;
            }

            double numerator = sumProd - ((sum1 * sum2) / count);
            double denominator1 = sumSquare1 - ((sum1 * sum1) / count);
            double denominator2 = sumSquare2 - ((sum2 * sum2) / count);
            double denominator = Math.Sqrt(denominator1 * denominator2);

            if (denominator < Double.Epsilon) return 1;
            return numerator / denominator;
        }

        public SimilarMovie[] GetSimilarMovies(Movie m, int count)
        {
            BestKeeper<SimilarMovie> best = new BestKeeper<SimilarMovie>(count,
                                                 (s1, s2) => Math.Sign(s2.Similarity - s1.Similarity));

            foreach (var other in Movies)
            {
                if (other == m) continue;
                SimilarMovie sM = new SimilarMovie(other, SimilarityPearson(m, other));
                best.Add(sM);
            }
            return best.ToArray();
        }

        public List<Movie> GetRecommendedMovies(Movie m, SimilarMovie[] similarMovies, int count)
        {
            Dictionary<Movie, double> recommendedMovies = new Dictionary<Movie, double>();

            for (int i = 0; i < similarMovies.Length; i++)
            {
                if (similarMovies[i].Movie == m) continue;
                if (similarMovies[i].Movie.RatingAverage.Count == 0) continue;

                recommendedMovies[similarMovies[i].Movie] = ( similarMovies[i].Movie.RatingAverage.Sum(x => x.Value) / similarMovies[i].Movie.RatingAverage.Count );
               
            }

            return (from entry in recommendedMovies orderby entry.Value descending select entry.Key).Take(count).ToList<Movie>();
        }

        public double SimilarityPearson( User u1, User u2 )
        {
            IEnumerable<Movie> common = u1.Ratings.Keys.Intersect( u2.Ratings.Keys );
            double count = common.Count();
            if( count == 0 ) return 0;
            if( count == 1 ) return SimilarityNorm2( u1, u2 );
            double sumProd = 0;
            double sumSquare1 = 0;
            double sumSquare2 = 0;
            double sum1 = 0;
            double sum2 = 0;

            #region loop
            foreach( Movie m in common )
            {
                int r1 = u1.Ratings[m];
                int r2 = u2.Ratings[m];
                sum1 += r1;
                sum2 += r2;
                sumSquare1 += r1 * r1;
                sumSquare2 += r2 * r2;
                sumProd += r1 * r2;
            }
            #endregion

            #region computing result
            double numerator = sumProd - ((sum1 * sum2) / count);
            double denominator1 = sumSquare1 - ((sum1 * sum1) / count);
            double denominator2 = sumSquare2 - ((sum2 * sum2) / count);
            double denominator = Math.Sqrt( denominator1 * denominator2 );
            #endregion

            if( denominator < Double.Epsilon ) return 1;
            return numerator / denominator;
        }

        public double DistanceNorm2( User u1, User u2 )
        {
            if( u1 == u2 && u1.Ratings.Count == 0 ) return 0.0;
            bool atLeastOneMovieInCommon = false;
            double sumSquare = 0;
            foreach( var r in u1.Ratings )
            {
                // U1 has rated movieU1 with ratingU1.
                Movie movieU1 = r.Key;
                int ratingU1 = r.Value;
                // Does U2 have an advice about this movie?
                int ratingU2;
                if( u2.Ratings.TryGetValue( movieU1, out ratingU2 ) )
                {
                    // Yes, U2 have seen it.
                    atLeastOneMovieInCommon = true;
                    // We sum the square of the difference.
                    sumSquare += Math.Pow( ratingU1 - ratingU2, 2 );
                }
            }
            return atLeastOneMovieInCommon ? Math.Sqrt( sumSquare ) : Double.PositiveInfinity;
        }

        public double SimilarityNorm2( User u1, User u2 )
        {
            return 1 / (1 + DistanceNorm2( u1, u2 ));
        }

        public SimilarUser[] GetSimilarUsers( User u, int count )
        {
            BestKeeper<SimilarUser> best = new BestKeeper<SimilarUser>( count, 
                                                    (s1,s2) => Math.Sign( s2.Similarity - s1.Similarity ) );
            //BestKeeper<SimilarUser> worst = new BestKeeper<SimilarUser>( count, 
            //                                        (s1,s2) => Math.Sign( s1.Similarity - s2.Similarity ) );
            foreach( var other in Users )
            {
                if( other == u ) continue;
                SimilarUser sU = new SimilarUser( other, SimilarityPearson( u, other ) );
                best.Add( sU );
            }
            return best.ToArray();
        }

        public List<Movie> GetRecommendedMovies( User u, SimilarUser[] similarUsers, int count )
        {
            Dictionary<Movie, int> recommendedMovies = new Dictionary<Movie, int>();

            for (int i = 0; i < similarUsers.Length; i++)
            {
                foreach (var rating in similarUsers[i].User.Ratings)
                {
                    if (u.Ratings.ContainsKey( rating.Key ) ) continue;

                    if (recommendedMovies.ContainsKey(rating.Key)) {
                        recommendedMovies[rating.Key] += rating.Value;
                    } else {
                        recommendedMovies[rating.Key] = rating.Value;
                    }
                }
            }

            return (from entry in recommendedMovies orderby entry.Value descending select entry.Key).Take( count ).ToList<Movie>();
        }
    }
}
