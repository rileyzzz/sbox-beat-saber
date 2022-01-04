using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sandbox;

namespace Utils
{
	class VertexEquality : EqualityComparer<Vertex>
	{
		public bool Vector4Equals( Vector4 a, Vector4 b )
		{
			return a.x == b.x &&
				a.y == b.y &&
				a.z == b.z &&
				a.w == b.w;
		}

		public override bool Equals( Vertex x, Vertex y )
		{
			return x.Position == y.Position &&
				x.Normal == y.Normal &&
				Vector4Equals( x.TexCoord0, y.TexCoord0 );
		}

		public override int GetHashCode( [DisallowNull] Vertex v )
		{
			int hash = v.Position.GetHashCode();
			hash ^= v.Normal.GetHashCode();
			hash ^= v.TexCoord0.GetHashCode();
			return hash;
		}
	}

	public class SlicedModel
	{
		public List<Vertex> Vertices = new();
		public List<int> Indices = new();

		public Dictionary<Vertex, int> IndexMap = new Dictionary<Vertex, int>( new VertexEquality() );

		//public VertexAttribute[] Layout;

		//public SlicedModel(VertexAttribute[] layout)
		//{
		//	//Layout = layout;
		//}

		public void AddTriangle( Vertex a, Vertex b, Vertex c )
		{
			//return;
			Indices.Add( GetOrCreateIndex(a) );
			Indices.Add( GetOrCreateIndex(b) );
			Indices.Add( GetOrCreateIndex(c) );
		}

		public void AddQuad( Vertex a, Vertex b, Vertex c, Vertex d )
		{
			//Indices.Add( GetOrCreateIndex( a ) );
			//Indices.Add( GetOrCreateIndex( b ) );
			//Indices.Add( GetOrCreateIndex( c ) );

			//Indices.Add( GetOrCreateIndex( b ) );
			//Indices.Add( GetOrCreateIndex( c ) );
			//Indices.Add( GetOrCreateIndex( d ) );

			Indices.Add( GetOrCreateIndex( c ) );
			Indices.Add( GetOrCreateIndex( b ) );
			Indices.Add( GetOrCreateIndex( a ) );

			Indices.Add( GetOrCreateIndex( b ) );
			Indices.Add( GetOrCreateIndex( c ) );
			Indices.Add( GetOrCreateIndex( d ) );
		}

		int GetOrCreateIndex(Vertex v)
		{
			if ( IndexMap.TryGetValue( v, out var index ) )
				return index;

			int count = Vertices.Count;
			Vertices.Add( v );
			IndexMap[v] = count;

			return count;
		}

		public Mesh ToMesh(Material material)
		{
			var mesh = new Mesh(material);

			mesh.CreateVertexBuffer(Vertices.Count, Vertex.Layout, Vertices);
			mesh.CreateIndexBuffer(Indices.Count, Indices);

			return mesh;
		}
	}

	public static class ModelSlicer
	{

		public static void SliceModel(Model model, Material material, Plane plane, out Mesh FrontMesh, out Mesh BackMesh)
		{
			Vertex[] vertices = model.GetVertices();
			uint[] indices = model.GetIndices();

			SlicedModel Front = new();
			SlicedModel Back = new();

			//loop through each triangle
			for ( int i = 0; i + 2 < indices.Length; i += 3 )
			{
				uint a_index = indices[i + 0];
				uint b_index = indices[i + 1];
				uint c_index = indices[i + 2];

				Vertex a = vertices[a_index];
				Vertex b = vertices[b_index];
				Vertex c = vertices[c_index];

				bool[] infront = new bool[] {
					plane.IsInFront( a.Position ),
					plane.IsInFront( b.Position ),
					plane.IsInFront( c.Position )
				};

				int numInFront = 0;
				foreach(var front in infront)
				{
					if ( front ) numInFront++;
				}


				if ( infront[0] && infront[1] && infront[2] )
					Front.AddTriangle( a, b, c );
				else if ( !infront[0] && !infront[1] && !infront[2] )
					Back.AddTriangle( a, b, c );
				else
				{
					// literal edge cases. wow!
					var triangle = new Vertex[] { a, b, c };

					var triangleDest = numInFront == 1 ? Front : Back;
					var quadDest = numInFront == 2 ? Front : Back;

					int splitVertex = 0;
					for( int idx = 0; idx < 3; idx++ )
					{
						if((numInFront == 1 && infront[idx]) || (numInFront == 2 && !infront[idx]))
						{
							splitVertex = idx;
							break;
						}	
					}

					SliceTriangle(new Vertex[] { a, b, c }, splitVertex, plane, out Vertex[] o_triangle, out Vertex[] o_quad );

					triangleDest.AddTriangle( o_triangle[0], o_triangle[1], o_triangle[2]);
					quadDest.AddQuad( o_quad[0], o_quad[1], o_quad[2], o_quad[3]);
				}
			}

			//Material mat = Material.Load( "dev/helper/testgrid.vmat" );
			FrontMesh = Front.ToMesh( material );
			BackMesh = Back.ToMesh( material );
		}

		static void SliceTriangle(Vertex[] triangle, int splitVertex, Plane plane, out Vertex[] a_triangle, out Vertex[] bc_quad)
		{
			int a = splitVertex;
			int b = splitVertex + 1;
			if ( b > 2 ) b = 0;
			int c = splitVertex - 1;
			if ( c < 0 ) c = 2;

			Vertex split1 = SplitEdge(triangle[a], triangle[b], plane);
			Vertex split2 = SplitEdge(triangle[a], triangle[c], plane);

			a_triangle = new Vertex[] { triangle[a], split1, split2 };
			//bc_quad = new Vertex[] { split1, split2, triangle[c], triangle[b] };
			//bc_quad = new Vertex[] { split1, triangle[c], split2, triangle[b] };
			bc_quad = new Vertex[] { split1, split2, triangle[b], triangle[c] };
			//bc_quad = new Vertex[] { split1, split2, triangle[c], triangle[b] };
		}

		static Vertex SplitEdge(Vertex a, Vertex b, Plane plane )
		{
			Vector3 intersect = LinePlaneIntersect(a.Position, b.Position, plane, out float t);

			Vertex v = new();
			v.Position = intersect;
			v.Normal = Vector3.Lerp(a.Normal, b.Normal, t);
			v.TexCoord0 = Vector4.Lerp(a.TexCoord0, b.TexCoord0, t);
			v.Tangent = Vector4.Lerp(a.Tangent, b.Tangent, t);

			return v;
		}

		static Vector3 LinePlaneIntersect(Vector3 a, Vector3 b, Plane plane, out float t)
		{
			//Ray r = new Ray(a, b - a);

			////TODO speed this up
			//Vector3 intersect = plane.Trace( r, true ) ?? default;
			//t = (intersect - a).Length / (b - a).Length;
			//return intersect;

			Vector3 coord = a;
			Vector3 ray = b - a;

			float d = (coord - plane.Origin).Dot( plane.Normal );
			float x = ray.Dot( plane.Normal );

			t = d / x;

			return coord - ray * t;
		}
	}
}
