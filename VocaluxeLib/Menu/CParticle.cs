﻿#region license
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
using System.Xml.Serialization;
using VocaluxeLib.Draw;
using VocaluxeLib.Xml;

namespace VocaluxeLib.Menu
{
    [XmlType("ParticleEffect")]
    public struct SThemeParticleEffect
    {
        [XmlAttribute(AttributeName = "Name")] public string Name;

        public string Skin;
        public SRectF Rect;
        public SThemeColor Color;
        public EParticleType Type;
        public float Size;
        public int MaxNumber;
    }

    public enum EParticleType
    {
        Twinkle,
        Star,
        Snow,
        Flare,
        PerfNoteStar
    }

    class CParticle
    {
        #region private vars
        private readonly int _PartyModeID;
        private readonly string _TextureName;
        private readonly CTextureRef _Texture;
        private SRectF _Rect;
        private float _Size;
        private SColorF _Color;
        private float _Alpha = 1;
        private float _Angle; //0..360°
        private readonly float _MaxAge; //[s]
        private float _Age; //[s]
        private readonly float _Vx; //movement speed in x-axis [pix/s]
        private readonly float _Vy; //movement speed in y-axis [pix/s]
        private readonly float _Vr; //rotation speed [rpm]
        private readonly float _Rotation; //start rotation 0..360°
        private readonly float _Vsize; //size changing speed: period [s]
        private float _LastTime;
        private readonly EParticleType _Type;

        private readonly Stopwatch _Timer = new Stopwatch();
        #endregion private vars

        #region public vars
        //public bool Visible;
        public float Alpha2 = 1f;

        public float X
        {
            get { return _Rect.X; }
            set { _Rect.X = value; }
        }

        public float Y
        {
            get { return _Rect.Y; }
            set { _Rect.Y = value; }
        }

        public float Size
        {
            get { return _Size; }
            set
            {
                _Rect.W = value;
                _Rect.H = value;
                _Size = value;
            }
        }

        public float Alpha
        {
            get { return _Alpha; }
            set { _Alpha = value; }
        }

        public SColorF Color
        {
            get { return _Color; }
            set { _Color = value; }
        }

        public bool IsAlive
        {
            get { return _Age < _MaxAge || Math.Abs(_MaxAge) < float.Epsilon; }
        }
        #endregion public vars

        #region Constructors
        public CParticle(int partyModeID, string textureName, SColorF color, float x, float y, float size, float maxage, float z, float vx, float vy, float vr, float vsize,
                         EParticleType type)
        {
            _PartyModeID = partyModeID;
            _TextureName = textureName;
            _Color = color;
            _Rect = new SRectF(x, y, size, size, z);
            _Size = size;
            _Vx = vx;
            _Vy = vy;
            _Vr = vr;
            _Vsize = vsize;
            _Type = type;

            _MaxAge = maxage;
            _Rotation = (float)(CBase.Game.GetRandomDouble() * 360.0);
        }

        public CParticle(int partyModeID, CTextureRef texture, SColorF color, float x, float y, float size, float maxage, float z, float vx, float vy, float vr, float vsize,
                         EParticleType type)
        {
            _PartyModeID = partyModeID;
            _TextureName = String.Empty;
            _Texture = texture;
            _Color = color;
            _Rect = new SRectF(x, y, size, size, z);
            _Size = size;
            _Vx = vx;
            _Vy = vy;
            _Vr = vr;
            _Vsize = vsize;
            _Type = type;

            _MaxAge = maxage;
            _Rotation = (float)(CBase.Game.GetRandomDouble() * 360.0);
        }
        #endregion Constructors

