using Sandbox;
using System;
using System.Collections.Generic;

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
	}
}
