using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Netsphere.Server.Game;

public static class ItemParser
{
    public static List<Item> ParseItems(string path)
    {
        var doc = XDocument.Load(path);
        var items = new List<Item>();

        foreach (var element in doc.Descendants())
        {
            var name = element.Name.LocalName;

            if (name != "weapon" &&
                name != "Action" &&
                name != "item")
                continue;

            var id =
                element.Attribute("item_key")?.Value ??
                element.Attribute("name")?.Value;

            if (string.IsNullOrEmpty(id))
                continue;

            var item = new Item
            {
                Id = id
            };

            var baseNode = element.Element("base");
            if (baseNode != null)
            {
                var sex = baseNode.Attribute("sex")?.Value;

                if (!string.IsNullOrEmpty(sex))
                {
                    if (Enum.TryParse<ItemGender>(sex, true, out var gender))
                        item.Gender = gender;
                }
            }

            items.Add(item);
        }

        return items;
    }
}
