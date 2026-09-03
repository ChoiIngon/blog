#include "MetaData.h"

void MetaData::Init(const Cell* cells, size_t count, const Schema& schema)
{
    char* self = reinterpret_cast<char*>(this);

    for (size_t i = 0; i < count; i++)
    {
        const Cell& cell = cells[i];
        if (nullptr == cell.value || cell.value->empty())
        {
            continue;
        }

        const Header* header = cell.header;
        if (false == header->resolved)
        {
            header->field = schema.Find(header->name);
            header->resolved = true;
        }

        if (nullptr == header->field)
        {
            continue;
        }

        header->field->setter(self + header->field->offset, cell);
    }
}
