using Sandbox;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeatSaber
{
	//public partial class NetworkContainer<T> : BaseNetworkable
	//{
	//	public T Data;

	//	public NetworkContainer()
	//	{
	//	}

	//	public static implicit operator T(NetworkContainer<T> container) => container.Data;
	//}

	public enum LevelDifficulty
	{
		Easy,
		Normal,
		Hard,
		Expert,
		ExpertPlus
	}

	public class BeatSaberSong// : BaseNetworkable
	{
		//[JsonIgnore]
		//public int Index { get; set; }
		//[Net]
		[JsonIgnore]
		public string Directory { get; set; }

		//[Net]
		[JsonPropertyName( "_version" )]
		public string Version { get; set; }

		//[Net]
		[JsonPropertyName( "_songName" )]
		public string SongName { get; set; }

		//[Net]
		[JsonPropertyName( "_songSubName" )]
		public string SongSubName { get; set; }
		
		//[Net]
		[JsonPropertyName( "_songAuthorName" )]
		public string SongAuthorName { get; set; }

		//[Net]
		[JsonPropertyName( "_levelAuthorName" )]
		public string LevelAuthorName { get; set; }

		//[Net]
		[JsonPropertyName( "_beatsPerMinute" )]
		public float BPM { get; set; }

		//[Net]
		[JsonPropertyName( "_shuffle" )]
		public float Shuffle { get; set; }

		//[Net]
		[JsonPropertyName( "_shufflePeriod" )]
		public float ShufflePeriod { get; set; }

		//[Net]
		[JsonPropertyName( "_previewStartTime" )]
		public float PreviewStartTime { get; set; }

		//[Net]
		[JsonPropertyName( "_previewDuration" )]
		public float PreviewDuration { get; set; }

		//[Net]
		[JsonPropertyName( "_songFilename" )]
		public string SongFilename { get; set; }

		//[Net]
		[JsonPropertyName( "_coverImageFilename" )]
		public string CoverImageFilename { get; set; }

		//[Net]
		[JsonPropertyName( "_environmentName" )]
		public string EnvironmentName { get; set; }

		//[Net]
		[JsonPropertyName( "_allDirectionsEnvironmentName" )]
		public string AllDirectionsEnvironmentName { get; set; }

		//[Net]
		[JsonPropertyName( "_songTimeOffset" )]
		public float SongTimeOffset { get; set; }

		//[Net]
		[JsonPropertyName( "_difficultyBeatmapSets" )]
		public DifficultyBeatmapSet[] DifficultyBeatmapSets { get; set; }

		//[Net]
		[JsonExtensionData]
		public Dictionary<string, JsonElement> ExtensionData { get; set; }

		public Networked GetNetworked()
		{
			return new Networked( this );
		}

		public class Networked : BaseNetworkable, INetworkSerializer
		{
			public BeatSaberSong Song = new();

			public static implicit operator BeatSaberSong(Networked networked) => networked.Song;

			public Networked()
			{
			}

			public Networked( BeatSaberSong song )
			{
				Song = song;
			}

			void INetworkSerializer.Write( NetWrite write )
			{
				write.Write( Song.Directory );
				write.Write( Song.Version );
				write.Write( Song.SongName );
				write.Write( Song.SongSubName );
				write.Write( Song.SongAuthorName );
				write.Write( Song.LevelAuthorName );
				write.Write( Song.BPM );
				write.Write( Song.Shuffle );
				write.Write( Song.ShufflePeriod );
				write.Write( Song.PreviewStartTime );
				write.Write( Song.PreviewDuration );
				write.Write( Song.SongFilename );
				write.Write( Song.CoverImageFilename );
				write.Write( Song.EnvironmentName );
				write.Write( Song.AllDirectionsEnvironmentName );
				write.Write( Song.SongTimeOffset );

				write.Write( Song.DifficultyBeatmapSets.Length );
				foreach ( var set in Song.DifficultyBeatmapSets )
					set.Write( write );
			}

			void INetworkSerializer.Read( ref NetRead read )
			{
				Song.Directory = read.ReadString();
				Song.Version = read.ReadString();
				Song.SongName = read.ReadString();
				Song.SongSubName = read.ReadString();
				Song.SongAuthorName = read.ReadString();
				Song.LevelAuthorName = read.ReadString();
				Song.BPM = read.Read<float>();
				Song.Shuffle = read.Read<float>();
				Song.ShufflePeriod = read.Read<float>();
				Song.PreviewStartTime = read.Read<float>();
				Song.PreviewDuration = read.Read<float>();
				Song.SongFilename = read.ReadString();
				Song.CoverImageFilename = read.ReadString();
				Song.EnvironmentName = read.ReadString();
				Song.AllDirectionsEnvironmentName = read.ReadString();
				Song.SongTimeOffset = read.Read<float>();

				//Log.Info( "reading data " +
				//		Song.Directory + "\n" +
				//		Song.Version + "\n" +
				//		Song.SongName + "\n" +
				//		Song.SongSubName + "\n" +
				//		Song.SongAuthorName + "\n" +
				//		Song.LevelAuthorName + "\n" +
				//		Song.BPM + "\n" +
				//		Song.Shuffle + "\n" +
				//		Song.ShufflePeriod + "\n" +
				//		Song.PreviewStartTime + "\n" +
				//		Song.PreviewDuration + "\n" +
				//		Song.SongFilename + "\n" +
				//		Song.CoverImageFilename + "\n" +
				//		Song.EnvironmentName + "\n" +
				//		Song.AllDirectionsEnvironmentName + "\n" +
				//		Song.SongTimeOffset
				//	);

				Song.DifficultyBeatmapSets = new DifficultyBeatmapSet[ read.Read<int>() ];
				for ( int i = 0; i < Song.DifficultyBeatmapSets.Length; i++ )
					Song.DifficultyBeatmapSets[i] = DifficultyBeatmapSet.Read( ref read );
			}
		}
	}

	public class DifficultyBeatmapSet// : BaseNetworkable
	{
		//[Net]
		[JsonPropertyName( "_beatmapCharacteristicName" )]
		public string CharacteristicName { get; set; }

		//[Net]
		[JsonPropertyName( "_difficultyBeatmaps" )]
		public DifficultyBeatmap[] DifficultyBeatmaps { get; set; }

		//[Net]
		[JsonExtensionData]
		public Dictionary<string, JsonElement> ExtensionData { get; set; }

		public void Write( NetWrite write )
		{
			write.Write( CharacteristicName );
			write.Write( DifficultyBeatmaps.Length );
			foreach ( var beatmap in DifficultyBeatmaps )
				beatmap.Write( write );
		}

		public static DifficultyBeatmapSet Read( ref NetRead read )
		{
			DifficultyBeatmapSet set = new();
			set.CharacteristicName = read.ReadString();

			set.DifficultyBeatmaps = new DifficultyBeatmap[read.Read<int>()];
			for ( int i = 0; i < set.DifficultyBeatmaps.Length; i++ )
				set.DifficultyBeatmaps[i] = DifficultyBeatmap.Read( ref read );

			return set;
		}
	}

	public class DifficultyBeatmap// : BaseNetworkable
	{
		//[Net]
		[JsonPropertyName( "_difficulty" )]
		public string difficultyStr { get; set; }

		//[JsonIgnore]
		public LevelDifficulty Difficulty => Enum.Parse<LevelDifficulty>( difficultyStr );

		//[Net]
		[JsonPropertyName( "_difficultyRank" )]
		public int DifficultyRank { get; set; }

		//[Net]
		[JsonPropertyName( "_beatmapFilename" )]
		public string BeatmapFilename { get; set; }

		//[Net]
		[JsonPropertyName( "_noteJumpMovementSpeed" )]
		public float NoteJumpSpeed { get; set; }

		//[Net]
		[JsonPropertyName( "_noteJumpStartBeatOffset" )]
		public float NoteJumpStartBeatOffset { get; set; }

		//[Net]
		[JsonExtensionData]
		public Dictionary<string, JsonElement> ExtensionData { get; set; }

		public void Write( NetWrite write )
		{
			write.Write( difficultyStr );
			write.Write( DifficultyRank );
			write.Write( BeatmapFilename );
			write.Write( NoteJumpSpeed );
			write.Write( NoteJumpStartBeatOffset );
		}

		public static DifficultyBeatmap Read( ref NetRead read )
		{
			DifficultyBeatmap beatmap = new();
			beatmap.difficultyStr = read.ReadString();
			beatmap.DifficultyRank = read.Read<int>();
			beatmap.BeatmapFilename = read.ReadString();
			beatmap.NoteJumpSpeed = read.Read<float>();
			beatmap.NoteJumpStartBeatOffset = read.Read<float>();

			return beatmap;
		}
	}

	public class BeatSaberLevel// : BaseNetworkable
	{
		//[Net]
		[JsonPropertyName( "_version" )]
		public string Version { get; set; }

		//[Net]
		[JsonPropertyName( "_notes" )]
		public BeatSaberNote[] Notes { get; set; }

		//[Net]
		[JsonPropertyName( "_obstacles" )]
		public BeatSaberObstacle[] Obstacles { get; set; }

		//[Net]
		[JsonPropertyName( "_events" )]
		public BeatSaberEvent[] Events { get; set; }

		//[Net]
		[JsonExtensionData]
		public Dictionary<string, JsonElement> ExtensionData { get; set; }

		public Networked GetNetworked()
		{
			return new Networked( this );
		}

		public class Networked : BaseNetworkable, INetworkSerializer
		{
			public BeatSaberLevel Level = new();

			public static implicit operator BeatSaberLevel( Networked networked ) => networked.Level;

			public Networked()
			{
			}

			public Networked( BeatSaberLevel level )
			{
				Level = level;
			}

			void INetworkSerializer.Write( NetWrite write )
			{
				write.Write( Level.Version );

				write.Write( Level.Notes.Length );
				foreach ( var note in Level.Notes )
					note.Write( write );

				write.Write( Level.Obstacles.Length );
				foreach ( var obstacle in Level.Obstacles )
					obstacle.Write( write );

				write.Write( Level.Events.Length );
				foreach ( var evt in Level.Events )
					evt.Write( write );
			}

			void INetworkSerializer.Read( ref NetRead read )
			{
				Level.Version = read.ReadString();

				Level.Notes = new BeatSaberNote[ read.Read<int>() ];
				for ( int i = 0; i < Level.Notes.Length; i++ )
					Level.Notes[i] = BeatSaberNote.Read( ref read );

				Level.Obstacles = new BeatSaberObstacle[ read.Read<int>() ];
				for ( int i = 0; i < Level.Obstacles.Length; i++ )
					Level.Obstacles[i] = BeatSaberObstacle.Read( ref read );

				Level.Events = new BeatSaberEvent[ read.Read<int>() ];
				for ( int i = 0; i < Level.Obstacles.Length; i++ )
					Level.Events[i] = BeatSaberEvent.Read( ref read );
			}
		}
	}

	public enum NoteType
	{
		Red,
		Blue,
		Unused,
		Bomb
	}

	public enum CutDirection
	{
		Up,
		Down,
		Left,
		Right,
		UpLeft,
		UpRight,
		DownLeft,
		DownRight,
		Any
	}

	public interface BeatSaberObject
	{
		public float Time { get; set; }
	}

	public class BeatSaberNote : /*BaseNetworkable,*/ BeatSaberObject
	{
		//[Net]
		[JsonPropertyName( "_time" )]
		public float Time { get; set; }

		//[Net]
		[JsonPropertyName( "_lineIndex" )]
		public int LineIndex { get; set; }

		//[Net]
		[JsonPropertyName( "_lineLayer" )]
		public int LineLayer { get; set; }

		//[Net]
		[JsonPropertyName( "_type" )]
		public NoteType Type { get; set; }

		//[Net]
		[JsonPropertyName( "_cutDirection" )]
		public CutDirection Direction { get; set; }

		//[Net]
		[JsonExtensionData]
		public Dictionary<string, JsonElement> ExtensionData { get; set; }

		public void Write( NetWrite write )
		{
			write.Write( Time );
			write.Write( LineIndex );
			write.Write( LineLayer );
			write.Write( Type );
			write.Write( Direction );
		}

		public static BeatSaberNote Read( ref NetRead read )
		{
			BeatSaberNote Note = new();

			Note.Time = read.Read<float>();
			Note.LineIndex = read.Read<int>();
			Note.LineLayer = read.Read<int>();
			Note.Type = read.Read<NoteType>();
			Note.Direction = read.Read<CutDirection>();

			return Note;
		}
	}

	public enum ObstacleType
	{
		FullHeight,
		Crouch
	}

	public class BeatSaberObstacle : /*BaseNetworkable,*/ BeatSaberObject
	{
		//[Net]
		[JsonPropertyName( "_time" )]
		public float Time { get; set; }

		//[Net]
		[JsonPropertyName( "_lineIndex" )]
		public int LineIndex { get; set; }

		//[Net]
		[JsonPropertyName( "_type" )]
		public ObstacleType Type { get; set; }

		//[Net]
		[JsonPropertyName( "_duration" )]
		public float Duration { get; set; }

		//[Net]
		[JsonPropertyName( "_width" )]
		public int Width { get; set; }

		//[Net]
		[JsonExtensionData]
		public Dictionary<string, JsonElement> ExtensionData { get; set; }

		public void Write( NetWrite write )
		{
			write.Write( Time );
			write.Write( LineIndex );
			write.Write( Type );
			write.Write( Duration );
			write.Write( Width );
		}

		public static BeatSaberObstacle Read( ref NetRead read )
		{
			BeatSaberObstacle Obstacle = new();

			Obstacle.Time = read.Read<float>();
			Obstacle.LineIndex = read.Read<int>();
			Obstacle.Type = read.Read<ObstacleType>();
			Obstacle.Duration = read.Read<float>();
			Obstacle.Width = read.Read<int>();

			return Obstacle;
		}
	}

	public enum EventType
	{
		BackLasers,
		RingLights,
		LeftRotatingLasers,
		RightRotatingLasers,
		CenterLights,
		BoostLightColors,
		ExtraLeftLights,
		ExtraRightLights,
		RingSpin,
		RingZoom,
		ExtraLeftLasers,
		ExtraRightLasers,
		LeftRotatingLaserSpeed,
		RightRotatingLaserSpeed,
		EarlyRotation,
		LateRotation,
		Misc0,
		Misc1
	}

	public class BeatSaberEvent : /*BaseNetworkable,*/ BeatSaberObject
	{
		//[Net]
		[JsonPropertyName( "_time" )]
		public float Time { get; set; }

		//[Net]
		[JsonPropertyName( "_type" )]
		public EventType Type { get; set; }

		//[Net]
		[JsonPropertyName( "_value" )]
		public int Value { get; set; }

		//[Net]
		[JsonPropertyName("_floatValue")]
		public float FloatValue { get; set; }

		//[Net]
		[JsonExtensionData]
		public Dictionary<string, JsonElement> ExtensionData { get; set; }

		public void Write( NetWrite write )
		{
			write.Write( Time );
			write.Write( Type );
			write.Write( Value );
			write.Write( FloatValue );
		}

		public static BeatSaberEvent Read( ref NetRead read )
		{
			BeatSaberEvent Event = new();

			Event.Time = read.Read<float>();
			Event.Type = read.Read<EventType>();
			Event.Value = read.Read<int>();
			Event.FloatValue = read.Read<float>();

			return Event;
		}
	}
}
