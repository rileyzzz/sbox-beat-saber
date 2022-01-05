using System;
using Sandbox;

namespace BeatSaber
{
	public partial class VisualizerGrid : ModelEntity
	{
		static readonly Model GridModel = Model.Load( "models/grid.vmdl" );
		//static readonly Material GridMaterial = Material.Load( "materials/grid.vmat" );

		Material LocalMaterial;
		Texture DataTexture;

		public VisualizerGrid()
		{

		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( GridModel );

			//DataTexture = Texture.Create( MusicStream.ChannelBufferSize, MusicStream.ChannelBufferSize, ImageFormat.R32F ).WithDynamicUsage().Finish();
			DataTexture = Texture.Create( 256, 256, ImageFormat.R32F ).WithDynamicUsage().Finish();

			//LocalMaterial = GridMaterial.CreateCopy();
			LocalMaterial = Material.Load( "materials/grid.vmat" );
			//LocalMaterial.OverrideTexture("Grid Data", DataTexture);
			//LocalMaterial.OverrideTexture("Grid Data", Texture.Load( "dev/vgui/materials/hud/icon_check.jpg" ) );

			SetMaterialOverride( LocalMaterial );
		}

		float Lerp( float a, float b, float f )
		{
			return a + f * (b - a);
		}

		public void UpdateTexture( float[] x_data, float[] y_data )
		{
			//float[] tex_data = new float[DataTexture.Width * DataTexture.Height];
			byte[] tex_data = new byte[DataTexture.Width * DataTexture.Height * 4];

			for( int y = 0; y < DataTexture.Height; y++ )
			{
				for( int x = 0; x < DataTexture.Width; x++ )
				{
					int idx = y * DataTexture.Width + x;

					float x_t = (float)x / DataTexture.Width;
					float y_t = (float)y / DataTexture.Height;

					x_t = Math.Abs(x_t - 0.5f) * 2.0f;
					int x_target = (int)Lerp(32, MusicStream.ChannelBufferSize / 4, x_t);
					int y_target = (int)Lerp(0, MusicStream.ChannelBufferSize - 1, y_t);

					// this is stupid. we should be able to just reinterpret the damn float data as a byte array but
					// that kinda shit isn't whitelisted. guess the technology isn't there yet /shrug
					float val = (x_data[x_target] / 4.0f + y_data[y_target] * 1.0f) * 5.0f;

					val = Math.Min( val, 40.0f );
					var bytes = BitConverter.GetBytes( val );

					tex_data[idx * 4 + 0] = bytes[0];
					tex_data[idx * 4 + 1] = bytes[1];
					tex_data[idx * 4 + 2] = bytes[2];
					tex_data[idx * 4 + 3] = bytes[3];

					//tex_data[y * DataTexture.Width + x] = x_data[x] + y_data[y];
				}
			}

			DataTexture.Update( tex_data );

			LocalMaterial.OverrideTexture( "g_tGridData", DataTexture );
		}

		//public override void DoRender( SceneObject obj )
		//{
		//	base.DoRender( obj );


		//	var vb = Render.GetDynamicVB();

		//}
	}
}
