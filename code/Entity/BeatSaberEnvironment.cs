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
		public static float UnitSize = 20.0f;

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

		List<Note> ClientNotes = new();

		const int numVisualizerBars = 256;
		VisualizerBar[] LeftBars;
		VisualizerBar[] RightBars;

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

				LeftBars[i].Position = new Vector3( i * UnitSize, 80.0f, 0.0f );
				RightBars[i].Position = new Vector3( i * UnitSize, -80.0f, 0.0f );

				LeftBars[i].Scale = 0.04f;
				RightBars[i].Scale = 0.04f;
			}

			//foreach ( var data in NoteData )
			foreach ( var data in Level.Notes )
			{
				var ent = Create<Note>();
				ent.Data = data;

				ent.Position = new Vector3( data.Time * UnitSize, data.LineIndex * UnitSize - (3 * UnitSize / 2.0f), data.LineLayer * UnitSize + UnitSize / 2.0f );
				ClientNotes.Add(ent);
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


		[Event.Tick]
		void Tick()
		{
			if ( !IsClient || !Playing )
				return;

			float beatsPerSecond = Song.BPM / 60.0f;

			//Time.Delta
			//float moveSpeed
			//foreach (var note in ClientNotes)
			//{
			//	note.Position += new Vector3( Time.Delta * beatsPerSecond * UnitSize, 0.0f, 0.0f );
			//}

			bool newHitThisTick = false;

			float offset = Stream.TimeElapsed * beatsPerSecond * UnitSize;
			foreach ( var note in ClientNotes )
			{
				var data = note.Data;
				float noteTime = data.Time * UnitSize - offset;
				note.Position = note.Position.WithX( noteTime );

				if(noteTime <= 0 && !note.SoundPlayed)
				{
					note.SoundPlayed = true;
					newHitThisTick = true;

					note.Slice();
				}
			}

			if(newHitThisTick)
				PlaySound("note_hit");
			
			var left_data = Stream.GetVisualizerData(0);
			var right_data = Stream.GetVisualizerData(1);

			if ( left_data.Length > 0 && right_data.Length > 0 )
			{
				UpdateVisualizerRow(left_data, LeftBars);
				UpdateVisualizerRow(right_data, RightBars);
			}
		}
	}
}
