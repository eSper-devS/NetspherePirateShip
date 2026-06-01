using System;

namespace Netsphere.Server.Game
{
    public static class ItemLogic
    {
        public static ItemType GetType(string id)
        {
            foreach (ItemType t in Enum.GetValues(typeof(ItemType)))
            {
                if (id.StartsWith(((int)t).ToString()))
                    return t;
            }

            throw new Exception("Unknown item type: " + id);
        }

        public static (int mainTab, int subTab) CalculateTabInfo(ItemType type)
        {
            switch (type)
            {
                case ItemType.CLOTHING_HEAD:
                    return (3, 2);
                case ItemType.CLOTHING_FACE:
                    return (3, 3);
                case ItemType.CLOTHING_JACKET:
                    return (3, 4);
                case ItemType.CLOTHING_PANTS:
                    return (3, 5);
                case ItemType.CLOTHING_GLOVES:
                    return (3, 6);
                case ItemType.CLOTHING_SHOES:
                    return (3, 7);
                case ItemType.CLOTHING_ACC:
                    return (3, 8);
                case ItemType.CLOTHING_PET:
                    return (3, 9);

                case ItemType.WEAPON_MELEE:
                    return (2, 1);
                case ItemType.WEAPON_GUN:
                    return (2, 3);
                case ItemType.WEAPON_HEAVY:
                    return (2, 4);
                case ItemType.WEAPON_SNIPE:
                    return (2, 5);

                case ItemType.WEAPON_DEPLOY:
                case ItemType.WEAPON_SPECIAL:
                    return (2, 6);

                case ItemType.WEAPON_THROWING:
                    return (2, 7);
                case ItemType.SKILL:
                    return (2, 8);

                default:
                    throw new Exception("Invalid type");
            }
        }

        public static bool TryGetType(string id, out ItemType type)
        {
            type = default;

            foreach (ItemType t in Enum.GetValues(typeof(ItemType)))
            {
                if (id.StartsWith(((int)t).ToString()))
                {
                    type = t;
                    return true;
                }
            }

            return false;
        }
    }
}
