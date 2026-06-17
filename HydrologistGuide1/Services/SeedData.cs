using HydrologistGuide1.Models;
using HydrologistGuide1.Models;
using System;
using System.Collections.Generic;

namespace HydrologistGuide1.Services;

/// <summary>
/// Provides initial demonstration data for the first run of the application.
/// </summary>
public static class SeedData
{
    public static List<River> Create()
    {
        var nile = New("Ніл", 6650, "Середземне море", MouthType.Sea, 84, 3349000);
        var amazon = New("Амазонка", 6400, "Атлантичний океан", MouthType.Ocean, 6590, 7050000);

        var mississippi = New("Міссісіпі", 3730, "Мексиканська затока", MouthType.Gulf, 580, 2980000);
        var missouri = New("Міссурі", 3767, "Міссісіпі", MouthType.River, 86, 1371000, mississippi.Id);
        var ohio = New("Огайо", 1579, "Міссісіпі", MouthType.River, 230, 490600, mississippi.Id);

        var yangtze = New("Янцзи", 6300, "Східнокитайське море", MouthType.Sea, 960, 1808500);
        var yellow = New("Хуанхе", 5464, "Бохайська затока", MouthType.Gulf, 58, 752000);

        var danube = New("Дунай", 2850, "Чорне море", MouthType.Sea, 203, 817000);

        var dnieper = New("Дніпро", 2201, "Чорне море", MouthType.Sea, 53.5, 504000);
        var desna = New("Десна", 1130, "Дніпро", MouthType.River, 11.4, 88900, dnieper.Id);
        var pripyat = New("Прип'ять", 761, "Дніпро", MouthType.River, 14.5, 121000, dnieper.Id);

        var congo = New("Конго", 4700, "Атлантичний океан", MouthType.Ocean, 1300, 3680000);
        var niger = New("Нігер", 4180, "Гвінейська затока", MouthType.Gulf, 177, 2117000);

        var ganges = New("Ганг", 2525, "Бенгальська затока", MouthType.Gulf, 380, 1080000);
        var brahmaputra = New("Брахмапутра", 2900, "Ганг", MouthType.River, 700, 651000, ganges.Id);

        var volga = New("Волга", 3530, "Каспійське море", MouthType.Lake, 254, 1360000);
        var kama = New("Кама", 1805, "Волга", MouthType.River, 117, 507000, volga.Id);
        var oka = New("Ока", 1500, "Волга", MouthType.River, 40, 245000, volga.Id);

        var rhine = New("Рейн", 1233, "Північне море", MouthType.Sea, 75, 185000);

        return new List<River>
        {
            nile,
            amazon,
            mississippi,
            missouri,
            ohio,
            yangtze,
            yellow,
            danube,
            dnieper,
            desna,
            pripyat,
            congo,
            niger,
            ganges,
            brahmaputra,
            volga,
            kama,
            oka,
            rhine
        };
    }

    private static River New(
        string name,
        double length,
        string flowsInto,
        MouthType type,
        double runoff,
        double basin,
        Guid? parentId = null)
    {
        return new River
        {
            Id = Guid.NewGuid(),
            Name = name,
            LengthKm = length,
            FlowsIntoName = flowsInto,
            MouthType = type,
            AnnualRunoffKm3 = runoff,
            BasinAreaKm2 = basin,
            ParentRiverId = parentId
        };
    }
}