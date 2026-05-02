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
            
            _scenario = OutlineScenario.Idle;
            _colorMode = ColorMode.Base; 
            _colorBreath = ColorBreath.None;

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
                    case OutlineScenario.Idle: await Idle(); break;
                    case OutlineScenario.CalmBreath: await CalmBreath(); break;
                    case OutlineScenario.AmbientNoise: await AmbientNoise(); break;
                    case OutlineScenario.BlinkEvent: await BlinkEvent(); break;
                    case OutlineScenario.StressPulse: await StressPulse(); break;
                    case OutlineScenario.HorrorFlash: await HorrorFlash(); break;
                    case OutlineScenario.ScreenFlood: await ScreenFlood(); break;
                    case OutlineScenario.SlowPressure: await SlowPressure(); break;
                    case OutlineScenario.ChaosEvent: await ChaosEvent(); break;
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
                case ColorMode.Warm: c = warmColor; break;
                case ColorMode.Cold: c = coldColor; break;
                case ColorMode.Horror: c = horrorColor; break;
                case ColorMode.Dream: c = dreamColor; break;
            }

            float time = Time.time;

            if(_colorBreath != ColorBreath.None)
            {
                float power = (_colorBreath == ColorBreath.Soft) ? 0.02f :
                              (_colorBreath == ColorBreath.Medium) ? 0.05f : 0.08f;

                float t = Mathf.Sin(time * Random.Range(0.4f,0.9f)) * power;

                c += new Color(t,t,t);
            }

            float drift = Mathf.PerlinNoise(time * 0.1f, 42f) * 0.03f;
            c.r += drift;
            c.g -= drift * 0.5f;

            float noise = (Mathf.PerlinNoise(time * 6f, 99f) - 0.5f) * 0.02f;
            c += new Color(noise,noise,noise);

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
            float freq = 0.4f + Mathf.PerlinNoise(Time.time*0.2f,0)*0.4f;
            float amp = 0.02f + Mathf.PerlinNoise(0,Time.time*0.3f)*0.03f;

            float t = Mathf.Sin(Time.time * freq);
            float target = _baseStrength + t * amp;

            if(Random.value < 0.01f)
                target += Random.Range(-0.03f,0.03f);

            _currentStrength = Mathf.Lerp(_currentStrength,target,Time.deltaTime*2);

            SetOutline(_currentStrength);
            await UniTask.NextFrame();
        }

        static async UniTask AmbientNoise()
        {
            float t = Time.time;

            float n = Mathf.PerlinNoise(t*0.7f,0)*0.6f +
                      Mathf.PerlinNoise(t*2.1f,10)*0.3f +
                      Mathf.PerlinNoise(t*5.3f,25)*0.1f;

            float target = Mathf.Lerp(0.1f,0.55f,n);

            if(Random.value < 0.02f)
                target = Random.Range(-0.02f,0.6f);

            _currentStrength = Mathf.Lerp(_currentStrength,target,Time.deltaTime*Random.Range(2f,4f));

            SetOutline(_currentStrength);
            await UniTask.NextFrame();
        }

        static async UniTask BlinkEvent()
        {
            SetOutline(Random.Range(0.02f,0.1f));
            await UniTask.Delay(Random.Range(40,90));

            SetOutline(Random.Range(0.4f,0.7f));
            await UniTask.Delay(Random.Range(80,140));

            _scenario = OutlineScenario.CalmBreath;
        }

        static async UniTask StressPulse()
        {
            int count = Random.Range(3,7);

            for(int i=0;i<count;i++)
            {
                SetOutline(Random.Range(-0.02f,0.15f));
                await UniTask.Delay(Random.Range(40,120));

                SetOutline(Random.Range(0.4f,0.7f));
                await UniTask.Delay(Random.Range(80,180));

                if(Random.value < 0.25f)
                {
                    SetOutline(Random.Range(0.2f,0.5f));
                    await UniTask.Delay(Random.Range(30,90));
                }
            }

            _scenario = OutlineScenario.AmbientNoise;
        }

        static async UniTask HorrorFlash()
        {
            int flashes = Random.Range(1,3);

            for(int i=0;i<flashes;i++)
            {
                SetOutline(Random.Range(-0.08f,0.02f));
                await UniTask.Delay(Random.Range(60,140));

                SetOutline(Random.Range(0.05f,0.2f));
                await UniTask.Delay(Random.Range(60,120));
            }

            _scenario = (Random.value < 0.3f)
                ? OutlineScenario.ChaosEvent
                : OutlineScenario.StressPulse;
        }

        static async UniTask ScreenFlood()
        {
            SetOutline(Random.Range(-0.08f,-0.02f));
            await UniTask.Delay(Random.Range(150,300));

            _scenario = OutlineScenario.CalmBreath;
        }

        static async UniTask SlowPressure()
        {
            float t = Time.time * 0.05f;

            float wave = Mathf.PingPong(t,1);
            float noise = Mathf.PerlinNoise(t*2f,5f)*0.2f;

            float target = Mathf.Lerp(0.6f,0.02f,wave) + noise;

            if(Random.value < 0.005f)
                target = -0.05f;

            _currentStrength = Mathf.Lerp(_currentStrength,target,Time.deltaTime*Random.Range(0.5f,1.5f));

            SetOutline(_currentStrength);
            await UniTask.NextFrame();
        }

        static async UniTask ChaosEvent()
        {
            int count = Random.Range(5,10);

            for(int i=0;i<count;i++)
            {
                float value = Random.Range(-0.05f,0.7f);

                if(Random.value < 0.15f)
                    value = Random.Range(-0.1f,0.9f);

                SetOutline(value);
                await UniTask.Delay(Random.Range(30,150));
            }

            float r = Random.value;

            _scenario = (r < 0.33f) ? OutlineScenario.Idle :
                        (r < 0.66f) ? OutlineScenario.CalmBreath :
                                      OutlineScenario.AmbientNoise;
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
#endif
        }

        public static void SetScenario(OutlineScenario scenario) => _scenario = scenario;
        public static void SetColor(ColorMode mode) => _colorMode = mode;
        public static void SetBreath(ColorBreath breath) => _colorBreath = breath;

        public static void OnDisable()
        {
            _disable = true;

            SSHStyleManager.API.Outline.SetColorOutlineStrength(_baseStrength);
            SSHStyleManager.API.Outline.SetColorOutlineColor(baseColor);
        }
    }
}