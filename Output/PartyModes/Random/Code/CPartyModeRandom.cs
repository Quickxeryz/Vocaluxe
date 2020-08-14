#region license
// This file is part of Vocaluxe.
// 
// Vocaluxe is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Vocaluxe is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Vocaluxe. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using VocaluxeLib.Game;
using VocaluxeLib.Menu;
using VocaluxeLib.Songs;
using VocaluxeLib.Utils.Combinatorics;

[assembly: ComVisible(false)]

namespace VocaluxeLib.PartyModes.Random
{
    public abstract class CPartyScreenRandom : CMenuParty
    {
        protected new CPartyModeRandom _PartyMode;

        public override void Init()
        {
            base.Init();
            _PartyMode = (CPartyModeRandom)base._PartyMode;
        }
    }

    // ReSharper disable ClassNeverInstantiated.Global
    public sealed class CPartyModeRandom : CPartyMode
    // ReSharper restore ClassNeverInstantiated.Global
    {
        public override int MinMics
        {
            get { return 1; }
        }
        public override int MaxMics
        {
            get { return 6; }
        }
        public override int MinPlayers
        {
            get { return 1; }
        }
        public override int MaxPlayers
        {
            get { return 20; }
        }
        public override int MinTeams
        {
            get { return 1; }
        }
        public override int MaxTeams
        {
            get { return 6; }
        }

        public override int MinPlayersPerTeam
        {
            get { return 1; }
        }
        public override int MaxPlayersPerTeam
        {
            get { return 10; }
        }

        private enum EStage
        {
            Config,
            Songs,
            Names,
            Main,
            SongSelection,
            Singing,
            MedleySinging
        }

        public struct SData
        {
            public int NumPlayer;
            public int[] NumPlayerInTeams;
            public List<Guid>[] ProfileIdsFromPlayerInTeams;
            public List<Guid>[] RandomProfileIdsFromPlayerInTeams;
            public int NumMics;
            public int NumRounds;
            public int NumJokers;
            public int[] RemindJokers;
            public bool[,] GameModes;
            public int[] TeamPoints;

            public ESongSource SongSource;
            public ESongSorting Sorting;
            public int CategoryIndex;
            public int PlaylistID;

            public EGameMode GameMode;
            public int NumMedleySongs;

            public int CurrentRoundNr;

            public List<int> Songs;

            public EOffOn RefillJokers;
        }

        public SData GameData;
        private EStage _Stage;

        public CPartyModeRandom(int id) : base(id)
        {
            _ScreenSongOptions.Selection.RandomOnly = false;
            _ScreenSongOptions.Selection.PartyMode = true;
            _ScreenSongOptions.Selection.CategoryChangeAllowed = true;
            _ScreenSongOptions.Selection.NumJokers = new int[] { 5, 5 };
            _ScreenSongOptions.Selection.TeamNames = new string[] { "foo", "bar" };

            _ScreenSongOptions.Sorting.SearchString = String.Empty;
            _ScreenSongOptions.Sorting.SearchActive = false;
            _ScreenSongOptions.Sorting.DuetOptions = EDuetOptions.NoDuets;
            _ScreenSongOptions.Sorting.FilterPlaylistID = -1;

            _ScreenSongOptions.Sing.MuteSong = false;
            _ScreenSongOptions.Sing.ShowText = true;
            _ScreenSongOptions.Sing.ShowNotes = true;

            GameData = new SData
            {
                NumPlayer = 0,
                NumPlayerInTeams = new int[1],
                ProfileIdsFromPlayerInTeams = new List<Guid>[1],
                RandomProfileIdsFromPlayerInTeams = new List<Guid>[1],
                NumMics = 1,
                NumRounds = 1,
                NumJokers = 0,
                RemindJokers = new int[1],
                CurrentRoundNr = 1,
                GameModes = new bool[1,1],
                TeamPoints = new int[1],
                Sorting = CBase.Config.GetSongSorting(),
                SongSource = ESongSource.TR_SONGSOURCE_ALLSONGS,
                PlaylistID = 0,
                CategoryIndex = 0,
                GameMode = EGameMode.TR_GAMEMODE_NORMAL,
                NumMedleySongs = 5,
                Songs = new List<int>(),
                RefillJokers = EOffOn.TR_CONFIG_OFF
            };
        }

        public override void SetDefaults()
        {
            _Stage = EStage.Config;

            _ScreenSongOptions.Sorting.IgnoreArticles = CBase.Config.GetIgnoreArticles();
            _ScreenSongOptions.Sorting.SongSorting = CBase.Config.GetSongSorting();
            _ScreenSongOptions.Sorting.Tabs = EOffOn.TR_CONFIG_OFF;
            _ScreenSongOptions.Selection.SongIndex = -1;

            if (CBase.Config.GetTabs() == EOffOn.TR_CONFIG_ON && _ScreenSongOptions.Sorting.SongSorting != ESongSorting.TR_CONFIG_NONE)
                _ScreenSongOptions.Sorting.Tabs = EOffOn.TR_CONFIG_ON;

        }

        public override bool Init()
        {
            if (!base.Init())
                return false;

            SetDefaults();
            return true;
        }

        public override void UpdateGame()
        {
        }

        private IMenu _GetNextScreen()
        {
            switch (_Stage)
            {
                case EStage.Config:
                    return _Screens["CPartyScreenRandomConfig"];
                case EStage.Songs:
                    return _Screens["CPartyScreenRandomSongs"];
                case EStage.Names:
                    return _Screens["CPartyScreenRandomNames"];
                case EStage.Main:
                    return _Screens["CPartyScreenRandomMain"];
                case EStage.SongSelection:
                    return CBase.Graphics.GetScreen(EScreen.Song);
                case EStage.Singing:
                    return CBase.Graphics.GetScreen(EScreen.Sing);
                case EStage.MedleySinging:
                    return CBase.Graphics.GetScreen(EScreen.Sing);
                default:
                    throw new ArgumentException("Invalid stage: " + _Stage);
            }
        }

        private void _FadeToScreen()
        {
            CBase.Graphics.FadeTo(_GetNextScreen());
        }

        public void Next()
        {
            switch (_Stage)
            {
                case EStage.Config:
                    _Stage = EStage.Songs;
                    break;
                case EStage.Songs:
                    _Stage = EStage.Names;
                    break;
                case EStage.Names:
                    _Stage = EStage.Main;
                    CBase.Songs.ResetSongSung();
                    GameData.CurrentRoundNr = 1;
                    _RandomizeSingerIds();
                    _RandomizeSingOptions();
                    GameData.RemindJokers = new int[GameData.NumMics];
                    for(int i = 0; i<GameData.NumMics; i++)
                    {
                        GameData.RemindJokers[i] = GameData.NumJokers;
                    }
                    GameData.TeamPoints = new int[GameData.NumMics];
                    for(int i = 0; i < GameData.TeamPoints.Length; i++)
                    {
                        GameData.TeamPoints[i] = 0;
                    }
                    break;
                case EStage.Main:
                    _Stage = EStage.SongSelection;
                    _PrepareSongSelection();
                    break;
                case EStage.SongSelection:
                    _Stage = EStage.Singing;
                    break;
                case EStage.Singing:
                    _Stage = EStage.Main;
                    _UpdateScores();
                    break;
                default:
                    throw new ArgumentException("Invalid stage: " + _Stage);
            }
            _FadeToScreen();
        }

        public void Back()
        {
            switch (_Stage)
            {
                case EStage.Config:
                    CBase.Graphics.FadeTo(EScreen.Party);
                    return;
                case EStage.Songs:
                    _Stage = EStage.Config;
                    break;
                case EStage.Names:
                    _Stage = EStage.Songs;
                    break;
                case EStage.Main:
                    _Stage = EStage.Names;
                    break;
                default: // Rest is not allowed
                    throw new ArgumentException("Invalid stage: " + _Stage);
            }
            _FadeToScreen();
        }

        public override IMenu GetStartScreen()
        {
            return _Screens["CPartyScreenRandomConfig"];
        }

        public override SScreenSongOptions GetScreenSongOptions()
        {
            return _ScreenSongOptions;
        }

        public override void OnSongChange(int songIndex, ref SScreenSongOptions screenSongOptions)
        {
            if (_ScreenSongOptions.Selection.SelectNextRandomSong && songIndex != -1)
                _ScreenSongOptions.Selection.SelectNextRandomSong = false;

            _ScreenSongOptions.Selection.SongIndex = songIndex;

            screenSongOptions = _ScreenSongOptions;
        }

        public override void OnCategoryChange(int categoryIndex, ref SScreenSongOptions screenSongOptions)
        {
            if (categoryIndex != -1 || CBase.Config.GetTabs() == EOffOn.TR_CONFIG_OFF)
            {
                //If category is selected or tabs off: only random song selection
                _ScreenSongOptions.Selection.SelectNextRandomSong = true;
                _ScreenSongOptions.Selection.RandomOnly = true;
            }
            else
            {
                //If no category is selected: let user choose category
                _ScreenSongOptions.Selection.SongIndex = -1;
                _ScreenSongOptions.Selection.RandomOnly = false;
            }

            _ScreenSongOptions.Selection.CategoryIndex = categoryIndex;

            screenSongOptions = _ScreenSongOptions;
        }

        public override void SetSearchString(string searchString, bool visible)
        {
            _ScreenSongOptions.Sorting.SearchString = searchString;
            _ScreenSongOptions.Sorting.SearchActive = visible;
        }

        public override void JokerUsed(int teamNr)
        {
            GameData.RemindJokers[teamNr]--;
            _ScreenSongOptions.Selection.NumJokers = GameData.RemindJokers;
            _ScreenSongOptions.Selection.RandomOnly = true;
            _ScreenSongOptions.Selection.CategoryChangeAllowed = false;
        }

        public override void SongSelected(int songID)
        {
            _StartRound(new int[] { songID });

            Next();
        }

        public void UpdateSongList()
        {
            if (GameData.Songs.Count > 0)
                return;

            switch (GameData.SongSource)
            {
                case ESongSource.TR_SONGSOURCE_PLAYLIST:
                    for (int i = 0; i < CBase.Playlist.GetSongCount(GameData.PlaylistID); i++)
                    {
                        int id = CBase.Playlist.GetSong(GameData.PlaylistID, i).SongID;
                    }
                    break;

                case ESongSource.TR_SONGSOURCE_ALLSONGS:
                    ReadOnlyCollection<CSong> avSongs = CBase.Songs.GetSongs();
                    break;

                case ESongSource.TR_SONGSOURCE_CATEGORY:
                    CBase.Songs.SortSongs(GameData.Sorting, EOffOn.TR_CONFIG_ON, CBase.Config.GetIgnoreArticles(), "", EDuetOptions.All, -1);
                    CBase.Songs.SetCategory(GameData.CategoryIndex);
                    avSongs = CBase.Songs.GetVisibleSongs();

                    CBase.Songs.SetCategory(-1);
                    break;
            }
            GameData.Songs.Shuffle();
        }

        private void _PrepareSongSelection()
        {
            _ScreenSongOptions.Selection.RandomOnly = true;
            _ScreenSongOptions.Selection.SelectNextRandomSong = true;

            _ScreenSongOptions.Sorting.IgnoreArticles = CBase.Config.GetIgnoreArticles();

            switch (GameData.SongSource)
            {
                case ESongSource.TR_SONGSOURCE_ALLSONGS:
                    _ScreenSongOptions.Sorting.SongSorting = CBase.Config.GetSongSorting();
                    _ScreenSongOptions.Sorting.Tabs = CBase.Config.GetTabs();
                    _ScreenSongOptions.Sorting.FilterPlaylistID = -1;

                    _ScreenSongOptions.Selection.SongIndex = -1;
                    _ScreenSongOptions.Selection.CategoryIndex = -1;
                    _ScreenSongOptions.Selection.CategoryChangeAllowed = true;
                    break;

                case ESongSource.TR_SONGSOURCE_CATEGORY:
                    _ScreenSongOptions.Sorting.SongSorting = GameData.Sorting;
                    _ScreenSongOptions.Sorting.Tabs = EOffOn.TR_CONFIG_ON;
                    _ScreenSongOptions.Sorting.FilterPlaylistID = -1;

                    _ScreenSongOptions.Selection.CategoryIndex = GameData.CategoryIndex;
                    _ScreenSongOptions.Selection.CategoryChangeAllowed = false;
                    break;

                case ESongSource.TR_SONGSOURCE_PLAYLIST:
                    _ScreenSongOptions.Sorting.SongSorting = CBase.Config.GetSongSorting();
                    _ScreenSongOptions.Sorting.Tabs = EOffOn.TR_CONFIG_OFF;
                    _ScreenSongOptions.Sorting.FilterPlaylistID = GameData.PlaylistID;

                    _ScreenSongOptions.Selection.CategoryChangeAllowed = false;
                    break;
            }

            _SetNumJokers();
            _SetTeamNames();
        }

        private void _SetNumJokers()
        {
            if(GameData.RefillJokers == EOffOn.TR_CONFIG_ON)
            {
                for (int i = 0; i < GameData.NumMics; i++)
                    GameData.RemindJokers[i] = GameData.NumJokers;
            }
            _ScreenSongOptions.Selection.NumJokers = GameData.RemindJokers;
        }

        private void _SetTeamNames()
        {
            _ScreenSongOptions.Selection.TeamNames = new string[GameData.NumMics];

            for (int i = 0; i < GameData.NumMics; i++)
                _ScreenSongOptions.Selection.TeamNames[i] = CBase.Profiles.GetPlayerName(GameData.RandomProfileIdsFromPlayerInTeams[i][GameData.CurrentRoundNr - 1]);
        }

        private bool _StartRound(int[] songIDs)
        {
            //Reset game
            CBase.Game.Reset();
            CBase.Game.ClearSongs();

            #region PlayerNames
            CBase.Game.SetNumPlayer(GameData.NumMics);
            SPlayer[] players = CBase.Game.GetPlayers();

            for (int i = 0; i < GameData.NumMics; i++)
            {
                //default values
                players[i].ProfileID = Guid.Empty;
            }

            for (int i = 0; i < GameData.NumMics; i++)
            {
                //try to fill with correct player data
                players[i].ProfileID = GameData.RandomProfileIdsFromPlayerInTeams[i][GameData.CurrentRoundNr - 1];
            }
            #endregion PlayerNames
            #region Song options
            _ScreenSongOptions.Sing.MuteSong = GameData.GameModes[GameData.CurrentRoundNr - 1,0];
            _ScreenSongOptions.Sing.ShowText = GameData.GameModes[GameData.CurrentRoundNr - 1,1];
            _ScreenSongOptions.Sing.ShowNotes = GameData.GameModes[GameData.CurrentRoundNr - 1,2];
            #endregion Son options
            #region SongQueue
            //Add all songs with configure game mode to song queue
            for (int i = 0; i < songIDs.Length; i++)
                CBase.Game.AddSong(songIDs[i], GameData.GameMode);
            #endregion SongQueue
            
            return true;
        }

        public void _UpdateScores()
        {
            CPoints points = CBase.Game.GetPoints();
            SPlayer[] playerFromPoints = points.GetPlayer(0, GameData.NumMics);
            for (int i = 0; i < GameData.NumMics; i++)
            {
                GameData.TeamPoints[i] = (int)Math.Round(playerFromPoints[i].Points);
            }

            if (GameData.CurrentRoundNr == GameData.NumRounds)
            {
                _Stage = EStage.Config;
            }
            GameData.CurrentRoundNr++;
        }

        public override void LeavingHighscore()
        {
            Next();
        }

        public void _RandomizeSingerIds()
        {
            GameData.RandomProfileIdsFromPlayerInTeams = new List<Guid>[GameData.NumMics];

            System.Random rnd = new System.Random();
            List<Guid>[] help = new List<Guid>[GameData.NumMics];
            int h = 0;
            for(int j = 0; j<GameData.NumMics; j++)
            {
                help[j] = new List<Guid>();
                h = 0;
                for (int i = 0; i < GameData.NumRounds; i++)
                {
                    if (GameData.ProfileIdsFromPlayerInTeams[j].Count <= h)
                    {
                        h = 0;
                    }
                    help[j].Add(GameData.ProfileIdsFromPlayerInTeams[j][h]);
                    h++;
                }    
            }
            for (int j = 0; j < GameData.NumMics; j++)
            {
                GameData.RandomProfileIdsFromPlayerInTeams[j] = new List<Guid>();
                for (int i = 0; i < GameData.NumRounds; i++)
                {
                    h = rnd.Next(0, help[j].Count);
                    GameData.RandomProfileIdsFromPlayerInTeams[j].Add(help[j][h]);
                    help[j].RemoveAt(h);
                }
            }
        }
        
        public void _RandomizeSingOptions()
        {
            System.Random rnd = new System.Random();
            GameData.GameModes = new bool[GameData.NumRounds,3];
            for(int i = 0; i<GameData.NumRounds; i++)
            {
                GameData.GameModes[i,0] = (0 == rnd.Next(0, 2));
                GameData.GameModes[i,1] = (0 == rnd.Next(0, 2));
                GameData.GameModes[i,2] = (0 == rnd.Next(0, 2));
            }
        }
    }
}