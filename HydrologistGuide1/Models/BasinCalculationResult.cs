using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;

namespace HydrologistGuide1.Models;

/// <summary>
/// Result of runoff and basin area calculation for a river, sea, lake, ocean, or gulf.
/// </summary>
public class BasinCalculationResult
{
    public string ObjectName { get; set; } = string.Empty;

    public double TotalAnnualRunoffKm3 { get; set; }

    public double TotalBasinAreaKm2 { get; set; }

    public List<string> IncludedRivers { get; set; } = new();

    public override string ToString()
    {
        return $"{ObjectName}: річний стік = {TotalAnnualRunoffKm3:N2} км³/рік; " +
               $"площа басейну = {TotalBasinAreaKm2:N0} км²; " +
               $"ураховано річок: {IncludedRivers.Count}";
    }
}

