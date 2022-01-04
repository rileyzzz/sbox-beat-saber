using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace BeatSaber
{
	public partial class VRWalkController : BasePlayerController
	{
		public VRWalkController()
		{

		}

		public override void FrameSimulate()
		{
			base.FrameSimulate();

			EyeRot = Input.VR.Head.Rotation;
		}

		public override void Simulate()
		{
			base.Simulate();

			EyeRot = Input.VR.Head.Rotation;
		}
	}
}
