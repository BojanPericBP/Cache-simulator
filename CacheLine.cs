using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;


public class CacheLine
{
	public static int brPodatkaUBloku;
	public ConcurrentDictionary<short, Block> nizWayeva;
	public List<short> lru;
	public  static byte POC_VR = 0;

	public CacheLine(int way, int brPodatkaUBlokuarg)
	{
		brPodatkaUBloku = brPodatkaUBlokuarg;

		nizWayeva = new ConcurrentDictionary<short,Block>();
		lru = new List<short>(way);

		for (short i = 1; i < way + 1; ++i)
		{
			lru.Add((short)(-i));
			Byte[] tmpBlok = new Byte[brPodatkaUBloku];
			for (int j = 0; j < tmpBlok.Length; ++j)
			{
				tmpBlok[j] = POC_VR;
			}
			nizWayeva.TryAdd((short)(-i), new Block(tmpBlok));
		}

	}
}
