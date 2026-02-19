using System;

namespace _Scripts.Stage.Item
{
    public enum CarriableType
    {
        Ingredient = 1 << 0, 
        Plate = 1 << 1, 
        Cookware = 1 << 2
    }
}