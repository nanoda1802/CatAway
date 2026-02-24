using System;

namespace _Scripts.Stage.Item
{
    public enum CarriableType
    {
        None = 1 << 0,
        Ingredient = 1 << 1, 
        Plate = 1 << 2, 
        Cookware = 1 << 3
    }
}