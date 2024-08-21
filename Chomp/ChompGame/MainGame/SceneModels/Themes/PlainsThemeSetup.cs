using ChompGame.Data;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels.Themes
{
    class PlainsThemeSetup : ThemeSetup
    {
        public PlainsThemeSetup(ChompGameModule m) : base(m) { }
        public override void SetupVRAMPatternTable(
          NBitPlane masterPatternTable,
          NBitPlane vramPatternTable,
          SystemMemory memory)
        {                
            //tile row 1
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 5, 6, 1),
                destinationPoint: new Point(1, 0),
                _specs,
                memory);

            //tile row 2
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 12, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);                            
        }

        public override void BuildBackgroundNameTable(NBitPlane nameTable)
        {
            byte mountain1Pos, mountain2Pos, groundPos;

            mountain1Pos = (byte)_sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Back1, includeStatusBar: false);
            mountain2Pos = (byte)(_sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Back2, includeStatusBar: false));
            groundPos = (byte)_sceneDefinition.GetBackgroundLayerTile(BackgroundLayer.Foreground, includeStatusBar: false);

            AddPlainsMountainTiles(nameTable, mountain1Pos, mountain2Pos, groundPos);
        }

        private void AddPlainsMountainTiles(NBitPlane nameTable, byte mountain1Pos, byte mountain2Pos, byte groundPos)
        {
            string layer1Row1 = "00000500";
            string layer1Row2 = "34121625";

            // 8 9 A B C
            // 1 2 3 4 5
            //mountain layer 1
            nameTable.SetFromString(0, mountain1Pos,0,
                $@"{layer1Row1}{layer1Row1}{layer1Row1}{layer1Row1}
                         {layer1Row2}{layer1Row2}{layer1Row2}{layer1Row2}",
                shouldReplace: b => b == 0);

            string layer2Row1 = "0005000012000000";
            string layer2Row2 = "3416234166212340";


            //mountain layer 2
            nameTable.SetFromString(0, mountain2Pos, 0,
              $@"{layer2Row1}{layer2Row1}
                       {layer2Row2}{layer2Row2}",
                shouldReplace: b => b == 0);
        }

    }
}
