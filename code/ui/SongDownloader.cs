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
		Panel PagesContainer;
		PageButton LeftButton;
		PageButton RightButton;
		Label PageLabel;

		List<DownloadPanel> Songs = new();

		BeatSaver.RequestInfo FinderTask = null;

		int Page = 0;

		public SongDownloader()
		{
			StyleSheet.Load( "/ui/SongDownloader.scss" );

			Content = AddChild<Panel>( "content" );

			SearchBar = Content.AddChild<TextEntry>( "searchBar" );
			SearchBar.AddEventListener( "onchange", SearchEdit );

			DownloadContainer = Content.AddChild<Panel>( "downloadContainer" );

			PagesContainer = Content.AddChild<Panel>( "pagesContainer" );
			
			LeftButton = PagesContainer.AddChild<PageButton>( "button" );
			PageLabel = PagesContainer.AddChild<Label>( "text" );
			RightButton = PagesContainer.AddChild<PageButton>( "button" );

			LeftButton.Icon.Text = "navigate_before";
			RightButton.Icon.Text = "navigate_next";

			LeftButton.AddEventListener( "onclick", PreviousPage );
			RightButton.AddEventListener( "onclick", NextPage );

			Refresh();
		}

		void PreviousPage()
		{
			if ( --Page < 0 )
				Page = 0;

			Refresh();
		}

		void NextPage()
		{
			Page++;

			Refresh();
		}

		public void Refresh()
		{
			SearchBar.Placeholder = SearchBar.Text == "" ? "Search..." : "";
			PageLabel.Text = (Page + 1).ToString();

			// end any running tasks
			Log.Info("cancel task");
			if ( FinderTask != null )
				FinderTask.Cancel = true;


			//if ( SearchBar.Text == "" )
			//	FinderTask = BeatSaver.GetLatestMaps( MapsFound );
			//else
			//	FinderTask = BeatSaver.SearchMaps( MapsFound, Page, SearchBar.Text );
			FinderTask = BeatSaver.SearchMaps( MapsFound, Page, SearchBar.Text );
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

			//go back to the last page if there aren't any more maps
			if( maps.Length == 0 && Page > 0 )
			{
				Page--;
				Refresh();
			}
		}

		public void SearchEdit()
		{
			Page = 0;
			Refresh();
		}
	}

	public partial class PageButton : Panel
	{
		public IconPanel Icon;

		public PageButton()
		{
			Icon = AddChild<IconPanel>( "icon" );
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

			CreateEvent( "onclick" );
			e.StopPropagation();
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

			if ( Map.Installed )
				Download.SetInstalled();

			Title.Text = Map.Name;
			Description.Text = Map.Description;
			
			var coverTexture = Texture.Load( Map.Versions[0].CoverURL );
			Cover.Texture = coverTexture;
		}
	}

	public partial class DownloadButton : Panel
	{
		Label Text;
		IconPanel PlayIcon;

		BeatSaver.RequestInfo downloadTask = null;

		bool Installed = false;

		public DownloadButton()
		{
			Text = AddChild<Label>( "text" );
			Text.Text = "Download";

			PlayIcon = AddChild<IconPanel>( "icon" );
			PlayIcon.Text = "play_arrow";
		}

		public void SetInstalled()
		{
			Installed = true;
			Text.Text = "Installed";
			PlayIcon.Text = "";
			SetClass( "hover", false );
		}

		protected override void OnMouseOver( MousePanelEvent e )
		{
			base.OnMouseOver( e );

			if ( Installed )
				return;

			SetClass( "hover", true );
			e.StopPropagation();
		}

		protected override void OnMouseOut( MousePanelEvent e )
		{
			base.OnMouseOut( e );

			if ( Installed )
				return;

			SetClass( "hover", false );
			e.StopPropagation();
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );

			if ( Installed )
				return;

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

			if( downloadTask != null && !Installed )
			{
				if ( !downloadTask.Complete )
					Text.Text = (downloadTask.Progress * 100.0f).ToString() + "%";
				else
					SetInstalled();
			}
		}
	}
}
