using Sandbox;
using Sandbox.Joints;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace BeatSaber
{
	public partial class VRHand : ModelEntity
	{
		public virtual Input.VrHand Hand { get; }

		//public IPhysicsJoint SaberJoint { get; set; }
		[Net] public Lightsaber Saber { get; set; }

		bool triggerPressed = false;

		public VRHand()
		{

		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/hand_dummy.vmdl" );
			//SetModel( "models/sabers/basic_saber.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );
			//SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

			//SetupPhysicsFromSphere( PhysicsMotionType.Static, new Vector3(), 1.0f );
			//SetupPhysicsFromOBB(PhysicsMotionType.Static, new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

			Position = Hand.Transform.Position;
			Rotation = Hand.Transform.Rotation;

			//Saber = new Lightsaber() { Parent = this };
			//Saber = new Lightsaber() { Owner = this };
			Saber = new Lightsaber();
			Saber.Position = Hand.Transform.Position;
			Saber.Rotation = Hand.Transform.Rotation;

			PhysicsJoint.Weld
				//.From( this, 0 )
				.From( PhysicsBody, new Vector3() )
				.To( Saber.PhysicsBody, new Vector3() )
				.WithBlockSolverEnabled()
				//.WithLinearSpring( 8.0f, 0.8f, 300.0f )
				.WithLinearSpring( 8.0f, 0.8f, 0.0f )
				.Create();

			//SaberJoint.LocalAnchor1 = new Vector3();
			//PhysicsJoint.Weld.
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			Transform = Hand.Transform;
			//Animate();

			if( IsServer )
			{
				//SaberJoint.LocalAnchor1 = Position;

				//SaberJoint.LocalAnchor1 = new Vector3();
				//SaberJoint.LocalAnchor1 = Position;

				//if our saber is too far away, reset its position
				if ( Vector3.DistanceBetween( Position, Saber.Position ) > 20.0f )
					Saber.Transform = Transform;

				const float triggerOnThreshold = 0.5f;
				const float triggerOffThreshold = 0.2f;

				if ( !triggerPressed && Hand.Trigger > triggerOnThreshold )
				{
					triggerPressed = true;
					Saber.Extend = !Saber.Extend;
				}
				else if ( triggerPressed && Hand.Trigger < triggerOffThreshold )
				{
					triggerPressed = false;
				}

				//if ( Hand.Trigger > 0.5f )
				//{
				//	var test = Create<ModelEntity>();

				//	test.SetModel( "models/sbox_props/cola_can/cola_can.vmdl" );
				//	test.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
				//	test.Transform = Transform;

				//	test.CollisionGroup = CollisionGroup.ConditionallySolid;
				//	//test.RemoveCollisionLayer( CollisionLayer.WORLD_GEOMETRY );
				//	test.ClearCollisionLayers();

				//	test.SetInteractsAs( CollisionLayer.Debris );
				//	//test.SetInteractsExclude( CollisionLayer.WORLD_GEOMETRY );
				//}
			}

		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );

			Transform = Hand.Transform;

			//Log.Info("pos " + Position);
		}

		//void Animate()
		//{
		//	//Log.Info( "index curl " + Hand.GetFingerCurl( 1 ) );
		//	SetAnimFloat( "FingerCurl_Thumb", Hand.GetFingerCurl( 0 ) );
		//	SetAnimFloat( "FingerCurl_Index", Hand.GetFingerCurl( 1 ) );
		//	SetAnimFloat( "FingerCurl_Middle", Hand.GetFingerCurl( 2 ) );
		//	SetAnimFloat( "FingerCurl_Ring", Hand.GetFingerCurl( 3 ) );
		//	SetAnimFloat( "FingerCurl_Pinky", Hand.GetFingerCurl( 4 ) );
		//}
	}

	public class LeftHand : VRHand
	{
		public override Input.VrHand Hand => Input.VR.LeftHand;

		public override void Spawn()
		{
			//SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
			//SetInteractsAs( CollisionLayer.LEFT_HAND );

			//SetupPhysicsFromModel( PhysicsMotionType.Static );

			base.Spawn();
		}
	}

	public class RightHand : VRHand
	{
		public override Input.VrHand Hand => Input.VR.RightHand;

		public override void Spawn()
		{
			//SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
			//SetInteractsAs( CollisionLayer.RIGHT_HAND );

			//SetupPhysicsFromModel( PhysicsMotionType.Static );

			base.Spawn();
		}
	}
}
