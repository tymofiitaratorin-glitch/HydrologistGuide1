using HydrologistGuide1.Models;
using HydrologistGuide1.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HydrologistGuide1.Services;

/// <summary>
/// Stores river records and provides operations for loading and saving data.
/// </summary>
public class HydrologyRepository
{
    private readonly List<River> _rivers = new();

    public IReadOnlyList<River> Rivers => _rivers;

    public void ReplaceAll(IEnumerable<River> rivers)
    {
        _rivers.Clear();
        _rivers.AddRange(rivers.Select(r => r.Clone()));
    }

    public void Add(River river)
    {
        var errors = RiverValidator.Validate(river, _rivers);

        if (errors.Count > 0)
            throw new ArgumentException(string.Join(Environment.NewLine, errors));

        _rivers.Add(river.Clone());
    }

    public void Update(River river)
    {
        var errors = RiverValidator.Validate(river, _rivers);

        if (errors.Count > 0)
            throw new ArgumentException(string.Join(Environment.NewLine, errors));

        int index = _rivers.FindIndex(r => r.Id == river.Id);

        if (index < 0)
            throw new ArgumentException("Річку для редагування не знайдено.");

        _rivers[index] = river.Clone();
    }

    public void Delete(Guid id)
    {
        var river = _rivers.FirstOrDefault(r => r.Id == id);

        if (river == null)
            throw new ArgumentException("Річку для видалення не знайдено.");

        foreach (var child in _rivers.Where(r => r.ParentRiverId == id))
            child.ParentRiverId = null;

        _rivers.Remove(river);
    }

    public IEnumerable<River> Search(
        string name,
        string flowsInto,
        MouthType? mouthType,
        double? minLength,
        double? maxLength)
    {
        IEnumerable<River> query = _rivers;

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(r =>
                r.Name.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(flowsInto))
        {
            query = query.Where(r =>
                r.FlowsIntoName.Contains(flowsInto.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (mouthType.HasValue)
            query = query.Where(r => r.MouthType == mouthType.Value);

        if (minLength.HasValue)
            query = query.Where(r => r.LengthKm >= minLength.Value);

        if (maxLength.HasValue)
            query = query.Where(r => r.LengthKm <= maxLength.Value);

        return query.OrderBy(r => r.Name).ToList();
    }

    public void Save(string path)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(_rivers, options);
        File.WriteAllText(path, json);
    }

    public void Load(string path)
    {
        string json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<List<River>>(json) ?? new List<River>();
        ReplaceAll(data);
    }
}