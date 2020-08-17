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
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using VocaluxeLib.Songs;
using VocaluxeLib.Menu;
using System.Security.Cryptography;

namespace VocaluxeLib.PartyModes.Random
{
    // ReSharper disable UnusedMember.Global
    public class CPartyScreenRandomTeamNames : CPartyScreenRandom
    // ReSharper restore UnusedMember.Global
    {
        // Version number for theme files. Increment it, if you've changed something on the theme files!
        protected override int _ScreenVersion
        {
            get { return 1; }
        }

        private const string _ButtonNext = "ButtonNext";
        private const string _ButtonSelectedTeam = "ButtonSelectedTeam";

        private const string _TextTeams = "TextTeams";

        private List<CText> _Teams;

        private int _AusgewaehltesTeam = 1;
        private SColorF _White = new SColorF(1, 1, 1, 1);
        private SColorF _Color = new SColorF(Convert.ToSingle(31.0 / 255.0), 0, Convert.ToSingle(161.0 / 255.0), 1);

        public override void Init()
        {
            base.Init();
            _ThemeTexts = new string[]
            {
                _TextTeams
            };
            _ThemeButtons = new string[] 
            {
                _ButtonNext, _ButtonSelectedTeam
            };
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
                        if (_Teams[_AusgewaehltesTeam].Text.Length > 0)
                        {
                            _Teams[_AusgewaehltesTeam].Text = _Teams[_AusgewaehltesTeam].Text.Remove(_Teams[_AusgewaehltesTeam].Text.Length - 1);
                        }
                        break;
                    case Keys.Escape:
                        break;
                    case Keys.Enter:
                        if (_Buttons[_ButtonNext].Selected)
                        {
                            _PartyMode.GameData.TeamNames = new string[_PartyMode.GameData.NumMics];
                            for (int i = 0; i < _PartyMode.GameData.TeamNames.Length; i++)
                            {
                                _PartyMode.GameData.TeamNames[i] = _Teams[i + 1].Text;
                            }
                            
                            _PartyMode.Next();
                        }
                        else if ((_Buttons[_ButtonSelectedTeam].Selected))
                        {
                            _Teams[_AusgewaehltesTeam].Color = _White;
                            if (_AusgewaehltesTeam < _PartyMode.GameData.NumMics)
                            {
                                _AusgewaehltesTeam ++; 
                            } else
                            {
                                _AusgewaehltesTeam = 1;
                            }
                            _Teams[_AusgewaehltesTeam].Color = _Color;
                        }
                        break;
                    case Keys.Left:
                        break;
                    case Keys.Right:
                        break;
                    case Keys.Up:
                        break;
                    case Keys.Down:
                        break;
                    default:
                        _Teams[_AusgewaehltesTeam].Text += new KeysConverter().ConvertToString(keyEvent.Key);
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
                if (_Buttons[_ButtonNext].Selected)
                {
                    _PartyMode.GameData.TeamNames = new string[_PartyMode.GameData.NumMics];
                    for (int i = 0; i < _PartyMode.GameData.TeamNames.Length; i++)
                    {
                        _PartyMode.GameData.TeamNames[i] = _Teams[i + 1].Text;
                    }

                    _PartyMode.Next();
                }
                else if ((_Buttons[_ButtonSelectedTeam].Selected))
                {
                    _Teams[_AusgewaehltesTeam].Color = _White;
                    if (_AusgewaehltesTeam < _PartyMode.GameData.NumMics)
                    {
                        _AusgewaehltesTeam++;
                    }
                    else
                    {
                        _AusgewaehltesTeam = 1;
                    }
                    _Teams[_AusgewaehltesTeam].Color = _Color;
                }
            }
            return true;
        }

        public override void OnShow()
        {
            base.OnShow();
            
            _Teams = new List<CText>();
            for (int i = 0; i <= _PartyMode.GameData.NumMics; i++)
            {
                _Teams.Add(GetNewText(_Texts[_TextTeams]));
                _AddText(_Teams[i]);
            }
            _Teams[0].X = 650;
            _Teams[0].Y = 200;
            _Teams[0].Text = "Teamnamen:";
            _Teams[0].Visible = true;
            for (int i = 1; i <= _PartyMode.GameData.NumMics; i++)
            {
                _Teams[i].X = 650;
                _Teams[i].Y = 200 + i * 50;
                _Teams[i].Text = "Team " + i;
                _Teams[i].Visible = true;
            }
            _Teams[_AusgewaehltesTeam].Color = _Color;
        }

        public override bool UpdateGame()
        {
            return true;
        }
    }
}