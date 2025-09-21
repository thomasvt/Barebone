using FMOD;

namespace Barebone.FMod
{
    public class SoundSystem : IDisposable
    {
        private FMOD.System _system;

        public SoundSystem()
        {
            var result = Factory.System_Create(out _system);
            if (result != RESULT.OK) throw new SoundSystemException($"Failed to create FMOD system with error {result}.");
            result = _system.init(512, INITFLAGS.NORMAL, IntPtr.Zero);
            if (result != RESULT.OK) throw new SoundSystemException($"Failed to initialize FMOD system with error {result}.");
        }

        public ISound LoadSound(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"FModSound file not found: {filePath}");
            var result = _system.createSound(filePath, MODE.DEFAULT, out var sound) ;
            if (result != RESULT.OK)
                throw new SoundSystemException($"Failed to load sound with error {result}: {filePath}");
            return new Barebone.FMod.Sound(sound);
        }

        /// <summary>
        /// Call this every frame or sounds will stop after about a minute.
        /// </summary>
        public void Update()
        {
            _system.update();
        }

        public void Dispose()
        {
            _system.release();
        }
    }
}
