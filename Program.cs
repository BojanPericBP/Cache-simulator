using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;


class Program
{
	static void ucitajInstrukcijeIzFajla(params Core[] jezgra)
	{
		try
		{
			StreamReader sr = new StreamReader("dat.txt");
			string line;
			int i = 0;
			while((line = sr.ReadLine()) != null)
				jezgra[i++ % jezgra.Length].nizInstrukcija.Add(line);
			sr.Close();
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
		}
	}

	static public void pokreniSimulaciju(int id, params Core[] jezgra)
	{
		foreach (Core c in jezgra)
		{
			c.nizCpuJezgra = jezgra;
			c.izvrsavajInstrukcije(id);
		}
	}

	static void Main(string[] args)
	{
		int velicinaRama = 0;
		int velicinaKesa = 0;
		int velicinaBloka = 0;
		int way = 0;

		ArrayList powOfTwo = new ArrayList();
		for (int i = 1; i < 13; i++)
		{
			powOfTwo.Add((int)Math.Pow(2, i));
		}

		Console.WriteLine("Unesite velicinu RAM-a u bajtima [" + powOfTwo[2]+ " - " + powOfTwo[powOfTwo.Count- 1] + "]:");

		try
		{
			//-----RAM-----------------------------------
			velicinaRama = Int16.Parse(Console.ReadLine());
			if (!powOfTwo.Contains(velicinaRama) || velicinaRama < 8)
			{
				throw new Exception("Nije u opsegu ili nije stepen broja 2.");
			}
			byte[] RAM = new Byte[velicinaRama];
			for (int i = 0; i < velicinaRama; i++)
			{
				RAM[i] = (byte)(i);
			}


			//-----velicina Kesa-----------------------------------
			Console.WriteLine("Unesite velicinu KESa u bajtima (mora biti stepen broja 2): ");
			velicinaKesa = Int16.Parse(Console.ReadLine());
			if (velicinaKesa > velicinaRama || !powOfTwo.Contains(velicinaKesa))
			{
				throw new Exception("Velicina kesa je veca od velicine RAM-a ili nije stepen broja 2.");
			}

			//--------------------------------------

			//------WAY--------------------------------
			Console.WriteLine("Unesite way {1,2,4}: ");
			way = Int16.Parse(Console.ReadLine());
			if (way != 1 && way != 2 && way != 4)
			{
				throw new Exception("Neispravan unos.");
			}
			//--------------------------------------

			//-----velicina BLOKA-----------------------------------
			Console.WriteLine("Unesite velicinu bloka u bajtima, mora biti manji ili jednak velicini kesa ( " + velicinaKesa + "B ): ");
			velicinaBloka = Int16.Parse(Console.ReadLine());

			if (!powOfTwo.Contains(velicinaBloka) || velicinaBloka * way > velicinaKesa)
			{
				throw new Exception("Nemoguce kreirati kes sa unesenim vrijednostima.");
			}
			//--------------------------------------

			//ZA JEDNO CPUJEZGRO
			/*Core c1 = new Core(velicinaBloka, velicinaKesa, RAM, way, velicinaRama);
			Thread t1 = new Thread(() => pokreniSimulaciju(Thread.GetCurrentProcessorId(),c1));
			ucitajInstrukcijeIzFajla(c1);
			t1.Start();*/

			//ZA DVA CPUJEZGRA (odkomentarisati c2.join();)
			Core c1 = new Core(velicinaBloka, velicinaKesa, RAM, way, velicinaRama);
			Core c2=new Core(c1);
			ucitajInstrukcijeIzFajla(c1,c2);
			Thread t1 = new Thread(() => pokreniSimulaciju(Thread.GetCurrentProcessorId(),c1,c2));
			Thread t2 = new Thread(() => pokreniSimulaciju(Thread.GetCurrentProcessorId(),c1,c2));
			t1.Start();
			t2.Start();
			try
		{
			t1.Join();
			t2.Join();
		}
		catch (Exception e) { Console.WriteLine(e.Message); }


		Console.WriteLine("----------------------------------");
		Console.WriteLine("Ukupno: Kes pogodaka: " + Cache.kesHit + ", Kes promasaja: " + Cache.kesMiss);
		Console.WriteLine("----------------------------------");
		Console.WriteLine("Ram na kraju:");

		foreach (Byte e in RAM)
			Console.WriteLine(e);

	}
	catch (Exception e)
	{
		Console.WriteLine(e.Message);
	}
	}
}
