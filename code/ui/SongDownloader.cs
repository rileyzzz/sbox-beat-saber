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

		BeatSaver.RequestInfo FinderTask = null;


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
			Log.Info("cancel task");
			if ( FinderTask != null )
				FinderTask.Cancel = true;


			if ( SearchBar.Text == "" )
				FinderTask = BeatSaver.GetLatestMaps( MapsFound );
			else
				FinderTask = BeatSaver.SearchMaps( MapsFound, 0, SearchBar.Text );
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
		Label Description;

		Panel Controls;
		DownloadButton Download;

		//Label Description;


		Panel ImageContainer;
		Image Cover;

		public DownloadPanel()
		{
			Details = AddChild<Panel>( "details" );
			Title = Details.AddChild<Label>( "title" );
			Description = Details.AddChild<Label>( "description" );

			Controls = Details.AddChild<Panel>( "controls" );
			Download = Controls.AddChild<DownloadButton>( "download" );

			ImageContainer = AddChild<Panel>( "imageContainer" );
			Cover = ImageContainer.AddChild<Image>( "image" );
		}

		public void SetMap(MapDetail map)
		{
			Map = map;

			Title.Text = Map.Name;
			Description.Text = Map.Description;

			Cover.SetTexture( Map.Versions[0].CoverURL );
		}
	}

	public partial class DownloadButton : Panel
	{
		Label Text;
		IconPanel PlayIcon;

		BeatSaver.RequestInfo downloadTask = null;

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

			if ( downloadTask != null )
				return;

			Download( download.Map );

			e.StopPropagation();
		}

		public void Download( MapDetail map )
		{
			if ( downloadTask != null )
				return;

			downloadTask = BeatSaver.DownloadMap( map );
		}

		public override void Tick()
		{
			base.Tick();

			if( downloadTask != null )
			{
				Text.Text = downloadTask.Complete ? "Complete" : (downloadTask.Progress * 100.0f).ToString() + "%";
			}
		}
	}
}
