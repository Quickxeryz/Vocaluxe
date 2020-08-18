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
using System.Windows.Forms;
using VocaluxeLib.Menu;
using VocaluxeLib.Songs;

namespace VocaluxeLib.PartyModes.Random
{
    // ReSharper disable UnusedMember.Global
    public class CPartyScreenRandomMain : CPartyScreenRandom
    // ReSharper restore UnusedMember.Global
    {
        // Version number for theme files. Increment it, if you've changed something on the theme files!
        protected override int _ScreenVersion
        {
            get { return 1; }
        }

        private const string _ButtonNextRound = "ButtonNextRound";
        private const string _TextNextPlayer = "TextNextPlayer";
        private const string _StaticNextPlayer = "StaticNextPlayer";
        private const string _TextNextGameMode = "TextNextGameMode";
        private const string _TextPoints = "TextPoints";

        private List<CText> _NextPlayerTexts;
        private List<CStatic> _NextPlayerStatics;
        private List<CText> _NextGameMode;
        private List<CText> _Points;

        private bool flag = true;

        public override void Init()
        {
            base.Init();

            _ThemeTexts = new string[]
            {
                _TextNextPlayer, _TextNextGameMode, _TextPoints
            };
            _ThemeButtons = new string[]
            {
                _ButtonNextRound
            };
            _ThemeStatics = new string[] 
            { 
                _StaticNextPlayer
            };
        }

        public override void LoadTheme(string xmlPath)
        {
            base.LoadTheme(xmlPath);

            _NextPlayerTexts = new List<CText>();
            _NextPlayerStatics = new List<CStatic>();
            _Points = new List<CText>();

            for (int i = 0; i < _PartyMode.MaxPlayers; i++)
            {
                _NextPlayerTexts.Add(GetNewText(_Texts[_TextNextPlayer]));
                _AddText(_NextPlayerTexts[_NextPlayerTexts.Count - 1]);
                _NextPlayerStatics.Add(GetNewStatic(_Statics[_StaticNextPlayer]));
                _AddStatic(_NextPlayerStatics[_NextPlayerStatics.Count - 1]);
                _NextPlayerStatics[_NextPlayerStatics.Count - 1].Aspect = EAspect.Crop;
            }
            _Statics[_StaticNextPlayer].Visible = false;

            _NextGameMode = new List<CText>();
            for (int i = 0; i <= 4; i++)
            {
                _NextGameMode.Add(GetNewText(_Texts[_TextNextGameMode]));
                _AddText(_NextGameMode[i]);
            }
            _NextGameMode[0].X = 50;
            _NextGameMode[0].Y = 50;
            _NextGameMode[0].Text = "Spielmodus:";
            _NextGameMode[0].Visible = true;
        }

        public override void OnShow()
        {
            base.OnShow();

            _UpdateNextPlayerPositions();
            _UpdateNextPlayerContents();
            for (int i = 1; i <= 4; i++)
            {
                _NextGameMode[i].X = 50;
                _NextGameMode[i].Y = 50 + i * 50;
                switch (i)
                {
                    case 1:
                        if (_PartyMode.GameData.GameModes[_PartyMode.GameData.CurrentRoundNr - 1, i - 1])
                        {
                            _NextGameMode[i].Text = "ohne Ton";
                        }
                        else
                        {
                            _NextGameMode[i].Text = "mit Ton";
                        }
                        break;
                    case 2:
                        if (_PartyMode.GameData.GameModes[_PartyMode.GameData.CurrentRoundNr - 1, i - 1])
                        {
                            _NextGameMode[i].Text = "mit Text";
                        }
                        else
                        {
                            _NextGameMode[i].Text = "ohne Text";
                        }
                        break;
                    case 3:
                        if (_PartyMode.GameData.GameModes[_PartyMode.GameData.CurrentRoundNr - 1, i - 1])
                        {
                            _NextGameMode[i].Text = "mit Noten";
                        }
                        else
                        {
                            _NextGameMode[i].Text = "ohne Noten";
                        }
                        break;
                    case 4:
                        if (_PartyMode.GameData.GameModes[_PartyMode.GameData.CurrentRoundNr - 1, i - 1])
                        {
                            _NextGameMode[i].Text = "Bis " + _PartyMode.GameData.MaxPointsSong[_PartyMode.GameData.CurrentRoundNr - 1] + " Punkte"; 
                        }
                        else
                        {
                            _NextGameMode[i].Text = "";
                        }
                        break;
                }
                _NextGameMode[i].Visible = true;
            }
            if(flag)
            {
                for (int i = 0; i <= _PartyMode.GameData.NumMics; i++)
                {
                    _Points.Add(GetNewText(_Texts[_TextPoints]));
                    _AddText(_Points[i]);
                }
                _Points[0].X = 1000;
                _Points[0].Y = 50;
                _Points[0].Text = "Punktzahl:";
                _Points[0].Visible = true;
                flag = false;
            }
            for (int i = 1; i<=_PartyMode.GameData.NumMics; i++)
            {
                _Points[i].X = 1000;
                _Points[i].Y = 50 + i * 50;
                _Points[i].Text = _PartyMode.GameData.TeamNames[i - 1] + ": " + _PartyMode.GameData.TeamPoints[i - 1];
                _Points[i].Color = CBase.Themes.GetPlayerColor(i);
                _Points[i].Visible = true;
            }
        }

        public override bool UpdateGame()
        {
            return true;
        }

        public override bool HandleInput(SKeyEvent keyEvent)
        {
            base.HandleInput(keyEvent);

            if (keyEvent.KeyPressed) { }
            else
            {
                switch (keyEvent.Key)
                {
                    case Keys.Back:
                        break;
                    case Keys.Escape:
                        break;
                    case Keys.Enter:
                        if (_Buttons[_ButtonNextRound].Selected) 
                        {
                            _PartyMode.Next();
                        }
                        break;
                }
            }
            return true;
        }

        public override bool HandleMouse(SMouseEvent mouseEvent)
        {
            base.HandleMouse(mouseEvent);

            if (mouseEvent.LB && _IsMouseOverCurSelection(mouseEvent))
            {
                if (_Buttons[_ButtonNextRound].Selected) 
                {
                    _PartyMode.Next();
                }
            }

            if (mouseEvent.RB)
            {
                if (_PartyMode.GameData.CurrentRoundNr == 1)
                {
                    CBase.Graphics.FadeTo(EScreen.Party);
                }
            }
            if (mouseEvent.Wheel != 0)
            {
            }

            return true;
        }

        private void _UpdateNextPlayerPositions()
        {
            float x = (float)CBase.Settings.GetRenderW() / 2 -
                      ((_PartyMode.GameData.NumMics * _Statics[_StaticNextPlayer].Rect.W) + ((_PartyMode.GameData.NumMics - 1) * 15)) / 2;
            const float staticY = 590;
            const float textY = 550;
            for (int i = 0; i < _PartyMode.GameData.NumMics; i++)
            {
                //static
                _NextPlayerStatics[i].X = x;
                _NextPlayerStatics[i].Y = staticY;
                _NextPlayerStatics[i].Visible = true;
                //text
                _NextPlayerTexts[i].X = x + _Statics[_StaticNextPlayer].Rect.W / 2;
                _NextPlayerTexts[i].Y = textY;
                _NextPlayerTexts[i].Visible = true;

                x += _Statics[_StaticNextPlayer].Rect.W + 15;
            }
            for (int i = _PartyMode.GameData.NumMics; i < _PartyMode.MaxPlayers; i++)
            {
                _NextPlayerStatics[i].Visible = false;
                _NextPlayerTexts[i].Visible = false;
            }
        }

        private void _UpdateNextPlayerContents()
        {
            for (int i = 0; i < _PartyMode.GameData.NumMics; i++)
            {
                Guid id = _PartyMode.GameData.RandomProfileIdsFromPlayerInTeams[i][_PartyMode.GameData.CurrentRoundNr - 1];
                _NextPlayerStatics[i].Texture = CBase.Profiles.GetAvatar(id);
                _NextPlayerTexts[i].Text = CBase.Profiles.GetPlayerName(id);
                _NextPlayerTexts[i].Color = CBase.Themes.GetPlayerColor(i + 1);
            }
        }

        private void _EndParty()
        {
            CBase.Graphics.FadeTo(EScreen.Party);
        }
    }
}