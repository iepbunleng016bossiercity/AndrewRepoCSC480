using Spotify.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Spotify.Tests
{
    public class ArtistTests
    {

        private SpotifyService Spotify { get; set; }
        public ArtistTests()
        {
            Spotify = new SpotifyService();   
        }

        [Theory]
        [InlineData(ArtistConstants.Blink182, "blink-182")]
        [InlineData(ArtistConstants.HEALTH, "HEALTH")]
        [InlineData(ArtistConstants.WonderYears, "The Wonder Years")]
        public async Task GetArtist_NotNull_ReturnsExpectedArtist(string artistId, string expectedName)
        {
            var artist = await Spotify.GetArtistAsync(artistId);
            Assert.NotNull(artist);
            Assert.Equal(expectedName, artist.Name);            
        }

        [Theory]
        [InlineData(ArtistConstants.WonderYears)]
        public async Task GetRelatedArtists_NotNull_Returns20Matches(string artistId)
        {
            var results = (await Spotify.GetRelatedArtistsAsync(artistId)).ToList();
            Assert.NotNull(results);
            Assert.Equal(20, results.Count());
            var artist = await Spotify.GetArtistAsync(artistId);
            // filter predicate to make sure an artist didn't end up on their own playlist somehow.
            Assert.DoesNotContain(results, artist => artist.Id == artistId);
            
        }
    }
}
