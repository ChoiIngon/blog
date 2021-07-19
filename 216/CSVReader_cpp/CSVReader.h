#ifndef _CSV_READER_H_
#define _CSV_READER_H_

#include <map>
#include <vector>
#include <string>

class CSVReader
{
public:
	class iterator
	{
	public:
		iterator(std::vector<std::vector<std::string>>::const_iterator itr);

		const std::vector<std::string>& operator * () const;
		iterator& operator ++ ();
		iterator& operator ++ (int);
		iterator* operator -> ();
		bool operator != (const iterator& itr) const;
		bool operator == (const iterator& itr) const;

		const std::string& GetValue(int index);
	private:
		std::vector<std::vector<std::string>>::const_iterator row_iterator;
	};

	typedef iterator Row;
public:
	bool ReadFile(const std::string& filePath);
	bool ReadStream(const std::string& stream);
		
	size_t GetRowCount() const;
	const std::vector<std::string>& GetRow(size_t rowIndex) const;
	const std::string& GetCell(int rowIndex, int columnIndex) const;
	const std::vector<std::string>& operator[](size_t rowIndex) const;

	iterator begin() const;
	iterator end() const;
private:
	std::vector<std::vector<std::string>> rows;
};

#endif