#pragma once

#include "CSVReader.h"
#include <string>
#include <string_view>
#include <vector>
#include <memory>
#include <charconv>
#include <cstddef>
#include <type_traits>
#include <cassert>
#include <stdexcept>
#include <algorithm>
#undef min

template <class META> class MetaSchema;

struct MetaData
{
public:
    struct Field;

    struct Header
    {
        int                     index = -1;
        std::string             name;
        std::shared_ptr<Header> child;

        // 컬럼 이름 -> Field 검색 결과 캐시. 파일 당 컬럼 당 한 번만 검색한다
        mutable const Field*    field = nullptr;
        mutable bool            resolved = false;
    };

    // shared_ptr / string 복사가 없는 경량 뷰. row 당 vector 를 재사용한다
    struct Cell
    {
        const Header*      header = nullptr;
        const std::string* value = nullptr;
    };

    // 캡쳐가 없으므로 std::function 이 아니라 순수 함수 포인터. 힙 할당이 없다
    using Setter = void (*)(void* member, const Cell& cell);

    struct Field
    {
        std::string_view name;      // #member 문자열 리터럴. 정적 수명이라 복사 불필요
        std::ptrdiff_t   offset;    // MetaData 서브오브젝트 기준 멤버 오프셋
        Setter           setter;
    };

    // 타입 당 단 하나만 만들어지는 필드 테이블
    class Schema
    {
    public:
        void Add(const Field& field) { fields.push_back(field); }

        [[nodiscard]] const Field* Find(std::string_view name) const
        {
            for (const Field& field : fields)
            {
                if (toUpper(field.name) == toUpper(name))
                {
                    return &field;
                }
            }
            return nullptr;
        }

        [[nodiscard]] size_t Size() const { return fields.size(); }

    private:
        std::string toUpper(std::string_view str) const
        {
            std::string upper(str);
            std::transform(upper.begin(), upper.end(), upper.begin(),
                [](unsigned char c) { return static_cast<char>(std::toupper(c)); });
            return upper;
        }

        std::vector<Field> fields;
    };

    // 스키마 빌드 중에만 존재하는 임시 기록
    struct Binding
    {
        const MetaData*  owner;
        const void*      address;
        std::string_view name;
        Setter           setter;
    };

    inline static thread_local std::vector<Binding>* bindings = nullptr;

public:
    template <class META>
    class Reader
    {
    public:
        using MetaDatas = std::vector<std::shared_ptr<META>>;

        bool Read(const std::string& file)
        {
            const Schema& schema = MetaSchema<META>::Get();   // 프로세스 당 최초 1회만 생성

            CSVReader reader;
            reader.ReadFile(file);

            std::vector<std::shared_ptr<Header>> headers;
            for (const std::string& cell : reader[0])
            {
                headers.push_back(ReadHeader(cell));
            }

            std::vector<Cell> cells;    // row 마다 재사용
            for (size_t rowNum = 2; rowNum < reader.GetRowCount(); rowNum++)
            {
                const std::vector<std::string>& row = reader[rowNum];

                cells.clear();
                cells.reserve(row.size());
                for (size_t columnNum = 0; columnNum < row.size(); columnNum++)
                {
                    cells.push_back(Cell{ headers[columnNum].get(), &row[columnNum] });
                }

                std::shared_ptr<META> meta = std::make_shared<META>();
                meta->Init(cells.data(), cells.size(), schema);
                metadatas.push_back(std::move(meta));
            }
            return true;
        }

        auto begin() { return metadatas.begin(); }
        auto end()   { return metadatas.end(); }

    private:
        MetaDatas metadatas;

