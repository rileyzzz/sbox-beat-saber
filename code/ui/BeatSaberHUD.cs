using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace BeatSaber
{
	public partial class BeatSaberHUDEntity : Sandbox.HudEntity<RootPanel>
	{
		BeatSaberHUDContainer Container;
		
		VROverlayPanel VRSongList;
		
		public BeatSaberHUDEntity()
		{
			if ( !IsClient )
				return;

			RootPanel.StyleSheet.Load( "/ui/HUD.scss" );

			Container = RootPanel.AddChild<BeatSaberHUDContainer>( "HUD" );


		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			if ( Local.Client.IsUsingVr )
			{
				VRSongList = new VROverlayPanel( RootPanel );
				VRSongList.SetTransformAbsolute( new Transform( new Vector3( 10.0f, 0.0f, 0.0f ) ) );
			}
		}
	}

	public partial class BeatSaberHUDContainer : Panel
	{
		SidebarPanel Sidebar;

		public Panel CurrentTab;

		//SongBrowser Browser;
		//SongDownloader Downloader;
		public BeatSaberHUDContainer()
		{
			Sidebar = AddChild<SidebarPanel>( "sidebar" );
			CurrentTab = AddChild<SongBrowser>( "browser" );
		}

		public void SetBrowserTab()
		{
			if ( CurrentTab != null )
				CurrentTab.Delete();

			CurrentTab = AddChild<SongBrowser>( "browser" );
		}

		public void SetDownloadTab()
		{
			if ( CurrentTab != null )
				CurrentTab.Delete();

			CurrentTab = AddChild<SongDownloader>( "downloader" );
		}
	}

	public partial class SidebarPanel : Panel
	{
		SidebarButton BrowserButton;
		SidebarButton DownloadButton;

		public SidebarPanel()
		{
			BrowserButton = AddChild<SidebarButton>( "button" );
			BrowserButton.Icon.Text = "queue_music";
			BrowserButton.AddEventListener( "onclick", SelectBrowserTab );

			DownloadButton = AddChild<SidebarButton>( "button" );
			DownloadButton.Icon.Text = "download";
			DownloadButton.AddEventListener( "onclick", SelectDownloadTab );
		}

		void SelectBrowserTab()
		{
			if ( Parent is not BeatSaberHUDContainer container )
				return;
			container.SetBrowserTab();
		}

		void SelectDownloadTab()
		{
			if ( Parent is not BeatSaberHUDContainer container )
				return;
			container.SetDownloadTab();
		}
	}

	public partial class SidebarButton : Panel
	{
		public Label Icon;

		public SidebarButton()
		{
			Icon = AddChild<Label>( "icon" );
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
}
