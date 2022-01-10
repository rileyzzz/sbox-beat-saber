using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace BeatSaber
{
	public partial class SongInfoPanel : Panel
	{
		Panel Details;
		Label Title;
		SongInfoEntry Author;
		SongInfoEntry BeatmapAuthor;
		SongInfoEntry BPM;

		public ProgressBar Progress;

		Image SongImage;

		
		public SongInfoPanel()
		{
			StyleSheet.Load( "/ui/SongInfoPanel.scss" );

			Details = AddChild<Panel>( "details" );
			Title = Details.AddChild<Label>( "title" );

			Author = Details.AddChild<SongInfoEntry>( "entry" );
			Author.Icon.Text = "piano";

			BeatmapAuthor = Details.AddChild<SongInfoEntry>( "entry" );
			BeatmapAuthor.Icon.Text = "edit";

			BPM = Details.AddChild<SongInfoEntry>( "entry" );
			BPM.Icon.Text = "speed";

			Progress = Details.AddChild<ProgressBar>( "progress" );

			SongImage = AddChild<Image>( "image" );
		}

		public void SetSong( BeatSaberSong song )
		{
			Title.Text = song.SongName;

			Author.Text.Text = song.SongAuthorName;
			BeatmapAuthor.Text.Text = song.LevelAuthorName;
			BPM.Text.Text = song.BPM.ToString();


			SongImage.Texture = song.CoverTexture;
		}
	}

	public partial class SongInfoEntry : Panel
	{
		public IconPanel Icon;
		public Label Text;

		public SongInfoEntry()
		{
			Icon = AddChild<IconPanel>( "icon" );
			Text = AddChild<Label>( "text" );
		}
	}

	public partial class ProgressBar : Panel
	{
		Panel Bar;

		public float Progress
		{
			set
			{
				Bar.Style.Width = Length.Fraction( value );
				Bar.Style.Dirty();
			}
		}

		public ProgressBar()
		{
			Bar = AddChild<Panel>( "bar" );

		}
	}
}
