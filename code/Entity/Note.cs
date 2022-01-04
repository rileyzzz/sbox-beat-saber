using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeatSaber
{
	public partial class Note : ModelEntity
	{
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

		void Update()
		{
			if(Data.Type == NoteType.Red || Data.Type == NoteType.Blue)
			{
				SetModel( "models/block.vmdl" );
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

		void CreateGib(string name, Mesh mesh)
		{
			var gib = new PropGib
			{
				Position = Position,
				Rotation = Rotation,
				Scale = Scale,
				Invulnerable = 0.2f,
				BreakpieceName = name
			};

			gib.SetModel( Model.Builder.AddMesh( mesh ).AddCollisionSphere( 5.0f ).Create() );
			gib.SetInteractsAs( CollisionLayer.Debris );

			_ = FadeAsync( gib, 1.5f );
		}

		static async Task FadeAsync( Prop gib, float fadeTime )
		{
			fadeTime += Rand.Float( -1, 1 );

			if ( fadeTime < 0.5f )
				fadeTime = 0.5f;

			await gib.Task.DelaySeconds( fadeTime );

			var fadePerFrame = 5 / 255.0f;

			while ( gib.RenderColor.a > 0 )
			{
				var c = gib.RenderColor;
				c.a -= fadePerFrame;
				gib.RenderColor = c;
				await gib.Task.Delay( 20 );
			}

			gib.Delete();
		}

		public void Slice()
		{
			Plane testPlane = new Plane(new Vector3(), new Vector3(0.0f, 1.0f, 0.0f));

			Material SlicedMaterial = Material.Load( "materials/block/block.vmat" );
			Utils.ModelSlicer.SliceModel( GetModel(), SlicedMaterial, testPlane, out Mesh FrontMesh, out Mesh BackMesh );

			SetModel( "" );

			CreateGib("front", FrontMesh);
			CreateGib("back", BackMesh);
		}
	}
}
