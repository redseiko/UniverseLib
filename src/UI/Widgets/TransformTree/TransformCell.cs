namespace UniverseLib.UI.Widgets;

using System;

using UnityEngine;
using UnityEngine.UI;

using UniverseLib.UI.Models;
using UniverseLib.UI.Widgets.ScrollView;
using UniverseLib.Utility;

public class TransformCell : ICell
{
    public float DefaultHeight => 25f;

    public bool Enabled => enabled;
    private bool enabled;

    public Action<CachedTransform> OnExpandToggled;
    public Action<CachedTransform> OnEnableToggled;
    public Action<GameObject> OnGameObjectClicked;

    public CachedTransform cachedTransform;

    public GameObject UIRoot { get; set; }
    public RectTransform Rect { get; set; }

    public ButtonRef ExpandButton;
    public ButtonRef NameButton;
    public Toggle EnabledToggle;
    public InputFieldRef SiblingIndex;

    public LayoutElement spacer;

    public void Enable()
    {
        enabled = true;
        UIRoot.SetActive(true);
    }

    public void Disable()
    {
        enabled = false;
        UIRoot.SetActive(false);
    }

    public void ConfigureCell(CachedTransform cached)
    {
        if (cached == null)
        {
            Universe.LogWarning("Setting TransformTree cell but the CachedTransform is null!");
            return;
        }

        if (!Enabled)
            Enable();

        cachedTransform = cached;

        spacer.minWidth = cached.Depth * 15;

        if (cached.Value)
        {
            string name = cached.Value.name?.Trim();
            if (string.IsNullOrEmpty(name))
                name = "<i><color=#808080FF>untitled</color></i>";
            NameButton.ButtonTMPText.text = name;
            NameButton.ButtonTMPText.color = cached.Value.gameObject.activeSelf ? Color.white : Color.grey;

            EnabledToggle.Set(cached.Value.gameObject.activeSelf, false);

            if (!cached.Value.parent)
                SiblingIndex.GameObject.SetActive(false);
            else
            {
                SiblingIndex.GameObject.SetActive(true);
                if (!SiblingIndex.Component.isFocused)
                    SiblingIndex.Text = cached.Value.GetSiblingIndex().ToString();
            }

            int childCount = cached.Value.childCount;
            if (childCount > 0)
            {
                NameButton.ButtonTMPText.text = $"<color=#808080FF>[{childCount}]</color> {NameButton.ButtonTMPText.text}";

                ExpandButton.Component.interactable = true;
                ExpandButton.ButtonTMPText.text = cached.Expanded ? "▼" : "►";
                ExpandButton.ButtonTMPText.color = cached.Expanded ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.3f, 0.3f, 0.3f);
            }
            else
            {
                ExpandButton.Component.interactable = false;
                ExpandButton.ButtonTMPText.text = "▪";
                ExpandButton.ButtonTMPText.color = new Color(0.3f, 0.3f, 0.3f);
            }
        }
        else
        {
            NameButton.ButtonTMPText.text = $"[Destroyed]";
            NameButton.ButtonTMPText.color = Color.red;

            SiblingIndex.GameObject.SetActive(false);
        }
    }

    public void OnMainButtonClicked()
    {
        if (cachedTransform.Value)
            OnGameObjectClicked?.Invoke(cachedTransform.Value.gameObject);
        else
            Universe.LogWarning("The object was destroyed!");
    }

    public void OnExpandClicked()
    {
        OnExpandToggled?.Invoke(cachedTransform);
    }

    private void OnEnableClicked(bool value)
    {
        OnEnableToggled?.Invoke(cachedTransform);
    }

    private void OnSiblingIndexEndEdit(string input)
    {
        if (this.cachedTransform == null || !this.cachedTransform.Value)
            return;

        if (int.TryParse(input.Trim(), out int index))
            this.cachedTransform.Value.SetSiblingIndex(index);

        this.SiblingIndex.Text = this.cachedTransform.Value.GetSiblingIndex().ToString();
    }

    public GameObject CreateContent(GameObject parent)
    {
        UIRoot = UIFactory.CreateUIObject("TransformCell", parent);
        UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(UIRoot, false, false, true, true, 2, childAlignment: TextAnchor.MiddleCenter);
        Rect = UIRoot.GetComponent<RectTransform>();
        Rect.anchorMin = new Vector2(0, 1);
        Rect.anchorMax = new Vector2(0, 1);
        Rect.pivot = new Vector2(0.5f, 1);
        Rect.sizeDelta = new Vector2(25, 25);
        UIFactory.SetLayoutElement(UIRoot, minWidth: 100, flexibleWidth: 9999, minHeight: 25, flexibleHeight: 0);

        GameObject spacerObj = UIFactory.CreateUIObject("Spacer", UIRoot, new Vector2(0, 0));
        UIFactory.SetLayoutElement(spacerObj, minWidth: 0, flexibleWidth: 0, minHeight: 0, flexibleHeight: 0);
        this.spacer = spacerObj.GetComponent<LayoutElement>();

        // Expand arrow

        ExpandButton = UIFactory.CreateTMPButton(UIRoot, "ExpandButton", "►");
        UIFactory.SetLayoutElement(ExpandButton.Component.gameObject, minWidth: 15, flexibleWidth: 0, minHeight: 25, flexibleHeight: 0);

        // Enabled toggle

        GameObject toggleObj = UIFactory.CreateToggle(UIRoot, "BehaviourToggle", out EnabledToggle, out Text behavText, default, 17, 17);
        UIFactory.SetLayoutElement(toggleObj, minHeight: 17, flexibleHeight: 0, minWidth: 17);
        EnabledToggle.onValueChanged.AddListener(OnEnableClicked);

        // Name button

        GameObject nameBtnHolder = UIFactory.CreateHorizontalGroup(this.UIRoot, "NameButtonHolder",
            false, false, true, true, childAlignment: TextAnchor.MiddleLeft);
        UIFactory.SetLayoutElement(nameBtnHolder, flexibleWidth: 9999, minHeight: 25, flexibleHeight: 0);
        nameBtnHolder.AddComponent<RectMask2D>();

        NameButton = UIFactory.CreateTMPButton(nameBtnHolder, "NameButton", "Name");
        UIFactory.SetLayoutElement(NameButton.Component.gameObject, flexibleWidth: 9999, minHeight: 25, flexibleHeight: 0);
        NameButton.ButtonTMPText.overflowMode = TMPro.TextOverflowModes.Overflow;
        NameButton.ButtonTMPText.alignment = TMPro.TextAlignmentOptions.Left;

        // Sibling index input

        SiblingIndex = UIFactory.CreateInputField(this.UIRoot, "SiblingIndexInput", string.Empty);
        SiblingIndex.Component.textComponent.fontSize = 11;
        SiblingIndex.Component.textComponent.alignment = TextAnchor.MiddleRight;
        Image siblingImage = SiblingIndex.GameObject.GetComponent<Image>();
        siblingImage.color = new(0f, 0f, 0f, 0.25f);
        UIFactory.SetLayoutElement(SiblingIndex.GameObject, 35, 20, 0, 0);
        SiblingIndex.Component.GetOnEndEdit().AddListener(OnSiblingIndexEndEdit);

        // Setup selectables

        Color normal = new(0.11f, 0.11f, 0.11f);
        Color highlight = new(0.25f, 0.25f, 0.25f);
        Color pressed = new(0.05f, 0.05f, 0.05f);
        Color disabled = new(1, 1, 1, 0);
        RuntimeHelper.SetColorBlock(ExpandButton.Component, normal, highlight, pressed, disabled);
        RuntimeHelper.SetColorBlock(NameButton.Component, normal, highlight, pressed, disabled);

        NameButton.OnClick += OnMainButtonClicked;
        ExpandButton.OnClick += OnExpandClicked;

        UIRoot.SetActive(false);

        return this.UIRoot;
    }
}
