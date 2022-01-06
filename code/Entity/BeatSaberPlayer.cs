using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using AForge.Math;

namespace BeatSaber
{
	public partial class BeatSaberPlayer : Player
	{
		[Net, Local] public LeftHand LeftHand { get; set; }
		[Net, Local] public RightHand RightHand { get; set; }

		public BeatSaberPlayer()
		{

		}


		public override void ClientSpawn()
		{
			base.ClientSpawn();

			Log.Info( "client spawn" );

			//testStream = new MusicStream( "testlevel/Beat It.ogg" );
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


			//if ( IsClient )
			//{
			//	if ( testStream == null )
			//		return;
			//	//Log.Info( "visualize" );
			//	var data = testStream.GetVisualizerData();
			//	int groupSize = (data.Length / 2) / bars.Length;
			//	//int groupSize = data.Length / bars.Length;
			//	int offset = 0;

			//	if ( data.Length > 0 )
			//	{
			//		for ( int i = 0; i < bars.Length; i++ )
			//		{
			//			int group = (i + offset) * groupSize;

			//			//float avgValue = 0.0f;
			//			//for ( int j = 0; j < groupSize; j++ )
			//			//	avgValue += data[group + j];
			//			//avgValue /= groupSize;

			//			float maxValue = 0.0f;
			//			for ( int j = 0; j < groupSize; j++ )
			//				maxValue = Math.Max( maxValue, data[group + j] );

			//			bars[i].Position = bars[i].Position.WithZ( Math.Abs( maxValue ) * 2.0f + 15.0f );
			//		}
			//	}
			//}
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


		//MusicStream testStream = null;
		//VisualizerBar[] bars;

		//[ClientCmd]
		//public static void play_song()
		//{
		//	if ( Local.Pawn is not BeatSaberPlayer player )
		//		return;

		//	player.testStream = new MusicStream( "Peanut Butter Jelly - Galantis/song.ogg" );
		//	//player.testStream = new MusicStream( "test4.ogg" );

		//	player.testStream.Play();

		//	const int numBars = 128;
		//	player.bars = new VisualizerBar[numBars];
		//	for ( int i = 0; i < player.bars.Length; i++ )
		//	{
		//		player.bars[i] = Create<VisualizerBar>();
		//		player.bars[i].Position = new Vector3( 0.0f, (i - numBars / 2) * 2.0f, 0.0f );
		//		player.bars[i].Scale = 0.02f;
		//	}
		//}
	}
}
