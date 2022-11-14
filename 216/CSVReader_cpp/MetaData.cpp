#include "MetaData.h"

#include <algorithm>
#include <cstdlib>

MetaData::Header::Header()
    : index(-1)
    , name("")
    , child(nullptr)
{
}

MetaData::Cell::Cell()
    : header(nullptr)
    , value("")
{
}

void MetaData::Init(const std::vector<std::shared_ptr<Cell>>& row)
{
    for(auto& cell : row)
    {
        const std::string& key = cell->header->name;
        const std::string& value = cell->value;
        if ("" == value)
        {
            continue;
        }

        if (bind_functions.end() == bind_functions.find(key))
        {
            continue;
        }

        bind_functions[key](cell);
    }
}

void MetaData::Allocation(bool& member, const std::shared_ptr<Cell>& cell)
{
    std::string lower = cell->value;
    std::transform(lower.begin(), lower.end(), lower.begin(), [](unsigned char c) { return std::tolower(c); });

    if ("false" == lower || "0" == lower)
    {
        member = false;
        return;
    }
    member = true;
}

void MetaData::Allocation(int16_t& member, const std::shared_ptr<Cell>& cell)
{
    member = (int16_t)std::stoi(cell->value);
}

void MetaData::Allocation(uint16_t& member, const std::shared_ptr<Cell>& cell)
{
    member = (uint16_t)std::stoul(cell->value);
}

void MetaData::Allocation(int32_t& member, const std::shared_ptr<Cell>& cell)
{
    member = std::stoi(cell->value);
}

void MetaData::Allocation(uint32_t& member, const std::shared_ptr<Cell>& cell)
{
    member = std::stoul(cell->value);
}

void MetaData::Allocation(int64_t& member, const std::shared_ptr<Cell>& cell)
{
    member = std::stoll(cell->value);
}

void MetaData::Allocation(uint64_t& member, const std::shared_ptr<Cell>& cell)
{
    member = std::stoull(cell->value);
}

void MetaData::Allocation(float& member, const std::shared_ptr<Cell>& cell)
{
    member = std::stof(cell->value);
}

void MetaData::Allocation(double& member, const std::shared_ptr<Cell>& cell)
{
    member = std::stod(cell->value);
}

void MetaData::Allocation(std::string& member, const std::shared_ptr<Cell>& cell)
{
    member = cell->value;
}

void MetaData::Allocation(MetaData& member, const std::shared_ptr<Cell>& cell)
{
    std::shared_ptr<Cell> childCell = std::make_shared<Cell>();
    childCell->header = cell->header->child;
    childCell->value = cell->value;
    std::vector<std::shared_ptr<Cell>> cells { childCell };

    member.Init(cells);
}
