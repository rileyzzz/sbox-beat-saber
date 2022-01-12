using System;
using System.Collections.Generic;
using Sandbox;

namespace BeatSaber
{
	public class Obstacle : ModelEntity
	{
		static readonly Material ObstacleMaterial = Material.Load( "materials/obstacle.vmat" );
		static readonly Material ObstacleOutlineMaterial = Material.Load( "materials/obstacle_outline.vmat" );

		BeatSaberObstacle _data;
		public BeatSaberObstacle Data
		{
			get => _data;
			set
			{
				_data = value;
				Update();
			}
		}

		public Obstacle()
		{

		}

		public override void Spawn()
		{
			base.Spawn();


		}

		public void Update()
		{
			CreateMesh();

			//Model.Builder.AddMesh
		}

		public void CreateMesh()
		{
			var unitSize = BeatSaberEnvironment.UnitSize;
			var verticalOffset = BeatSaberEnvironment.VerticalOffset;
			// note speed affects the distance between note blocks
			//var noteSpeed = BeatSaberEnvironment.Current.NoteSpeed;
			var noteDist = BeatSaberEnvironment.Current.DistPerBeat;

			Vector3 offset = new Vector3( 0.0f, 0.0f, (Data.Type == ObstacleType.FullHeight ? 0 : 2 * unitSize) + verticalOffset );
			Vector3 extent = new Vector3( Data.Duration * noteDist, -Data.Width * unitSize, (Data.Type == ObstacleType.FullHeight ? 3 : 1) * unitSize );
			Vector3 origin = extent / 2 + offset;

			var mesh = new Mesh( ObstacleMaterial );

			{
				var vb = new VertexBuffer();
				vb.Init( true );
				vb.AddCube( origin, extent, new Rotation() );

				mesh.CreateBuffers( vb );
			}

			var outlineMesh = new Mesh( ObstacleOutlineMaterial );
			const float outlineThickness = 0.5f;

			{
				var vb = new VertexBuffer();
				vb.Init( true );
				vb.AddCube( new Vector3(0.0f, 0.0f, origin.z), new Vector3( outlineThickness, outlineThickness, extent.z ), new Rotation() );
				vb.AddCube( new Vector3( extent.x, 0.0f, origin.z ), new Vector3( outlineThickness, outlineThickness, extent.z ), new Rotation() );
				vb.AddCube( new Vector3( 0.0f, extent.y, origin.z ), new Vector3( outlineThickness, outlineThickness, extent.z ), new Rotation() );
				vb.AddCube( new Vector3( extent.x, extent.y, origin.z ), new Vector3( outlineThickness, outlineThickness, extent.z ), new Rotation() );

				vb.AddCube( new Vector3( origin.x, 0.0f, offset.z ), new Vector3( extent.x, outlineThickness, outlineThickness ), new Rotation() );
				vb.AddCube( new Vector3( origin.x, extent.y, offset.z ), new Vector3( extent.x, outlineThickness, outlineThickness ), new Rotation() );
				vb.AddCube( new Vector3( origin.x, 0.0f, 3 * unitSize + verticalOffset ), new Vector3( extent.x, outlineThickness, outlineThickness ), new Rotation() );
				vb.AddCube( new Vector3( origin.x, extent.y, 3 * unitSize + verticalOffset ), new Vector3( extent.x, outlineThickness, outlineThickness ), new Rotation() );

				vb.AddCube( new Vector3( 0.0f, origin.y, offset.z ), new Vector3( outlineThickness, extent.y, outlineThickness ), new Rotation() );
				vb.AddCube( new Vector3( extent.x, origin.y, offset.z ), new Vector3( outlineThickness, extent.y, outlineThickness ), new Rotation() );
				vb.AddCube( new Vector3( 0.0f, origin.y, 3 * unitSize + verticalOffset ), new Vector3( outlineThickness, extent.y, outlineThickness ), new Rotation() );
				vb.AddCube( new Vector3( extent.x, origin.y, 3 * unitSize + verticalOffset ), new Vector3( outlineThickness, extent.y, outlineThickness ), new Rotation() );

				outlineMesh.CreateBuffers( vb );
			}

			Model = Model.Builder.AddMesh( mesh ).AddMesh(outlineMesh).AddCollisionBox( extent / 2, origin ).Create();
			SetupPhysicsFromModel( PhysicsMotionType.Static );
		}
	}
}
