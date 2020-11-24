using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Spotify.Data;
using SpotifyAPI.Web;

namespace Spotify.Pages
{
    public partial class Index : ComponentBase
    {
        [Inject]
        private NavigationManager NavManager { get; set; }

        [Inject]
        private SpotifyService SpotifyService { get; set; }

        private Uri _authUri;

        private IEnumerable<FullArtist> _results;

        

        private string _authCode;

        private SpotifyClientConfig _cfg;

        private string _playlistUrl;

        protected override void OnInitialized()
        {
        }

        public async Task GenerateArtists()
        {
            _results = await SpotifyService.GetRelatedArtistsAsync(ArtistConstants.Blink182);
            StateHasChanged();
        }

        protected async Task CreatePlaylist()
        {
            //_spotify = new SpotifyClient(_cfg);
            //var userId = (await _spotify.UserProfile.Current()).Id;
            //var request = new PlaylistCreateRequest("Melomane Test")
            //{
            //    Collaborative = false,
            //    Description = "Beep boop",
            //};
            //var playlist = await _spotify.Playlists.Create(userId, request);

            //// https://developer.spotify.com/documentation/web-api/reference/artists/get-artists-top-tracks/
            //// Artist's Top Tracks returns 10 tracks. Pick one at random, so you don't hear the same song from
            //// the same band over and over again.
            //Random rand = new Random();
            //List<string> trackUris = new List<string>();

            //foreach (FullArtist artist in _results)
            //{
            //    var tracks = (await _spotify.Artists.GetTopTracks(artist.Id, new ArtistsTopTracksRequest("US"))).Tracks;
            //    var selectedTrack = tracks.ElementAt(rand.Next(tracks.Count));
            //    trackUris.Add(selectedTrack.Uri);
            //}

            //await _spotify.Playlists.AddItems(playlist.Id, new PlaylistAddItemsRequest(trackUris));
            //_playlistUrl = playlist.Uri;
        }
    }
}