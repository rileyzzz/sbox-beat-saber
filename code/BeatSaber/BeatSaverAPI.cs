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

		public static void GetLatestMaps( out Sandbox.Internal.Http task, Action<MapDetail[]> callback )
		{
			task = new Sandbox.Internal.Http( new Uri( "https://beatsaver.com/api/maps/latest" ) );

			var req = task.GetStringAsync();
			req.ContinueWith(response => {
				if ( !response.IsCompleted )
					return;

				//Log.Info( "response " + req.Result );
				var data = JsonSerializer.Deserialize<MapDetailResponse>( response.Result );
				callback( data.Maps );
			} );
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

		public static void SearchMaps( out Sandbox.Internal.Http task, Action<MapDetail[]> callback, int page = 0, string query = "" )
		{
			string url = "https://beatsaver.com/api/search/text/" + page;

			Dictionary<string, string> q = new();

			if ( query != "" )
				q["q"] = query;

			url += BuildQuery( q );

			Log.Info( "sending query " + url );
			task = new Sandbox.Internal.Http( new Uri( url ) );
			var req = task.GetStringAsync();
			req.ContinueWith( response => {
				Log.Info("response");
				if ( !response.IsCompleted )
					return;
				Log.Info( "response was complete for " + url );
				var data = JsonSerializer.Deserialize<MapDetailResponse>( response.Result );
				callback( data.Maps );
			} );
		}

		public static void DownloadMap( out Sandbox.Internal.Http task, MapDetail map )
		{
			string mapURL = map.Versions[0].DownloadURL;
			Log.Info( "Downloading map " + mapURL );

			task = new Sandbox.Internal.Http( new Uri( mapURL ) );
			task.GetStreamAsync().ContinueWith(response => {
				if ( !response.IsCompleted )
					return;

				Stream stream = response.Result;
				
				var fs = FileSystem.Data;

				if ( !fs.DirectoryExists( "download" ) )
					fs.CreateDirectory("download");

				string mapPath = "download/" + map.ID;

				fs.CreateDirectory( mapPath );

				using ( var zip = new ZipArchive(stream, ZipArchiveMode.Read) )
				{
					foreach(var entry in zip.Entries)
					{
						using ( var data = entry.Open())
						using ( var o = fs.OpenWrite( mapPath + "/" + entry.FullName ) )
						{
							data.CopyTo( o );
							Log.Info("Wrote file " + mapPath + "/" + entry.FullName );
						}
					}
				}

				Log.Info( "Download complete." );

				// refresh our song list
				if ( Game.Current is not BeatSaberGame game )
					return;
				game.LoadSongs();

			} );
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
