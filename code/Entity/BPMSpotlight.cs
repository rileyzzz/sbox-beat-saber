using System;
using Sandbox;

namespace BeatSaber
{
	public partial class BPMSpotlight : AnimEntity
	{
		SpotLightEntity Light;

		public BPMSpotlight()
		{

		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/spotlight.vmdl" );
			UseAnimGraph = true;

			Light = Create<SpotLightEntity>();
			Light.SetParent( this, "light" );

			Light.Brightness = 1.0f;

			//Light.UseFogNoShadows();
			//stength?
			//Light.FogStength = 1.0f;


			Light.InnerConeAngle = 15;
			Light.OuterConeAngle = 30;
			Light.FogStength = 10.0f;
		}

		public void Pulse()
		{
			SetAnimVector( "target", Vector3.Random );

			Light.Color = Color.Random;

			//Light.FogStength = 1000.0f;
		}
	}
}
