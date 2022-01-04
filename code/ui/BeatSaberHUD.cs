using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace BeatSaber
{
	public partial class BeatSaberHUDEntity : Sandbox.HudEntity<RootPanel>
	{
		SongBrowser Browser;

		public BeatSaberHUDEntity()
		{
			if ( !IsClient )
				return;

			RootPanel.StyleSheet.Load( "/ui/HUD.scss" );

			Browser = RootPanel.AddChild<SongBrowser>( "browser" );

		}
	}
}
