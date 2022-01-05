using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace BeatSaber
{
	public partial class SongBrowser : Panel
	{
		Panel SongContainer;
		DetailsPanel DetailsPanel;

		List<SongPanel> Songs = new();

		SongPanel _selected;
		public SongPanel Selected
		{
			get => _selected;
			set
			{
				_selected = value;
				UpdateSelected();
				DetailsPanel.UpdateSelected( Selected.Song );
			}
		}

		public SongBrowser()
		{
			StyleSheet.Load( "/ui/SongBrowser.scss" );

			SongContainer = AddChild<Panel>("container");
			DetailsPanel = AddChild<DetailsPanel>("detailsPanel");

			Update();
		}

		public void Update()
		{
			if ( Game.Current is not BeatSaberGame game )
				return;

			foreach ( var song in Songs )
				song.Delete();
			Songs.Clear();

			Log.Info("adding songs");
			foreach( var song in game.LocalSongs )
			{
				if ( song == null || song.DifficultyBeatmapSets == null || song.DifficultyBeatmapSets.Length == 0 )
					continue;

				Log.Info( song.SongName );
				var panel = SongContainer.AddChild<SongPanel>( "song" );
				panel.SetSong( song );
				Songs.Add( panel );
			}
		}

		void UpdateSelected()
		{
			foreach ( var song in Songs )
				song.SetClass( "selected", song == Selected );
		}
	}

	public partial class SongPanel : Panel
	{
		public BeatSaberSong Song;
		
		Panel Details;
		Panel ImageContainer;
		Image Image;

		Label Title;
		Label Author;

		public SongPanel()
		{
			Details = AddChild<Panel>("details");
			ImageContainer = AddChild<Panel>("imageContainer");
			Image = ImageContainer.AddChild<Image>("image");

			Title = Details.AddChild<Label>("title");
			Author = Details.AddChild<Label>("author");
		}

		public void SetSong( BeatSaberSong song )
		{
			Song = song;

			Title.Text = song.SongName + " " + song.SongSubName;
			Author.Text = song.SongAuthorName + "\n" + song.LevelAuthorName;

			Image.SetTexture(Song.Directory + Song.CoverImageFilename);
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			if ( Parent.Parent is not SongBrowser browser )
				return;

			browser.Selected = this;
			e.StopPropagation();
		}
	}

	public partial class DetailsPanel : Panel
	{
		BeatSaberSong Song = null;
		
		Label Title;
		Panel DifficultyContainer;
		DifficultySelector Difficulty;

		Panel DescriptionContainer;

		Panel SongControls;
		PlayButton Play;

		public DetailsPanel()
		{
			Title = AddChild<Label>( "title" );
			DifficultyContainer = AddChild<Panel>( "difficultyContainer" );
			Difficulty = DifficultyContainer.AddChild<DifficultySelector>( "difficulty" );

			DescriptionContainer = AddChild<Panel>( "descriptionContainer" );

			SongControls = AddChild<Panel>( "songControls" );
			Play = SongControls.AddChild<PlayButton>( "play" );
		}

		public void UpdateSelected(BeatSaberSong song)
		{
			Song = song;

			Title.Text = song.SongName + " " + song.SongSubName;

			Difficulty.SetDifficulties( Song.DifficultyBeatmapSets[0].DifficultyBeatmaps );
		}

		public void PlaySong()
		{
			BeatSaberGame.PlaySong( Song.SongName, Difficulty.SelectionIndex );
		}
	}

	public partial class DifficultySelector : Panel
	{
		public List<DifficultySelection> Entries = new();

		DifficultySelection _selection = null;
		public DifficultySelection Selection
		{
			get => _selection;
			set
			{
				_selection = value;
				UpdateSelection();
			}
		}

		public int SelectionIndex => Entries.IndexOf( Selection );

		public DifficultySelector()
		{
		}

		public void SetDifficulties( IList<DifficultyBeatmap> beatmaps )
		{
			foreach ( var entry in Entries )
				entry.Delete();
			Entries.Clear();

			foreach(var beatmap in beatmaps)
			{
				var entry = AddChild<DifficultySelection>( "entry" );
				//Log.Info("difficulty " + beatmap.difficultyStr);
				entry.Text.Text = beatmap.Difficulty.ToString().Replace("Plus", "+");
				Entries.Add( entry );
			}

			if ( Entries.Count > 0 )
				Selection = Entries[0];
		}

		void UpdateSelection()
		{
			foreach ( var entry in Entries )
				entry.SetClass( "selected", entry == Selection );
		}
	}

	public partial class DifficultySelection : Panel
	{
		public Label Text;
		public DifficultySelection()
		{
			Text = AddChild<Label>( "text" );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			if ( Parent is not DifficultySelector selector )
				return;

			selector.Selection = this;

			e.StopPropagation();
		}
	}

	public partial class PlayButton : Panel
	{
		Label Text;
		IconPanel PlayIcon;

		public PlayButton()
		{
			Text = AddChild<Label>( "text" );
			Text.Text = "Play";

			PlayIcon = AddChild<IconPanel>( "icon" );
			PlayIcon.Text = "play_arrow";
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			base.OnMouseOver( e );

			SetClass( "hover", true );
			e.StopPropagation();
		}

		protected override void OnMouseOut( MousePanelEvent e )
		{
			base.OnMouseOut( e );

			SetClass( "hover", false );
			e.StopPropagation();
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			if ( Parent.Parent is not DetailsPanel details )
				return;

			details.PlaySong();
			e.StopPropagation();
		}
	}
}
