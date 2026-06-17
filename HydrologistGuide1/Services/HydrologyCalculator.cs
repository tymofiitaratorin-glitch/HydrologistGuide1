using HydrologistGuide1.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HydrologistGuide1.Services;

/// <summary>
/// Calculates total annual runoff and basin area for rivers and receiving water objects.
/// </summary>
public class HydrologyCalculator
{
    private readonly IReadOnlyList<River> _rivers;

    public HydrologyCalculator(IReadOnlyList<River> rivers)
    {
        _rivers = rivers;
    }

    public BasinCalculationResult CalculateForRiver(Guid riverId)
    {
        var river = _rivers.FirstOrDefault(r => r.Id == riverId)
            ?? throw new ArgumentException("Обрану річку не знайдено.");

        var included = new List<River>();

        AddRiverWithTributaries(river.Id, included, new HashSet<Guid>());

        return BuildResult(river.Name, included);
    }

    public BasinCalculationResult CalculateForWaterObject(string waterObjectName)
    {
        if (string.IsNullOrWhiteSpace(waterObjectName))
            throw new ArgumentException("Вкажіть назву моря, озера, океану або затоки.");

        var included = new List<River>();
        var visited = new HashSet<Guid>();

        var directRivers = _rivers.Where(r =>
            r.FlowsIntoName.Equals(waterObjectName.Trim(), StringComparison.OrdinalIgnoreCase));

        foreach (var river in directRivers)
            AddRiverWithTributaries(river.Id, included, visited);

        return BuildResult(waterObjectName.Trim(), included);
    }

    private void AddRiverWithTributaries(Guid riverId, List<River> included, HashSet<Guid> visited)
    {
        if (!visited.Add(riverId))
            return;

        var river = _rivers.FirstOrDefault(r => r.Id == riverId);

        if (river == null)
            return;

        included.Add(river);

        foreach (var child in _rivers.Where(r => r.ParentRiverId == riverId))
            AddRiverWithTributaries(child.Id, included, visited);
    }

    private static BasinCalculationResult BuildResult(string objectName, List<River> rivers)
    {
        return new BasinCalculationResult
        {
            ObjectName = objectName,
            TotalAnnualRunoffKm3 = rivers.Sum(r => r.AnnualRunoffKm3),
            TotalBasinAreaKm2 = rivers.Sum(r => r.BasinAreaKm2),
            IncludedRivers = rivers.Select(r => r.Name).OrderBy(n => n).ToList()
        };
    }
}