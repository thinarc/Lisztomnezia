using UnityEngine;
using Cysharp.Threading.Tasks;
using SmnStyleHardline.Core;

namespace _Project.Scripts.Effects
{
    public static class OutlineMover
    {
        public enum OutlineScenario
        {
            Idle = 1,
            CalmBreath = 2,
            AmbientNoise = 3,
            BlinkEvent = 4,
            StressPulse = 5,
            HorrorFlash = 6,
            ScreenFlood = 7,
            SlowPressure = 8,
            ChaosEvent = 9
        }

        public enum ColorMode
        {
            Base,
            Warm,
            Cold,
            Horror,
            Dream
        }

        public enum ColorBreath
        {
            None,
            Soft,
            Medium,
            Strong
        }

        static OutlineScenario _scenario = OutlineScenario.Idle;

        static ColorMode _colorMode = ColorMode.Base;
        static ColorBreath _colorBreath = ColorBreath.None;

        static float _baseStrength;
        static float _currentStrength;

        static bool _disable;

        static Color baseColor = new Color32(8,9,12,255);
        static Color accentColor = new Color(0.9f,0.9f,0.9f);

        static Color warmColor = new Color(0.6f,0.3f,0.2f);
        static Color coldColor = new Color(0.3f,0.45f,0.7f);
        static Color horrorColor = new Color(0.4f,0.05f,0.05f);
        static Color dreamColor = new Color(0.35f,0.2f,0.5f);

        public static void Initialize(float strength)
        {
            _baseStrength = strength;
            _currentStrength = strength;

            SSHStyleManager.API.Outline.SetColorOutlineStrength(strength);
            SSHStyleManager.API.Outline.SetColorOutlineColor(baseColor);

            _disable = false;

            Run().Forget();
        }

        static async UniTaskVoid Run()
        {
            while(!_disable)
            {
                HandleDevKeys();

                switch(_scenario)
                {
                    case OutlineScenario.Idle:
                        await Idle();
                        break;

                    case OutlineScenario.CalmBreath:
                        await CalmBreath();
                        break;

                    case OutlineScenario.AmbientNoise:
                        await AmbientNoise();
                        break;

                    case OutlineScenario.BlinkEvent:
                        await BlinkEvent();
                        break;

                    case OutlineScenario.StressPulse:
                        await StressPulse();
                        break;

                    case OutlineScenario.HorrorFlash:
                        await HorrorFlash();
                        break;

                    case OutlineScenario.ScreenFlood:
                        await ScreenFlood();
                        break;

                    case OutlineScenario.SlowPressure:
                        await SlowPressure();
                        break;

                    case OutlineScenario.ChaosEvent:
                        await ChaosEvent();
                        break;
                }
            }
        }

        static void SetOutline(float strength)
        {
            SSHStyleManager.API.Outline.SetColorOutlineStrength(strength);
            SSHStyleManager.API.Outline.SetColorOutlineColor(ApplyColor());
        }

        static Color ApplyColor()
        {
            Color c = baseColor;

            switch(_colorMode)
            {
                case ColorMode.Base: c = baseColor; break;
                case ColorMode.Warm: c = warmColor; break;
                case ColorMode.Cold: c = coldColor; break;
                case ColorMode.Horror: c = horrorColor; break;
                case ColorMode.Dream: c = dreamColor; break;
            }

            if(_colorBreath != ColorBreath.None)
            {
                float power = 0.02f;

                if(_colorBreath == ColorBreath.Medium) power = 0.05f;
                if(_colorBreath == ColorBreath.Strong) power = 0.08f;

                float t = Mathf.Sin(Time.time * 0.6f) * power;

                c.r += t;
                c.g += t;
                c.b += t;
            }

            return c;
        }

        static async UniTask Idle()
        {
            _currentStrength = Mathf.Lerp(_currentStrength,_baseStrength,Time.deltaTime*2);
            SetOutline(_currentStrength);
            await UniTask.NextFrame();
        }

        static async UniTask CalmBreath()
        {
            float t = Mathf.Sin(Time.time*0.5f);

            float target = _baseStrength + t*0.04f;

            _currentStrength = Mathf.Lerp(_currentStrength,target,Time.deltaTime*2);

            SetOutline(_currentStrength);

            await UniTask.NextFrame();
        }

