namespace Minesweeper.Tiles
{
    public struct Tile
    {
        public enum Type
        {
            Undefined, 
            Empty,
            Bomb
        }
        public Type type;
        public Rectangle sourceRectangle;

        public int x, y;
        public int number;
        public bool revealed, exploded, canInteract;
        public bool flagged, questioned;
    }
}