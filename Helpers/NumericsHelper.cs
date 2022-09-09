namespace Minesweeper.Helpers
{
    public static class NumericsHelper
    {
        public static int GetValue(this float f)
        {
            if(f == 0) return (int)f;
            return (int)(MathF.Abs(f) / f);
        }

        public static int GetValue(this int i)
        {
            if(i == 0) return i;
            return (int)(MathF.Abs(i) / i);
        }
    }
}