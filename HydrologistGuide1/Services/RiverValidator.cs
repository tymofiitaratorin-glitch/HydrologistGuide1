using HydrologistGuide1.Models;
using System.Collections.Generic;
using System.Linq;

namespace HydrologistGuide1.Services;

/// <summary>
/// Validates river records before saving them to the repository.
/// </summary>
public static class RiverValidator
{
    public static List<string> Validate(River river, IEnumerable<River> existingRivers)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(river.Name))
            errors.Add("Назва річки є обов'язковою.");
        else if (river.Name.Trim().Length > 80)
            errors.Add("Назва річки не може перевищувати 80 символів.");

        if (river.LengthKm <= 0)
            errors.Add("Довжина річки має бути більшою за нуль.");

        if (string.IsNullOrWhiteSpace(river.FlowsIntoName))
            errors.Add("Потрібно вказати, куди впадає річка.");
        else if (river.FlowsIntoName.Trim().Length > 80)
            errors.Add("Назва водного об'єкта не може перевищувати 80 символів.");

        if (river.AnnualRunoffKm3 < 0)
            errors.Add("Річний стік не може бути від'ємним.");

        if (river.BasinAreaKm2 <= 0)
            errors.Add("Площа басейну має бути більшою за нуль.");

        bool duplicate = existingRivers.Any(r =>
            r.Id != river.Id &&
            r.Name.Trim().ToLowerInvariant() == river.Name.Trim().ToLowerInvariant());

        if (duplicate)
            errors.Add("Річка з такою назвою вже існує у довіднику.");

        if (river.ParentRiverId == river.Id)
            errors.Add("Річка не може бути власною притокою.");

        return errors;
    }
}
