using System;

public struct VectInt3
{
	public int x,y,z;

	public VectInt3( int x, int y, int z )
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public static void Iterate3D( int offset, int length, PositionAction action )
	{
		int end = length + offset;

		for( int z = offset; z < end; ++z )
		{
			for( int y = offset; y < end; ++y )
			{
				for( int x = offset; x < end; ++x )
				{
					action( new VectInt3 (x, y, z) );
				}
			}
		}
	}

	public VectInt3 WrapTo( int offset, int length )
	{
		return new VectInt3 (
			mod((x - offset), length) + offset,
			mod((y - offset), length) + offset,
			mod((z - offset), length) + offset
		);
	}

	private static int mod(int value, int modulo)
	{
		return (value%modulo + modulo)%modulo;
	}

	public static VectInt3 operator +( VectInt3 vectInt3a, VectInt3 vectInt3b) 
	{
		return new VectInt3( (vectInt3a.x + vectInt3b.x), (vectInt3a.y + vectInt3b.y), (vectInt3a.z + vectInt3b.z) );
	} 
}