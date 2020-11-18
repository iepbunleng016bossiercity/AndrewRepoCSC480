using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Components;
using Spotify.Data;
using SpotifyAPI.Web;

namespace Spotify.Pages
{
    public partial class Index : ComponentBase
    {
        [Inject]
        private NavigationManager NavManager { get; set; }

        private Uri _authUri;

        private ICollection<FullArtist> _relatedArtists;

        private IEnumerable<FullArtist> _results;

        private SortedList<double, FullArtist> _rankedArtists;

        private SpotifyClient _spotify;

        private string _authCode;

        private readonly SpotifyClientConfig _defaultConfig = SpotifyClientConfig.CreateDefault();

        private SpotifyClientConfig _cfg;

        private readonly string CLIENTID = Environment.GetEnvironmentVariable("CLIENTID");

        private readonly string CLIENTSECRET = Environment.GetEnvironmentVariable("CLIENTSECRET");

        private string _playlistUrl;

        protected override void OnInitialized()
        {
            var loginRequest = new LoginRequest(NavManager.ToAbsoluteUri(NavManager.BaseUri), CLIENTID, LoginRequest.ResponseType.Code)
            {
                Scope = new[] { 
                    Scopes.PlaylistReadPrivate, 
                    Scopes.PlaylistReadCollaborative, 
                    Scopes.PlaylistModifyPrivate, 
                    Scopes.PlaylistModifyPublic }
            };
            _authUri = loginRequest.ToUri();

            _relatedArtists = new List<FullArtist>();
            _rankedArtists = new SortedList<double, FullArtist>();
            _results = new List<FullArtist>();

            var uri = new Uri(NavManager.Uri);
            var queryDictionary = HttpUtility.ParseQueryString(uri.Query);
            if (!queryDictionary.HasKeys())
            {
                return;
            }
            _authCode = queryDictionary[0];
        }

        public async Task GenerateArtists()
        {
            var authResponse = await new OAuthClient().RequestToken(new AuthorizationCodeTokenRequest(CLIENTID, CLIENTSECRET, _authCode, NavManager.ToAbsoluteUri(NavManager.BaseUri)));
            _cfg  = _defaultConfig.WithAuthenticator(new AuthorizationCodeAuthenticator(CLIENTID, CLIENTSECRET, authResponse));

            _spotify = new SpotifyClient(_cfg);

            var startingArtist = await _spotify.Artists.Get(ArtistConstants.blink182);
            var challengeGenres = startingArtist.Genres;

            for (int i = 0; i < 7; i++)
            {
                _relatedArtists = (await _spotify.Artists.GetRelatedArtists(startingArtist.Id)).Artists;
                foreach (FullArtist artist in _relatedArtists)
                {
                    var genreScore = artist.Genres.Intersect(challengeGenres).Count();
                    var artistKey = (1.0 / genreScore) * artist.Followers.Total;
                    if (!_rankedArtists.ContainsKey(artistKey))
                    {
                        System.Diagnostics.Debug.WriteLine($"Adding {artist.Name} with score {artistKey}");
                        _rankedArtists.Add(artistKey, artist);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("|");
                        System.Diagnostics.Debug.WriteLine($"\\__ Skipping {artist.Name} with score {artistKey}");
                    }
                }
                startingArtist = _rankedArtists.GetValueOrDefault(_rankedArtists.Keys[0]);
            }
            _results = _rankedArtists.Values.Take(20);
            StateHasChanged();
        }

        protected async Task CreatePlaylist()
        {
            _spotify = new SpotifyClient(_cfg);
            var userId = (await _spotify.UserProfile.Current()).Id;
            var request = new PlaylistCreateRequest("Melomane Test")
            {
                Collaborative = false,
                Description = "Beep boop",
            };
            var playlist = await _spotify.Playlists.Create(userId, request);

            // https://developer.spotify.com/documentation/web-api/reference/artists/get-artists-top-tracks/
            // Artist's Top Tracks returns 10 tracks. Pick one at random, so you don't hear the same song from
            // the same band over and over again.
            Random rand = new Random();
            List<string> trackUris = new List<string>();

            foreach (FullArtist artist in _results)
            {
                var tracks = (await _spotify.Artists.GetTopTracks(artist.Id, new ArtistsTopTracksRequest("US"))).Tracks;
                var selectedTrack = tracks.ElementAt(rand.Next(tracks.Count));
                trackUris.Add(selectedTrack.Uri);
            }

            await _spotify.Playlists.AddItems(playlist.Id, new PlaylistAddItemsRequest(trackUris));
            _playlistUrl = playlist.Uri;
        }
    }
}