        static async UniTask AmbientNoise()
        {
            float n = Mathf.PerlinNoise(Time.time*0.7f,0);

            float target = Mathf.Lerp(0.15f,0.5f,n);

            _currentStrength = Mathf.Lerp(_currentStrength,target,Time.deltaTime*3);

            SetOutline(_currentStrength);

            await UniTask.NextFrame();
        }

        // blink → breath
        static async UniTask BlinkEvent()
        {
            SetOutline(0.07f);
            await UniTask.Delay(50);

            SetOutline(0.6f);
            await UniTask.Delay(120);

            _scenario = OutlineScenario.CalmBreath;
        }

        // серия импульсов
        static async UniTask StressPulse()
        {
            for(int i=0;i<4;i++)
            {
                SetOutline(Random.Range(0.04f,0.15f));
                await UniTask.Delay(70);

                SetOutline(0.6f);
                await UniTask.Delay(120);
            }

            _scenario = OutlineScenario.AmbientNoise;
        }

        // flash → агрессия
        static async UniTask HorrorFlash()
        {
            SetOutline(-0.04f);
            await UniTask.Delay(100);

            SetOutline(0.08f);
            await UniTask.Delay(80);

            _scenario = OutlineScenario.StressPulse;
        }

        // flood → дыхание
        static async UniTask ScreenFlood()
        {
            SetOutline(-0.05f);

            await UniTask.Delay(200);

            _scenario = OutlineScenario.CalmBreath;
        }

        // медленное давление
        static async UniTask SlowPressure()
        {
            float target = Mathf.Lerp(0.6f,0.02f,Mathf.PingPong(Time.time*0.05f,1));

            _currentStrength = Mathf.Lerp(_currentStrength,target,Time.deltaTime);

            SetOutline(_currentStrength);

            await UniTask.NextFrame();
        }

        static async UniTask ChaosEvent()
        {
            for(int i=0;i<6;i++)
            {
                SetOutline(Random.Range(-0.03f,0.6f));
                await UniTask.Delay(Random.Range(40,120));
            }

            _scenario = OutlineScenario.Idle;
        }

        static void HandleDevKeys()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD

            if(Input.GetKeyDown(KeyCode.Alpha1)) _scenario = OutlineScenario.Idle;
            if(Input.GetKeyDown(KeyCode.Alpha2)) _scenario = OutlineScenario.CalmBreath;
            if(Input.GetKeyDown(KeyCode.Alpha3)) _scenario = OutlineScenario.AmbientNoise;
            if(Input.GetKeyDown(KeyCode.Alpha4)) _scenario = OutlineScenario.BlinkEvent;
            if(Input.GetKeyDown(KeyCode.Alpha5)) _scenario = OutlineScenario.StressPulse;
            if(Input.GetKeyDown(KeyCode.Alpha6)) _scenario = OutlineScenario.HorrorFlash;
            if(Input.GetKeyDown(KeyCode.Alpha7)) _scenario = OutlineScenario.ScreenFlood;
            if(Input.GetKeyDown(KeyCode.Alpha8)) _scenario = OutlineScenario.SlowPressure;
            if(Input.GetKeyDown(KeyCode.Alpha9)) _scenario = OutlineScenario.ChaosEvent;

            if(Input.GetKeyDown(KeyCode.Y)) _colorMode = ColorMode.Base;
            if(Input.GetKeyDown(KeyCode.U)) _colorMode = ColorMode.Warm;
            if(Input.GetKeyDown(KeyCode.I)) _colorMode = ColorMode.Cold;
            if(Input.GetKeyDown(KeyCode.O)) _colorMode = ColorMode.Horror;
            if(Input.GetKeyDown(KeyCode.P)) _colorMode = ColorMode.Dream;

            if(Input.GetKeyDown(KeyCode.J)) _colorBreath = ColorBreath.Soft;
            if(Input.GetKeyDown(KeyCode.K)) _colorBreath = ColorBreath.Medium;
            if(Input.GetKeyDown(KeyCode.L)) _colorBreath = ColorBreath.Strong;

#endif
        }

        public static void OnDisable()
        {
            _disable = true;

            SSHStyleManager.API.Outline.SetColorOutlineStrength(_baseStrength);
            SSHStyleManager.API.Outline.SetColorOutlineColor(baseColor);
        }
    }
}