using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CSVReader : IEnumerable
{
	private Dictionary<string, int> column_name_to_index = null;
	private List<string> column_names = null;
	private List<List<string>> rows = null;

	public class Row
	{
		public Row(CSVReader reader, List<string> row)
		{
			this.reader = reader;
			this.row = row;
		}

		public string GetValue(string columnName)
		{
			return GetValue(reader.GetIndex(columnName.ToLower()));
		}

		public string GetValue(int index)
		{
			return row[index];
		}

		private List<string> row;
		private CSVReader reader;
	}

	public void ReadStream(string stream)
	{
		StringReader reader = new StringReader(stream);
		char[] trimChar = "\r".ToCharArray();

		// read column name
		{
			string line = reader.ReadLine();
			column_names = new List<string>(line.Trim(trimChar).Split(','));
			column_name_to_index = new Dictionary<string, int>();
			for (int i = 0; i < column_names.Count; ++i)
			{
				if (true == String.IsNullOrEmpty(column_names[i]))
				{
					throw new System.Exception("empty or null column name is not allowed");
				}

				column_names[i] = column_names[i].Trim(trimChar).ToLower();
				column_name_to_index.Add(column_names[i], i);
			}
		}

		// read data type
		{
			reader.ReadLine();
		}

		{
			rows = new List<List<string>>();
			List<string> row = new List<string>();
			int quotes = 0;
			int prev = 0;
			int ch = 0;

			string cell = "";
			while ((ch = reader.Read()) >= 0)
			{
				switch (ch)
				{
					case '"':
						++quotes;
						break;
					case ',':
					case '\n':
						if (0 == quotes || ('"' == prev && 0 == quotes % 2))
						{
							if (2 <= quotes)
							{
								cell = cell.Substring(1);
								cell = cell.Substring(0, cell.Length - 1);
							}
							if (2 < quotes)
							{
								cell = cell.Replace("\"\"", "\"");
							}
							cell = cell.Trim(trimChar);
							row.Add(cell);
							cell = "";
							prev = 0;
							quotes = 0;
							if ('\n' == ch)
							{
								rows.Add(row);
								row = new List<string>();
							}
							continue;
						}
						break;
					default:
						break;
				}
				prev = ch;
				cell += Convert.ToChar(prev);
			}
		}
	}

	public void ReadFile(string fileName)
	{
		FileStream file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

		byte[] buffer = new byte[file.Length];
		file.Read(buffer, 0, buffer.Length);
		file.Close();

		string text = System.Text.Encoding.Default.GetString(buffer, 0, buffer.Length);
		ReadStream(text);
	}

	public int GetIndex(string columnName)
	{
		if (false == column_name_to_index.ContainsKey(columnName))
		{
			return -1;
		}

		return column_name_to_index[columnName];
	}

	public List<string> GetColumnNames()
	{
		return column_names;
	}

	public Row GetRow(int index)
	{
		if (0 > index || rows.Count <= index)
		{
			throw new System.IndexOutOfRangeException();
		}

		Row row = new Row(this, rows[index]);
		return row;
	}

	public int GetRowCount()
	{
		return rows.Count;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		for (int i = 0; i < rows.Count; i++)
		{
			yield return GetRow(i);
		}
	}
}