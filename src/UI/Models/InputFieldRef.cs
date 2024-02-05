namespace UniverseLib.UI.Models;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public interface ILegacyTextGenerator {
  public abstract TextGenerator TextGenerator { get; }
  public abstract bool ReachedMaxVerts { get; }
}

public interface IBaseInputFieldRef {
  public event Action<string> OnValueChanged;

  public GameObject GameObject { get; }
  public RectTransform Transform { get; }

  public string Text { get; set; }
  public string PlaceholderText { get; set; }

  public abstract bool IsFocused { get; }
  public abstract int CaretPosition { get; }
  public abstract Vector3 GetCaretScreenPosition();
}

/// <summary>
/// A simple wrapper class for working with InputFields, with some helpers and performance improvements.
/// </summary>
public sealed class InputFieldRef : UIModel, IBaseInputFieldRef, ILegacyTextGenerator
{
    // Static

    internal static readonly HashSet<InputFieldRef> inputsPendingUpdate = new();

    internal static void UpdateInstances()
    {
        while (inputsPendingUpdate.Any())
        {
            InputFieldRef inputField = inputsPendingUpdate.First();
            LayoutRebuilder.MarkLayoutForRebuild(inputField.Transform);
            inputField.OnValueChanged?.Invoke(inputField.Component.text);

            inputsPendingUpdate.Remove(inputField);
        }
    }

    // Instance

    /// <summary>
    /// Invoked at most once per frame, if the input was changed in the previous frame.
    /// </summary>
    public event Action<string> OnValueChanged;

    /// <summary>
    /// The actual InputField component which this object is a reference to.
    /// </summary>
    public InputField Component { get; }

    /// <summary>
    /// The placeholder Text component.
    /// </summary>
    public Text Placeholder { get; }

    /// <summary>
    /// The GameObject which the InputField is attached to.
    /// </summary>
    public override GameObject UIRoot => Component.gameObject;

    /// <summary>
    /// The GameObject which the InputField is attached to.
    /// </summary>
    public GameObject GameObject => Component.gameObject;

    /// <summary>
    /// The RectTransform for this InputField.
    /// </summary>
    public RectTransform Transform { get; }

    /// <summary>
    /// The Text set to the InputField.
    /// </summary>
    public string Text
    {
        get => Component.text;
        set => Component.text = value;
    }

    /// <summary>
    /// The Text set to the InputField placeholder.
    /// </summary>
    public string PlaceholderText {
      get => Placeholder.text;
      set => Placeholder.text = value;
    }

    public bool IsFocused => Component.isFocused;

    /// <summary>
    /// A reference to the InputField's cachedInputTextGenerator.
    /// </summary>
    public TextGenerator TextGenerator => Component.cachedInputTextGenerator;

    /// <summary>
    /// Returns true if the InputField's vertex count has reached the <see cref="UniversalUI.MAX_TEXT_VERTS"/> limit.
    /// </summary>
    public bool ReachedMaxVerts => TextGenerator.vertexCount >= UniversalUI.MAX_TEXT_VERTS;

    public int CaretPosition => Component.caretPosition;

    public Vector3 GetCaretScreenPosition() {
      int caretIdx = Math.Max(0, Math.Min(TextGenerator.characterCount - 1, Component.caretPosition));
      return TextGenerator.characters[caretIdx].cursorPos;
    }

    public InputFieldRef(InputField component)
    {
        Component = component;
        Transform = component.GetComponent<RectTransform>();
        Placeholder = component.placeholder.TryCast<Text>();
        component.onValueChanged.AddListener(OnInputChanged);
    }

    private void OnInputChanged(string value) {
      inputsPendingUpdate.Add(this);
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    public override void ConstructUI(GameObject parent) => throw new NotImplementedException();
}
