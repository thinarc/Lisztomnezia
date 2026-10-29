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
            OnOsu = 7,
            SlowPressure = 8,
            ChaosEvent = 9,
            RabbitCatch = 10
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

        static float _worldRecovery = 1f;
        static bool _disable;
        static bool _running;
        static bool _sketchActive;
        static int _scenarioVersion;

        static Color baseColor = new Color32(8,9,12,255);
        static Color _currentColor = baseColor;
        static Color warmColor = new Color32(43, 40, 37, 255);
        static Color coldColor = new Color32(36, 40, 45, 255);
        static Color horrorColor = new Color32(100, 22, 25, 255);
        static Color dreamColor = new Color32(41, 38, 45, 255);

        public static void Initialize(float strength)
        {
            _baseStrength = strength;
            _currentStrength = strength;

            SSHStyleManager.API.Outline.SetColorOutlineStrength(ToOutlineThreshold(strength));
            SSHStyleManager.API.Outline.SetColorOutlineColor(baseColor);

            SetScenario(OutlineScenario.Idle);
            _currentColor = baseColor;
            _colorMode = ColorMode.Base;
            _colorBreath = ColorBreath.None;

            _disable = false;

            if (!_running) Run().Forget();
        }

        static async UniTaskVoid Run()
        {
            _running = true;
            try
            {
                while(!_disable)
                {
                    HandleDevKeys();

                    switch(_scenario)
                    {
                        case OutlineScenario.RabbitCatch: await RabbitCatch(); break;
                        case OutlineScenario.Idle: await Idle(); break;
                        case OutlineScenario.CalmBreath: await CalmBreath(); break;
                        case OutlineScenario.AmbientNoise: await AmbientNoise(); break;
                        case OutlineScenario.BlinkEvent: await BlinkEvent(); break;
                        case OutlineScenario.StressPulse: await StressPulse(); break;
                        case OutlineScenario.HorrorFlash: await HorrorFlash(); break;
                        case OutlineScenario.OnOsu: await OnOsu(); break;
                        case OutlineScenario.SlowPressure: await SlowPressure(); break;
                        case OutlineScenario.ChaosEvent: await ChaosEvent(); break;
                    }
                }
            }
            finally { _running = false; }
        }

        // The shader uses Strength as a Step threshold: higher values cover less.
        // Keep the old scenario rhythms, but confine broad flashes to a subtler range.
        static float ToOutlineThreshold(float strength)
        {
            return strength < 1f
                ? Mathf.Lerp(Mathf.Lerp(0.08f, 0.18f, _worldRecovery), 0.65f, Mathf.InverseLerp(-0.1f, 1f, strength))
                : Mathf.Lerp(0.65f, 3f, Mathf.InverseLerp(1f, 3f, strength));

        }

        static void SetOutline(float strength)
        {
            SSHStyleManager.API.Outline.SetColorOutlineStrength(ToOutlineThreshold(strength));
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
                float power = (_colorBreath == ColorBreath.Soft) ? 0.004f :
                              (_colorBreath == ColorBreath.Medium) ? 0.008f : 0.012f;

                float t = Mathf.Sin(time * 0.65f) * power;

                c += new Color(t,t,t);
            }

            float drift = Mathf.PerlinNoise(time * 0.1f, 42f) * 0.004f;
            c.r += drift;
            c.g -= drift * 0.5f;

            float noise = (Mathf.PerlinNoise(time * 6f, 99f) - 0.5f) * 0.004f;
            c += new Color(noise,noise,noise);

            _currentColor = Color.Lerp(_currentColor, c, 1f - Mathf.Exp(-Time.deltaTime * 8f));
            return _currentColor;
        }

        static async UniTask Idle()
        {
            if (!_sketchActive)
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
            var version = _scenarioVersion;
            SetOutline(Random.Range(0.02f,0.1f));
            await UniTask.Delay(Random.Range(40,90));
            if (_disable || version != _scenarioVersion) return;

            SetOutline(Random.Range(0.4f,0.7f));
            await UniTask.Delay(Random.Range(80,140));
            if (_disable || version != _scenarioVersion) return;

            _scenario = OutlineScenario.CalmBreath;
        }

        static async UniTask StressPulse()
        {
            var version = _scenarioVersion;
            int count = Random.Range(3,7);

            for(int i=0;i<count;i++)
            {
                SetOutline(Random.Range(-0.02f,0.15f));
                await UniTask.Delay(Random.Range(40,120));
                if (_disable || version != _scenarioVersion) return;

                SetOutline(Random.Range(0.4f,0.7f));
                await UniTask.Delay(Random.Range(80,180));
                if (_disable || version != _scenarioVersion) return;

                if(Random.value < 0.25f)
                {
                    SetOutline(Random.Range(0.2f,0.5f));
                    await UniTask.Delay(Random.Range(30,90));
                    if (_disable || version != _scenarioVersion) return;
                }
            }

            _scenario = OutlineScenario.AmbientNoise;
        }

        static async UniTask HorrorFlash()
        {
            var version = _scenarioVersion;
            int flashes = Random.Range(1,3);

            for(int i=0;i<flashes;i++)
            {
                SetOutline(Random.Range(-0.08f,0.02f));
                await UniTask.Delay(Random.Range(60,140));
                if (_disable || version != _scenarioVersion) return;

                SetOutline(Random.Range(0.05f,0.2f));
                await UniTask.Delay(Random.Range(60,120));
                if (_disable || version != _scenarioVersion) return;
            }

            _scenario = (Random.value < 0.3f)
                ? OutlineScenario.ChaosEvent
                : OutlineScenario.StressPulse;
        }

        public static void CatchRabbit()
        {
            SetScenario(OutlineScenario.RabbitCatch);
            SetColor(ColorMode.Horror);
            SetBreath(ColorBreath.None);
            _currentColor = horrorColor;
            // Apply immediately at the tear, even if a previous scenario is awaiting.
            SetOutline(0.05f);
        }

        static async UniTask RabbitCatch()
        {
            var version = _scenarioVersion;
            const float duration = 0.65f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (_disable || version != _scenarioVersion) return;
                var fade = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                _currentColor = Color.Lerp(horrorColor, baseColor, fade);
                SSHStyleManager.API.Outline.SetColorOutlineColor(_currentColor);
                _currentStrength = Mathf.Lerp(0.05f, _baseStrength, fade);
                SSHStyleManager.API.Outline.SetColorOutlineStrength(ToOutlineThreshold(_currentStrength));
                await UniTask.NextFrame();
                elapsed += Time.deltaTime;
            }
            if (_disable || version != _scenarioVersion) return;
            SetColor(ColorMode.Base);
            SetScenario(OutlineScenario.CalmBreath);
        }

        static async UniTask OnOsu()
        {
            while (true)
            {
                _currentStrength = Mathf.MoveTowards(_currentStrength, 3f, Time.deltaTime * 2);
                SetOutline(_currentStrength);
                await UniTask.Yield();
                if (Mathf.Approximately(_currentStrength, 3f) || _scenario != OutlineScenario.OnOsu) break;
            }
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
            var version = _scenarioVersion;
            int count = Random.Range(5,10);

            for(int i=0;i<count;i++)
            {
                float value = Random.Range(-0.05f,0.7f);

                if(Random.value < 0.15f)
                    value = Random.Range(-0.1f,0.9f);

                SetOutline(value);
                await UniTask.Delay(Random.Range(30,150));
                if (_disable || version != _scenarioVersion) return;
            }

            float r = Random.value;

            _scenario = (r < 0.33f) ? OutlineScenario.Idle :
                        (r < 0.66f) ? OutlineScenario.CalmBreath :
                                      OutlineScenario.AmbientNoise;
        }

        static void HandleDevKeys()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(Input.GetKeyDown(KeyCode.Alpha1)) SetScenario(OutlineScenario.Idle);
            if(Input.GetKeyDown(KeyCode.Alpha2)) SetScenario(OutlineScenario.CalmBreath);
            if(Input.GetKeyDown(KeyCode.Alpha3)) SetScenario(OutlineScenario.AmbientNoise);
            if(Input.GetKeyDown(KeyCode.Alpha4)) SetScenario(OutlineScenario.BlinkEvent);
            if(Input.GetKeyDown(KeyCode.Alpha5)) SetScenario(OutlineScenario.StressPulse);
            if(Input.GetKeyDown(KeyCode.Alpha6)) SetScenario(OutlineScenario.HorrorFlash);
            if(Input.GetKeyDown(KeyCode.Alpha7)) SetScenario(OutlineScenario.OnOsu);
            if(Input.GetKeyDown(KeyCode.Alpha8)) SetScenario(OutlineScenario.SlowPressure);
            if(Input.GetKeyDown(KeyCode.Alpha9)) SetScenario(OutlineScenario.ChaosEvent);