        public void Update()
        {
            if (!IsAlive)
                return;

            if (!_Timer.IsRunning)
                _Timer.Start();

            float currentTime = _Timer.ElapsedMilliseconds / 1000f;
            float timediff = currentTime - _LastTime;

            _Age = currentTime;

            // update alpha
            if (_MaxAge > 0f)
            {
                switch (_Type)
                {
                    case EParticleType.Twinkle:
                        _Alpha = 1f - _Age / _MaxAge;
                        break;

                    case EParticleType.Star:
                        _Alpha = 1f - _Age / _MaxAge;
                        break;

                    case EParticleType.Snow:
                        _Alpha = (float)Math.Sqrt((Math.Sin(_Age / _MaxAge * Math.PI * 2 - 0.5 * Math.PI) + 1) / 2);
                        break;

                    case EParticleType.Flare:
                        _Alpha = 1f - _Age / _MaxAge;
                        break;

                    case EParticleType.PerfNoteStar:
                        _Alpha = 1f - _Age / _MaxAge;
                        break;
                }
            }

            // update position
            switch (_Type)
            {
                case EParticleType.Twinkle:
                    X += _Vx * timediff;
                    Y += _Vy * timediff;
                    break;

                case EParticleType.Star:
                    X += _Vx * timediff;
                    Y += _Vy * timediff;
                    break;

                case EParticleType.Snow:
                    var maxy = (int)Math.Round(CBase.Settings.GetRenderH() - _Size * 0.4f);

                    if (Math.Round(Y) < maxy)
                    {
                        float vdx = 0f;
                        if (Math.Abs(_Vx) > float.Epsilon)
                            vdx = (float)Math.Sin(currentTime / _Vx * Math.PI);

                        X += _Vx * timediff * (0.5f + vdx);

                        Y += _Vy * timediff * (vdx * vdx / 2f + 0.5f);
                        if (Y >= maxy)
                            Y = maxy;
                    }
                    break;

                case EParticleType.Flare:
                    X += _Vx * timediff;
                    Y += _Vy * timediff;
                    break;

                case EParticleType.PerfNoteStar:
                    X += _Vx * timediff;
                    Y += _Vy * timediff;
                    break;
            }


            // update size
            if (Math.Abs(_Vsize) > float.Epsilon)
            {
                float size = _Size;
                switch (_Type)
                {
                    case EParticleType.Twinkle:
                        size = _Size * (1f - currentTime / _Vsize);
                        break;

                    case EParticleType.Star:
                        size = _Size * (1f - currentTime / _Vsize);
                        break;

                    case EParticleType.Snow:
                        size = _Size * (float)Math.Sqrt((Math.Sin(currentTime / _Vsize * Math.PI * 2 - 0.5 * Math.PI) + 1) / 2);
                        break;

                    case EParticleType.Flare:
                        size = _Size * (1f - currentTime / _Vsize);
                        break;

                    case EParticleType.PerfNoteStar:
                        size = _Size * (1f - currentTime / _Vsize);
                        break;
                }

                _Rect.X += (_Rect.W - size) / 2f;
                _Rect.Y += (_Rect.H - size) / 2f;
                _Rect.W = size;
                _Rect.H = size;
            }

            // update rotation
            if (Math.Abs(_Vr) > 0.01)
            {
                float r = currentTime * _Vr / 60f;
                _Angle = _Rotation + 360f * (r - (float)Math.Floor(r));
                _Rect.Rotation = _Angle;
            }

            _LastTime = currentTime;
        }

        public void Pause()
        {
            _Timer.Stop();
        }

        public void Resume()
        {
            _Timer.Start();
        }

        public void Draw()
        {
            // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
            if (!String.IsNullOrEmpty(_TextureName))
                // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                CBase.Drawing.DrawTexture(CBase.Themes.GetSkinTexture(_TextureName, _PartyModeID), _Rect, new SColorF(_Color.R, _Color.G, _Color.B, _Color.A * Alpha2 * _Alpha));
            else
                CBase.Drawing.DrawTexture(_Texture, _Rect, new SColorF(_Color.R, _Color.G, _Color.B, _Color.A * Alpha2 * _Alpha));
        }
    }

    public class CParticleEffect : IMenuElement
    {
        private readonly int _PartyModeID;
        private SThemeParticleEffect _Theme;
        private bool _ThemeLoaded;

        public CTextureRef Texture;
        public SColorF Color;
        public SRectF Rect;

        public bool Selected;
        public bool Visible;

        private readonly List<CParticle> _Stars;
        private readonly Stopwatch _SpawnTimer;
        private float _NextSpawnTime;

        public float Alpha = 1f;

        public string GetThemeName()
        {
            return _Theme.Name;
        }

        public bool ThemeLoaded
        {
            get { return _ThemeLoaded; }
        }

        public bool IsAlive
        {
            get { return _Stars.Count > 0 || !_SpawnTimer.IsRunning; }
        }

        public CParticleEffect(int partyModeID)
        {
            _PartyModeID = partyModeID;
            _Theme = new SThemeParticleEffect();
            _Stars = new List<CParticle>();
            _SpawnTimer = new Stopwatch();
            _NextSpawnTime = 0f;
            Visible = true;
        }

        public CParticleEffect(int partyModeID, int maxNumber, SColorF color, SRectF rect, string skin, float size, EParticleType type)
        {
            _PartyModeID = partyModeID;
            _Theme = new SThemeParticleEffect();
            _Stars = new List<CParticle>();
            Rect = rect;
            Color = color;
            _Theme.Skin = skin;
            _Theme.MaxNumber = maxNumber;
            _Theme.Size = size;
            _Theme.Type = type;
            _SpawnTimer = new Stopwatch();
            _NextSpawnTime = 0f;
            Visible = true;
        }

