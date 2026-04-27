using System.Globalization;
using System.Text;
using GoodHamburger.Domain.Common;

namespace GoodHamburger.Domain.Menu;

public static class MenuCatalog
{
    private static readonly IReadOnlyDictionary<MenuItemCode, MenuItemDefinition> Items = new Dictionary<MenuItemCode, MenuItemDefinition>
    {
        [MenuItemCode.XBurger] = new(MenuItemCode.XBurger, "X Burger", MenuItemCategory.Sandwich, 5.00m),
        [MenuItemCode.XEgg] = new(MenuItemCode.XEgg, "X Egg", MenuItemCategory.Sandwich, 4.50m),
        [MenuItemCode.XBacon] = new(MenuItemCode.XBacon, "X Bacon", MenuItemCategory.Sandwich, 7.00m),
        [MenuItemCode.Fries] = new(MenuItemCode.Fries, "Batata frita", MenuItemCategory.Side, 2.00m),
        [MenuItemCode.Soda] = new(MenuItemCode.Soda, "Refrigerante", MenuItemCategory.Drink, 2.50m),
    };

    private static readonly IReadOnlyDictionary<string, MenuItemCode> ParseMap = Items.Values
        .SelectMany(item => new[]
        {
            (Key: Normalize(item.Code.ToString()), Value: item.Code),
            (Key: Normalize(item.Name), Value: item.Code)
        })
        .GroupBy(x => x.Key)
        .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyCollection<MenuItemDefinition> All => Items.Values
        .OrderBy(item => item.Category)
        .ThenBy(item => item.Price)
        .ToArray();

    public static bool TryGetDefinition(MenuItemCode code, out MenuItemDefinition definition)
        => Items.TryGetValue(code, out definition!);

    public static MenuItemDefinition GetDefinition(MenuItemCode code)
        => Items.TryGetValue(code, out var definition)
            ? definition
            : throw new UnknownMenuItemCodeException(code.ToString());

    public static bool TryParse(string? code, out MenuItemCode itemCode)
    {
        itemCode = default;
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return ParseMap.TryGetValue(Normalize(code), out itemCode);
    }

    public static MenuItemDefinition GetDefinition(string code)
        => TryParse(code, out var itemCode)
            ? GetDefinition(itemCode)
            : throw new UnknownMenuItemCodeException(code);

    public static IReadOnlyCollection<MenuItemDefinition> ResolveItems(IEnumerable<MenuItemCode> codes)
    {
        ArgumentNullException.ThrowIfNull(codes);
        return codes.Select(GetDefinition).ToArray();
    }

    private static string Normalize(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(c))
            {
                builder.Append(char.ToUpperInvariant(c));
            }
        }

        return builder.ToString();
    }
}

public sealed class UnknownMenuItemCodeException(string code) : DomainException("menu_item_not_found", $"O item '{code}' não existe no cardápio.")
{
    public string CodeValue { get; } = code;
}
