using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

class Core
{
	public List<String> nizInstrukcija;
	public Cache cache;
	public Byte[] RAM;
	public int way;
	public int duzinaAdrese;
	public int velicinaRama;
	public int velicinaBloka;
	public Core[] nizCpuJezgra;

	public Core(int velicinaBloka, int velicinaKesa, Byte[] ram, int way, int velicinaRama)
	{
		duzinaAdrese = (int)(Math.Log(velicinaRama) / Math.Log(2));
		Cache.brBitaOffset = (int)(Math.Log(velicinaBloka) / Math.Log(2));
		Cache.brSetova = velicinaKesa / (velicinaBloka * way);
		Cache.brBitaIndex = (int)(Math.Log(Cache.brSetova) / Math.Log(2));
		Cache.brBitaTaga = duzinaAdrese - Cache.brBitaIndex - Cache.brBitaOffset;

		this.velicinaBloka = velicinaBloka;
		this.RAM = ram;
		this.way = way;
		this.velicinaRama = velicinaRama;
		cache = new Cache(ram, way, velicinaBloka);

		nizInstrukcija = new List<String>();
	}

	public Core(Core c2)
	{
		this.RAM = c2.RAM;
		this.way = c2.way;
		this.duzinaAdrese = c2.duzinaAdrese;
		this.velicinaRama = c2.velicinaRama;
		this.velicinaBloka = c2.velicinaBloka;
		cache = new Cache(RAM, this.way, velicinaBloka);
		nizInstrukcija = new List<string>();
	}

	String shortToBinary(short adresa)
	{
		//String s = Int32.ToBinaryString(adresa);
		String s = Convert.ToString(adresa, 2);
		//StringBuffer sb = new StringBuilder(s);//nije tred safe

		while (s.Length < duzinaAdrese)
		{
			//sb.Insert(0, '0');
			s = '0' + s;
		}
		return s;
	}

	short odrediTagZaIzbacivanje(ICollection<short> tagoviUKesu, short realSet)
	{
		List<short> tagovi = new List<short>(tagoviUKesu);
		short tmpTag = 0;
		for (int i = 0; i < nizInstrukcija.Count && tagovi.Count > 0; ++i)
		{
			String adresa = shortToBinary(Int16.Parse(nizInstrukcija[i].Trim().Split(" ")[0]));
			tmpTag = odrediTagIzAdrese(adresa);
			short tmpSet = odrediSetIzAdrese(adresa);
			if (tmpSet == realSet)
			{ tagovi.RemoveAll(item => item == tmpTag); }
		}

		if (tagovi.Count > 0)
		{
			if (tagovi.Contains((short)-1))
			{
				return -1;
			}
			else if (tagovi.Contains((short)-2))
			{
				return -2;
			}
			else if (tagovi.Contains((short)-3))
			{
				return -3;
			}
			else if (tagovi.Contains((short)-4))
			{
				return -4;
			}
			return (short)tagovi[0];
		}
		else
		{
			return tmpTag;
		}
	}
	