        std::shared_ptr<Header> ReadHeader(const std::string& cellValue)
        {
            std::shared_ptr<Header> root = std::make_shared<Header>();

            std::string column = cellValue;
            std::size_t dotPos = cellValue.find('.');
            if (std::string::npos != dotPos)
            {
                column = cellValue.substr(0, dotPos);
            }

            std::size_t braceStartPos = column.find('[');
            std::size_t braceEndPos = column.find(']');

            if ((std::string::npos != braceStartPos && std::string::npos == braceEndPos) ||
                (std::string::npos == braceStartPos && std::string::npos != braceEndPos))
            {
                throw std::runtime_error("column name error:" + cellValue + ", unmatched brace");
            }

            if (std::string::npos != braceStartPos && std::string::npos != braceEndPos)
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

public:
    void Init(const Cell* cells, size_t count, const Schema& schema);

protected:
    // 일반 객체 생성 시에는 if 한 번으로 끝난다. 등록도, 할당도 없다
    template <class T>
    void Bind(std::string_view name, T& member)
    {
        if (nullptr == bindings)
        {
            return;
        }
        bindings->push_back(Binding{ this, &member, name, &MetaData::MemberSetter<T> });
    }

    template <class T>
    void Bind(std::string_view name, std::vector<T>& member)
    {
        if (nullptr == bindings)
        {
            return;
        }
        bindings->push_back(Binding{ this, &member, name, &MetaData::VectorSetter<T> });
    }

    // 멤버 함수 포인터를 템플릿 인자로 받아 this 캡쳐를 제거한다
    template <auto FUNC>
    void BindFunc(std::string_view name)
    {
        using Class = typename MemberFunctionTraits<decltype(FUNC)>::Class;
        static_assert(std::is_base_of_v<MetaData, Class>, "META_FUNC: not a MetaData member function");

        if (nullptr == bindings)
        {
            return;
        }
        bindings->push_back(Binding{ this, static_cast<Class*>(this), name, &MetaData::FunctionSetter<FUNC> });
    }

private:
    template <class T> struct MemberFunctionTraits;
    template <class C> struct MemberFunctionTraits<void (C::*)(const std::string&)> { using Class = C; };
    template <class C> struct MemberFunctionTraits<void (C::*)(std::string_view)>   { using Class = C; };

    template <class T> struct IsSharedPtr : std::false_type {};
    template <class T> struct IsSharedPtr<std::shared_ptr<T>> : std::true_type { using Element = T; };

    template <class T>
    static void MemberSetter(void* member, const Cell& cell)
    {
        Assign(*static_cast<T*>(member), cell);
    }

    template <class T>
    static void VectorSetter(void* member, const Cell& cell)
    {
        std::vector<T>& elements = *static_cast<std::vector<T>*>(member);

        const int index = cell.header->index;
        if (0 > index)
        {
            throw std::runtime_error(ErrorMessage(cell, "array column needs [index]"));
        }
        if (elements.size() <= static_cast<size_t>(index))
        {
            elements.resize(index + 1);
        }
        Assign(elements[index], cell);
    }

    template <auto FUNC>
    static void FunctionSetter(void* self, const Cell& cell)
    {
        using Class = typename MemberFunctionTraits<decltype(FUNC)>::Class;
        (static_cast<Class*>(self)->*FUNC)(*cell.value);
    }

    static std::string ErrorMessage(const Cell& cell, std::string_view reason)
    {
        return "metadata error: column '" + cell.header->name + "', value '" + *cell.value + "' - " + std::string(reason);
    }

    template <class T>
    static void ParseNumber(T& member, const Cell& cell)
    {
        const char* first = cell.value->data();
        const char* last = first + cell.value->size();

        const std::from_chars_result result = std::from_chars(first, last, member);
        if (std::errc() != result.ec || last != result.ptr)
        {
            throw std::runtime_error(ErrorMessage(cell, "not a number"));
        }
    }

    // 타입 분기를 if constexpr 하나로 모은다. 지원하지 않는 타입은 컴파일 타임에 잡힌다
    template <class T>
    static void Assign(T& member, const Cell& cell)
    {
        if constexpr (std::is_same_v<T, bool>)
        {
            std::string lower(*cell.value);
            std::transform(lower.begin(), lower.end(), lower.begin(),
                [](unsigned char c) { return static_cast<char>(std::tolower(c)); });
            member = !("false" == lower || "0" == lower);
        }
        else if constexpr (std::is_integral_v<T> || std::is_floating_point_v<T>)
        {
            ParseNumber(member, cell);
        }
        else if constexpr (std::is_same_v<T, std::string>)
        {
            member = *cell.value;
        }
        else if constexpr (IsSharedPtr<T>::value)
        {
            using Element = typename IsSharedPtr<T>::Element;
            static_assert(std::is_base_of_v<MetaData, Element>, "shared_ptr member must point to a MetaData");

            if (nullptr == member)
            {
                member = std::make_shared<Element>();
            }
            const Cell child{ cell.header->child.get(), cell.value };
            member->Init(&child, 1, MetaSchema<Element>::Get());
        }
        else if constexpr (std::is_base_of_v<MetaData, T>)
        {
            const Cell child{ cell.header->child.get(), cell.value };
            member.Init(&child, 1, MetaSchema<T>::Get());
        }
        else
        {
            static_assert(!sizeof(T*),
                "META_INIT: unsupported member type. enum 이나 사용자 정의 타입은 META_FUNC 를 사용하십시오");
        }
    }
};

// 타입 당 하나의 Schema. 최초 사용 시점에 프로토타입 객체 하나로 오프셋을 수집한다
template <class META>
class MetaSchema
{
public:
    static const MetaData::Schema& Get()
    {
        static const MetaData::Schema schema = Build();   // magic static. 스레드 안전
        return schema;
    }

private:
    static MetaData::Schema Build()
    {
        static_assert(std::is_base_of_v<MetaData, META>, "META must derive from MetaData");
        static_assert(std::is_default_constructible_v<META>, "META must be default constructible");

        std::vector<MetaData::Binding> bindings;
        std::vector<MetaData::Binding>* previous = MetaData::bindings;
        MetaData::bindings = &bindings;

        META prototype;   // 생성자의 META_INIT / META_FUNC 가 bindings 에 기록된다

        MetaData::bindings = previous;

        MetaData::Schema schema;
        const MetaData* base = static_cast<const MetaData*>(&prototype);
        const char* basePointer = reinterpret_cast<const char*>(base);

        for (const MetaData::Binding& binding : bindings)
        {
            if (binding.owner != base)
            {
                continue;   // 값으로 포함된 중첩 MetaData 멤버. 그 타입의 스키마에서 처리된다
            }

            schema.Add(MetaData::Field{
                binding.name,
                reinterpret_cast<const char*>(binding.address) - basePointer,
                binding.setter });
        }
        return schema;
    }
};

#define META_INIT(member) \
	Bind(#member, member)

#define META_FUNC(member, func) \
	BindFunc<&func>(#member)
