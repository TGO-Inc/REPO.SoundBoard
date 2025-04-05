using MenuLib;
using MenuLib.MonoBehaviors;
using UnityEngine;
using UnityEngine.UI;

namespace SoundBoard.Models.UI;

public class SettingsPageSoundItem : ISoundItem
{
    private readonly SoundItemConfig _config;

    private REPOButton? _keyBindButton;

    private SettingsPageSoundItem(SoundItemConfig config)
    {
        _config = config;
    }

    public event Action<KeyCode>? OnKeyBindChanged;
    public event Action<float>? OnVolumeChanged;
    public event Action<bool>? OnBehaviorChanged;

    private void VolumeChanged(float volume)
    {
        _config.Volume = volume;
        Entry.SoundBoard.FileService.UpdateSoundItem(_config.Name, _config);
        OnVolumeChanged?.Invoke(volume);
    }

    private void BehaviorChanged(bool behavior)
    {
        _config.State = behavior;
        Entry.SoundBoard.FileService.UpdateSoundItem(_config.Name, _config);
        OnBehaviorChanged?.Invoke(behavior);
    }

    public IEnumerable<(REPOPopupPage.ScrollViewBuilderDelegate, int paddingTop, int paddingBottom)> Init()
    {
        yield return (parent =>
        {
            var label = MenuAPI.CreateREPOLabel(_config.Name, parent.transform, new Vector2(-3, 0));
            label.labelTMP.fontSize = 15;
            label.labelTMP.color = Color.white;
            label.labelTMP.margin = Vector4.zero;
            return label.rectTransform;
        }, 0, 0);

        yield return (parent =>
        {
            var volumeSlider = MenuAPI.CreateREPOSlider("Volume", "", VolumeChanged,
                parent.transform, new Vector2(35, 0), 0f, 200f, 1,
                _config.Volume, barBehavior: REPOSlider.BarBehavior.UpdateWithValue);

            volumeSlider.labelTMP.fontSize = 12;
            volumeSlider.transform.localScale = new Vector2(0.85f, 0.8f);
            return volumeSlider.rectTransform;
        }, -10, 0);

        yield return (parent =>
        {
            var button = MenuAPI.CreateREPOToggle("Behavior", BehaviorChanged, parent.transform, new Vector2(35, 0),
                "Play", "Toggle", _config.State);
            button.labelTMP.transform.localScale = new Vector2(0.8f, 0.8f);
            button.labelTMP.transform.localPosition -= new Vector3(23, 0, 0);
            button.transform.localScale = new Vector2(0.85f, 0.8f);
            return button.rectTransform;
        }, -6, 0);

        yield return (parent =>
        {
            _keyBindButton = MenuAPI.CreateREPOButton("", () =>
            {
                Entry.SoundBoard.GetNewKeyBind(k => KeyBindChanged(k));
                _keyBindButton!.labelTMP.text = "_";
                _keyBindButton.labelTMP.fontSize = 28;
            }, parent.transform, new Vector2(-5, 0));

            _keyBindButton.labelTMP.fontSize = 28;
            _keyBindButton.overrideButtonSize = new Vector2(34.5f, 40);

            KeyBindChanged(_config.Key, false);

            {
                const float borderThickness = 1.25f;
                var borderColor = new Color(12f / 256, 76f / 256, 200f / 256, 1f);
                var padding = new Vector2(0f, -5f);

                // Get the RectTransform of the text
                var textRect = _keyBindButton.labelTMP.GetComponent<RectTransform>();

                // Create a container for the borderlines as a sibling of the text
                var borderContainer = new GameObject("BorderContainer");
                borderContainer.transform.SetParent(textRect.parent, false);
                // Ensure the border is drawn behind the text
                borderContainer.transform.SetAsFirstSibling();

                // Set up the container to have the same position as the text,
                // with extra padding if needed.
                var containerRect = borderContainer.AddComponent<RectTransform>();
                containerRect.anchorMin = textRect.anchorMin;
                containerRect.anchorMax = textRect.anchorMax;
                containerRect.pivot = textRect.pivot;
                containerRect.anchoredPosition = textRect.anchoredPosition + new Vector2(1, 1.5f);
                containerRect.sizeDelta = textRect.sizeDelta + padding;

                // Create four borderlines
                CreateLine(borderContainer.transform, new Vector2(0, 1), new Vector2(1, 1), borderThickness,
                    borderColor, "TopLine");
                CreateLine(borderContainer.transform, new Vector2(0, 0), new Vector2(1, 0), borderThickness,
                    borderColor, "BottomLine");
                CreateLine(borderContainer.transform, new Vector2(0, 0), new Vector2(0, 1), borderThickness,
                    borderColor, "LeftLine");
            }

            return _keyBindButton.rectTransform;
        }, -40, 0);

        yield return (parent => MenuAPI.CreateREPOSpacer(parent).rectTransform, 5, 0);
    }

    private static void CreateLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax, float thickness, Color color,
        string name)
    {
        var line = new GameObject(name);
        line.transform.SetParent(parent, false);
        var img = line.AddComponent<Image>();
        img.color = color;
        var rt = line.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // For vertical lines, set width; for horizontal, set height
        if (Mathf.Approximately(anchorMin.x, anchorMax.x))
            // Vertical line
            rt.sizeDelta = new Vector2(thickness, 0);
        else if (Mathf.Approximately(anchorMin.y, anchorMax.y))
            // Horizontal line
            rt.sizeDelta = new Vector2(0, thickness);
    }

    private void KeyBindChanged(KeyCode key, bool signal = true)
    {
        if (signal)
            OnKeyBindChanged?.Invoke(key);

        if (_keyBindButton is null)
            return;

        _config.Key = key;
        Entry.SoundBoard.FileService.UpdateSoundItem(_config.Name, _config);

        var str = key switch
        {
            KeyCode.None => "_",
            _ => key.ToString()
        };
        _keyBindButton.labelTMP.text = str;

        const int startingFontSize = 28;
        const int startingLengthThreshold = 2;
        const int iterations = 6;

        for (var i = 0; i < iterations; i++)
        {
            // Calculate font size: 28 -> 24 -> 20 -> 18 -> 16
            var fontSize = startingFontSize - i * 4;
            _keyBindButton.labelTMP.fontSize = fontSize;

            // Calculate length threshold: 2 -> 4 -> 6 -> 8 -> 10
            var lengthThreshold = startingLengthThreshold + i;

            if (str.Length < lengthThreshold)
                return;
        }
    }

    public static SettingsPageSoundItem CreateAndBind(SoundItemConfig soundItemConfig, Action<KeyCode> onKeyBindChanged,
        Action<float> onVolumeChanged, Action<bool> onBehaviorChanged)
    {
        var item = new SettingsPageSoundItem(soundItemConfig);
        item.OnKeyBindChanged += onKeyBindChanged;
        item.OnVolumeChanged += onVolumeChanged;
        item.OnBehaviorChanged += onBehaviorChanged;
        return item;
    }
}