	public void izvrsavajInstrukcije(int id)
	{
		ispisiKes(id);
		Console.WriteLine("Pocetno stanje kesa.");
		short tmpTag = 0;
		short tmpSet = -1;
		Boolean jednaLinija = false;
		while (nizInstrukcija.Count != 0)
		{
			String[] linija = (nizInstrukcija[0].Trim()).Split(" ");
			nizInstrukcija.RemoveAt(0);
			if (linija.Length != 2)
			{
				Console.WriteLine("------------------------------------------------------");
				Console.Write("\nGreska u komandi: ");//puca
				foreach (String s in linija)
				{
					Console.Write(s + " ");
				}
				Console.WriteLine("\n");
				continue;
			}
			short adresaTrenutnogPodatka = Int16.Parse(linija[0]);
			String komandaString = linija[1];

			tmpTag = odrediTagIzAdrese(shortToBinary(adresaTrenutnogPodatka));
			tmpSet = odrediSetIzAdrese(shortToBinary(adresaTrenutnogPodatka));

			if (tmpSet == -1)
			{
				tmpSet = 0;
				jednaLinija = true;
			}

			bool isCacheHit = cache.optimalniAlgoritam(tmpTag, tmpSet, odrediTagZaIzbacivanje(cache.nizSetova[tmpSet].nizWayeva.Keys, tmpSet), jednaLinija);///TODO algoritam
																																		  //bool isCacheHit=cache.lruAlgoritam(tmpTag,tmpSet,jednaLinija); //nadje najstariji way, odradi write back i					upise novi blok u kes, ako je kes miss

			if (komandaString.Equals("w"))
			{
				lock(RAM)
				{
					foreach (Core c in nizCpuJezgra)
						c.cache.editujKes(tmpTag, tmpSet, odrediOffsetIzAdrese(shortToBinary(adresaTrenutnogPodatka)));
				}	
			}
			else if (!komandaString.Equals("r"))
			{
				Console.WriteLine("Nepostojeca komanda.");
			}

			//lock(RAM)
			//{
				ispisiKes(id);
				Console.WriteLine("Izvrsena istrukcija: \"" + komandaString + " adresa " + adresaTrenutnogPodatka + "\"" + (isCacheHit ? ", Kes pogodak" : ", Kes promasaj"));
			//}

		}
		cache.writeBack(tmpTag, tmpSet, jednaLinija); //vrati u ram ako je nesto editovano, jer neka nit moze raditi duze od ostalih
	}

	short odrediTagIzAdrese(String adresa)
	{
		String tag = "";
		for (int i = 0; i < Cache.brBitaTaga; ++i)
			tag += adresa[i];
		return (short)Convert.ToInt16(tag, 2);
	}

	short odrediSetIzAdrese(String adresa)
	{
		String set = "";
		for (int i = Cache.brBitaTaga; i < (Cache.brBitaIndex + Cache.brBitaTaga); ++i)
			set += adresa[i];
		if (set == "") return -1;
		return (short)Convert.ToInt16(set, 2);
	}

	short odrediOffsetIzAdrese(String adresa)
	{
		String offset = "";
		for (int i = Cache.brBitaTaga + Cache.brBitaIndex; i < (Cache.brBitaIndex + Cache.brBitaTaga + Cache.brBitaOffset); ++i)
			offset += adresa[i];
		return (short)Convert.ToInt16(offset, 2);
	}

	void printArray(byte[] arr)
    {
		foreach (var e in arr)
			Console.Write(e + " ");
		Console.WriteLine();
    }

	void ispisiKes(int id)
	{
		lock (this)
		{
			Console.WriteLine("------------------------------------------------------");
			Console.WriteLine("Jezgro ID:" + id + " Stanje kesa:");
			int brRazmaka = (CacheLine.brPodatkaUBloku + 1) * 3 + 4;
			String razmak = "";
			for (int i = 0; i < brRazmaka; i++)
			{
				razmak += " ";
			}
			Console.Write("    ");
			for (int i = 0; i < way; ++i)
			{
				Console.Write("way" + i + razmak + "    ");
			}

			Console.WriteLine();

			Console.Write("set ");
			for (int i = 0; i < way; i++)
			{
				Console.Write("" + "tag " + "blok" + razmak);
			}
			Console.WriteLine();

			for (int i = 0; i < cache.nizSetova.Length; ++i)
			{
				Console.Write(" " + i);
				foreach (var e in cache.nizSetova[i].nizWayeva)
				{
					
					Console.Write("  " + (e.Key < 0 ? e.Key.ToString() : "  " + e.Key) + "   " + string.Join(",", e.Value.dataBlock) + (e.Value.dataBlock[e.Value.dataBlock.Length - 2] > 9 ? "    " : "      "));
				}
				Console.WriteLine();
			}
		}
	}
}
