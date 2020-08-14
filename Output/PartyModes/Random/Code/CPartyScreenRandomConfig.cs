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

namespace VocaluxeLib.PartyModes.Random
{
    // ReSharper disable UnusedMember.Global
    public class CPartyScreenRandomConfig : CPartyScreenRandom
        // ReSharper restore UnusedMember.Global
    {
        // Version number for theme files. Increment it, if you've changed something on the theme files!
        protected override int _ScreenVersion
        {
            get { return 2; }
        }

        private const string _SelectSlideNumPlayer = "SelectSlideNumPlayer";
        private const string _SelectSlideNumMics = "SelectSlideNumMics";
        private const string _SelectSlideNumRounds = "SelectSlideNumRounds";
        private const string _SelectSlideNumJokers = "SelectSlideNumJokers";
        private const string _SelectSlideRefillJokers = "SelectSlideRefillJokers";

        private const string _ButtonNext = "ButtonNext";
        private const string _ButtonBack = "ButtonBack";

        private bool _ConfigOk = true;

        public override void Init()
        {
            base.Init();

            _ThemeSelectSlides = new string[] { _SelectSlideNumPlayer, _SelectSlideNumMics, _SelectSlideNumRounds, _SelectSlideRefillJokers};
            _ThemeButtons = new string[] {_ButtonNext, _ButtonBack};
        }

        public override bool HandleInput(SKeyEvent keyEvent)
        {
            base.HandleInput(keyEvent);

            if (keyEvent.KeyPressed) {}
            else
            {
                switch (keyEvent.Key)
                {
                    case Keys.Back:
                    case Keys.Escape:
                        _PartyMode.Back();
                        break;

                    case Keys.Enter:
                        _UpdateSlides();

                        if (_Buttons[_ButtonBack].Selected)
                            _PartyMode.Back();

                        if (_Buttons[_ButtonNext].Selected)
                            _PartyMode.Next();
                        break;

                    case Keys.Left:
                        _UpdateSlides();
                        break;

                    case Keys.Right:
                        _UpdateSlides();
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
                _UpdateSlides();
                if (_Buttons[_ButtonBack].Selected)
                    _PartyMode.Back();

                if (_Buttons[_ButtonNext].Selected)
                    _PartyMode.Next();
            }

            if (mouseEvent.RB)
                _PartyMode.Back();

            return true;
        }

        public override void OnShow()
        {
            base.OnShow();

            Debug.Assert(CBase.Config.GetMaxNumMics() >= 1);

            _FillSlides();
            _UpdateSlides();
        }

        public override bool UpdateGame()
        {
            _Buttons[_ButtonNext].Visible = _ConfigOk;
            return true;
        }

        private void _FillSlides()
        {
            _SelectSlides[_SelectSlideNumPlayer].Clear();
            for (int i = 1; i <= 20; i++)
            {
                _SelectSlides[_SelectSlideNumPlayer].AddValue(i);
            }
            _SelectSlides[_SelectSlideNumPlayer].SelectedTag = _PartyMode.GameData.NumPlayer;

            _SelectSlides[_SelectSlideNumMics].Clear();
            for(int i = 1; i<=6; i++) 
            {
                _SelectSlides[_SelectSlideNumMics].AddValue(i);
            }
            _SelectSlides[_SelectSlideNumMics].SelectedTag = _PartyMode.GameData.NumMics;

            _SelectSlides[_SelectSlideNumRounds].Clear();
            for (int i = 1; i <= 100; i++)
            {
                _SelectSlides[_SelectSlideNumRounds].AddValue(i);
            }
            _SelectSlides[_SelectSlideNumRounds].SelectedTag = _PartyMode.GameData.NumRounds;

            _SelectSlides[_SelectSlideNumJokers].Clear();
            for (int i = 0; i <= 10; i++)
            {
                _SelectSlides[_SelectSlideNumJokers].AddValue(i);
            }
            _SelectSlides[_SelectSlideNumJokers].SelectedTag = _PartyMode.GameData.NumJokers;

            _SelectSlides[_SelectSlideRefillJokers].AddValues(Enum.GetNames(typeof(EOffOn)));
            _SelectSlides[_SelectSlideRefillJokers].Selection = (int)_PartyMode.GameData.RefillJokers;
        }

        private void _UpdateSlides()
        {
            _PartyMode.GameData.NumPlayer = _SelectSlides[_SelectSlideNumPlayer].SelectedTag;
            _PartyMode.GameData.NumMics = _SelectSlides[_SelectSlideNumMics].SelectedTag;
            _PartyMode.GameData.NumRounds = _SelectSlides[_SelectSlideNumRounds].SelectedTag;
            _PartyMode.GameData.NumJokers = _SelectSlides[_SelectSlideNumJokers].SelectedTag;
            _PartyMode.GameData.RefillJokers = (EOffOn)_SelectSlides[_SelectSlideRefillJokers].Selection;
        }
    }
}