namespace Minesweeper.Tiles
{
    public class Board
    {
        public Tile[,] tiles;
        public int height, width;
        public int bombAmount;

        private readonly int tileSizeOrgin;
        public int tileSize;
        private readonly Texture2D tileSheet;

        public Board(Texture2D tileSheet, int tileSize, int tileSizeOrgin, int bombAmount = default)
        {
            this.tileSheet = tileSheet;
            this.tileSize = tileSize;
            this.tileSizeOrgin = tileSizeOrgin;
            this.bombAmount = bombAmount;
        }

        #region Generation
        public void Generate()
        {
            tiles = new Tile[width, height];

            GenerateTiles();
            GenerateBombs();
            GetNumbers();
            GetTextures();
        }

        private void GenerateTiles()
        {
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    tiles[x, y] = new() {
                        x = x,
                        y = y,
                        revealed = false,
                        type = Tile.Type.Undefined,
                        canInteract = true
                    };
                }
            }
        }

        private void GenerateBombs()
        {
            Random rd = new();
            if(bombAmount > width * height) bombAmount = width * height;
            for(int i = bombAmount; i > 0; i--) {
                int x = rd.Next(width), y = rd.Next(height);
                while(tiles[x, y].type == Tile.Type.Bomb) {
                    x = rd.Next(width);
                    y = rd.Next(height);
                }

                tiles[x, y].type = Tile.Type.Bomb;
            }

            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    if(tiles[x, y].type != Tile.Type.Bomb)
                        tiles[x, y].type = Tile.Type.Empty;
                }
            }
        }

        private void GetNumbers()
        {
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    tiles[x, y].number = GetTileNumber(x, y);
                }
            }
        }

        private int GetTileNumber(int x, int y)
        {
            int count = 0;
            for(int checkX = -1; checkX < 2; checkX++) {
                for(int checkY = -1; checkY < 2; checkY++) {
                    if(x + checkX < 0 || x + checkX >= width) continue;
                    if(y + checkY < 0 || y + checkY >= height) continue;
                    if(tiles[x + checkX, y + checkY].type == Tile.Type.Bomb) count++;
                }
            }
            return count;
        }

        private void GetTextures()
        {
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    tiles[x, y].sourceRectangle = GetSourceRectangle(x, y);
                }
            }
        }

        public Rectangle GetSourceRectangle(int x, int y)
        {
            Tile tile = tiles[x, y]; //shortcut
            int xStep;
            if(tile.revealed) xStep = GetRevealedTileXStep(tile);
            else xStep = GetHiddenTileXStep(tile);

            return new(new(xStep * tileSizeOrgin, 0), new(tileSizeOrgin, tileSizeOrgin));
        }

        private static int GetRevealedTileXStep(Tile tile)
        {
            int xStep = 0;
            switch(tile.type) {
                case Tile.Type.Empty:
                    if(tile.flagged) xStep = 7;
                    else if(tile.questioned) xStep = 4;
                    else if(tile.number != 0) xStep = GetNumberTileXStep(tile);
                    break;
                case Tile.Type.Bomb:
                    if(tile.flagged) xStep = 2;
                    else if(tile.exploded) xStep = 6; else xStep = 5;
                    break;
                default:
                    throw new Exception("State was Undefined");
            }
            return xStep;
        }

        private static int GetNumberTileXStep(Tile tile)
        {
            int numberOffset = 7;
            return numberOffset + tile.number;
        }

        private static int GetHiddenTileXStep(Tile tile)
        {
            if(tile.flagged) return 2;
            if(tile.questioned) return 3;
            return 1;
        }

        public void Regenerate()
        {
            GetTextures();
        }
        #endregion Generation

        #region Interaction
        public int[] PositionToTile(Vector2 position)
        {
            Vector2 returnVector = Vector2.Floor(position / tileSize);
            return new int[2] { (int)returnVector.X, (int)returnVector.Y };
        }

        public void RevealTileAt(int x, int y)
        {
            if(!tiles[x, y].canInteract) return;
            if(tiles[x, y].flagged) return;
            if(x < 0) x = 0; if(y < 0) y = 0;
            if(x >= width) x = width - 1; if(y >= height) y = height - 1;

            if(tiles[x, y].type == Tile.Type.Bomb) {
                tiles[x, y].exploded = true;
                Boom();
                return;
            }
            else if(tiles[x, y].number == 0) {
                RevealTilesAround(x, y);
                return;
            }
            tiles[x, y].revealed = true;
        }

        public void RevealTilesAround(int x, int y, bool chordMode = false)
        {
            tiles[x, y].revealed = true;

            for(int xOffset = -1; xOffset < 2; xOffset++) {
                for(int yOffset = -1; yOffset < 2; yOffset++) {
                    if(xOffset == 0 && yOffset == 0) continue;
                    if(x + xOffset < 0 || x + xOffset >= width) continue;
                    if(y + yOffset < 0 || y + yOffset >= height) continue;

                    if(tiles[x + xOffset, y + yOffset].revealed == true) continue;
                    if(tiles[x + xOffset, y + yOffset].type == Tile.Type.Bomb) {
                        if(tiles[x + xOffset, y + yOffset].flagged == true) continue;
                        if(chordMode) Boom();
                        else throw new Exception();
                    }

                    if(tiles[x + xOffset, y + yOffset].number == 0)
                        RevealTilesAround(x + xOffset, y + yOffset);
                    else
                        RevealTileAt(x + xOffset, y + yOffset);
                }
            }
        }

        public void Boom()
        {
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    tiles[x, y].revealed = true;
                    tiles[x, y].canInteract = false;
                }
            }
        }

        public void FlagTileAt(int x, int y)
        {
            if(!tiles[x, y].canInteract) return;
            if(tiles[x, y].revealed == true) return;
            tiles[x, y].flagged = !tiles[x, y].flagged;
        }

        public void ChordAt(int x, int y)
        {
            int count = 0;
            for(int xOffset = -1; xOffset < 2; xOffset++) {
                for(int yOffset = -1; yOffset < 2; yOffset++) {
                    if(xOffset == 0 && yOffset == 0) continue;
                    if(x + xOffset < 0 || x + xOffset >= width) continue;
                    if(y + yOffset < 0 || y + yOffset >= height) continue;

                    if(tiles[x + xOffset, y + yOffset].flagged) count++;
                }
            }

            if(count != tiles[x, y].number) return;
            else RevealTilesAround(x, y, true);
        }
        #endregion Interaction

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach(Tile tile in tiles) {
                Rectangle rect = new(new(tile.x * tileSize, tile.y * tileSize), new(tileSize));
                spriteBatch.Draw(tileSheet, rect, tile.sourceRectangle, Color.White);
            }
        }
    }
}