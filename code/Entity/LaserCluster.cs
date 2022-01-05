using System;
using Sandbox;

namespace BeatSaber
{
	struct FadeTarget
	{
		public Color TargetColor = Color.Black;
		public bool Instant = true;
		public bool Flash = false;
		public Color FlashReturnColor = Color.White;

		public float FadeTime = 0.0f;

		public FadeTarget( Color target, bool instant, bool flash, Color returnColor = new Color() )
		{
			TargetColor = target;
			Instant = instant;
			Flash = flash;
			FlashReturnColor = returnColor;
		}

		public FadeTarget()
		{
		}
	}

	public partial class LaserCluster : Entity
	{
		const int numLasers = 5;
		ModelEntity[] Lasers = new ModelEntity[numLasers];

		static readonly Model LaserModel = Model.Load( "models/laser.vmdl" );

		FadeTarget Fade = new();

		const float FlashDuration = 0.5f;
		const float TransitionRate = 0.04f;

		static readonly Color Blue = new Color( 87.0f / 255.0f, 143.0f / 255.0f, 235.0f / 255.0f, 1.0f );
		static readonly Color Red = new Color( 232.0f / 255.0f, 77.0f / 255.0f, 77.0f / 255.0f, 1.0f );
		static readonly Color Black = new Color( 0, 0, 0, 0 );

		const float RotationSpeed = 4.0f;
		public int RotationSpeedMultiplier = 1;

		float LaserRotation = 0.0f;

		float Pitch = 45.0f;
		float TargetPitch = 45.0f;

		int BPMCounter = 0;

		Random rand;

		public LaserCluster()
		{
			rand = new Random();
		}

		public override void Spawn()
		{
			base.Spawn();

			for( int i = 0; i < numLasers; i++ )
			{
				Lasers[i] = new ModelEntity() { Parent = this };
				Lasers[i].SetModel( LaserModel );
				Lasers[i].RenderColor = Black;
				//Lasers[i].Rotation = Rotation.From( 45.0f, ((float)i / numLasers) * 360.0f, 0.0f );
			}
		}

		public void Event( LightEventType type )
		{
			Color LastColor = Fade.TargetColor;

			switch(type)
			{
				case LightEventType.Off:						Fade = new FadeTarget( Black, true, false ); break;
				case LightEventType.Blue:						Fade = new FadeTarget( Blue, true, false ); break;
				case LightEventType.BlueFlash:					Fade = new FadeTarget( Blue, false, true, LastColor ); break;
				case LightEventType.BlueFlashFadeToBlack:		Fade = new FadeTarget( Blue, false, true, Black ); break;
				case LightEventType.FadeToBlue:					Fade = new FadeTarget( Blue, false, false ); break;
				case LightEventType.Red:						Fade = new FadeTarget( Red, true, false ); break;
				case LightEventType.RedFlash:					Fade = new FadeTarget( Red, false, true, LastColor ); break;
				case LightEventType.RedFlashFadeToBlack:		Fade = new FadeTarget( Red, false, true, Black ); break;
				case LightEventType.FadeToRed:					Fade = new FadeTarget( Red, false, false ); break;

				default:
					Log.Warning("Invalid light event type");
					break;
			}
		}

		public void Pulse()
		{
			if ( ++BPMCounter % 2 == 0 )
				BPMCounter = 0;

			if(BPMCounter == 0)
				TargetPitch = rand.Float( 25.0f, 45.0f );
		}

		float Lerp( float a, float b, float f )
		{
			return a + f * (b - a);
		}

		[Event.Tick]
		void Tick()
		{
			LaserRotation += Time.Delta * RotationSpeed * RotationSpeedMultiplier;
			Pitch = Lerp( Pitch, TargetPitch, 0.04f );

			//foreach(var laser in Lasers)
			for ( int i = 0; i < numLasers; i++ )
			{
				var laser = Lasers[i];

				if ( !Fade.Flash )
					laser.RenderColor = Color.Lerp( laser.RenderColor, Fade.TargetColor, Fade.Instant ? 1.0f : TransitionRate );
				else if ( Fade.FadeTime < FlashDuration )
					laser.RenderColor = Color.Lerp( laser.RenderColor, Fade.TargetColor, 0.1f );
				else
					laser.RenderColor = Color.Lerp( laser.RenderColor, Fade.FlashReturnColor, 0.1f );

				//laser.RenderColor = laser.RenderColor.WithAlpha(1.0f);

				laser.Rotation = Rotation.From( Pitch, ((float)i / numLasers) * 360.0f + LaserRotation, 0.0f );
			}

			Fade.FadeTime += Time.Delta;
		}
	}
}
