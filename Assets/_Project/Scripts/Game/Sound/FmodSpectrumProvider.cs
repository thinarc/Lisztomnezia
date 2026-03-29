using System;
using System.Runtime.InteropServices;
using FMOD;
using FMODUnity;
using UnityEngine;

namespace _Project.Scripts.Sound
{
    public class FmodSpectrumProvider : MonoBehaviour
    {
        public static FmodSpectrumProvider Instance;

        private DSP _fft;
        private DSP_PARAMETER_FFT _fftData;

        public float[] Spectrum { get; private set; }

        private ChannelGroup _channelGroup;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.FFT, out _fft);

            _fft.setParameterInt((int)DSP_FFT.WINDOWSIZE, 1024);
            _fft.setParameterInt((int)DSP_FFT.WINDOW, (int)DSP_FFT_WINDOW_TYPE.HANNING);

            RuntimeManager.CoreSystem.getMasterChannelGroup(out _channelGroup);
            _channelGroup.addDSP(0, _fft);
        }

        void Update()
        {
            IntPtr data;
            uint length;

            _fft.getParameterData((int)DSP_FFT.SPECTRUMDATA, out data, out length);

            if (data == IntPtr.Zero) return;

            _fftData = Marshal.PtrToStructure<DSP_PARAMETER_FFT>(data);

            if (_fftData.spectrum.Length > 0) Spectrum = _fftData.spectrum[0]; // левый канал
        }
    }
}