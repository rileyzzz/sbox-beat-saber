using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace BeatSaber
{
	public partial class MultiplierPanel : Panel
	{
		Panel Multiplier;
		Label MultiplierText;

		public MultiplierPanel()
		{
			StyleSheet.Load( "/ui/MultiplierPanel.scss" );

			Multiplier = AddChild<Panel>( "multiplier" );
			MultiplierText = Multiplier.AddChild<Label>( "text" );

			SetMultiplier( 1, 0 );
		}

		public void SetMultiplier( int mult, int frac )
		{
			MultiplierText.Text = "x" + mult.ToString();

			Multiplier.SetClass( "top",		frac > 0 );
			Multiplier.SetClass( "right",	frac > 1 );
			Multiplier.SetClass( "bottom",	frac > 2 );
			Multiplier.SetClass( "left",	frac > 3 );

			Multiplier.Style.Dirty();
		}
	}
}
