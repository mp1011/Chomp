using ChompGame.Extensions;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ChompGame.MainGame.Editors;
using ChompGame.Data;
using System.Text;

namespace ChompGame.MainGame.Editors
{
    class TileEditor
    {
        private readonly TileModule _tileModule;
        private bool _running = false;

        private int? _pasteTile;

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
                (pixelPoint.X / _tileModule.Specs.TileWidth)-1,
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

        private void ExportArea(Point topLeft, int tileBegin=47, int width=16, int height=16)
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

                    if(adjusted > 15)
                        sb.Append("?");
                    else
                        sb.Append(adjusted.ToString("X"));
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

            return true;
        }
    }
}
