#include <iostream>
#include "MetaData.h"

struct Item : public MetaData
{
    struct Equip : public MetaData
    {
        enum class Part
        {
            None,
            Weapon,
            Armor
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
            if("Weapon" == text)
            {
                Part = Part::Weapon;
            }
            else if("Armor" == text)
            {
                Part = Part::Armor;
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

    std::string ID;
    int         Index;
    int         Type;
    int         Grade;
    int         MaxStack;
    std::shared_ptr<Equip>  Equip;
    std::shared_ptr<Price>  Price;
    std::vector<std::shared_ptr<Package>> Packages;

    Item()
        : ID("")
        , Index(0)
        , Type(0)
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
    void OnType(const std::string& value)
    {
        Type = 0;
        if("Package" == value)
        {
            Type = 1;
        }
    }
};

int main()
{
    MetaData::Reader<Item> reader;
    reader.Read("Item.csv");
    for(std::shared_ptr<Item> item : reader)
    {
        std::cout << item->ID << " " << item->MaxStack << std::endl;
        std::cout << item->Price->Value << " " << item->MaxStack << std::endl;
        for(auto& package : item->Packages)
        {
            std::cout << package->ID << " " << package->Count << std::endl;
        }
    }
    return 0;
}