using Sandbox;
using Sandbox.UI;
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

		//[Net] public IList<BeatSaberSong.Networked> Songs { get; set; } = new List<BeatSaberSong.Networked>();
		public List<BeatSaberSong> LocalSongs { get; set; } = new();

		//[Net] public BeatSaberSong.Networked TestSong { get; set; }


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
				
			}

			if( IsClient )
			{
				LoadSongs();

				//var VRSongList = new WorldPanel();
				//VRSongList.StyleSheet.Load( "/ui/HUD.scss" );

				//VRSongList.Position = new Vector3( 0.0f, 0.0f, 30.0f );
				//VRSongList.Rotation = Rotation.FromYaw( 180.0f );

				//VRSongList.SceneObject.Flags.IsTranslucent = false;
				//VRSongList.SceneObject.Flags.IsOpaque = true;

				
				//VRSongList.Position = new Vector3( 0.0f, 0.0f, 30.0f );
				//VRSongList.Rotation = Rotation.FromYaw( 180.0f );

				//VRSongList.SceneObject.Flags.IsTranslucent = false;
				//VRSongList.SceneObject.Flags.IsOpaque = true;
				
				////VRSongList.WorldScale = 2.0f;

				//VRSongList.AddChild<SongBrowser>( "browser" );

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
			if ( Local.Hud == null )
				return null;

			foreach ( var child in Local.Hud.Children )
			{
				if ( child is BeatSaberHUDContainer container && container.CurrentTab is SongBrowser browser )
					return browser;
			}
			return null;
		}

		public override void PostLevelLoaded()
		{
			base.PostLevelLoaded();

			//if ( IsClient )
			//	LoadSongs();
		}

		public void RefreshBrowser()
		{
			Log.Info( "songs changed" );
			if ( !IsClient )
				return;

			var browser = GetBrowser();
			if(browser != null) browser.Update();
		}

		BeatSaberSong LoadSong( string songDir )
		{
			var fs = FileSystem.Data;

			string infoPath = songDir + "info.dat";
			if ( !fs.FileExists( infoPath ) )
				return null;

			var song = fs.ReadJson<BeatSaberSong>( infoPath );

			song.Directory = songDir;
			Log.Info( "Loaded " + song.SongName + " (" + song.DifficultyBeatmapSets.Length + " sets)" );

			return song;

			//song.WriteNetworkData();

			//var netData = song.GetNetworked();
			//netData.WriteNetworkData();
			//LocalSongs.Add( netData );
		}

		public void LoadSongs()
		{
			LocalSongs = new List<BeatSaberSong>();
			
			var fs = FileSystem.Data;

			if ( !fs.DirectoryExists( "levels" ) )
				fs.CreateDirectory("levels");

			if ( !fs.DirectoryExists( "download" ) )
				fs.CreateDirectory( "download" );

			foreach ( var dir in fs.FindDirectory( "levels" ) )
			{
				string songDir = "levels/" + dir + "/";
				var song = LoadSong( songDir );
				if(song != null) LocalSongs.Add( song );
			}

			foreach ( var dir in fs.FindDirectory( "download" ) )
			{
				string songDir = "download/" + dir + "/";
				var song = LoadSong( songDir );
				if ( song != null ) LocalSongs.Add( song );
			}

			RefreshBrowser();
		}

		[ServerCmd]
		public static void PlaySong( string songDir, int difficulty )
		{
			if ( Game.Current is not BeatSaberGame game )
				return;

			Client songOwner = ConsoleSystem.Caller;

			//only allow the host to play songs
			if ( !songOwner.IsListenServerHost )
				return;

			//load the song on the server and make sure it's networked to all clients
			var Song = game.LoadSong( songDir );
			var netData = Song.GetNetworked();
			netData.WriteNetworkData();

			//Local.Hud.Style.Display = Sandbox.UI.DisplayMode.None;

			var fs = FileSystem.Data;

			//Log.Info( "num sets " + Song.Song.DifficultyBeatmapSets.Length );
			//Log.Info( "difficulty set " + Song.Song.DifficultyBeatmapSets[0].CharacteristicName + " " + Song.Song.DifficultyBeatmapSets[0].DifficultyBeatmaps.Count );

			var difficultyInfo = Song.DifficultyBeatmapSets[0].DifficultyBeatmaps[difficulty];
			var Level = fs.ReadJson<BeatSaberLevel>(Song.Directory + difficultyInfo.BeatmapFilename);

			//process level, make sure stuff is ordered
			Array.Sort(Level.Notes, (a, b) => { return a.Time.CompareTo(b.Time); });
			Array.Sort(Level.Obstacles, (a, b) => { return a.Time.CompareTo(b.Time); });
			Array.Sort(Level.Events, (a, b) => { return a.Time.CompareTo(b.Time); });

			var netLevel = Level.GetNetworked();
			netLevel.WriteNetworkData();

			game.Environment.Start( netData, difficulty, netLevel );
		}

		public override void ClientJoined(Client client)
		{
			base.ClientJoined(client);

			var player = new BeatSaberPlayer();
			client.Pawn = player;

			player.Respawn();

			player.Position = new Vector3( -25.0f, 0.0f, 0.0f );
			//RefreshBrowser( To.Single(client) );
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

			//Log.Info( "songs:" );
			//foreach ( var song in game.Songs )
			//	Log.Info( "\t" + song.SongName + " (" + song.DifficultyBeatmapSets.Count + " sets)" );

			//Log.Info( "test song " + game.TestSong.Song.SongName + " (" + game.TestSong.Song.DifficultyBeatmapSets.Length + " sets)" );
		}

		[ClientCmd]
		public static void debug_songs()
		{
			if ( Game.Current is not BeatSaberGame game )
				return;

			//Log.Info("songs:");
			//foreach ( var song in game.Songs )
			//	Log.Info( "\t" + song.SongName + " (" + song.DifficultyBeatmapSets.Count + " sets)" );

			//Log.Info( "test song " + game.TestSong.Song.SongName + " (" + game.TestSong.Song.DifficultyBeatmapSets.Length + " sets)" );
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
