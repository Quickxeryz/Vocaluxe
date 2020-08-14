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

namespace VocaluxeLib.PartyModes.Random
{
    // ReSharper disable UnusedMember.Global
    public class CPartyScreenRandomEnd : CPartyScreenRandom
    // ReSharper restore UnusedMember.Global
    {
        // Version number for theme files. Increment it, if you've changed something on the theme files!
        protected override int _ScreenVersion
        {
            get { return 1; }
        }

        private const string _ButtonNext = "ButtonNext";
        private const string _TextPoints = "TextPoints";

        private List<CText> _Points;

        public override void Init()
        {
            base.Init();
            _ThemeTexts = new string[]
            {
                _TextPoints
            };
            _ThemeButtons = new string[] 
            {
                _ButtonNext
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
                        break;
                    case Keys.Escape:
                        break;
                    case Keys.Enter:
                        if (_Buttons[_ButtonNext].Selected)
                        {
                            CBase.Graphics.FadeTo(EScreen.Party);
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
                if (_Buttons[_ButtonNext].Selected)
                {
                    CBase.Graphics.FadeTo(EScreen.Party);
                }
            }
            return true;
        }

        public override void OnShow()
        {
            base.OnShow();
            _Points = new List<CText>();
            for (int i = 0; i <= _PartyMode.GameData.NumMics; i++)
            {
                _Points.Add(GetNewText(_Texts[_TextPoints]));
                _AddText(_Points[i]);
            }
            _Points[0].X = 650;
            _Points[0].Y = 200;
            _Points[0].Text = "Endergebnis:";
            _Points[0].Visible = true;
            int[] place = {-1, -1}; //{Points, Index}
            for (int i = 1; i <= _PartyMode.GameData.NumMics; i++)
            {
                for(int j = 0; j<_PartyMode.GameData.TeamPoints.Length; j++)
                {
                    if(_PartyMode.GameData.TeamPoints[j] > place[0])
                    {
                        place[0] = _PartyMode.GameData.TeamPoints[j];
                        place[1] = j;
                    }
                }
                _Points[i].X = 650;
                _Points[i].Y = 200 + i * 50;
                _Points[i].Text = i + ". Platz mit " + _PartyMode.GameData.TeamPoints[place[1]] + " Punkten ist Team " + (place[1] + 1) + ".";
                _Points[i].Color = CBase.Themes.GetPlayerColor(i);
                _Points[i].Visible = true;
                _PartyMode.GameData.TeamPoints[place[1]] = - 1;
                place[0] = -1;
                place[1] = -1;
            }
        }

        public override bool UpdateGame()
        {
            return true;
        }
    }
}