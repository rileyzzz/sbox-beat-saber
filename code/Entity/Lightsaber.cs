using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace LightsaberGame
{
	public partial class Lightsaber : ModelEntity
	{
		[Net, Change] bool _extend { get; set; } = false;
		public bool Extend
		{
			get => _extend;
			set
			{
				_extend = value;
				//Blade.SetAnimBool( "extend", _extend );
				Log.Info("extend changed");
				CreateBladePhysics();
			}
		}

		[Net] public AnimEntity Blade { get; set; }
		PhysicsShape BladePhysics { get; set; }


		const int numBladeLights = 4;
		//PointLightEntity BladeLight { get; set; }
		PointLightEntity[] BladeLights = new PointLightEntity[numBladeLights];

		Sound BladeSound { get; set; }
		Sound? LongClash { get; set; } = null;

		bool swinging = false;
		float swingDelay = 0.0f;
		float LastVelocity = 0.0f;

		const float BladeHeight = 40.0f;

		int numTouching = 0;
		float touchStartTime = 0.0f;

		public Lightsaber()
		{
		}

		public void On_extendChanged(bool oldValue, bool newValue)
		{
			Log.Info("extend new value " + newValue);
			PlaySound( Extend ? "saber.extend" : "saber.retract" );
			BladeSound.SetVolume( _extend ? 1.0f : 0.0f );
			Blade.SetAnimBool( "extend", Extend );

			foreach ( var light in BladeLights )
				light.Enabled = Extend;

			//BladeLight.Enabled = Extend;
		}

		void CreateBladePhysics()
		{
			//if ( Extend )
			//	BladePhysics.EnableAllCollision();
			//else
			//	BladePhysics.DisableAllCollision();

			//PhysicsGroup.RebuildMass();
			//PhysicsGroup.Wake();

			if ( BladePhysics != null )
			{
				PhysicsBody.RemoveShape( BladePhysics );
				BladePhysics = null;
			}

			if ( Extend )
				BladePhysics = PhysicsBody.AddCapsuleShape( new Vector3(), new Vector3( BladeHeight, 0.0f, 0.0f ), 1.0f );

			
		}

		public override void Spawn()
		{
			base.Spawn();

			MoveType = MoveType.Physics;
			//CollisionGroup = CollisionGroup.Interactive;
			PhysicsEnabled = true;
			UsePhysicsCollision = true;


			SetModel( "models/sabers/basic_saber.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			//CollisionGroup = CollisionGroup.Prop;


			//BladePhysics = new PhysicsBody() { Parent = PhysicsBody };

			//BladePhysics.DisableAllCollision();

			//BladePhysics.AddCapsuleShape( new Vector3(), new Vector3(0.0f, 0.0f, 400.0f), 1.0f );

			//Blade = new LightsaberBlade() { Parent = this };
			//Blade = new LightsaberBlade();

			//PhysicsJoint.Weld
			//	//.From( this, 0 )
			//	.From( PhysicsBody, new Vector3() )
			//	.To( Blade.PhysicsBody, new Vector3() )
			//	.Create();


			//Blade.Rotation = Rotation.From( 90.0f, 0.0f, 0.0f );

			CollisionGroup = CollisionGroup.ConditionallySolid;
			//CollisionGroup = CollisionGroup.Trigger;

			ClearCollisionLayers();
			AddCollisionLayer( CollisionLayer.WINDOW );
			//AddCollisionLayer( CollisionLayer.HAND_ATTACHMENT );
			//AddCollisionLayer( CollisionLayer.Hitbox );

			//SetInteractsAs( CollisionLayer.HAND_ATTACHMENT );
			SetInteractsAs( CollisionLayer.WINDOW );

			//RemoveCollisionLayer( CollisionLayer.WORLD_GEOMETRY );
			//AddCollisionLayer( CollisionLayer.HAND_ATTACHMENT );
			//EnableSolidCollisions = false;

			Blade = new AnimEntity() { Parent = this };
			Blade.SetModel( "models/blade.vmdl" );
			Blade.Rotation = Rotation.From( 90.0f, 0.0f, 0.0f );

			Blade.UseAnimGraph = true;
			//Blade.SetAnimBool( "extend", false );
			//Blade.SetAnimBool( "extend", true );
			//Blade.SetAnimBool( "extend", false );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			BladeSound = PlaySound( "saber.hum" );
			BladeSound.SetVolume( 0.0f );

			//LongClash = PlaySound( "saber.clash_long" );
			//LongClash.SetVolume( 0.0f );

			const float lightDist = 1.0f / numBladeLights;

			for ( int i = 0; i < numBladeLights; i++ )
			{
				float offset = (i + 0.75f) * lightDist;

				var BladeLight = new PointLightEntity() { Parent = this };
				//BladeLight.Position = new Vector3( BladeHeight / 2.0f, 0.0f, 0.0f );
				BladeLight.Position = new Vector3( offset * BladeHeight, 0.0f, 0.0f );
				//BladeLight.Color = new Color( 0x2E67F8FF );
				BladeLight.Color = new Color( 0xFFF8672E );
				BladeLight.Brightness = 0.2f;
				
				BladeLight.EnableShadowCasting = false;
				BladeLight.DynamicShadows = false;

				BladeLight.Enabled = false;

				BladeLights[i] = BladeLight;
			}


			//BladeLight.LinearAttenuation = 1;
			//BladeLight.QuadraticAttenuation = 0.01f;
			//BladeLight.FadeDistanceMin = 8000.0f;
			//BladeLight.FadeDistanceMax = 10000.0f;
		}

		//[ClientRpc]
		//void StartTouchClient( Entity other )
		//{
		//	if ( numTouching++ == 0 && Extend && NetworkIdent < other.NetworkIdent )
		//		LongClash.SetVolume( 1.0f );

		//	Log.Info( "start touch client " + numTouching );
		//}

		//[ClientRpc]
		//void EndTouchClient( Entity other )
		//{
		//	if ( --numTouching == 0 && Extend && NetworkIdent < other.NetworkIdent )
		//		LongClash.SetVolume( 0.0f );

		//	Log.Info( "end touch client " + numTouching );
		//}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other is not Lightsaber saber )
				return;

			//only play the sound for one saber
			if( Extend && NetworkIdent < other.NetworkIdent )
			{
				PlaySound( "saber.clash" );
				touchStartTime = Time.Now;

				//Particles.Create( "particles/saber_spark.vpcf", tr.EndPos );
			}

			if ( numTouching++ == 0 && Extend && NetworkIdent < other.NetworkIdent )
				LongClash = PlaySound( "saber.clash_long" );

			//StartTouchClient( other );
		}

		public override void EndTouch( Entity other )
		{
			base.EndTouch( other );

			if ( other is not Lightsaber saber )
				return;

			const float minEndTime = 0.5f;

			if ( --numTouching == 0 )
			{
				LongClash?.Stop();
				LongClash = null;

				if( Extend && NetworkIdent < other.NetworkIdent && Time.Now - touchStartTime > minEndTime )
					PlaySound( "saber.clash_end" );
			}

			if(numTouching < 0)
			{
				Log.Warning("Negative number of entities touching.");
				numTouching = 0;
			}

			//EndTouchClient( other );
		}

		[Event.Tick]
		public void Tick()
		{
			//Extend = true;
			if ( Extend )
			{
				float vel = Velocity.Length;

				float accel = Math.Abs(vel - LastVelocity);
				LastVelocity = vel;

				
				//const float swing_start = 75.0f;
				//const float swing_stop = 15.0f;
				//const float time_between_swings = 0.4f;

				//if ( !swinging && vel > swing_start && swingDelay > time_between_swings )
				//{
				//	swinging = true;
				//	swingDelay = 0.0f;
				//	PlaySound( "saber.swing" );
				//}

				//if ( swinging && vel < swing_stop )
				//	swinging = false;

				const float swing_start = 30.0f;
				const float time_between_swings = 0.4f;

				if ( accel > swing_start && swingDelay > time_between_swings )
				{
					swingDelay = 0.0f;
					PlaySound( "saber.swing" );
				}

				swingDelay += Time.Delta;


				//Log.Info( "velocity " + Velocity.Length );
				//float targetPitch = 1.0f + Velocity.Length / 400.0f;
				//BladeSound.SetPitch( targetPitch );

				//if(IsClient)
				UpdateDecal();
			}

		}

		//void CreateSurfaceParticle(TraceResult tr)
		//{
		//	var surf = tr.Surface;
		//	string particleName = Rand.FromArray( surf.ImpactEffects.Regular );
		//	if ( string.IsNullOrWhiteSpace( particleName ) ) particleName = Rand.FromArray( surf.ImpactEffects.Bullet );

		//	surf = surf.GetBaseSurface();
		//	while ( string.IsNullOrWhiteSpace( particleName ) && surf != null )
		//	{
		//		particleName = Rand.FromArray( surf.ImpactEffects.Regular );
		//		if ( string.IsNullOrWhiteSpace( particleName ) ) particleName = Rand.FromArray( surf.ImpactEffects.Bullet );

		//		surf = surf.GetBaseSurface();
		//	}

		//	if ( !string.IsNullOrWhiteSpace( particleName ) )
		//	{
		//		var ps = Particles.Create( particleName, tr.EndPos );
		//		ps.SetForward( 0, tr.Normal );
		//	}
		//}

		void UpdateDecal()
		{
			//Log.Info("client " + IsClient);
			//Log.Info("ray " + Position + " - " + Position + Rotation.Forward * SaberHeight );
			var tr = Trace.Ray( Position, Position + Rotation.Forward * BladeHeight )
				.WorldOnly()
				.Run();

			//DebugOverlay.Line( tr.StartPos, tr.EndPos, Color.Orange, 0.1f );

			if ( tr.Hit )
			{
				//CreateSurfaceParticle( tr );

				if ( DecalDefinition.ByPath.TryGetValue( "decals/saber.scorch.decal", out var decal ) )
					decal.PlaceUsingTrace( tr );

				//Log.Info("hit " + Time.Now);
				//DebugOverlay.Line( tr.EndPos, tr.EndPos + tr.Normal * 10.0f, Color.Blue, 0.1f );
			}
		}
	}

	//public partial class LightsaberBlade : AnimEntity
	//{
	//	[Net] bool _extend { get; set; } = false;
	//	public bool Extend
	//	{
	//		get => _extend;
	//		set
	//		{
	//			_extend = value;
	//			SetAnimBool( "extend", _extend );
	//		}
	//	}

	//	public LightsaberBlade()
	//	{
	//	}

	//	public override void Spawn()
	//	{
	//		base.Spawn();

	//		MoveType = MoveType.Physics;
	//		CollisionGroup = CollisionGroup.Interactive;
	//		PhysicsEnabled = true;
	//		UsePhysicsCollision = true;

	//		SetModel( "models/blade.vmdl" );

	//		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
	//		CollisionGroup = CollisionGroup.Prop;


	//		//Extend = true;
	//		UseAnimGraph = true;

	//	}

	//	[Event.Tick]
	//	public void Tick()
	//	{

	//		Extend = true;
	//	}
	//}
}