        public CParticleEffect(int partyModeID, int maxNumber, SColorF color, SRectF rect, CTextureRef texture, float size, EParticleType type)
        {
            _PartyModeID = partyModeID;
            _Theme = new SThemeParticleEffect();
            _Stars = new List<CParticle>();
            Rect = rect;
            Color = color;
            _Theme.Skin = String.Empty;
            Texture = texture;
            _Theme.MaxNumber = maxNumber;
            _Theme.Size = size;
            _Theme.Type = type;
            _SpawnTimer = new Stopwatch();
            _NextSpawnTime = 0f;
            Visible = true;
        }

        public CParticleEffect(SThemeParticleEffect theme, int partyModeID)
        {
            _PartyModeID = partyModeID;
            _Theme = theme;
            _Stars = new List<CParticle>();
            _SpawnTimer = new Stopwatch();
            _NextSpawnTime = 0f;
            Visible = true;

            LoadTextures();
        }

        public bool LoadTheme(string xmlPath, string elementName, CXMLReader xmlReader)
        {
            string item = xmlPath + "/" + elementName;
            _ThemeLoaded = true;

            _ThemeLoaded &= xmlReader.GetValue(item + "/Skin", out _Theme.Skin, String.Empty);

            _ThemeLoaded &= xmlReader.TryGetFloatValue(item + "/X", ref Rect.X);
            _ThemeLoaded &= xmlReader.TryGetFloatValue(item + "/Y", ref Rect.Y);
            _ThemeLoaded &= xmlReader.TryGetFloatValue(item + "/Z", ref Rect.Z);
            _ThemeLoaded &= xmlReader.TryGetFloatValue(item + "/W", ref Rect.W);
            _ThemeLoaded &= xmlReader.TryGetFloatValue(item + "/H", ref Rect.H);

            if (xmlReader.GetValue(item + "/Color", out _Theme.Color.Name, String.Empty))
                _ThemeLoaded &= _Theme.Color.Get(_PartyModeID, out Color);
            else
            {
                _ThemeLoaded &= xmlReader.TryGetFloatValue(item + "/R", ref Color.R);
                _ThemeLoaded &= xmlReader.TryGetFloatValue(item + "/G", ref Color.G);
                _ThemeLoaded &= xmlReader.TryGetFloatValue(item + "/B", ref Color.B);
                _ThemeLoaded &= xmlReader.TryGetFloatValue(item + "/A", ref Color.A);
            }

            _ThemeLoaded &= xmlReader.TryGetEnumValue(item + "/Type", ref _Theme.Type);
            _ThemeLoaded &= xmlReader.TryGetFloatValue(item + "/Size", ref _Theme.Size);
            _ThemeLoaded &= xmlReader.TryGetIntValue(item + "/MaxNumber", ref _Theme.MaxNumber);

            if (_ThemeLoaded)
            {
                _Theme.Name = elementName;
                _Theme.Rect = new SRectF(Rect);
                _Theme.Color.Color = new SColorF(Color);
                LoadTextures();
            }
            return _ThemeLoaded;
        }

