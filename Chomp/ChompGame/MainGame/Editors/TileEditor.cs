using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ChompGame.MainGame.Editors;
using ChompGame.Data;
using System.Text;
using System;

namespace ChompGame.MainGame.Editors
{
    class TileEditor
    {
        private readonly TileModule _tileModule;
        private bool _running = false;

        private int? _pasteTile;
        private byte[] _topTiles = new byte[] { 24, 25, 26,27, 28 };
        private byte[] _topLeftTiles = new byte[] { 24, 27 };
        private byte[] _topRightTiles = new byte[] { 25, 28 };
        private byte[] _leftTiles = new byte[] { 24, 27, 32 };
        private byte[] _rightTiles = new byte[] { 24, 25, 35 };
        private byte[] _bottomLeftTiles = new byte[] { 19, 22, 32, 35,  };
        private byte[] _bottomRightTiles = new byte[] { 20, 21, 33, 36 };
        private byte[] _centerTiles = new byte[] { 34, 0 };
        private byte[] _bottomTiles = new byte[] { 31, 38 };

        public bool IsRunning => _running;

        public TileEditor(TileModule tileModule)
        {
            _tileModule = tileModule;
        }

        private bool ActivationKeyPressed()
        {
#if DEBUG
            return EditorInputHelper.IsKeyDown(Keys.LeftShift)
                && EditorInputHelper.IsKeyPressed(Keys.T);
#else 
            return false;
#endif
        }

        public bool CheckActivation()
        {
            bool keyPressed = ActivationKeyPressed();
            if (!keyPressed)
                return _running;

            _running = !_running;            
            return _running;
        }

        private Point GetNametableTileUnderMouse()
        {
            var pixelPoint = new Point(
                EditorInputHelper.MouseX, 
                EditorInputHelper.MouseY);

            return new Point(
                (pixelPoint.X / _tileModule.Specs.TileWidth),
                (pixelPoint.Y / _tileModule.Specs.TileHeight)-2);
        }

        private void OnTileClicked()
        {
            var tile = GetNametableTileUnderMouse();
            if (_pasteTile.HasValue)
            {
                _tileModule.NameTable[tile.X, tile.Y] = (byte)(_pasteTile.Value);
                _pasteTile = null;
                return;
            }

            if (EditorInputHelper.IsKeyDown(Keys.LeftControl))
                ExportArea(tile);
            else 
                ToggleTile(tile, EditorInputHelper.IsKeyDown(Keys.LeftAlt) ? -1 : 1);
        }

        private void ExportArea(Point topLeft, int tileBegin=16, int width=16, int height=16)
        {
            var sb = new StringBuilder();
            var lastY = 0;

            _tileModule.NameTable.ForEach(topLeft, new Point(topLeft.X + width, topLeft.Y + height),
                (x, y, b) =>
                {
                    var adjusted = b - tileBegin;
                    if (adjusted < 0 || adjusted > 255)
                        adjusted = 0;

                    if (y > lastY)
                    {
                        lastY = y;
                        sb.AppendLine();
                    }

                    if (adjusted < 10)
                        sb.Append(adjusted.ToString());
                    else
                        sb.Append((char)('A' + adjusted - 10));
                });

            System.Diagnostics.Debug.WriteLine("---------------------");
            System.Diagnostics.Debug.WriteLine(sb.ToString());
            System.Diagnostics.Debug.WriteLine("---------------------");
        }

        private void ToggleTile(Point tile, int offset)
        {
            var current = _tileModule.NameTable[tile.X, tile.Y];         
            _tileModule.NameTable[tile.X, tile.Y] = (byte)(current + offset);
            System.Diagnostics.Debug.WriteLine($"{tile.X},{tile.Y}={current + offset}");
        }

        private void CopyTile()
        {
            var tile = GetNametableTileUnderMouse();
            _pasteTile = _tileModule.NameTable[tile.X, tile.Y];
            System.Diagnostics.Debug.WriteLine($"COPY {tile.X},{tile.Y}={_pasteTile.Value}");
        }

        private void Scroll(int x, int y)
        {
            _tileModule.Scroll.X = (byte)(_tileModule.Scroll.X + x);
            _tileModule.Scroll.Y = (byte)(_tileModule.Scroll.Y + y);
        }

        private void CycleTile(byte[] set)
        {
            var tile = GetNametableTileUnderMouse();
            var current = _tileModule.NameTable[tile.X, tile.Y];
            int currentIndex = Array.IndexOf(set, current);
            if (currentIndex == -1)
                _tileModule.NameTable[tile.X, tile.Y] = set[0];
            else
                _tileModule.NameTable[tile.X, tile.Y] = set[(currentIndex + 1) % set.Length];
        }

        public bool Update()
        {
            if (!CheckActivation())
                return false;

            if (!_running && !ActivationKeyPressed())
                _running = true;
            else if (_running && ActivationKeyPressed())
                return false;

            if (EditorInputHelper.LeftClicked)
                OnTileClicked();
            else if (EditorInputHelper.RightClicked)
                CopyTile();

            if (EditorInputHelper.IsKeyDown(Keys.Left))
                Scroll(-1, 0);
            else if (EditorInputHelper.IsKeyDown(Keys.Right))
                Scroll(1, 0);
            
            if (EditorInputHelper.IsKeyDown(Keys.Up))
                Scroll(0, -1);
            else if (EditorInputHelper.IsKeyDown(Keys.Down))
                Scroll(0, 1);

            if (EditorInputHelper.IsKeyPressed(Keys.NumPad8))
                CycleTile(_topTiles);
            else if (EditorInputHelper.IsKeyPressed(Keys.NumPad7))
                CycleTile(_topLeftTiles);
            else if (EditorInputHelper.IsKeyPressed(Keys.NumPad4))
                CycleTile(_leftTiles);
            else if (EditorInputHelper.IsKeyPressed(Keys.NumPad1))
                CycleTile(_bottomLeftTiles);
            else if (EditorInputHelper.IsKeyPressed(Keys.NumPad2))
                CycleTile(_bottomTiles);
            else if (EditorInputHelper.IsKeyPressed(Keys.NumPad3))
                CycleTile(_bottomRightTiles);
            else if (EditorInputHelper.IsKeyPressed(Keys.NumPad8))
                CycleTile(_rightTiles);
            else if (EditorInputHelper.IsKeyPressed(Keys.NumPad9))
                CycleTile(_topRightTiles);
            else if (EditorInputHelper.IsKeyPressed(Keys.NumPad5))
                CycleTile(_centerTiles);
            else if (EditorInputHelper.IsKeyPressed(Keys.NumPad0))
                CycleTile(new byte[] { 0 });

            return true;
        }
    }
}
