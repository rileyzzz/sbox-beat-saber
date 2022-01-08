using Sandbox;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.IO.Compression;
using System.IO;

namespace BeatSaber
{
	public static class BeatSaver
	{
		struct MapDetailResponse
		{
			[JsonPropertyName( "docs" )]
			public MapDetail[] Maps { get; set; }
		}


		// this probably ain't thread safe, but that shit ain't whitelisted so too bad
		public class RequestInfo
		{
			public bool Cancel = false;
			public bool Complete = false;
			public float Progress = 0.0f;
		}

		public static RequestInfo GetLatestMaps( Action<MapDetail[]> callback )
		{
			RequestInfo info = new RequestInfo();

			var task = new Sandbox.Internal.Http( new Uri( "https://beatsaver.com/api/maps/latest" ) );

			var req = task.GetStringAsync();
			req.ContinueWith(response => {
				if ( !response.IsCompleted || info.Cancel )
					return;

				//Log.Info( "response " + req.Result );
				var data = JsonSerializer.Deserialize<MapDetailResponse>( response.Result );
				callback( data.Maps );

				info.Complete = true;
			} );

			return info;
		}

		static string BuildQuery(Dictionary<string, string> query)
		{
			if ( query.Count == 0 )
				return "";

			string str = "?";

			int i = 0;
			foreach(var q in query)
			{
				if ( i++ != 0 )
					str += "&";

				str += q.Key + "=" + q.Value;
			}

			return str;
		}

		public static RequestInfo SearchMaps( Action<MapDetail[]> callback, int page = 0, string query = "" )
		{
			string url = "https://beatsaver.com/api/search/text/" + page;
			//string url = "https://beatsaver.com/api/search/text/0/";

			Dictionary<string, string> q = new();

			if ( query != "" )
				q["q"] = query;

			//q["page"] = page.ToString();

			url += BuildQuery( q );

			Log.Info( "sending query " + url );

			RequestInfo info = new RequestInfo();

			var task = new Sandbox.Internal.Http( new Uri( url ) );
			var req = task.GetStringAsync();
			req.ContinueWith( response => {
				if ( !response.IsCompleted || info.Cancel )
					return;

				Log.Info( "response was complete for " + url );
				var data = JsonSerializer.Deserialize<MapDetailResponse>( response.Result );
				callback( data.Maps );

				info.Complete = true;
			} );

			return info;
		}

		public static bool IsInWhitelist(string ext)
		{
			if ( ext == ".dat" ||
				ext == ".json" ||
				ext == ".ogg" ||
				ext == ".egg" ||
				ext == ".jpg" ||
				ext == ".jpeg" ||
				ext == ".bmp" ||
				ext == ".png"
				)
				return true;

			return false;
		}

		public static RequestInfo DownloadMap( MapDetail map )
		{
			string mapURL = map.Versions[0].DownloadURL;
			Log.Info( "Downloading map " + mapURL );

			RequestInfo info = new RequestInfo();

			var task = new Sandbox.Internal.Http( new Uri( mapURL ) );

			task.GetStreamAsync().ContinueWith(response => {
				if ( !response.IsCompleted || info.Cancel )
					return;

				Stream stream = response.Result;
				
				var fs = FileSystem.Data;

				if ( !fs.DirectoryExists( "download" ) )
					fs.CreateDirectory("download");

				var mapPath = map.DownloadDirectory;
				fs.CreateDirectory( mapPath );

				//using ( var zip = new ZipArchive( stream, ZipArchiveMode.Read ) )
				{
					//this is dumb. ZipArchive* should be whitelisted, not ZipArchive.*
					var zip = new ZipArchive( stream, ZipArchiveMode.Read ).Entries;

					//foreach(var entry in zip.Entries)
					for ( int i = 0; i < zip.Count; i++ )
					{
						//var entry = zip.Entries[i];
						var entry = zip[i];
						var ext = entry.Name.Substring( entry.Name.LastIndexOf( "." ) );

						if(!IsInWhitelist(ext))
						{
							Log.Warning("File extension not in whitelist: " + entry.FullName + " for song " + map.Name);
							continue;
						}

						using ( var data = entry.Open() )
						using ( var o = fs.OpenWrite( mapPath + "/" + entry.FullName ) )
						{
							data.CopyTo( o );
							info.Progress = (float)(i + 1) / zip.Count;
							Log.Info( "Wrote file " + mapPath + "/" + entry.FullName );
						}
					}
				}

				Log.Info( "Download complete." );

				// refresh our song list
				if ( Game.Current is not BeatSaberGame game )
					return;
				game.LoadSongs();

				info.Complete = true;
			} );

			return info;
		}

		[ClientCmd]
		public static void test_beatsaver()
		{
			//GetLatestMaps( LogMaps );
		}

		public static void LogMaps( MapDetail[] maps )
		{
			Log.Info( "Found " + maps.Length + " maps" );
			foreach ( var map in maps )
			{
				Log.Info( "\t" + map.Name );
			}
		}
	}
}
