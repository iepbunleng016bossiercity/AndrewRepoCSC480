using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spotify.Data
{
    public class SpotifyService
    {
        private readonly string CLIENTID = Environment.GetEnvironmentVariable("CLIENTID");
        private readonly string CLIENTSECRET = Environment.GetEnvironmentVariable("CLIENTSECRET");

        private readonly SpotifyClientConfig _defaultConfig = SpotifyClientConfig.CreateDefault();
        private readonly SpotifyClient _spotify;

        public SpotifyService()
        {
            var request = new ClientCredentialsRequest(CLIENTID, CLIENTSECRET);
            var response = new OAuthClient(_defaultConfig).RequestToken(request).Result;

            _spotify = new SpotifyClient(_defaultConfig.WithToken(response.AccessToken));
        }

        public async Task<FullArtist> GetArtistAsync(string artistId)
        {
            return await _spotify.Artists.Get(artistId);
        }

        public async Task<IEnumerable<FullArtist>> GetRelatedArtistsAsync(string artistId)
        {
            var rankedArtists = new SortedList<double, FullArtist>();

            var startingArtist = await GetArtistAsync(artistId);
            var startingGenres = startingArtist.Genres;

            var artistResponse = await _spotify.Artists.GetRelatedArtists(artistId);

            for (int i = 0; i < 7; i++)
            {
                var relatedArtists = (await _spotify.Artists.GetRelatedArtists(startingArtist.Id)).Artists;

                foreach (FullArtist artist in relatedArtists)
                {
                    var genreScore = artist.Genres.Intersect(startingGenres).Count();

                    var artistKey = (1.0 / genreScore) * artist.Followers.Total;
                    if (!rankedArtists.ContainsKey(artistKey))
                    {
                        rankedArtists.Add(artistKey, artist);
                    }
                }
                startingArtist = rankedArtists.GetValueOrDefault(rankedArtists.Keys[0]);
            }
            return rankedArtists.Values.Take(20);
        }
    }
}
