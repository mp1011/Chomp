using Chomp.Models;
using Chomp.Services.Interfaces;
using Chomp.SystemModels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chomp.Services
{
    public class RenderService : IRenderService
    {
        private readonly GameSystem _gameSystem;
        private int _rasterX;
        private int _rasterY;
        private Color[] _screenData;
        private int _dataIndex;
        private int _drawFrame;

        //todo private List<IRasterInterrupt> _rasterInterrupts = new List<IRasterInterrupt>();
        //todo private IRasterEffect _currentEffect;

        public RenderService(GameSystem gameSystem)
        {
            _gameSystem = gameSystem;
            _screenData = new Color[_gameSystem.Specs.ScreenWidth * _gameSystem.Specs.ScreenHeight];
        }

        //todo 
        //public void SetEffect(IRasterEffect rasterEffect)
        //{
        //    _currentEffect = rasterEffect;
        //}

        //public void ClearInterrupts()
        //{
        //    _rasterInterrupts.Clear();
        //}

        //public void AddInterrupt(IRasterInterrupt rasterInterrupt)
        //{
        //    if (_rasterInterrupts.Any())
        //    {
        //        var previousLine = _rasterInterrupts.Last().VerticalLine;
        //        if (previousLine >= rasterInterrupt.VerticalLine)
        //            throw new Exception("Illegal scanline for interrupt");
        //    }

        //    _rasterInterrupts.Add(rasterInterrupt);
        //}

        public void Render(SpriteBatch spriteBatch, Texture2D canvas)
        {
            var nametableCell = _gameSystem.Background.GetTopLeftCell();
            IBytePoint patternTableCell;

            //goal, efficient draw routine
            throw new System.NotImplementedException();

        }
    }
}
