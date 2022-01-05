using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeatSaber
{
	//public partial class TestData : BaseNetworkable
	//{

	//	[Net]
	//	[JsonPropertyName( "_difficultyBeatmapSets" )]
	//	public IList<TestListItem> data { get; set; } = new List<TestListItem>();
	//}

	//public partial class TestListItem : BaseNetworkable
	//{
	//	[Net]
	//	public string name { get; set; } = "cool list item";
	//}

	public partial class BeatSaberGame : Game
	{
		[Net] public BeatSaberEnvironment Environment { get; set; }

		[Net] public IList<BeatSaberSong.Networked> Songs { get; set; } = new List<BeatSaberSong.Networked>();

		[Net] public BeatSaberSong.Networked TestSong { get; set; }


		//[Net] public IList<TestData> CoolData { get; set; } = new List<TestData>();

		//public List<BeatSaberPlayer> Players = new();

		public BeatSaberGame()
		{
			if (IsServer)
			{
				//new MinimalHudEntity();
				new BeatSaberHUDEntity();

				Environment = Create<BeatSaberEnvironment>();

				//var container = new TestData();
				//container.data.Add( new TestListItem() );
				//container.data.Add( new TestListItem() );
				//container.data.Add( new TestListItem() );
				//CoolData.Add( container );
				LoadSongs();
			}

			Global.PhysicsSubSteps = 8;
		}

		//[ServerCmd]
		//public static void test_networking_server()
		//{
		//	if ( Game.Current is not BeatSaberGame game )
		//		return;

		//	Log.Info( "CoolData has " + game.CoolData.Count + " items" );
		//}

		//[ClientCmd]
		//public static void test_networking()
		//{
		//	if ( Game.Current is not BeatSaberGame game )
		//		return;

		//	Log.Info( "CoolData has " + game.CoolData.Count + " items" );
		//}

		public SongBrowser GetBrowser()
		{
			foreach ( var child in Local.Hud.Children )
			{
				if ( child is SongBrowser browser )
					return browser;
			}
			return null;
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();

			//LoadSongs();
		}

		[ClientRpc]
		public void RefreshBrowser()
		{
			Log.Info( "songs changed" );
			if ( !IsClient )
				return;

			GetBrowser().Update();
		}


		public void LoadSongs()
		{
			Songs = new List<BeatSaberSong.Networked>();
			
			var fs = FileSystem.Data;

			if ( !fs.DirectoryExists( "levels" ) )
				fs.CreateDirectory("levels");

			foreach ( var dir in fs.FindDirectory( "levels" ) )
			{
				string songDir = "levels/" + dir + "/";
				string infoPath = songDir + "info.dat";
				if ( !fs.FileExists( infoPath ) )
					continue;

				var song = fs.ReadJson<BeatSaberSong>( infoPath );

				song.Directory = songDir;
				Log.Info("Loaded " + song.SongName + " " + song.DifficultyBeatmapSets.Length + " sets");
				//song.WriteNetworkData();

				var netData = song.GetNetworked();
				netData.WriteNetworkData();
				Songs.Add( netData );
			}

			//TestSong = Songs[0];

			////TestSong = new BeatSaberSong();
			//TestSong.Song.DifficultyBeatmapSets.Add(new DifficultyBeatmapSet());
			//TestSong.Song.DifficultyBeatmapSets.Add(new DifficultyBeatmapSet());
			//TestSong.WriteNetworkData();

			//var test = fs.ReadJson<TestData>("testdata.json");
			//var test = new TestData();
			//test.data.Add(new TestListItem());
			//test.data.Add(new TestListItem());
			//test.data.Add(new TestListItem());
			//fs.WriteJson("testdata2.json", test);

			RefreshBrowser();
		}

		BeatSaberSong.Networked FindSong(string name)
		{
			foreach(var song in Songs)
			{
				if ( song.Song.SongName == name )
					return song;
			}
			return null;
		}

		[ServerCmd]
		public static void PlaySong( string song, int difficulty )
		{
			if ( Game.Current is not BeatSaberGame game )
				return;

			var Song = game.FindSong( song );
			if ( Song == null )
				return;


			//Local.Hud.Style.Display = Sandbox.UI.DisplayMode.None;

			var fs = FileSystem.Data;

			//Log.Info( "num sets " + Song.Song.DifficultyBeatmapSets.Length );
			//Log.Info( "difficulty set " + Song.Song.DifficultyBeatmapSets[0].CharacteristicName + " " + Song.Song.DifficultyBeatmapSets[0].DifficultyBeatmaps.Count );

			var difficultyInfo = Song.Song.DifficultyBeatmapSets[0].DifficultyBeatmaps[difficulty];
			var Level = fs.ReadJson<BeatSaberLevel>(Song.Song.Directory + difficultyInfo.BeatmapFilename);

			//process level, make sure stuff is ordered
			Array.Sort(Level.Notes, (a, b) => { return a.Time.CompareTo(b.Time); });
			Array.Sort(Level.Obstacles, (a, b) => { return a.Time.CompareTo(b.Time); });
			Array.Sort(Level.Events, (a, b) => { return a.Time.CompareTo(b.Time); });

			var netLevel = Level.GetNetworked();
			netLevel.WriteNetworkData();

			game.Environment.Start( Song, difficulty, netLevel );
		}

		public override void ClientJoined(Client client)
		{
			base.ClientJoined(client);

			var player = new BeatSaberPlayer();
			client.Pawn = player;

			player.Respawn();

			RefreshBrowser( To.Single(client) );
			//Players.Add(player);
		}

		public override void ClientDisconnect(Client cl, NetworkDisconnectionReason reason)
		{
			base.ClientDisconnect(cl, reason);

		}

		public override void Simulate(Client cl)
		{
			base.Simulate(cl);
		}

		[ServerCmd]
		public static void start_song()
		{
			if ( Game.Current is not BeatSaberGame game )
				return;

			Log.Info( "starting song" );
			//game.PlaySong( game.Songs[7], 0 );
		}


		[ServerCmd]
		public static void server_debug_songs()
		{
			if ( Game.Current is not BeatSaberGame game )
				return;

			Log.Info( "songs:" );
			//foreach ( var song in game.Songs )
			//	Log.Info( "\t" + song.SongName + " (" + song.DifficultyBeatmapSets.Count + " sets)" );

			Log.Info( "test song " + game.TestSong.Song.SongName + " (" + game.TestSong.Song.DifficultyBeatmapSets.Length + " sets)" );
		}

		[ClientCmd]
		public static void debug_songs()
		{
			if ( Game.Current is not BeatSaberGame game )
				return;

			Log.Info("songs:");
			//foreach ( var song in game.Songs )
			//	Log.Info( "\t" + song.SongName + " (" + song.DifficultyBeatmapSets.Count + " sets)" );

			Log.Info( "test song " + game.TestSong.Song.SongName + " (" + game.TestSong.Song.DifficultyBeatmapSets.Length + " sets)" );
		}

		//[ServerCmd]
		//public static void extend_both()
		//{
		//	if ( Game.Current is not BeatSaberGame game )
		//		return;

		//	if ( !game.IsServer )
		//		return;

		//	Log.Info( "extending both" );

		//	foreach ( var player in game.Players)
		//	{

		//		player.LeftHand.Saber.Extend = true;
		//		player.RightHand.Saber.Extend = true;
		//	}

		//}
	}
}
