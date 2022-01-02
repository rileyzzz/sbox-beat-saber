using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace LightsaberGame
{
	public partial class VRCamera : Camera
	{
		Vector3 lastPos;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Position = pawn.EyePos;
			Rotation = pawn.EyeRot;

			lastPos = Position;
		}

		public override void Update()
		{
			var pawn = Local.Pawn;
			
			if ( pawn == null )
				return;

			var eyePos = pawn.EyePos;
			Position = eyePos;

			//if ( eyePos.Distance( lastPos ) < 300 )
			//{
			//	Position = Vector3.Lerp( eyePos.WithZ( lastPos.z ), eyePos, 20.0f * Time.Delta );
			//}
			//else
			//{
			//	Position = eyePos;
			//}

			Rotation = pawn.EyeRot;

			Viewer = pawn;
			lastPos = Position;

			ZNear = 1;
			ZFar = 8000;
		}
	}
}
