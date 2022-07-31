#include <iostream>
#include "MetaData.h"

struct Item : public MetaData
{
    struct Equip : public MetaData
    {
        enum class Part
        {
            None,
            Cloak,
            Body,
            Boots,
            Head,
            Gloves,
            LeftHand,
            RightHand
        };

        Part Part;
        int Attack;
        int Defense;
        int Speed;

        Equip()
            : Part(Part::None)
            , Attack(0)
            , Defense(0)
            , Speed(0)
        {
            META_FUNC(Part, Equip::OnPart);
            META_INIT(Attack);
            META_INIT(Defense);
            META_INIT(Speed);
        }

        void OnPart(const std::string& text)
        {
            Part = Part::None;
            if("Cloak" == text)
            {
                Part = Part::Cloak;
            }
            else if("Body" == text)
            {
                Part = Part::Body;
            }
            else if ("Boots" == text)
            {
                Part = Part::Boots;
            }
            else if ("Head" == text)
            {
                Part = Part::Head;
            }
            else if ("Gloves" == text)
            {
                Part = Part::Gloves;
            }
            else if ("LeftHand" == text)
            {
                Part = Part::LeftHand;
            }
            else if ("RightHand" == text)
            {
                Part = Part::RightHand;
            }
        }
    };

    struct Price : public MetaData
    {
        enum class Type
        {
            None,
            Gold,
            Jewel
        };

        Type Type;
        int Value;

        Price()
            : Type(Type::None)
            , Value(0)
        {
            META_FUNC(Type, Price::OnType);
            META_INIT(Value);
        }

        void OnType(const std::string& text)
        {
            if("Gold" == text)
            {
                Type = Type::Gold;
            }
            else if ("Jewel" == text)
            {
                Type = Type::Jewel;
            }
        }
    };

    struct Package : public MetaData
    {
        std::string ID;
        int		 Count;

        Package()
            : ID("")
            , Count(0)
        {
            META_INIT(ID);
            META_INIT(Count);
        }
    };

    enum class Type
    {
        None,
        Equip,
        Package
    };

    std::string ID;
    int         Index;
    Type        Type;
    int         Grade;
    int         MaxStack;
    std::shared_ptr<Equip>  Equip;
    std::shared_ptr<Price>  Price;
    std::vector<std::shared_ptr<Package>> Packages;

    Item()
        : ID("")
        , Index(0)
        , Type(Type::None)
        , Grade(0)
        , MaxStack(0)
    {
        META_INIT(ID);
        META_INIT(Index);
        META_FUNC(Type, Item::OnType);
        META_INIT(Grade);
        META_INIT(MaxStack);
        META_INIT(Price);
        META_INIT(Packages);
    }

private :
    void OnType(const std::string& text)
    {
        Type = Type::None;
        if("Package" == text)
        {
            Type = Type::Package;
        }
        else if("Equip" == text)
        {
            Type = Type::Equip;
        }
    }
};

int main()
{
    MetaData::Reader<Item> reader;
    reader.Read("EquipItem.csv");
    reader.Read("PackageItem.csv");
    for(std::shared_ptr<Item> item : reader)
    {
        std::cout << item->ID << " " << item->MaxStack << std::endl;
    }
    return 0;
}