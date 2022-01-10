using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeatSaber
{
	//public partial class Note : ModelEntity
	public partial class Note : AnimEntity
	{
		static readonly Model BlockModel = Model.Load( "models/block.vmdl" );
		static readonly Model BombNoteModel = Model.Load( "models/bomb_note.vmdl" );

		BeatSaberNote _data;
		public BeatSaberNote Data
		{
			get => _data;
			set
			{
				_data = value;
				Update();
			}
		}

		public bool Hit { get; set; } = false;
		public bool SoundPlayed { get; set; } = false;

		bool GibsCreated = false;
		Plane SlicePlane;
		Vector3 SliceVelocity;
		static readonly Material SliceMaterial = Material.Load( "materials/block/block.vmat" );

		public Note()
		{
		}

		float GetDirectionAngle( CutDirection dir )
		{
			switch ( dir )
			{
				case CutDirection.Down:			return 0.0f;
				case CutDirection.Up:			return 180.0f;
				case CutDirection.Left:			return -90.0f;
				case CutDirection.Right:		return 90.0f;
				case CutDirection.UpLeft:		return -135.0f;
				case CutDirection.UpRight:		return 135.0f;
				case CutDirection.DownLeft:		return -45.0f;
				case CutDirection.DownRight:	return 45.0f;
				case CutDirection.Any:			return 0.0f;
			}

			Log.Warning("Invalid direction");
			return 0.0f;
		}

		Rotation GetDirectionRotation( CutDirection dir )
		{
			return Rotation.FromRoll( GetDirectionAngle( Data.Direction ) );
		}

		//Vector3 GetDirectionVector( CutDirection dir )
		//{
		//	return Rotation.From( 0.0f, 180.0f, GetDirectionAngle( Data.Direction ) ).Down;
		//}

		protected override void OnSequenceFinished( bool looped )
		{
			base.OnSequenceFinished( looped );


		}

		void Update()
		{
			if(Data.Type == NoteType.Red || Data.Type == NoteType.Blue)
			{
				Model = BlockModel;
				SetupPhysicsFromModel( PhysicsMotionType.Static );
				CurrentSequence.Name = "enter_anim";

				//SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

				//this causes a crash for whatever reason
				//CollisionGroup = CollisionGroup.Trigger;

				Rotation = Rotation.FromYaw( 180.0f ) * GetDirectionRotation( Data.Direction );

				bool red = Data.Type == NoteType.Red;
				bool angle = Data.Direction != CutDirection.Any;

				int matgroup = 0;
				if ( angle )
					matgroup = red ? 1 : 2;
				else
					matgroup = red ? 3 : 4;

				//string matgroup = Data.Type == NoteType.Red ? "red_" : "blue_";

				SetMaterialGroup(matgroup);
				//Scale = BeatSaberEnvironment.UnitSize / 100.0f;
			}
			else if ( Data.Type == NoteType.Bomb )
			{
				Model = BombNoteModel;
				SetupPhysicsFromModel( PhysicsMotionType.Static ); SetupPhysicsFromModel( PhysicsMotionType.Static );
				CurrentSequence.Name = "enter_anim";

				Rotation = Rotation.FromYaw( 180.0f );
			}
			else
				Log.Warning("Unknown note type!");
		}

		void CreateGib( string name, Mesh mesh, Vector3[] vertices, Vector3 velocity )
		{
			//either AddMesh or AddCollisionHull likes to crash if the mesh is really small

			if ( vertices.Length <= 3 )
				return;

			var gib = new PropGib
			{
				Position = Position,
				Rotation = Rotation,
				Scale = Scale,
				Invulnerable = 0.2f,
				BreakpieceName = name
			};

			gib.Model = Model.Builder.AddMesh( mesh ).AddCollisionHull(vertices).Create();
			gib.SetInteractsAs( CollisionLayer.Debris );

			//gib.Velocity = GetDirectionVector(Data.Direction) * 500.0f;
			gib.Velocity = velocity.Normal * 300.0f;

			_ = FadeAsync( gib, 0.2f );
		}

		static async Task FadeAsync( Prop gib, float fadeTime )
		{
			//fadeTime += Rand.Float( -1, 1 );

			if ( fadeTime < 0.2f )
				fadeTime = 0.2f;

			await gib.Task.DelaySeconds( fadeTime );

			var fadePerFrame = 10 / 255.0f;

			while ( gib.RenderColor.a > 0 )
			{
				var c = gib.RenderColor;
				c.a -= fadePerFrame;
				gib.RenderColor = c;
				await gib.Task.Delay( 20 );
			}

			gib.Delete();
		}

		public void Slice( Vector3 origin, Vector3 normal, Vector3 velocity, bool red )
		{
			Hit = true;

			SlicePlane = new Plane( Transform.PointToLocal(origin), Transform.NormalToLocal(normal) );
			SliceVelocity = velocity;
		}

		[Event.Tick]
		void Tick()
		{
			// we can't do this in slice because that happens in the physics thread
			// seems like creating props there likes to crash the game
			if(Hit && !GibsCreated)
			{
				GibsCreated = true;

				Utils.ModelSlicer.SliceModel( Model, SliceMaterial, SlicePlane, out Mesh FrontMesh, out Vector3[] FrontVertices, out Mesh BackMesh, out Vector3[] BackVertices );

				SetModel( "" );
				EnableAllCollisions = false;


				if ( FrontMesh != null ) CreateGib( "front", FrontMesh, FrontVertices, SliceVelocity );
				if ( BackMesh != null ) CreateGib( "back", BackMesh, BackVertices, SliceVelocity );
			}
		}

		//void DebugDrawPlane(Plane plane)
		//{
		//	Vector3 up = Vector3.Cross(plane.Normal, new Vector3(0.0f, 1.0f, 0.0f)).Normal;
		//	Vector3 right = Vector3.Cross(plane.Normal, up).Normal;

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
