using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace BeatSaber
{
	public partial class Lightsaber : ModelEntity
	{
		//[Net] public AnimEntity Blade { get; set; }
		public BladeEntity Blade { get; set; }

		public VRHand Hand => Owner as VRHand;

		//[Net] public bool Red { get; set; } = false;

		//bool _red = false;
		//public bool Red
		//{
		//	get => _red;
		//	set
		//	{
		//		_red = value;
		//		Blade?.SetMaterialOverride( Red ? RedMaterial : BlueMaterial );
		//	}
		//}

		//const float BladeHeight = 52.0f;
		public const float BladeHeight = 64.0f;

		public Lightsaber()
		{
			Transmit = TransmitType.Always;
		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/sabers/basic_saber.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );


			Log.Info( "saber spawn" );
			
			Blade = Create<BladeEntity>();
			Blade.SetParent( this );

			Blade.Rotation = Rotation.From( 90.0f, 0.0f, 0.0f );
			//Blade.SetMaterialOverride( Red ? RedMaterial : BlueMaterial );

		}

		//public override void ClientSpawn()
		//{
		//	base.ClientSpawn();

		//	//Log.Info( "saber clientspawn" );
		//	//Blade = new BladeEntity() { Parent = this };
		//	//Blade.Rotation = Rotation.From( 90.0f, 0.0f, 0.0f );
		//	//Blade.SetMaterialOverride( Red ? RedMaterial : BlueMaterial );

		//	//const float lightDist = 1.0f / numBladeLights;

		//	//for ( int i = 0; i < numBladeLights; i++ )
		//	//{
		//	//	float offset = (i + 0.75f) * lightDist;

		//	//	var BladeLight = new PointLightEntity() { Parent = this };
		//	//	//BladeLight.Position = new Vector3( BladeHeight / 2.0f, 0.0f, 0.0f );
		//	//	BladeLight.Position = new Vector3( offset * BladeHeight, 0.0f, 0.0f );
		//	//	//BladeLight.Color = new Color( 0x2E67F8FF );
		//	//	BladeLight.Color = new Color( Red ? 0xFF2020BA : 0xFFF8672E );
		//	//	BladeLight.Brightness = 0.2f;
				
		//	//	BladeLight.EnableShadowCasting = false;
		//	//	BladeLight.DynamicShadows = false;

		//	//	BladeLight.Enabled = false;

		//	//	BladeLights[i] = BladeLight;
		//	//}


		//	//BladeLight.LinearAttenuation = 1;
		//	//BladeLight.QuadraticAttenuation = 0.01f;
		//	//BladeLight.FadeDistanceMin = 8000.0f;
		//	//BladeLight.FadeDistanceMax = 10000.0f;
		//}


		[Event.Tick]
		public void Tick()
		{
			if ( !IsClient )
				return;

			//Log.Info("saber position " + Position);

			//Log.Info("lightsaber trace");
			var tr = Trace.Ray( Position, Position + Rotation.Forward * BladeHeight )
				.WorldOnly()
				.Run();

			//DebugOverlay.TraceResult( tr );
			if ( tr.Hit )
			{
				//if ( tr.Entity is Note note )
				//{
				//	Vector3 normal = Vector3.Cross( Rotation.Forward, Velocity.Normal );
				//	note.Slice( Position, normal.Normal, Velocity.Normal );
				//}
				//else
				{

					if ( DecalDefinition.ByPath.TryGetValue( "decals/saber.scorch.decal", out var decal ) )
						decal.PlaceUsingTrace( tr );
				}
			}
		}
	}

	public partial class BladeEntity : AnimEntity
	{
		Lightsaber Saber => Parent as Lightsaber;

		static readonly Model BladeModel = Model.Load( "models/blade.vmdl" );
		static readonly Material BlueMaterial = Material.Load( "materials/models/blade_blue.vmat" );
		static readonly Material RedMaterial = Material.Load( "materials/models/blade_red.vmat" );


		// this is awful. we really don't need two red variables
		// but spawn gets called really weirdly
		[Net] bool _red { get; set; } = false;

		public bool Red
		{
			get => _red;
			set
			{
				_red = value;

				SetMaterialGroup( Red ? 1 : 0 );

				foreach ( var light in BladeLights )
					light.Color = new Color( Red ? 0xFF2020BA : 0xFFF8672E );
			}
		}


		const int numBladeLights = 4;
		PointLightEntity[] BladeLights = new PointLightEntity[numBladeLights];

		public BladeEntity()
		{

		}

		//public void OnRedChanged( bool oldValue, bool newValue )
		//{
		//	//Log.Info( "red changed " + IsClient + " " + Red );
		//	//SetMaterialOverride( Red ? RedMaterial : BlueMaterial );
		//}

		public override void Spawn()
		{
			base.Spawn();

			Log.Info( "blade spawn client " + IsClient );
			//SetModel( BladeModel );
			Model = BladeModel;

			//SetMaterialGroup( Saber.Red ? 1 : 0 );

			SetupPhysicsFromModel( PhysicsMotionType.Static );
			CollisionGroup = CollisionGroup.Trigger;

			const float lightDist = 1.0f / numBladeLights;

			for ( int i = 0; i < numBladeLights; i++ )
			{
				float offset = (i + 0.75f) * lightDist;

				var BladeLight = new PointLightEntity() { Parent = this };
				//BladeLight.Position = new Vector3( BladeHeight / 2.0f, 0.0f, 0.0f );
				BladeLight.Position = new Vector3( offset * Lightsaber.BladeHeight, 0.0f, 0.0f );
				//BladeLight.Color = new Color( 0x2E67F8FF );
				//BladeLight.Color = new Color( Saber.Red ? 0xFF2020BA : 0xFFF8672E );
				BladeLight.Brightness = 0.2f;

				BladeLight.EnableShadowCasting = false;
				BladeLight.DynamicShadows = false;

				BladeLight.Enabled = false;

				BladeLights[i] = BladeLight;
			}

			UseAnimGraph = true;
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( !IsClient )
				return;

			var hand = Saber.Hand.Hand;

			if ( other is Note note && note.CanSlice )
			{
				Vector3 normal = Vector3.Cross( Rotation.Up, hand.Velocity.Normal );
				note.Slice( Position, normal.Normal, hand.Velocity.Normal, Red );

				//var p = Particles.Create( "particles/saber_slice.vpcf", this, false );
				var p = Particles.Create( "particles/saber_slice.vpcf", this );

				// set particle color
				var color = Red ? Color.Red : Color.Blue;
				p.SetPosition( 1, new Vector3( color.r, color.g, color.b ) );

				//var p = Particles.Create( "particles/saber_slice.vpcf", this, "particles" );

			}
		}

		//[Event.Tick]
		//void DrawPlane()
		//{
		//	if ( IsClient )
		//		return;


		//	Vector3 normal = Vector3.Cross( Rotation.Up, Saber.Hand.Hand.Velocity.Normal );
		//	DebugDrawPlane( new Plane( Position, normal ) );

		//	//Log.Info( "vel " + Velocity );
		//	DebugOverlay.Line( Position, Position + normal * 20.0f, Color.Blue );
		//}

		//void DebugDrawPlane( Plane plane )
		//{
		//	Vector3 up = Vector3.Cross( plane.Normal, new Vector3( 0.0f, 1.0f, 0.0f ) ).Normal;
		//	Vector3 right = Vector3.Cross( plane.Normal, up ).Normal;

		//	int gridSize = 8;

		//	float gridScale = 10.0f;

		//	up *= gridScale;
		//	right *= gridScale;

		//	for ( int x = -gridSize; x <= gridSize; x++ )
		//		DebugOverlay.Line( plane.Origin + right * x - up * gridSize, plane.Origin + right * x + up * gridSize, Color.Green, 100.0f );

		//	for ( int y = -gridSize; y <= gridSize; y++ )
		//		DebugOverlay.Line( plane.Origin + up * y - right * gridSize, plane.Origin + up * y + right * gridSize, Color.Green, 100.0f );
		//}
	}
}
