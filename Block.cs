using System;
using System.Collections.Generic;
using System.Text;

public class Block
{
	public Byte[] dataBlock;
	public bool dirtyBit;

	public Block(Byte[] tmpBlock)
	{
		dataBlock = tmpBlock;
		dirtyBit = false;
	}
}