#endif
        }

        public static void SetScenario(OutlineScenario scenario)
        {
            _sketchActive = false;
            _scenarioVersion++;
            _scenario = scenario;
        }

        public static void SetSketchProgress(float progress)
        {
            SetColor(ColorMode.Cold);
            SetBreath(progress > 0.5f ? ColorBreath.Medium : ColorBreath.Soft);
            // Keep the response continuous instead of starting random flash sequences.
            SetScenario(OutlineScenario.Idle);
            _sketchActive = true;
            var target = Mathf.Lerp(0.55f, 0.15f, Mathf.SmoothStep(0f, 1f, progress));
            _currentStrength = Mathf.Lerp(_currentStrength, target, 1f - Mathf.Exp(-Time.deltaTime * 10f));
            SetOutline(_currentStrength);
        }
        public static void SetWorldRecovery(float value) => _worldRecovery = Mathf.Clamp01(value);

        public static void SetColor(ColorMode mode) => _colorMode = mode;
        public static void SetBreath(ColorBreath breath) => _colorBreath = breath;

        public static void OnDisable()
        {
            _disable = true;
            _scenarioVersion++;
            _sketchActive = false;

            SSHStyleManager.API.Outline.SetColorOutlineStrength(ToOutlineThreshold(_baseStrength));
            SSHStyleManager.API.Outline.SetColorOutlineColor(baseColor);
        }
    }
}
