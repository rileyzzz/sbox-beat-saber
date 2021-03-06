using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeatSaber
{
	public partial class BeatSaberEnvironment : Entity
	{
		// Config

		const float MetersToInches = 39.3701f;

		//size of a grid unit
		//public const float UnitSize = 23.0f;
		// according to the highly reputable beat saber discord notes are 0.6m and layers are 0.8m
		// that seems wack though so we're eyeballing it
		public const float UnitSize = 0.6f * MetersToInches;

		public const float VerticalOffset = UnitSize / 2.0f;

		// number of beats the user has to hit the note
		//const float NotePlayableWindow = 2.0f;
		// https://kivalevan.me/BeatSaber-MappingUtility/
		float HJD => Max(0.25f, CalcHJD() + Beatmap.NoteJumpStartBeatOffset);

		// amount of time between note 'incoming' and when it becomes playable
		//const float NoteIncomingWindow = 1.0f;
		// animation time (seconds) * BPS
		float NoteIncomingWindow => (20.0f / 10.0f) * BeatsPerSecond;

		// number of beats the note lingers
		//const float LateSliceWindow = 1.0f;
		float LateSliceWindow => HJD;

		// note speedup without sacrificing BPM
		// this is in m/s so convert to in/s
		public float NoteSpeed => Beatmap.NoteJumpSpeed * MetersToInches;
		public float NoteSpeedMeters => Beatmap.NoteJumpSpeed;
		//public float NoteSpeed => Beatmap.NoteJumpSpeed;


		// how far away should notes come from
		//const float IncomingNoteDistance = 400.0f;
		const float IncomingObstacleDistance = 1600.0f;

		// distance from origin to spawn note miss particles
		const float MissParticleDistance = 400.0f;

		// amount of notes we consider a 'streak'
		// if we're in a streak we'll keeps playing hit sounds
		// even if the notes aren't hit strictly before their sound is supposed to be played
		// it will reset the streak and start playing miss sounds once we have a definite miss (note leaves playable region or player hits the wrong side)
		// if this is too high players will notice the lack of hit sounds if they hit anything a bit late before they're in a streak
		// if it's too low players will start noticing the hit sounds even when they haven't hit notes
		// not sure this is exactly what the original game does, but it seems like a decent way of dealing with sound timing to keep things synchronized with the BPM
		const int HitTolerance = 4;

		// Helpers
		public static BeatSaberEnvironment Current = null;

		// Network data
		[Net] BeatSaberSong.Networked _netSong { get; set; }
		[Net] BeatSaberLevel.Networked _netLevel { get; set; }
		[Net] int Difficulty { get; set; } = 0;

		// Level data
		public BeatSaberSong Song => _netSong.Song;
		DifficultyBeatmap Beatmap => Song.DifficultyBeatmapSets[0].DifficultyBeatmaps[Difficulty];
		public BeatSaberLevel Level => _netLevel.Level;

		float BeatsPerSecond => Song.BPM / 60.0f;
		float BeatsElapsed => Stream.TimeElapsed * BeatsPerSecond;

		// (in/s) / (b/s) -> in/b
		public float DistPerBeat => NoteSpeed / BeatsPerSecond;
		public float DistPerBeatMeters => NoteSpeedMeters / BeatsPerSecond;

		MusicStream Stream;

		bool Playing = false;
		bool MapGenerated = false;

		// Map data
		List<Note> ActiveNotes = new();
		List<Obstacle> ActiveObstacles = new();

		const int numVisualizerBars = 256;
		VisualizerBar[] LeftBars;
		VisualizerBar[] RightBars;
		VisualizerGrid Grid;

		const int numSpotlights = 10;
		BPMSpotlight[] Spotlights;

		RingGroup Rings;

		List<AwesomePillar> Pillars = new();

		LaserCluster LeftLasers;
		LaserCluster RightLasers;

		WorldPanel InfoRoot;
		SongInfoPanel InfoPanel;

		WorldPanel ScoreRoot;
		ScorePanel ScorePanel;

		WorldPanel MultiplierRoot;
		MultiplierPanel MultiplierPanel;

		int BeatsPlayed = 0;

		int CurrentNote = 0;
		int CurrentObstacle = 0;
		int CurrentEvent = 0;

		int _combo = 0;
		public int Combo
		{
			get => _combo;
			set
			{
				_combo = value;
				if ( ScorePanel != null )
					ScorePanel.Combo.Text = _combo.ToString();
			}
		}

		int _score = 0;
		public int Score
		{
			get => _score;
			set
			{
				_score = value;
				if ( ScorePanel != null )
					ScorePanel.TargetScore = _score;
			}
		}

		int Multiplier = 1;
		int MultiplierFrac = 0;


		int ColorCycle = 0;
		Color[] CycleColors = new Color[] {
			Color.Red,
			Color.Blue,
			Color.Green,
			Color.Magenta
		};

		//float lastMissTime = 0.0f;
		int notesSinceLastMiss = HitTolerance;

		public BeatSaberEnvironment()
		{
			Transmit = TransmitType.Always;
		}

		public void Start( BeatSaberSong.Networked song, int difficulty, BeatSaberLevel.Networked level )
		{
			if ( !IsServer )
			{
				Log.Error( "Tried to start map on client" );
				return;
			}
			
			Current = this;

			_netSong = song;
			_netLevel = level;
			Difficulty = difficulty;

			// clear active stuff just in case
			foreach ( var note in ActiveNotes )
				note.Delete();
			ActiveNotes.Clear();

			foreach ( var obstacle in ActiveObstacles )
				obstacle.Delete();
			ActiveObstacles.Clear();

			//NoteData = new List<BeatSaberNote>(Level.Notes);

			Log.Info( Level.Notes.Length + " notes" );
			Log.Info( "Environment start server" );
			//StartClient();
			StartClient();
		}

		[ClientRpc]
		void StartClient()
		{
			Current = this;

			Local.Hud.Style.Display = Sandbox.UI.DisplayMode.None;

			//reset these clientside
			BeatsPlayed = 0;
			ColorCycle = 0;

			CurrentNote = 0;
			CurrentObstacle = 0;
			CurrentEvent = 0;

			Combo = 0;
			Score = 0;

			Log.Info( "playing " + Song.Directory + Song.SongFilename );
			Stream = new MusicStream( Song.Directory + Song.SongFilename );
			Stream.Play();

			Playing = true;
			GenerateMap();

			InfoPanel.SetSong( Song );
		}

		void SongFinished()
		{
			if ( !Playing )
				return;
			Playing = false;

			Log.Info( "Song finished." );

			foreach ( var note in ActiveNotes )
				note.Delete();
			ActiveNotes.Clear();

			foreach ( var obstacle in ActiveObstacles )
				obstacle.Delete();
			ActiveObstacles.Clear();

			Stream.Stop();
			Stream = null;

			Local.Hud.Style.Display = Sandbox.UI.DisplayMode.Flex;
		}

		void GenerateMap()
		{
			//clientside atm
			if ( !IsClient || MapGenerated )
				return;

			MapGenerated = true;

			InfoRoot = new WorldPanel();
			InfoRoot.Transform = new Transform( new Vector3(400.0f, 0.0f, 100.0f), Rotation.From( 25.0f, 180.0f, 0.0f ), 4.0f );

			InfoPanel = InfoRoot.AddChild<SongInfoPanel>( "infoPanel" );


			float panelWidth = 144.0f;
			Vector3 panelPos = new Vector3( 300.0f, 100.0f, 20.0f );
			Rotation panelRot = Rotation.From( 0.0f, -155.0f, 0.0f );

			ScoreRoot = new WorldPanel();
			ScoreRoot.Transform = new Transform( panelPos - panelRot.Right * (panelWidth / 2), panelRot, 4.0f );

			ScorePanel = ScoreRoot.AddChild<ScorePanel>( "scorePanel" );

			panelPos = new Vector3( 300.0f, -100.0f, 20.0f );
			panelRot = Rotation.From( 0.0f, 155.0f, 0.0f );

			MultiplierRoot = new WorldPanel();
			MultiplierRoot.Transform = new Transform( panelPos - panelRot.Right * (panelWidth / 2), panelRot, 4.0f );

			MultiplierPanel = MultiplierRoot.AddChild<MultiplierPanel>( "multiplierPanel" );

			LeftBars = new VisualizerBar[numVisualizerBars];
			RightBars = new VisualizerBar[numVisualizerBars];

			for ( int i = 0; i < numVisualizerBars; i++ )
			{
				LeftBars[i] = Create<VisualizerBar>();
				RightBars[i] = Create<VisualizerBar>();

				LeftBars[i].Position = new Vector3( i * UnitSize, 200.0f, 0.0f );
				RightBars[i].Position = new Vector3( i * UnitSize, -200.0f, 0.0f );

				LeftBars[i].Scale = 0.04f;
				RightBars[i].Scale = 0.04f;
			}

			Grid = new VisualizerGrid() { Position = new Vector3( 1200.0f, 0.0f, -800.0f ), Scale = 10.0f };

			Spotlights = new BPMSpotlight[numSpotlights];
			for ( int i = 0; i < numSpotlights; i++ )
			{
				int rowIndex = i % (numSpotlights / 2);
				bool left = i < numSpotlights / 2;

				BPMSpotlight light = new BPMSpotlight();
				light.Position = new Vector3( rowIndex * UnitSize * 4.0f + 200.0f, left ? 200.0f : -200.0f, 100.0f );
				light.Rotation = Rotation.From( 180.0f, left ? 90.0f : -90.0f, 0.0f );

				Spotlights[i] = light;
			}

			Rings = new RingGroup() { Position = new Vector3( 400.0f, 0.0f, 0.0f ) };

			LeftLasers = new LaserCluster() { Position = new Vector3( 2800.0f, 400.0f, -1400.0f ) };
			RightLasers = new LaserCluster() { Position = new Vector3( 2800.0f, -400.0f, -1400.0f ) };

			//foreach ( var data in NoteData )
			//foreach ( var data in Level.Notes )
			//{
			//	var ent = Create<Note>();
			//	ent.Data = data;

			//	ent.Position = new Vector3( data.Time * UnitSize, data.LineIndex * UnitSize - (3 * UnitSize / 2.0f), data.LineLayer * UnitSize + UnitSize / 2.0f );
			//	ClientNotes.Add(ent);
			//}

			Pillars.Clear();
			foreach ( var entity in Entity.All )
			{
				if ( entity is not AwesomePillar pillar )
					continue;

				Pillars.Add( pillar );
			}

			//DebugOverlay.Line( new Vector3( 0.0f, -100.0f, 0.0f ), new Vector3( 0.0f, 100.0f, 0.0f ), 100.0f );
			//DebugOverlay.Line( new Vector3( -100.0f, 0.0f, 0.0f ), new Vector3( 100.0f, 0.0f, 0.0f ), 100.0f );

			//DrawDebugGrid();

			//var test = new ModelEntity();
			//var mesh = new Mesh( Material.Load( "materials/obstacle.vmat" ) );

			//var vb = new VertexBuffer();
			//vb.Init( true );
			//vb.AddCube( new Vector3(), new Vector3( 50.0f, 50.0f, 50.0f ), new Rotation() );
			//mesh.CreateBuffers( vb );

			//test.SetModel( Model.Builder.AddMesh( mesh ).Create() );
		}

		void DrawDebugGrid()
		{
			float laneWidth = UnitSize * 4;

			for( int i = 0; i <= 4; i++ )
			{
				Vector3 lanePos = new Vector3( 0.0f, i * UnitSize - laneWidth / 2, VerticalOffset );
				DebugOverlay.Line( lanePos, lanePos.WithX(400.0f), Color.Green, 100.0f );
				DebugOverlay.Line( lanePos, lanePos.WithZ( 3 * UnitSize + VerticalOffset ), Color.Green, 100.0f );
			}

			for(int i = 0; i <= 3; i++ )
			{
				Vector3 rowPos = new Vector3( 0.0f, -laneWidth / 2, i * UnitSize + VerticalOffset );
				DebugOverlay.Line( rowPos, rowPos.WithY( laneWidth / 2 ), Color.Green, 100.0f );
			}
		}

		void UpdateVisualizerRow(float[] fft_data, VisualizerBar[] bars)
		{
			int groupSize = (fft_data.Length / 2) / numVisualizerBars;

			const int FFToffset = 0;

			for ( int i = 0; i < numVisualizerBars; i++ )
			{
				int group = (i + FFToffset) * groupSize;

				float avgValue = 0.0f;
				for ( int j = 0; j < groupSize; j++ )
					avgValue += fft_data[group + j];
				avgValue /= groupSize;

				//float maxValue = 0.0f;
				//for ( int j = 0; j < groupSize; j++ )
				//	maxValue = Math.Max( maxValue, fft_data[group + j] );

				bars[i].Position = bars[i].Position.WithZ( Math.Abs( avgValue ) * 1.0f + 15.0f );
			}
		}

		float Lerp( float a, float b, float f )
		{
			return a + f * (b - a);
		}

		float GetObjectTimeOffset( BeatSaberObject obj, bool animateIncoming )
		{
			float objTime = obj.Time - BeatsElapsed;

			// if we're in the playable window or later, set position as normal
			if ( objTime <= HJD )
			{
				//float targetPosition = objTime * UnitSize * NoteSpeed;
				return objTime * DistPerBeat;
			}
			else if ( animateIncoming )
			{
				float incomingTime = 1.0f - (objTime - HJD);
				return Lerp( IncomingObstacleDistance, HJD * DistPerBeat, incomingTime );
			}

			//return NotePlayableWindow * UnitSize * NoteSpeed;
			return HJD * DistPerBeat;
		}

		Vector3 GetNotePosition( BeatSaberNote note )
		{
			return new Vector3( GetObjectTimeOffset(note, false), -(note.LineIndex * UnitSize - (3 * UnitSize / 2.0f)), note.LineLayer * UnitSize + UnitSize / 2.0f + VerticalOffset );
		}

		Vector3 GetObstaclePosition( BeatSaberObstacle obstacle )
		{
			return new Vector3( GetObjectTimeOffset( obstacle, true ), (2 - obstacle.LineIndex) * UnitSize, 0.0f );
		}

		void ResetMultiplier()
		{
			Multiplier = 1;
			MultiplierFrac = 0;
			MultiplierPanel.SetMultiplier( Multiplier, MultiplierFrac );
		}

		void BumpMultiplier()
		{
			if ( Multiplier >= 8 )
				return;

			if ( ++MultiplierFrac > 3 )
			{
				MultiplierFrac = 0;
				Multiplier *= 2;
			}

			MultiplierPanel.SetMultiplier( Multiplier, MultiplierFrac );
		}

		public void NoteHit( int score )
		{
			//lastHitTime = Time.Now;
			notesSinceLastMiss++;

			BumpMultiplier();
			Combo++;
			Score += score * Multiplier;
		}

		public void NoteMiss( Note note )
		{
			notesSinceLastMiss = 0;

			Combo = 0;
			ResetMultiplier();

			Particles.Create( "particles/note_miss.vpcf", note.Position.WithX( MissParticleDistance ) );
		}

		public void BombHit()
		{
			Combo = 0;
			ResetMultiplier();
		}

		public void ObstacleHit()
		{
			Combo = 0;
			ResetMultiplier();
		}

		[Event.Tick]
		void Tick()
		{
			if ( !IsClient || !Playing )
				return;

			InfoPanel.Progress.Progress = Stream.FractionElapsed;

			if ( Stream.Finished )
			{
				SongFinished();
				return;
			}

			//float timeSinceLastHit = Time.Now - lastHitTime;
			//float timeSinceLastMiss = Time.Now - lastMissTime;


			if( (int)BeatsElapsed > BeatsPlayed )
			{
				BeatsPlayed = (int)BeatsElapsed;

				//BPM pulse
				foreach ( var light in Spotlights )
					light.Pulse();

				if ( ++ColorCycle >= CycleColors.Length )
					ColorCycle = 0;

				LeftLasers.Pulse();
				RightLasers.Pulse();

			}

			while( CurrentEvent < Level.Events.Length && Level.Events[CurrentEvent].Time <= BeatsElapsed )
			{
				//Log.Info( "playing event " + (CurrentEvent + 1) + "/" + Level.Events.Length );
				var evt = Level.Events[CurrentEvent++];

				switch ( evt.Type )
				{
					case EventType.LeftRotatingLasers: LeftLasers.Event( (LightEventType)evt.Value ); break;
					case EventType.RightRotatingLasers: RightLasers.Event( (LightEventType)evt.Value ); break;
					case EventType.LeftRotatingLaserSpeed: LeftLasers.RotationSpeedMultiplier = evt.Value; break;
					case EventType.RightRotatingLaserSpeed: RightLasers.RotationSpeedMultiplier = evt.Value; break;
					case EventType.RingSpin: Rings.Spin(); break;
					case EventType.RingZoom: Rings.Zoom(); break;
					default: break;
				}
			}

			while ( CurrentNote < Level.Notes.Length && Level.Notes[CurrentNote].Time - BeatsElapsed <= HJD + NoteIncomingWindow )
			{
				var note = Level.Notes[CurrentNote++];

				var ent = Create<Note>();
				ent.Data = note;

				ent.Position = GetNotePosition( note );
				ActiveNotes.Add( ent );
			}

			while ( CurrentObstacle < Level.Obstacles.Length && Level.Obstacles[CurrentObstacle].Time - BeatsElapsed <= HJD + NoteIncomingWindow )
			{
				var obstacle = Level.Obstacles[CurrentObstacle++];

				var ent = Create<Obstacle>();
				ent.Data = obstacle;

				Log.Info( "creating obstacle with width " + obstacle.Width + " duration " + obstacle.Duration + " lineindex " + obstacle.LineIndex );
				ent.Position = GetObstaclePosition( obstacle );
				ActiveObstacles.Add( ent );
			}

			//Time.Delta
			//float moveSpeed
			//foreach (var note in ClientNotes)
			//{
			//	note.Position += new Vector3( Time.Delta * beatsPerSecond * UnitSize, 0.0f, 0.0f );
			//}

			bool newHitThisTick = false;
			bool newMissThisTick = false;
			bool newExplosionThisTick = false;

			//stale notes that need to be removed
			List<Note> RemoveNotes = new();
			List<Obstacle> RemoveObstacles = new();

			foreach ( var note in ActiveNotes )
			{
				var data = note.Data;
				float noteTime = data.Time - BeatsElapsed;

				// if we're in the playable window or later, set position as normal
				note.Position = note.Position.WithX( GetObjectTimeOffset( data, false ) );

				if ( noteTime <= 0 && !note.SoundPlayed )
				{
					note.SoundPlayed = true;

					if( data.Type != NoteType.Bomb )
					{
						if ( note.Hit || notesSinceLastMiss >= HitTolerance )
							newHitThisTick = true;
						else
							newMissThisTick = true;
					}
					else
						newExplosionThisTick = true;
				}

				if( noteTime <= -LateSliceWindow)
				{
					// TODO add a small miss marker
					// don't play a sound effect because that will be off time
					if( data.Type != NoteType.Bomb && !note.Hit )
					{
						// if it's still not hit count this as a miss
						NoteMiss( note );
					}

					RemoveNotes.Add( note );
				}
			}

			foreach(var note in RemoveNotes)
			{
				note.Delete();
				ActiveNotes.Remove( note );
			}

			if (newHitThisTick)
				PlaySound("note_hit");

			foreach( var obstacle in ActiveObstacles )
			{
				var data = obstacle.Data;
				float obstacleTime = data.Time - BeatsElapsed;

				obstacle.Position = obstacle.Position.WithX( GetObjectTimeOffset( data, true ) );

				// Local.Client.IsUsingVr is false on clients, even when you're in VR!
				// Wow!

				// we cant do this :)
				// obstacle.WorldSpaceBounds.Contains( Input.VR.Head.Position )
				// could just construct a new BBox but that seems goofy
				if ( Input.VR.Head.Position.x >= obstacle.WorldSpaceBounds.Mins.x &&
					Input.VR.Head.Position.y >= obstacle.WorldSpaceBounds.Mins.y &&
					Input.VR.Head.Position.z >= obstacle.WorldSpaceBounds.Mins.z &&
					Input.VR.Head.Position.x < obstacle.WorldSpaceBounds.Maxs.x &&
					Input.VR.Head.Position.y < obstacle.WorldSpaceBounds.Maxs.y &&
					Input.VR.Head.Position.z < obstacle.WorldSpaceBounds.Maxs.z
					)
					ObstacleHit();

				//DebugOverlay.Line( Input.VR.Head.Position, Input.VR.Head.Position + new Vector3(40.0f, 0.0f, 0.0f) );
				//DebugOverlay.Box( obstacle.WorldSpaceBounds.Mins, obstacle.WorldSpaceBounds.Maxs );

				if ( obstacleTime <= -data.Duration - LateSliceWindow )
					RemoveObstacles.Add( obstacle );
			}

			foreach( var obstacle in RemoveObstacles )
			{
				obstacle.Delete();
				ActiveObstacles.Remove( obstacle );
			}

			//we should always have a left window
			var left_window = Stream.GetVisualizerWindow(0);
			var right_window = Stream.GetVisualizerWindow(1);
			
			if ( left_window != null )
				UpdateVisualizerRow( left_window?.GetFFTData1D(), LeftBars );

			if ( right_window != null )
				UpdateVisualizerRow( right_window?.GetFFTData1D(), RightBars );
			
			if(left_window != null)
			{
				((MusicStream.VisualizerWindow)left_window).GetFFTData2D( out float[] x, out float[] y );

				Grid.UpdateTexture( x, y );

				float peak = 0.0f;
				foreach ( var freq in left_window?.Data )
					peak = Math.Max( peak, Math.Abs( freq ) );


				peak *= 2.0f;
				foreach ( var pillar in Pillars )
					pillar.RenderColor = (CycleColors[ColorCycle] * peak).WithAlpha( 1.0f );
			}
		}

		int CalcHJD()
		{
			if ( DistPerBeatMeters > 9 ) return 1;
			if ( DistPerBeatMeters > 4.5 ) return 2;
			return 4;
		}

		float Max(float a, float b)
		{
			return a > b ? a : b;
		}
	}
}
