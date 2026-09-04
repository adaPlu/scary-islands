using UnityEngine;
using UnityEngine.Audio;
using ScaryIslands.VR;

namespace ScaryIslands.UI
{
    /// <summary>
    /// Persistent VR settings model + runtime bridge. Designed for world-space UI buttons,
    /// sliders and toggles without requiring a specific UI package.
    /// </summary>
    public sealed class SettingsMenu : MonoBehaviour
    {
        private const string Prefix = "scaryislands.settings.";

        [Header("Optional runtime references")]
        [SerializeField] private ArmSwingLocomotion locomotion;
        [SerializeField] private PlayerWings wings;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private GameObject menuRoot;

        public float MasterVolume { get; private set; } = 1f;
        public float MusicVolume { get; private set; } = 0.8f;
        public float SfxVolume { get; private set; } = 1f;
        public float TurnDegrees { get; private set; } = 30f;
        public float ComfortVignette { get; private set; } = 0.35f;
        public float LocomotionStrength { get; private set; } = 1f;
        public float FlightStrength { get; private set; } = 1f;
        public float Brightness { get; private set; } = 1f;
        public bool SnapTurn { get; private set; } = true;
        public bool ComfortVignetteEnabled { get; private set; } = true;
        public bool FlightEnabled { get; private set; } = true;
        public bool LeftHanded { get; private set; }
        public bool Subtitles { get; private set; } = true;
        public bool HighContrast { get; private set; }
        public bool ReducedMotion { get; private set; }

        private void Awake()
        {
            Load();
            ApplyAll();
            if (menuRoot != null)
                menuRoot.SetActive(false);
        }

        public void Open()
        {
            if (menuRoot != null) menuRoot.SetActive(true);
        }

        public void Close()
        {
            if (menuRoot != null) menuRoot.SetActive(false);
            Save();
        }

        public void ToggleMenu()
        {
            if (menuRoot == null) return;
            menuRoot.SetActive(!menuRoot.activeSelf);
            if (!menuRoot.activeSelf) Save();
        }

        public void SetMasterVolume(float value) { MasterVolume = Mathf.Clamp01(value); ApplyAudio(); }
        public void SetMusicVolume(float value) { MusicVolume = Mathf.Clamp01(value); ApplyAudio(); }
        public void SetSfxVolume(float value) { SfxVolume = Mathf.Clamp01(value); ApplyAudio(); }
        public void SetTurnDegrees(float value) { TurnDegrees = Mathf.Clamp(value, 15f, 90f); }
        public void SetComfortVignette(float value) { ComfortVignette = Mathf.Clamp01(value); }
        public void SetLocomotionStrength(float value) { LocomotionStrength = Mathf.Clamp(value, 0.25f, 2f); ApplyMovement(); }
        public void SetFlightStrength(float value) { FlightStrength = Mathf.Clamp(value, 0.25f, 2f); ApplyMovement(); }
        public void SetBrightness(float value) { Brightness = Mathf.Clamp(value, 0.6f, 1.5f); ApplyVisuals(); }
        public void SetSnapTurn(bool value) { SnapTurn = value; }
        public void SetComfortVignetteEnabled(bool value) { ComfortVignetteEnabled = value; }
        public void SetFlightEnabled(bool value) { FlightEnabled = value; ApplyMovement(); }
        public void SetLeftHanded(bool value) { LeftHanded = value; }
        public void SetSubtitles(bool value) { Subtitles = value; }
        public void SetHighContrast(bool value) { HighContrast = value; }
        public void SetReducedMotion(bool value) { ReducedMotion = value; }

        public void ResetDefaults()
        {
            MasterVolume = 1f;
            MusicVolume = 0.8f;
            SfxVolume = 1f;
            TurnDegrees = 30f;
            ComfortVignette = 0.35f;
            LocomotionStrength = 1f;
            FlightStrength = 1f;
            Brightness = 1f;
            SnapTurn = true;
            ComfortVignetteEnabled = true;
            FlightEnabled = true;
            LeftHanded = false;
            Subtitles = true;
            HighContrast = false;
            ReducedMotion = false;
            ApplyAll();
            Save();
        }

