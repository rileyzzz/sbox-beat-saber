using Sandbox;

namespace BeatSaber
{
	[Library( "awesome_pillar" )]
	[Hammer.RenderFields]
	[Hammer.Model( Model = "models/pillar.vmdl", Archetypes = ModelArchetype.static_prop_model )]
	public partial class AwesomePillar : ModelEntity
	{
		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );

			//RenderColor = Color.Blue;
		}
	}
}
