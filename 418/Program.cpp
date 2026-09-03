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
            Hand,
            TwoHands
        };

        Part Part;
        int Attack;
        int Defense;
        int Speed;

        Equip()
            : Part(Equip::Part::None)
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
            static const std::map<std::string, decltype(Part)> table = {
                {"Cloak", Equip::Part::Cloak},
                {"Body", Equip::Part::Body},
                {"Boots", Equip::Part::Boots},
                {"Head", Equip::Part::Head},
                {"Gloves", Equip::Part::Gloves},
                {"Hand", Equip::Part::Hand},
                {"TwoHands", Equip::Part::TwoHands}
            };
            Part = Equip::Part::None;
            auto it = table.find(text);
            if (it != table.end())
            {
                Part = it->second;
            }
        }

        std::string ToString()
        {
            std::string partText;
            switch(Part)
            {
            case Equip::Part::Cloak: partText = "Cloak"; break;
            case Equip::Part::Body: partText = "Body"; break;
            case Equip::Part::Boots: partText = "Boots"; break;
            case Equip::Part::Head: partText = "Head"; break;
            case Equip::Part::Gloves: partText = "Gloves"; break;
            case Equip::Part::Hand: partText = "Hand"; break;
            case Equip::Part::TwoHands: partText = "TwoHands"; break;
            default: partText = "None"; break;
            }

            return "Part=" + partText
                + ", Attack=" + std::to_string(Attack)
                + ", Defense=" + std::to_string(Defense)
                + ", Speed=" + std::to_string(Speed);
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
            static const std::map<std::string, decltype(Type)> table = {
                { "Gold", Type::Gold },
                { "Jewel", Type::Jewel }
            };

            Type = Type::None;
            auto it = table.find(text);
            if(it != table.end())
            {
                Type = it->second;
            }
        }

        std::string ToString()
        {
            std::string typeText;
            switch(Type)
            {
            case Price::Type::Gold: typeText = "Gold"; break;
            case Price::Type::Jewel: typeText = "Jewel"; break;
            default: typeText = "None"; break;
            }

            return "Type=" + typeText
                + ", Value=" + std::to_string(Value);
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

        std::string ToString()
        {
            return "ID=" + ID
                + ", Count=" + std::to_string(Count);
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
        META_INIT(Equip);
        META_INIT(Price);
        META_INIT(Packages);
    }

    std::string ToString()
    {
        std::string typeText;
        switch(Type)
        {
        case Item::Type::Equip: typeText = "Equip"; break;
        case Item::Type::Package: typeText = "Package"; break;
        default: typeText = "None"; break;
        }

        std::string text = "ID=" + ID
            + ", Index=" + std::to_string(Index)
            + ", Type=" + typeText
            + ", Grade=" + std::to_string(Grade)
            + ", MaxStack=" + std::to_string(MaxStack);

        text += ", Equip={";
        text += (nullptr != Equip) ? Equip->ToString() : "null";
        text += "}";

        text += ", Price={";
        text += (nullptr != Price) ? Price->ToString() : "null";
        text += "}";

        text += ", Packages=[";
        for (size_t i = 0; i < Packages.size(); ++i)
        {
            if (0 < i)
            {
                text += ", ";
            }

            text += "{";
            text += (nullptr != Packages[i]) ? Packages[i]->ToString() : "null";
            text += "}";
        }
        text += "]";

        return text;
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
        std::cout << item->ToString() << std::endl;
    }
    return 0;
}