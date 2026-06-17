using HydrologistGuide1.Models;
using System;

namespace HydrologistGuide1.Models;

/// <summary>
/// Describes a world river in the hydrologist guide.
/// </summary>
public class River
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public double LengthKm { get; set; }

    public string FlowsIntoName { get; set; } = string.Empty;

    public MouthType MouthType { get; set; } = MouthType.Sea;

    public double AnnualRunoffKm3 { get; set; }

    public double BasinAreaKm2 { get; set; }

    public Guid? ParentRiverId { get; set; }

    public River Clone()
    {
        return new River
        {
            Id = Id,
            Name = Name,
            LengthKm = LengthKm,
            FlowsIntoName = FlowsIntoName,
            MouthType = MouthType,
            AnnualRunoffKm3 = AnnualRunoffKm3,
            BasinAreaKm2 = BasinAreaKm2,
            ParentRiverId = ParentRiverId
        };
    }
}
