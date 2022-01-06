using Sandbox;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeatSaber
{
	public partial class BeatSaberEnvironment : Entity
	{
		//size of a grid unit
		public static float UnitSize = 25.0f;

		//Network data
		[Net] BeatSaberSong.Networked _netSong { get; set; }
		[Net] BeatSaberLevel.Networked _netLevel { get; set; }
		[Net] int Difficulty { get; set; } = 0;

		//Level data
		public BeatSaberSong Song => _netSong.Song;
		DifficultyBeatmap Beatmap => Song.DifficultyBeatmapSets[0].DifficultyBeatmaps[Difficulty];
		public BeatSaberLevel Level => _netLevel.Level;


		MusicStream Stream;

		bool Playing = false;

		List<Note> ActiveNotes = new();

		const int numVisualizerBars = 256;
		VisualizerBar[] LeftBars;
		VisualizerBar[] RightBars;
		VisualizerGrid Grid;

		const int numSpotlights = 10;
		BPMSpotlight[] Spotlights;

		List<AwesomePillar> Pillars = new();

		LaserCluster LeftLasers;
		LaserCluster RightLasers;

		int BeatsPlayed = 0;

		int CurrentNote = 0;
		int CurrentEvent = 0;

		int ColorCycle = 0;
		Color[] CycleColors = new Color[] {
			Color.Red,
			Color.Blue,
			Color.Green,
			Color.Magenta
		};

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

			_netSong = song;
			_netLevel = level;
			Difficulty = difficulty;

			//NoteData = new List<BeatSaberNote>(Level.Notes);

			Log.Info( Level.Notes.Length + " notes" );
			Log.Info("Environment start server");
			//StartClient();
			StartClient();
		}

		[ClientRpc]
		void StartClient()
		{
			Local.Hud.Style.Display = Sandbox.UI.DisplayMode.None;

			Log.Info("playing " + Song.Directory + Song.SongFilename );
			Stream = new MusicStream( Song.Directory + Song.SongFilename );
			Stream.Play();

			Playing = true;
			GenerateMap();
		}

		void SongFinished()
		{
			if ( !Playing )
				return;
			Playing = false;

			Local.Hud.Style.Display = Sandbox.UI.DisplayMode.Flex;
		}

		void GenerateMap()
		{
			//clientside atm
			if ( !IsClient )
				return;

			LeftBars = new VisualizerBar[numVisualizerBars];
			RightBars = new VisualizerBar[numVisualizerBars];

			for( int i = 0; i < numVisualizerBars; i++ )
			{
				LeftBars[i] = Create<VisualizerBar>();
				RightBars[i] = Create<VisualizerBar>();

				LeftBars[i].Position = new Vector3( i * UnitSize, 200.0f, 0.0f );
				RightBars[i].Position = new Vector3( i * UnitSize, -200.0f, 0.0f );

				LeftBars[i].Scale = 0.04f;
				RightBars[i].Scale = 0.04f;
			}

			Grid = new VisualizerGrid() { Position = new Vector3(1200.0f, 0.0f, -800.0f), Scale = 10.0f };

			Spotlights = new BPMSpotlight[numSpotlights];
			for( int i = 0; i < numSpotlights; i++ )
			{
				int rowIndex = i % (numSpotlights / 2);
				bool left = i < numSpotlights / 2;

				BPMSpotlight light = new BPMSpotlight();
				light.Position = new Vector3( rowIndex * UnitSize * 4.0f + 200.0f, left ? 200.0f : -200.0f, 100.0f );
				light.Rotation = Rotation.From(180.0f, left ? 90.0f : -90.0f, 0.0f);

				Spotlights[i] = light;
			}

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
			foreach( var entity in Entity.All )
			{
				if ( entity is not AwesomePillar pillar )
					continue;

				Pillars.Add( pillar );
			}

			DebugOverlay.Line(new Vector3(0.0f, -100.0f, 0.0f), new Vector3(0.0f, 100.0f, 0.0f), 100.0f);
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

		[Event.Tick]
		void Tick()
		{
			if ( !IsClient || !Playing )
				return;

			if ( Stream.Finished )
			{
				SongFinished();
				return;
			}

			float beatsPerSecond = Song.BPM / 60.0f;
			float beatsElapsed = Stream.TimeElapsed * beatsPerSecond;

			if((int)beatsElapsed > BeatsPlayed)
			{
				BeatsPlayed = (int)beatsElapsed;

				//BPM pulse
				foreach ( var light in Spotlights )
					light.Pulse();

				Color pillarColor = Color.Random;

				if ( ++ColorCycle >= CycleColors.Length )
					ColorCycle = 0;

				LeftLasers.Pulse();
				RightLasers.Pulse();

			}

			while( CurrentEvent < Level.Events.Length && Level.Events[CurrentEvent].Time <= beatsElapsed )
			{
				//Log.Info( "playing event " + (CurrentEvent + 1) + "/" + Level.Events.Length );
				var evt = Level.Events[CurrentEvent++];

				switch ( evt.Type )
				{
					case EventType.LeftRotatingLasers: LeftLasers.Event( (LightEventType)evt.Value ); break;
					case EventType.RightRotatingLasers: RightLasers.Event( (LightEventType)evt.Value ); break;
					case EventType.LeftRotatingLaserSpeed: LeftLasers.RotationSpeedMultiplier = evt.Value; break;
					case EventType.RightRotatingLaserSpeed: RightLasers.RotationSpeedMultiplier = evt.Value; break;
					default: break;
				}
			}

			// number of beats the user has to hit the note
			const float NotePlayableWindow = 4.0f;

			// amount of time between note 'incoming' and when it becomes playable
			//const float NoteIncomingWindow = 1.0f;
			// animation time (seconds) * BPS
			float NoteIncomingWindow = (20.0f / 10.0f) * beatsPerSecond;

			// number of beats the note lingers
			const float LateSliceWindow = 1.0f;

			// note speedup without sacrificing BPM
			const float NoteSpeed = 2.0f;

			// how far away should notes come from
			const float IncomingNoteDistance = 400.0f;

			while( CurrentNote < Level.Notes.Length && Level.Notes[CurrentNote].Time - beatsElapsed <= NotePlayableWindow + NoteIncomingWindow )
			{
				var note = Level.Notes[CurrentNote++];

				var ent = Create<Note>();
				ent.Data = note;

				ent.Position = new Vector3( NotePlayableWindow * UnitSize * NoteSpeed, note.LineIndex * UnitSize - (3 * UnitSize / 2.0f), note.LineLayer * UnitSize + UnitSize / 2.0f );
				//ent.Position = new Vector3( IncomingNoteDistance, note.LineIndex * UnitSize - (3 * UnitSize / 2.0f), note.LineLayer * UnitSize + UnitSize / 2.0f );
				ActiveNotes.Add( ent );
			}

			//Time.Delta
			//float moveSpeed
			//foreach (var note in ClientNotes)
			//{
			//	note.Position += new Vector3( Time.Delta * beatsPerSecond * UnitSize, 0.0f, 0.0f );
			//}

			bool newHitThisTick = false;
			bool newMissThisTick = false;

			//stale notes that need to be removed
			List<Note> RemoveNotes = new();

			foreach ( var note in ActiveNotes )
			{
				var data = note.Data;
				float noteTime = data.Time - beatsElapsed;

				Vector3 oldPosition = note.Position;

				// if we're in the playable window or later, set position as normal
				if ( noteTime <= NotePlayableWindow )
				{
					float targetPosition = noteTime * UnitSize * NoteSpeed;
					note.Position = note.Position.WithX( noteTime * UnitSize * NoteSpeed );

				}
				//else
				//{
				//	// otherwise, we're incoming
				//	float incomingTime = 1.0f - (noteTime - NotePlayableWindow);
				//	float targetPosition = Lerp( IncomingNoteDistance, NotePlayableWindow * UnitSize * NoteSpeed, incomingTime );
				//	note.Position = note.Position.WithX( targetPosition );
				//}

				//for jigglebones
				//note.Velocity = note.Position - oldPosition;

				if ( noteTime <= 0 && !note.SoundPlayed )
				{
					note.SoundPlayed = true;

					if ( note.Hit )
						newHitThisTick = true;
					else
						newMissThisTick = true;
				}

				if( noteTime <= -LateSliceWindow)
					RemoveNotes.Add( note );
			}

			foreach(var note in RemoveNotes)
			{
				note.Delete();
				ActiveNotes.Remove( note );
			}

			if (newHitThisTick)
				PlaySound("note_hit");

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
	}
}
