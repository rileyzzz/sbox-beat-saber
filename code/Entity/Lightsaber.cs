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


		Sound? LongClash { get; set; } = null;

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

			Log.Info( "saber spawn" );
			Blade = new BladeEntity() { Parent = this };
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
		static readonly Material BlueMaterial = Material.Load( "materials/models/blade_blue.vmat" );
		static readonly Material RedMaterial = Material.Load( "materials/models/blade_red.vmat" );

		[Net, Change] public bool Red { get; set; } = false;


		const int numBladeLights = 4;
		PointLightEntity[] BladeLights = new PointLightEntity[numBladeLights];

		public BladeEntity()
		{

		}

		public void OnRedChanged( bool oldValue, bool newValue )
		{
			SetMaterialOverride( Red ? RedMaterial : BlueMaterial );
		}

		public override void Spawn()
		{
			base.Spawn();

			Log.Info( "blade spawn client " + IsClient );
			//SetModel( BladeModel );
			SetModel( "models/blade.vmdl" );


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
				BladeLight.Color = new Color( Red ? 0xFF2020BA : 0xFFF8672E );
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

			if(other is Note note)
			{
				Vector3 normal = Vector3.Cross( Rotation.Up, Velocity.Normal );
				note.Slice( Position, normal.Normal, Velocity.Normal, Red );
			}
		}
	}
}
