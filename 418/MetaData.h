#ifndef _META_DATA_H_
#define _META_DATA_H_

#include "CSVReader.h"
#include <string>
#include <map>
#include <vector>
#include <functional>
#include <memory>
#include <utility>
#include <cassert>
#include <stdexcept>
#undef min

struct MetaData
{
private :
    struct Header
    {
        int index;
        std::string name;
        std::shared_ptr<Header> child;

        Header();
    };

    struct Cell
    {
        std::shared_ptr<Header> header;
        std::string value;

        Cell();
    };

public :
    template <class META>
    class Reader
    {
    public:
        typedef std::vector<std::shared_ptr<META>> MetaDatas;

        bool Read(const std::string& file)
        {
            CSVReader reader;
            reader.ReadFile(file);

            std::vector<std::shared_ptr<Header>> headers;

            const auto& row = reader[0];    // header row
            for (auto& cell : row)
            {
                headers.push_back(ReadHeader(cell));
            }

            for (size_t rowNum = 2; rowNum < reader.GetRowCount(); rowNum++)
            {
                const auto& row = reader[rowNum];
                std::vector<std::shared_ptr<Cell>> cells;
                for (size_t columnNum = 0; columnNum < row.size(); columnNum++)
                {
                    std::shared_ptr<Cell> cell = std::make_shared<Cell>();
                    cell->header = headers[columnNum];
                    cell->value = row[columnNum];
                    cells.push_back(cell);
                }

                std::shared_ptr<META> meta = std::make_shared<META>();
                meta->Init(cells);
                metadatas.push_back(meta);
            }
            return true;
        }

        typename MetaDatas::iterator begin()
        {
            return metadatas.begin();
        }

        typename MetaDatas::iterator end()
        {
            return metadatas.end();
        }
    private:
        MetaDatas metadatas;
        std::shared_ptr<Header> ReadHeader(const std::string& cellValue)
        {
            std::shared_ptr<Header> root = std::make_shared<Header>();

            std::string column = cellValue;
            std::size_t dotPos = cellValue.find('.');
            if (std::string::npos != dotPos) // cellValue has '.'. it means this column is hierarchy
            {
                column = cellValue.substr(0, dotPos);
            }

            std::size_t braceStartPos = column.find('[');
            std::size_t braceEndPos = column.find(']');

            if ((std::string::npos != braceStartPos && std::string::npos == braceEndPos) ||
                (std::string::npos == braceStartPos && std::string::npos != braceEndPos)
                )
            {
                throw std::runtime_error("column name error:" + cellValue + ", unmatched brace");
            }

            if (std::string::npos != braceStartPos && std::string::npos != braceEndPos) // column has '[' and  ']'. it means column is array
            {
                root->index = std::stoi(column.substr(braceStartPos + 1, braceEndPos - braceStartPos - 1));
            }

            std::size_t columnEndPos = std::min(dotPos, braceStartPos);
            root->name = column.substr(0, columnEndPos);
            if (std::string::npos != dotPos)
            {
                root->child = ReadHeader(cellValue.substr(dotPos + 1));
            }

            return root;
        }
    };

public :
    void Init(const std::vector<std::shared_ptr<Cell>>& row);

protected:
    template <class T>
    void Bind(const std::string& name, T& member)
    {
        bind_functions.insert(std::make_pair(name, [this, &member](const std::shared_ptr<Cell>& cell) {
            this->Allocation(member, cell);
        }));
    }

    template <class T>
    void Bind(const std::string& name, std::vector<T>& member)
    {
        bind_functions.insert(std::make_pair(name, [this, &member](const std::shared_ptr<Cell>& cell) {
            int index = cell->header->index;
            assert(0 <= index);
            if ((int)member.size() <= index)
            {
                member.resize(index + 1);
            }

            T& elmt = member[index];
            this->Allocation(elmt, cell);
        }));
    }

    void BindFunc(const std::string& name, std::function<void(const std::string&)> customFunction)
    {
        bind_functions.insert(std::make_pair(name, [customFunction](const std::shared_ptr<Cell>& cell) {
            customFunction(cell->value);
        }));
    }
private :

    void Allocation(bool& member, const std::shared_ptr<Cell>& cell);
    void Allocation(int16_t& member, const std::shared_ptr<Cell>& cell);
    void Allocation(uint16_t& member, const std::shared_ptr<Cell>& cell);
    void Allocation(int32_t& member, const std::shared_ptr<Cell>& cell);
    void Allocation(uint32_t& member, const std::shared_ptr<Cell>& cell);
    void Allocation(int64_t& member, const std::shared_ptr<Cell>& cell);
    void Allocation(uint64_t& member, const std::shared_ptr<Cell>& cell);
    void Allocation(float& member, const std::shared_ptr<Cell>& cell);
    void Allocation(double& member, const std::shared_ptr<Cell>& cell);
    void Allocation(std::string& member, const std::shared_ptr<Cell>& cell);
    void Allocation(MetaData& member, const std::shared_ptr<Cell>& cell);
    template <class T>
    void Allocation(std::shared_ptr<T>& member, const std::shared_ptr<Cell>& cell)
    {
        std::shared_ptr<Cell> childCell = std::make_shared<Cell>();
        childCell->header = cell->header->child;
        childCell->value = cell->value;
        std::vector<std::shared_ptr<Cell>> cells{ childCell };

        if (nullptr == member)
        {
            member = std::make_shared<T>();
        }

        assert(nullptr != member);
        member->Init(cells);
    }

    std::map<std::string, std::function<void(const std::shared_ptr<Cell>&)>>	bind_functions;
};

#define META_INIT(member) \
	Bind(#member, member)

#define META_FUNC(member, func) \
	BindFunc(#member, std::bind(&func, this, std::placeholders::_1))

#endif