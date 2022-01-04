using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeatSaber
{
	public partial class Note : ModelEntity
	{
		static readonly Model BlockModel = Model.Load( "models/block.vmdl" );

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

		public bool Hit { get; set; } = true;
		public bool SoundPlayed { get; set; } = false;

		public Note()
		{
		}

		float GetDirectionAngle( CutDirection dir )
		{
			switch ( dir )
			{
				case CutDirection.Up:			return 0.0f;
				case CutDirection.Down:			return 180.0f;
				case CutDirection.Left:			return 90.0f;
				case CutDirection.Right:		return -90.0f;
				case CutDirection.UpLeft:		return 45.0f;
				case CutDirection.UpRight:		return -45.0f;
				case CutDirection.DownLeft:		return 135.0f;
				case CutDirection.DownRight:	return -135.0f;
				case CutDirection.Any:			return 0.0f;
			}

			Log.Warning("Invalid direction");
			return 0.0f;
		}

		Vector3 GetDirectionVector( CutDirection dir )
		{
			return Rotation.From( 0.0f, 180.0f, GetDirectionAngle( Data.Direction ) ).Down;
		}

		void Update()
		{
			if(Data.Type == NoteType.Red || Data.Type == NoteType.Blue)
			{
				SetModel( BlockModel );
				Rotation = Rotation.From( 0.0f, 180.0f, GetDirectionAngle(Data.Direction) );

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

		}

		void CreateGib( string name, Mesh mesh, Vector3[] vertices )
		{
			var gib = new PropGib
			{
				Position = Position,
				Rotation = Rotation,
				Scale = Scale,
				Invulnerable = 0.2f,
				BreakpieceName = name
			};

			gib.SetModel( Model.Builder.AddMesh( mesh ).AddCollisionHull(vertices).Create() );
			gib.SetInteractsAs( CollisionLayer.Debris );

			gib.Velocity = GetDirectionVector(Data.Direction) * 500.0f;

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

		static readonly Material sliceMaterial = Material.Load( "materials/block/block.vmat" );

		public void Slice()
		{
			Plane testPlane = new Plane(new Vector3(), Vector3.Random.Normal);
			
			Utils.ModelSlicer.SliceModel( BlockModel, sliceMaterial, testPlane, out Mesh FrontMesh, out Vector3[] FrontVertices, out Mesh BackMesh, out Vector3[] BackVertices );

			SetModel( "" );

			CreateGib( "front", FrontMesh, FrontVertices );
			CreateGib( "back", BackMesh, BackVertices );
		}
	}
}
