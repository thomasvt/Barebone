using System.Diagnostics.Metrics;
using FMOD;

namespace Barebone.FMod
{
    public interface ISound : IDisposable
    {
        void Play();
    }

    public record Sound : ISound
    {
        private readonly FMOD.Sound _sound;
        private readonly FMOD.System _system;

        public Sound(FMOD.Sound fModSound)
        {
            _sound = fModSound;
            fModSound.getSystemObject(out _system);
        }

        public void Play()
        {
            // Create the DSP
            // _system.createDSPByType(FMOD.DSP_TYPE.ECHO, out var echoDsp);
            // // Configure echo
            // echoDsp.setParameterFloat((int)FMOD.DSP_ECHO.DELAY, 1f);     // ms Echo delay.
            // echoDsp.setParameterFloat((int)FMOD.DSP_ECHO.FEEDBACK, 1f);   // 0–100% Echo decay per delay. 100.0 = No decay, 0.0 = total decay.
            // echoDsp.setParameterFloat((int)FMOD.DSP_ECHO.WETLEVEL, 5f);
            // echoDsp.setParameterFloat((int)FMOD.DSP_ECHO.DRYLEVEL, 5f);

            _system.getMasterChannelGroup(out var masterChannelGroup);
            _system.playSound(_sound, masterChannelGroup, true, out var channel);
            // channel.addDSP(0, echoDsp);
            channel.setPaused(false);
        }

        public void Dispose()
        {
            _sound.release();
        }
    }
}
