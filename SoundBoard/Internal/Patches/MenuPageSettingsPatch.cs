using HarmonyLib;
using MenuLib;
using MenuLib.MonoBehaviors;
using MenuLib.Structs;
using SoundBoard.File;
using UnityEngine;

namespace SoundBoard.Internal.Patches;

[HarmonyPatch(typeof(MenuPageSettings))]
internal class MenuPageSettingsPatch
{
    private static REPOPopupPage SoundBoardPage { get; set; } = null!;
    private static Core.SoundBoard SoundBoard => Entry.SoundBoard;
    private static MenuManager MenuManager => MenuManager.instance;

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void Start(MenuPageSettings __instance)
    {
        Transform? child = null;
        float spacing = 0;
        int i;
        for (i = 0; i < __instance.transform.childCount; i++)
        {
            var oldPos = child?.localPosition.y ?? 0;
            child = __instance.transform.GetChild(i);
            spacing = child.localPosition.y - oldPos;
            if (child.name.ToLowerInvariant().Contains("controls"))
                break;
        }

        if (child is null)
            return;

        spacing = Math.Abs(spacing);
        var pos = new Vector2(child.localPosition.x, child.transform.localPosition.y - spacing);
        SoundBoardPage = MenuAPI.CreateREPOPopupPage("Sound Board", localPosition: Vector2.zero, shouldCachePage: true);
        SoundBoardPage.maskPadding = new Padding(0, 0, 0, 0);
        SoundBoardPage.menuPage.onPageEnd.AddListener(Entry.SoundBoard.CancelNewKeyBind);

        var button = MenuAPI.CreateREPOButton("Sound Board", () =>
        {
            MenuManager.PageCloseAllAddedOnTop();
            SoundBoardPage.OpenPage(true);
        }, __instance.transform, pos);

        foreach (var sound in SoundBoard.Sounds)
        foreach (var (element, top, bottom) in sound.SettingsItem.Init())
            SoundBoardPage.AddElementToScrollView(element, top, bottom);

        SoundBoardPage.AddElementToScrollView(parent =>
        {
            var label = MenuAPI.CreateREPOButton("Open Audio Folder",
                () => FileIO.OpenFolder(Core.SoundBoard.AudioDirectory), parent.transform, Vector2.zero);
            label.labelTMP.fontSize = 28;
            label.labelTMP.color = Color.white;
            label.labelTMP.margin = new Vector4(-4, 0, 0, 0);
            return label.rectTransform;
        });

        __instance.transform.GetChild(i + 1).transform.localPosition = pos + new Vector2(0, -spacing);
    }
}