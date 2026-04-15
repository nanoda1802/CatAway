using System;

namespace _Scripts.Stage._Enums
{
    [Flags]
    public enum IngredientType
    {
        Bun = 1 << 0, // 비트 연산을 하기 때문에, 0부터 시작하지 않도록 해씀 (0은 Everything이 돼버려서)
        Patty = 1 << 1,
        Tomato = 1 << 2,
        Lettuce = 1 << 3,
        Cheese = 1 << 4
    }
}