        public void Save()
        {
            PlayerPrefs.SetFloat(Prefix + "master", MasterVolume);
            PlayerPrefs.SetFloat(Prefix + "music", MusicVolume);
            PlayerPrefs.SetFloat(Prefix + "sfx", SfxVolume);
            PlayerPrefs.SetFloat(Prefix + "turnDegrees", TurnDegrees);
            PlayerPrefs.SetFloat(Prefix + "vignette", ComfortVignette);
            PlayerPrefs.SetFloat(Prefix + "locomotion", LocomotionStrength);
            PlayerPrefs.SetFloat(Prefix + "flight", FlightStrength);
            PlayerPrefs.SetFloat(Prefix + "brightness", Brightness);
            PlayerPrefs.SetInt(Prefix + "snapTurn", SnapTurn ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "vignetteEnabled", ComfortVignetteEnabled ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "flightEnabled", FlightEnabled ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "leftHanded", LeftHanded ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "subtitles", Subtitles ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "highContrast", HighContrast ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "reducedMotion", ReducedMotion ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            MasterVolume = PlayerPrefs.GetFloat(Prefix + "master", 1f);
            MusicVolume = PlayerPrefs.GetFloat(Prefix + "music", 0.8f);
            SfxVolume = PlayerPrefs.GetFloat(Prefix + "sfx", 1f);
            TurnDegrees = PlayerPrefs.GetFloat(Prefix + "turnDegrees", 30f);
            ComfortVignette = PlayerPrefs.GetFloat(Prefix + "vignette", 0.35f);
            LocomotionStrength = PlayerPrefs.GetFloat(Prefix + "locomotion", 1f);
            FlightStrength = PlayerPrefs.GetFloat(Prefix + "flight", 1f);
            Brightness = PlayerPrefs.GetFloat(Prefix + "brightness", 1f);
            SnapTurn = PlayerPrefs.GetInt(Prefix + "snapTurn", 1) == 1;
            ComfortVignetteEnabled = PlayerPrefs.GetInt(Prefix + "vignetteEnabled", 1) == 1;
            FlightEnabled = PlayerPrefs.GetInt(Prefix + "flightEnabled", 1) == 1;
            LeftHanded = PlayerPrefs.GetInt(Prefix + "leftHanded", 0) == 1;
            Subtitles = PlayerPrefs.GetInt(Prefix + "subtitles", 1) == 1;
            HighContrast = PlayerPrefs.GetInt(Prefix + "highContrast", 0) == 1;
            ReducedMotion = PlayerPrefs.GetInt(Prefix + "reducedMotion", 0) == 1;
        }

        private void ApplyAll()
        {
            ApplyAudio();
            ApplyMovement();
            ApplyVisuals();
        }

        private void ApplyAudio()
        {
            AudioListener.volume = MasterVolume;
            if (audioMixer == null) return;
            audioMixer.SetFloat("MusicVolume", LinearToDb(MusicVolume));
            audioMixer.SetFloat("SfxVolume", LinearToDb(SfxVolume));
        }

        private void ApplyMovement()
        {
            if (locomotion != null)
                locomotion.SetSettingsSpeedMultiplier(LocomotionStrength);
            if (wings != null)
            {
                wings.SetFlightEnabled(FlightEnabled);
                wings.SetSettingsFlightMultiplier(FlightStrength);
            }
        }

        private void ApplyVisuals()
        {
            RenderSettings.ambientIntensity = Brightness;
        }

        private static float LinearToDb(float linear)
        {
            return Mathf.Log10(Mathf.Max(0.0001f, linear)) * 20f;
        }
    }
}
