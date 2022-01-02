using System;

namespace Sandbox
{
	public struct Memory<T>
	{
		T[] arr;

		public Memory( T[] _arr )
		{
			arr = (T[])_arr.Clone();
		}

		public Memory( T[] _arr, int start, int length )
		{
			arr = new T[length];
			Array.Copy( _arr, start, arr, 0, length );
		}

		public Memory<T> Slice(int start)
		{
			return new Memory<T>(arr, start, arr.Length - start);
		}

		public Memory<T> Slice(int start, int length)
		{
			return new Memory<T>( arr, start, length );
		}

		public int Length => arr.Length;

		public T[] Span => arr;

		public static Memory<T> Empty => new Memory<T>( new T[0] );
	}
}
