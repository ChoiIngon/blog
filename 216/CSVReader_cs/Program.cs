using System;

namespace CSVReader_cs
{
	class Program
	{
		static void Main(string[] args)
		{
			CSVReader reader = new CSVReader();
			reader.ReadFile("../../../Example.csv");

			foreach (var itr in reader)
			{
				Console.Write(itr.ToString());
			}
		}
	}
}
