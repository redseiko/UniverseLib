namespace UniverseLib.UI.Models;

using System;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public sealed class TMPInputFieldRef : UIModel, IBaseInputFieldRef, ILegacyTextGenerator {
  internal static readonly HashSet<TMPInputFieldRef> _inputsPendingUpdate = new();

  internal static void UpdateInstances() {
    while (_inputsPendingUpdate.Count > 0) {
      TMPInputFieldRef inputField = _inputsPendingUpdate.First();
      LayoutRebuilder.MarkLayoutForRebuild(inputField.Transform);

      inputField.OnValueChanged?.Invoke(inputField.Component.text);
      _inputsPendingUpdate.Remove(inputField);
    }
  }

  public event Action<string> OnValueChanged;

  public override GameObject UIRoot => Component.gameObject;

  public TMP_InputField Component { get; }
  public TMP_Text Placeholder { get; }
  public GameObject GameObject => Component.gameObject;
  public RectTransform Transform { get; }

  public string Text {
    get => Component.text;
    set => Component.text = value;
  }

  public string PlaceholderText {
    get => Placeholder.text;
    set => Placeholder.text = value;
  }

  public bool IsFocused => Component.isFocused;

  public TextGenerator TextGenerator => throw new NotImplementedException();
  public bool ReachedMaxVerts => throw new NotImplementedException();

  public int CaretPosition => Component.caretPosition;

  public Vector3 GetCaretScreenPosition() {
    return Component.textComponent.textInfo.characterInfo[Component.caretPosition].bottomLeft;
  }

  public TMPInputFieldRef(TMP_InputField component) {
    Component = component;
    Transform = component.GetComponent<RectTransform>();
    Placeholder = component.placeholder.GetComponentInChildren<TMP_Text>();
    Component.onValueChanged.AddListener(OnInputChanged);
  }

  private void OnInputChanged(string value) {
    _inputsPendingUpdate.Add(this);
  }

  public override void ConstructUI(GameObject parent) => throw new NotImplementedException();
}