        public void Update()
        {
            bool doSpawn = false;
            if (!_SpawnTimer.IsRunning)
            {
                _SpawnTimer.Start();
                _NextSpawnTime = 0f;
                doSpawn = true;
            }

            if (_SpawnTimer.ElapsedMilliseconds / 1000f > _NextSpawnTime && _NextSpawnTime >= 0f)
            {
                doSpawn = true;
                _SpawnTimer.Reset();
                _SpawnTimer.Start();
            }

            while (_Stars.Count < _Theme.MaxNumber && doSpawn)
            {
                float size = CBase.Game.GetRandom((int)_Theme.Size / 2) + _Theme.Size / 2;
                float lifetime = 0f;
                float vx = 0f;
                float vy = 0f;
                float vr = 0f;
                float vsize = 0f;
                _NextSpawnTime = 0f;

                switch (_Theme.Type)
                {
                    case EParticleType.Twinkle:
                        size = CBase.Game.GetRandom((int)_Theme.Size / 2) + _Theme.Size / 2;
                        lifetime = CBase.Game.GetRandom(500) / 1000f + 0.5f;
                        vx = -CBase.Game.GetRandom(10000) / 50f + 100f;
                        vy = -CBase.Game.GetRandom(10000) / 50f + 100f;
                        vr = -CBase.Game.GetRandom(500) / 100f + 2.5f;
                        vsize = lifetime * 2f;
                        break;

                    case EParticleType.Star:
                        size = CBase.Game.GetRandom((int)_Theme.Size / 2) + _Theme.Size / 2;
                        lifetime = CBase.Game.GetRandom(1000) / 500f + 0.2f;
                        vx = -CBase.Game.GetRandom(1000) / 50f + 10f;
                        vy = -CBase.Game.GetRandom(1000) / 50f + 10f;
                        vr = -CBase.Game.GetRandom(500) / 100f + 2.5f;
                        vsize = lifetime * 2f;
                        break;

                    case EParticleType.Snow:
                        size = CBase.Game.GetRandom((int)_Theme.Size / 2) + _Theme.Size / 2;
                        lifetime = CBase.Game.GetRandom(5000) / 50f + 10f;
                        vx = -CBase.Game.GetRandom(1000) / 50f + 10f;
                        vy = CBase.Game.GetRandom(1000) / 50f + Math.Abs(vx) + 10f;
                        vr = -CBase.Game.GetRandom(200) / 50f + 2f;
                        vsize = lifetime * 2f;

                        _NextSpawnTime = lifetime / _Theme.MaxNumber;
                        doSpawn = false;
                        break;

                    case EParticleType.Flare:
                        size = CBase.Game.GetRandom((int)_Theme.Size / 2) + _Theme.Size / 2;
                        lifetime = CBase.Game.GetRandom(500) / 1000f + 0.1f;
                        vx = -CBase.Game.GetRandom(2000) / 50f;
                        vy = -CBase.Game.GetRandom(2000) / 50f + 20f;
                        vr = -CBase.Game.GetRandom(2000) / 50f + 20f;
                        vsize = lifetime * 2f;
                        break;

                    case EParticleType.PerfNoteStar:
                        size = CBase.Game.GetRandom((int)_Theme.Size / 2) + _Theme.Size / 2;
                        lifetime = CBase.Game.GetRandom(1000) / 500f + 1.2f;
                        vx = 0f;
                        vy = 0f;
                        vr = CBase.Game.GetRandom(500) / 50f + 10f;
                        vsize = lifetime * 2f;
                        break;
                }

                var w = (int)(Rect.W - size / 4f);
                var h = (int)(Rect.H - size / 4f);

                if (w < 0)
                    w = 0;

                if (h < 0)
                    h = 0;

                CParticle star;
                if (!String.IsNullOrEmpty(_Theme.Skin))
                {
                    star = new CParticle(_PartyModeID, _Theme.Skin, Color,
                                         CBase.Game.GetRandom(w) + Rect.X - size / 4f,
                                         CBase.Game.GetRandom(h) + Rect.Y - size / 4f,
                                         size, lifetime, Rect.Z, vx, vy, vr, vsize, _Theme.Type);
                }
                else
                {
                    star = new CParticle(_PartyModeID, Texture, Color,
                                         CBase.Game.GetRandom(w) + Rect.X - size / 4f,
                                         CBase.Game.GetRandom(h) + Rect.Y - size / 4f,
                                         size, lifetime, Rect.Z, vx, vy, vr, vsize, _Theme.Type);
                }

                _Stars.Add(star);
            }

            if (_Theme.Type == EParticleType.Flare || _Theme.Type == EParticleType.PerfNoteStar || _Theme.Type == EParticleType.Twinkle)
                _NextSpawnTime = -1f;

            int i = 0;
            while (i < _Stars.Count)
            {
                _Stars[i].Update();
                if (!_Stars[i].IsAlive)
                    _Stars.RemoveAt(i);
                else
                    i++;
            }
        }

        public void Pause()
        {
            foreach (CParticle star in _Stars)
                star.Pause();
        }

        public void Resume()
        {
            foreach (CParticle star in _Stars)
                star.Resume();
        }

        public void Draw()
        {
            Update();
            foreach (CParticle star in _Stars)
            {
                star.Alpha2 = Alpha;
                star.Draw();
            }
        }

        public void UnloadTextures()
        {
            Texture = null;
        }

        public void LoadTextures()
        {
            _Theme.Color.Get(_PartyModeID, out Color);

            if (!String.IsNullOrEmpty(_Theme.Skin))
                Texture = CBase.Themes.GetSkinTexture(_Theme.Skin, _PartyModeID);

            Rect = _Theme.Rect;
        }

        public void ReloadTextures()
        {
            UnloadTextures();
            LoadTextures();
        }

        public SThemeParticleEffect GetTheme()
        {
            return _Theme;
        }

        #region ThemeEdit
        public void MoveElement(int stepX, int stepY)
        {
            Rect.X += stepX;
            Rect.Y += stepY;

            _Theme.Rect.X += stepX;
            _Theme.Rect.Y += stepY;
        }

        public void ResizeElement(int stepW, int stepH)
        {
            Rect.W += stepW;
            if (Rect.W <= 0)
                Rect.W = 1;

            Rect.H += stepH;
            if (Rect.H <= 0)
                Rect.H = 1;

            _Theme.Rect.W = Rect.W;
            _Theme.Rect.H = Rect.H;
        }
        #endregion ThemeEdit
    }
}