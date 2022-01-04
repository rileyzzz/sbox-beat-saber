using Sandbox;
using System;
using System.Collections.Generic;

namespace BeatSaber
{
	public partial class VisualizerBar : ModelEntity
	{
		public VisualizerBar()
		{

		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/dev/sphere.vmdl" );

		}
	}
}
