#include "CSVReader.h"
#include <fstream>
#include <streambuf>
#include <algorithm>
#include <sstream>

CSVReader::iterator::iterator(std::vector<std::vector<std::string>>::const_iterator itr)
	: row_iterator(itr)
{
}

const std::string& CSVReader::iterator::GetValue(int index)
{
	const std::vector<std::string>& row = (*row_iterator);
	if (0 > index || row.size() <= index)
	{
		throw std::out_of_range("invalid csv column index:" + std::to_string(index));
	}
	return row[index];
}

const std::vector<std::string>& CSVReader::iterator::operator * () const
{
	return *row_iterator;
}

CSVReader::iterator& CSVReader::iterator::operator ++ ()
{
	row_iterator++;
	return *this;
}

CSVReader::iterator& CSVReader::iterator::operator ++ (int)
{
	row_iterator++;
	return *this;
}

CSVReader::iterator* CSVReader::iterator::operator -> ()
{
	return this;
}

bool CSVReader::iterator::operator != (const CSVReader::iterator& itr) const
{
	if (row_iterator != itr.row_iterator)
	{
		return true;
	}
	return false;
}

bool CSVReader::iterator::operator == (const CSVReader::iterator& itr) const
{
	if (row_iterator == itr.row_iterator)
	{
		return true;
	}
	return false;
}

bool CSVReader::ReadFile(const std::string& filePath)
{
	std::ifstream file(filePath);
	if (true == file.fail())
	{
		throw std::ifstream::failure("fail to open file(path:" + filePath + ")");
	}

	std::string stream((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());
	return ReadStream(stream);
}

bool CSVReader::ReadStream(const std::string& str)
{
	std::stringstream stream(str);
	std::string line;
	std::string cell;

	char utf8bom[3] = {};
	std::streamsize size = stream.readsome(utf8bom, 3);
	if(3 == size)
	{
		if ((char)0xEF == utf8bom[0] && (char)0xBB == utf8bom[1] && (char)0xBF == utf8bom[2])
		{
			stream.seekg(3);
		}
	}
	
	// read data
	{
		std::vector<std::string> row;
		std::streampos pos = stream.tellg();
		int quotes = 0;
		char prev = 0;

		std::string cell;
		for (size_t i = pos; i < str.size(); i++)
		{
			switch (str[i])
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
						cell = cell.substr(1);
						cell = cell.substr(0, cell.size() - 1);
					}
					if (2 < quotes)
					{
						std::string::size_type findPos = 0;
						std::string::size_type offset = 0;
						const std::string pattern("\"\"");
						const std::string replace("\"");
						while ((findPos = cell.find(pattern, offset)) != std::string::npos)
						{
							cell.replace(cell.begin() + findPos, cell.begin() + findPos + pattern.size(), "\"");
							offset = findPos + replace.size();
						}
					}
					row.push_back(cell);
					cell = "";
					prev = 0;
					quotes = 0;
					if ('\n' == str[i])
					{
						rows.push_back(row);
						row.clear();
					}
					continue;
				}
				break;
			default:
				break;
			}
			cell += prev = str[i];
		}
	}
	
	return true;
}

size_t CSVReader::GetRowCount() const
{
	return rows.size();
}

CSVReader::iterator CSVReader::begin() const
{
	return iterator(rows.begin());
}

CSVReader::iterator CSVReader::end() const
{
	return iterator(rows.end());
}

const std::vector<std::string>& CSVReader::GetRow(size_t rowIndex) const
{
	if (0 > rowIndex || rows.size() <= rowIndex)
	{
		throw std::out_of_range("out of range row num:" + std::to_string(rowIndex));
	}
	return rows[rowIndex];
}

const std::vector<std::string>& CSVReader::operator [] (size_t rowIndex) const
{
	return GetRow(rowIndex);
}

const std::string& CSVReader::GetCell(int rowIndex, int columnIndex) const
{
	const std::vector<std::string>& row = GetRow(rowIndex);

	if (0 > columnIndex || row.size() <= columnIndex)
	{
		throw std::out_of_range("invalid csv column index:" + std::to_string(columnIndex));
	}
	return row[columnIndex];
}