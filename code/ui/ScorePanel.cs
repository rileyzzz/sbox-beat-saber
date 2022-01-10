using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace BeatSaber
{
	public partial class ScorePanel : Panel
	{
		Label ComboLabel;
		
		public Label Combo;
		public Label Score;

		public ScorePanel()
		{
			StyleSheet.Load( "/ui/ScorePanel.scss" );

			ComboLabel = AddChild<Label>( "comboLabel" );
			ComboLabel.Text = "COMBO";

			Combo = AddChild<Label>( "combo" );
			Combo.Text = "0";
			AddChild<Panel>( "divider" );

			Score = AddChild<Label>( "score" );
			Score.Text = "0";
		}
	}
}
