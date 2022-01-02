using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace LightsaberGame
{
	public partial class DuelPlayer : Player
	{
		[Net, Local] public LeftHand LeftHand { get; set; }
		[Net, Local] public RightHand RightHand { get; set; }

		public DuelPlayer()
		{

		}

		public override void Respawn()
		{
			//SetModel( "models/citizen/citizen.vmdl" );

			if ( Client.IsUsingVr )
			{
				Controller = new VRWalkController();
				Animator = new VRAnimator();
				Camera = new VRCamera();
			}
			else
			{
				Controller = new WalkController();
				Animator = new StandardPlayerAnimator();
				Camera = new FirstPersonCamera();
			}

			LeftHand?.Delete();
			RightHand?.Delete();

			LeftHand = new() { Owner = this };
			RightHand = new() { Owner = this };

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			
			UsePhysicsCollision = false;
			PhysicsEnabled = false;

			//base.Respawn();

			//EnableHitboxes = false;
			//CollisionGroup = CollisionGroup.Never;
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			LeftHand?.Simulate( cl );
			RightHand?.Simulate( cl );
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );

			LeftHand?.FrameSimulate( cl );
			RightHand?.FrameSimulate( cl );
		}

		public override void BuildInput( InputBuilder input )
		{
			base.BuildInput( input );

		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;

			LeftHand?.Delete();
			RightHand?.Delete();
		}


		public short ConvertSample( float sample )
		{
			return (short)(sample * short.MaxValue);
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			Log.Info( "client spawn" );


			using ( var vorbis = new NVorbis.VorbisReader( "testlevel/Beat It.ogg" ) )
			{
				Log.Info( "Loaded OGG file." );

				Log.Info( "Channels: " + vorbis.Channels );
				Log.Info( "Sample Rate: " + vorbis.SampleRate );
				Log.Info( "Length: " + vorbis.TotalTime );

				var sound = Sound.FromScreen( "audiostream.default" ).CreateStream( vorbis.SampleRate, vorbis.Channels );

				var readBuffer = new float[vorbis.Channels * vorbis.SampleRate / 5];  // 200ms
				var writeBuffer = new short[readBuffer.Length];

				int cnt;
				while ( (cnt = vorbis.ReadSamples( readBuffer, 0, readBuffer.Length )) > 0 )
				{
					for ( int i = 0; i < readBuffer.Length; i++ )
						writeBuffer[i] = ConvertSample( readBuffer[i] );

					sound.WriteData( writeBuffer );
				}


				//// get the channels & sample rate
				//var channels = vorbis.Channels;
				//var sampleRate = vorbis.SampleRate;

				//// OPTIONALLY: get a TimeSpan indicating the total length of the Vorbis stream
				//var totalTime = vorbis.TotalTime;

				//// create a buffer for reading samples
				//var readBuffer = new float[channels * sampleRate / 5];  // 200ms

				//// get the initial position (obviously the start)
				//var position = TimeSpan.Zero;

				//// go grab samples
				//int cnt;
				//while ( (cnt = vorbis.ReadSamples( readBuffer, 0, readBuffer.Length )) > 0 )
				//{
				//	// do stuff with the buffer
				//	// samples are interleaved (chan0, chan1, chan0, chan1, etc.)
				//	// sample value range is -0.99999994f to 0.99999994f unless vorbis.ClipSamples == false

				//	// OPTIONALLY: get the position we just read through to...
				//	position = vorbis.TimePosition;
				//}
			}
		}
	}
}
