using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChompGame.MainGame
{
    internal class PlayerDeathHandler
    {
        private GameByte _deathTimer;
        private StatusBar _statusBar;
        private GameByteEnum<ChompGameModule.GameState> _gameState;
        private ChompGameModule _gameModule;
        public PlayerDeathHandler(ChompGameModule gameModule, GameByte deathTimer, StatusBar statusBar, GameByteEnum<ChompGameModule.GameState> gameState)
        {
            _gameModule = gameModule;
            _deathTimer = deathTimer;
            _statusBar = statusBar;
            _gameState = gameState;
        }

        public void HandlePlayerDeath()
        {
            if (_deathTimer.Value == 0 && _statusBar.Health == 0)
            {
                _gameModule.MusicModule.CurrentSong = MusicModule.SongName.None;
                _gameModule.AudioService.PlaySound(ChompAudioService.Sound.PlayerDie);
                _deathTimer.Value = 1;
            }

            if (_deathTimer.Value > 0)
            {
                _deathTimer.Value++;

                if (_deathTimer > 64 && (_deathTimer.Value % 8) == 0)
                    _gameModule.GameSystem.CoreGraphicsModule.FadeAmount++;

                if (_deathTimer.Value == 255)
                {
                    if (_statusBar.Lives > 0)
                    { 
                        _gameState.Value = ChompGameModule.GameState.RestartScene;
                        SetLastTransitionLevel();
                    }
                    else
                        _gameState.Value = ChompGameModule.GameState.GameOver;
                    _deathTimer.Value = 0;
                }
            }
        }

        private void SetLastTransitionLevel()
        {
            while(!SceneBuilder.IsTransitionLevel(_gameModule.CurrentLevel))
                _gameModule.CurrentLevel--;            
        }
    }
}
