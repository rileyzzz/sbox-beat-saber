using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeatSaber
{
	public partial class SongDownloader : Panel
	{
		Panel Content;

		TextEntry SearchBar;
		Panel DownloadContainer;

		List<DownloadPanel> Songs = new();

		Sandbox.Internal.Http FinderTask = null;


		public SongDownloader()
		{
			StyleSheet.Load( "/ui/SongDownloader.scss" );

			Content = AddChild<Panel>( "content" );

			SearchBar = Content.AddChild<TextEntry>( "searchBar" );
			SearchBar.AddEventListener( "onchange", SearchEdit );

			DownloadContainer = Content.AddChild<Panel>( "downloadContainer" );

			Refresh();
		}

		public void Refresh()
		{
			SearchBar.Placeholder = SearchBar.Text == "" ? "Search..." : "";

			// end any running tasks
			try
			{
				Log.Info("dispose task");
				if ( FinderTask != null )
					FinderTask.Dispose();
			}
			catch(TaskCanceledException)
			{

			}

			if ( SearchBar.Text == "" )
				BeatSaver.GetLatestMaps( out FinderTask, MapsFound );
			else
				BeatSaver.SearchMaps( out FinderTask, MapsFound, 0, SearchBar.Text );
		}

		public void MapsFound( MapDetail[] maps )
		{
			foreach ( var song in Songs )
				song.Delete();
			Songs.Clear();
			foreach ( var map in maps )
			{
				var panel = DownloadContainer.AddChild<DownloadPanel>( "download" );
				panel.SetMap( map );
				Songs.Add( panel );
			}
		}

		public void SearchEdit()
		{
			Refresh();
		}
	}

	public partial class DownloadPanel : Panel
	{
		public MapDetail Map;

		Panel Details;
		Label Title;

		Panel Inner;
		Panel Controls;
		DownloadButton Download;

		//Label Description;


		Panel ImageContainer;
		Image Cover;

		public DownloadPanel()
		{
			Details = AddChild<Panel>( "details" );
			Title = Details.AddChild<Label>( "title" );

			Inner = Details.AddChild<Panel>( "inner" );

			Controls = Details.AddChild<Panel>( "controls" );
			Download = Controls.AddChild<DownloadButton>( "download" );

			ImageContainer = AddChild<Panel>( "imageContainer" );
			Cover = ImageContainer.AddChild<Image>( "image" );
		}

		public void SetMap(MapDetail map)
		{
			Map = map;

			Title.SetText( Map.Name );

			Cover.SetTexture( Map.Versions[0].CoverURL );
		}
	}

	public partial class DownloadButton : Panel
	{
		Label Text;
		IconPanel PlayIcon;

		Sandbox.Internal.Http downloadTask = null;

		public DownloadButton()
		{
			Text = AddChild<Label>( "text" );
			Text.Text = "Download";

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

			if ( Parent.Parent.Parent is not DownloadPanel download )
				return;

			Download( download.Map );

			e.StopPropagation();
		}

		public void Download( MapDetail map )
		{
			if ( downloadTask != null )
				return;

			BeatSaver.DownloadMap( out downloadTask, map );
		}
	}
}
