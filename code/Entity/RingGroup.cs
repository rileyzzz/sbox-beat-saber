using System;
using Sandbox;

namespace BeatSaber
{
	public partial class RingGroup : Entity
	{
		static readonly Model RingModel = Model.Load( "models/ring.vmdl" );
		
		const int numRings = 20;
		const float ringSpacing = 180.0f;

		float spinTarget = 0.0f;
		float zoomTarget = 1.0f;

		ModelEntity[] Rings = new ModelEntity[numRings];
		public RingGroup()
		{
		}

		public override void Spawn()
		{
			base.Spawn();

			for( int i = 0; i < Rings.Length; i++ )
			{
				var ring = new ModelEntity();
				ring.Parent = this;
				ring.Model = RingModel;
				ring.Position = new Vector3( i * ringSpacing, 0.0f, 0.0f );
				Rings[i] = ring;
			}
		}

		public void Spin()
		{
			spinTarget = Rand.Float( 0.0f, 360.0f );
		}

		public void Zoom()
		{
			zoomTarget = Rand.Float( 0.75f, 1.5f );
		}

		float Lerp( float a, float b, float f )
		{
			return a + f * (b - a);
		}

		[Event.Tick]
		public void Tick()
		{
			for ( int i = 0; i < Rings.Length; i++ )
			{
				float interp = (1.0f - ((float)i / numRings)) * 0.2f;
				Rings[i].Rotation = Rotation.Slerp( Rings[i].Rotation, Rotation.FromRoll( spinTarget ), interp );
				Rings[i].Scale = Lerp( Rings[i].Scale, zoomTarget, interp );
			}
		}
	}
}
