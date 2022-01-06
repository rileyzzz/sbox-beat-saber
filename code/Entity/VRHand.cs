using Sandbox;
using Sandbox.Joints;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace BeatSaber
{
	public partial class VRHand : ModelEntity
	{
		public virtual bool IsLeft { get; }
		public virtual Input.VrHand Hand { get; }

		//public IPhysicsJoint SaberJoint { get; set; }
		//[Net] public Lightsaber Saber { get; set; }
		public Lightsaber Saber { get; set; }

		//bool triggerPressed = false;

		public VRHand()
		{
			Transmit = TransmitType.Always;
		}

		public override void Spawn()
		{
			base.Spawn();

			Position = Hand.Transform.Position;
			Rotation = Hand.Transform.Rotation;

			//Saber = new Lightsaber();
			Saber = Create<Lightsaber>();
			Saber.Parent = this;
			Saber.Blade.Red = IsLeft;

			//Saber = new Lightsaber();
			//Saber.Parent = this;
			//Saber.Red = IsLeft;
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			Transform = Hand.Transform;

			Velocity = Hand.Velocity;

			//if( IsServer )
			//{
			//	//SaberJoint.LocalAnchor1 = Position;

			//	//SaberJoint.LocalAnchor1 = new Vector3();
			//	//SaberJoint.LocalAnchor1 = Position;

			//	//if our saber is too far away, reset its position
			//	if ( Vector3.DistanceBetween( Position, Saber.Position ) > 20.0f )
			//		Saber.Transform = Transform;

			//	const float triggerOnThreshold = 0.5f;
			//	const float triggerOffThreshold = 0.2f;

			//	if ( !triggerPressed && Hand.Trigger > triggerOnThreshold )
			//	{
			//		triggerPressed = true;
			//		Saber.Extend = !Saber.Extend;
			//	}
			//	else if ( triggerPressed && Hand.Trigger < triggerOffThreshold )
			//	{
			//		triggerPressed = false;
			//	}

			//	//if ( Hand.Trigger > 0.5f )
			//	//{
			//	//	var test = Create<ModelEntity>();

			//	//	test.SetModel( "models/sbox_props/cola_can/cola_can.vmdl" );
			//	//	test.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			//	//	test.Transform = Transform;

			//	//	test.CollisionGroup = CollisionGroup.ConditionallySolid;
			//	//	//test.RemoveCollisionLayer( CollisionLayer.WORLD_GEOMETRY );
			//	//	test.ClearCollisionLayers();

			//	//	test.SetInteractsAs( CollisionLayer.Debris );
			//	//	//test.SetInteractsExclude( CollisionLayer.WORLD_GEOMETRY );
			//	//}
			//}

		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );

			Transform = Hand.Transform;
		}
	}

	public class LeftHand : VRHand
	{
		public override bool IsLeft => true;
		public override Input.VrHand Hand => Input.VR.LeftHand;

		public override void Spawn()
		{
			base.Spawn();
		}
	}

	public class RightHand : VRHand
	{
		public override bool IsLeft => false;
		public override Input.VrHand Hand => Input.VR.RightHand;

		public override void Spawn()
		{
			base.Spawn();
		}
	}
}
