using System;
using System.Collections.Generic;
using System.Text;


public class Cache
{
	static public int brBitaTaga;
	static public int brBitaOffset;
	static public int brBitaIndex;
	static public int brSetova;
	public CacheLine[] nizSetova;
	static public int kesHit;
	static public int kesMiss;
	public Byte[] RAM;
	public int way;

	public Cache(Byte[] ram, int way, int brPodatakaUBloku)
	{
		nizSetova = new CacheLine[brSetova];
		for (int i = 0; i < nizSetova.Length; i++)
		{
			nizSetova[i] = new CacheLine(way, brPodatakaUBloku);
		}
		RAM = ram;
		this.way = way;
	}

	public void writeBack(short tag, short set, bool jednaLinija)
	{
		if (nizSetova[set].nizWayeva[tag].dirtyBit) //provjerata da li je dirty bit 1,ako nije nema upisa nazad u ram
		{
			Byte[] blok2 = nizSetova[set].nizWayeva[tag].dataBlock;

			String adresaPocetka = Convert.ToString(tag,2) + (jednaLinija ? "" : Convert.ToString(set,2));
			for (int i = 0; i < brBitaOffset; ++i)
				adresaPocetka += "0";
			short adresa = (short)Convert.ToInt32(adresaPocetka, 2);

			for (short i = 0; i < blok2.Length - 1; ++i)
				RAM[adresa + i] = blok2[i];
		}
	}

	public bool lruAlgoritam(short noviTag, short set, bool jednaLinija)
	{
		bool uKesu = traziUKesu(noviTag, set);
		if (!uKesu)
		{
			writeBack(nizSetova[set].lru[0], set, jednaLinija); //upisuje prethodni blok u ram
			Byte[] ucitaniBlok = ucitajBlokIzRama(noviTag, set, jednaLinija); //ucitava blok u kojem se nalazi trazeni podatak, jer blok nije u kesu  
			Block b;
			nizSetova[set].nizWayeva.TryRemove(nizSetova[set].lru[0], out b); //uklanja stari blok
			nizSetova[set].nizWayeva.TryAdd(noviTag, new Block(ucitaniBlok));//dodaje novi bloku kes

			nizSetova[set].lru.RemoveAt(0); //skida najstariji sa pocetka
		}
		else
		{
			nizSetova[set].lru.Remove(noviTag); //skida trenutni
		}

		nizSetova[set].lru.Add(noviTag); //stavlja na kraj, kao najnoviji
		return uKesu;
	}

	public bool optimalniAlgoritam(short noviTag, short set, short tagZaIzbacivanje, bool jednaLinija) //ako nadje u kesu nista ne radi
	{
		bool uKesu = traziUKesu(noviTag, set);

		if (!uKesu)
		{
			Block b;
			writeBack(tagZaIzbacivanje, set, jednaLinija); //write back taj koji je izabran
			Byte[] ucitaniBlok = ucitajBlokIzRama(noviTag, set, jednaLinija); //ucita novi blok iz rama
			nizSetova[set].nizWayeva.TryRemove(tagZaIzbacivanje,out b);
			nizSetova[set].nizWayeva.TryAdd(noviTag, new Block(ucitaniBlok));//upisi novi na njegovo mjesto	
		}
		return uKesu;
	}

	Byte[] ucitajBlokIzRama(short tagAdrese, short set, bool jednaLinija)
	{
		String adresaPocetka = Convert.ToString(tagAdrese,2) + (jednaLinija ? "" : Convert.ToString(set,2));
		for (int i = 0; i < brBitaOffset; ++i)
			adresaPocetka += "0";

		short adresa = (short)Convert.ToInt16(adresaPocetka, 2);

		Byte[] tmpBlok = new Byte[CacheLine.brPodatkaUBloku];

		for (short i = 0; i < CacheLine.brPodatkaUBloku; ++i)
			tmpBlok[i] = RAM[adresa++];
		return tmpBlok;
	}

	bool traziUKesu(short tag, short set) //vodi racuna o kes hit i kes miss
	{
		bool flag = nizSetova[set].nizWayeva.ContainsKey(tag);

		lock(RAM)
		{
			if(flag)
				kesHit++;
			else 
				kesMiss++;	
		}
		return flag;	
	}
	public void editujKes(short tag, short set, short offset)
	{
		if (nizSetova[set].nizWayeva.ContainsKey(tag))
		{
			System.Random random = new System.Random();
			nizSetova[set].nizWayeva[tag].dataBlock[offset] = (byte)random.Next(4);
            nizSetova[set].nizWayeva[tag].dirtyBit = true; //edituje validBit na 1	
		}
	}
}