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
	}
}
