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
using System.Runtime.ExceptionServices;
using VocaluxeLib.Menu;
using VocaluxeLib.Profile;

namespace VocaluxeLib.PartyModes.Random
{
    // ReSharper disable UnusedMember.Global
    public class CPartyScreenRandomNames : CMenuPartyNameSelection
        // ReSharper restore UnusedMember.Global
    {
        // Version number for theme files. Increment it, if you've changed something on the theme files!
        protected override int _ScreenVersion
        {
            get { return 1; }
        }

        private new CPartyModeRandom _PartyMode;

        public bool firstTime = true;

        public override void Init()
        {
            base.Init();
            _PartyMode = (CPartyModeRandom)base._PartyMode;
        }

        public override void OnShow()
        {
            base.OnShow();

            _CurrentTeam = 0;

            int[] amountPlayer = new int[_PartyMode.GameData.NumMics];
            int tooMuchPlayer = _PartyMode.GameData.NumPlayer % _PartyMode.GameData.NumMics;

            for (int i = 0; i < amountPlayer.Length; i++)
            {
                if (tooMuchPlayer > 0)
                {
                    amountPlayer[i] = (_PartyMode.GameData.NumPlayer / _PartyMode.GameData.NumMics) + 1;
                    tooMuchPlayer = tooMuchPlayer - 1;
                }
                else
                {
                    amountPlayer[i] = _PartyMode.GameData.NumPlayer / _PartyMode.GameData.NumMics;
                }
            }
            
            if (!firstTime)
            {
                for (int i = _PartyMode.GameData.NumMics; i < _TeamList.Length; i++)
                {
                    for (int j = 0; j < _PartyMode.GameData.NumPlayerInTeams[i]; j++)
                    {
                        _RemovePlayerByIndex(i, j);
                    }
                }
                for (int i = 0; i < _PartyMode.GameData.NumMics; i++)
                {
                    if (_NumPlayerTeams.Length > i)
                    {
                        for (int j = amountPlayer[i]; j < _NumPlayerTeams[i]; j++)
                        {
                            _RemovePlayerByIndex(i, j);
                        }
                    }
                }
            }

            SetPartyModeData(_PartyMode.GameData.NumMics, _PartyMode.GameData.NumPlayer, amountPlayer);
            
            List<Guid>[] ids;

            switch (_PartyMode.GameData.NumMics)
            {
                case 1:
                    ids = new List<Guid>[] { new List<Guid>() };
                    break;
                case 2:
                    ids = new List<Guid>[] { new List<Guid>(), new List<Guid>() };
                    break;
                case 3:
                    ids = new List<Guid>[] { new List<Guid>(), new List<Guid>(), new List<Guid>() };
                    break;
                case 4:
                    ids = new List<Guid>[] { new List<Guid>(), new List<Guid>(), new List<Guid>(), new List<Guid>() };
                    break;
                case 5:
                    ids = new List<Guid>[] { new List<Guid>(), new List<Guid>(), new List<Guid>(), new List<Guid>(), new List<Guid>() };
                    break;
                case 6:
                    ids = new List<Guid>[] { new List<Guid>(), new List<Guid>(), new List<Guid>(), new List<Guid>(), new List<Guid>(), new List<Guid>() };
                    break;
                default:    //error case
                    ids = new List<Guid>[] { new List<Guid>() };
                    break;
            }
            for (int i = 0; i < ids.Length; i++)
            {
                if (_PartyMode.GameData.ProfileIdsFromPlayerInTeams.Length > i)
                {
                    ids[i] = _PartyMode.GameData.ProfileIdsFromPlayerInTeams[i];
                }
            }
            SetPartyModeProfiles(ids);
        }

        public override void Back()
        {
            firstTime = false;

            _PartyMode.GameData.ProfileIdsFromPlayerInTeams = new List<Guid>[_TeamList.Length];
            for (int i = 0; i < _PartyMode.GameData.ProfileIdsFromPlayerInTeams.Length; i++)
            {
                _PartyMode.GameData.ProfileIdsFromPlayerInTeams[i] = _TeamList[i];
            }
            _PartyMode.GameData.NumPlayerInTeams = new int[_NumPlayerTeams.Length];
            for (int i = 0; i < _PartyMode.GameData.NumPlayerInTeams.Length; i++)
            {
                _PartyMode.GameData.NumPlayerInTeams[i] = _NumPlayerTeams[i];
            }

            _PartyMode.Back();
        }

        public override void Next()
        {
            firstTime = false;

            _PartyMode.GameData.NumMics = _TeamList.Length;
            _PartyMode.GameData.NumPlayer = 0;
            for (int i = 0; i < _NumPlayerTeams.Length; i++)
            {
                _PartyMode.GameData.NumPlayer += _NumPlayerTeams[i];
            }

            _PartyMode.GameData.NumPlayerInTeams = new int[_NumPlayerTeams.Length];
            for (int i = 0; i < _PartyMode.GameData.NumPlayerInTeams.Length; i++)
            {
                _PartyMode.GameData.NumPlayerInTeams[i] = _NumPlayerTeams[i];
            }
            _PartyMode.GameData.ProfileIdsFromPlayerInTeams = new List<Guid>[_TeamList.Length];
            for (int i = 0; i < _PartyMode.GameData.ProfileIdsFromPlayerInTeams.Length; i++)
            {
                _PartyMode.GameData.ProfileIdsFromPlayerInTeams[i] = _TeamList[i];
            }

            _PartyMode.Next();
        }
    }
}