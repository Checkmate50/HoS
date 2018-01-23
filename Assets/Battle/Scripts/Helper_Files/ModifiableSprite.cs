using System.Collections.Generic;
using UnityEngine;

public abstract class ModifiableSprite : MonoBehaviour {
  protected SpriteRenderer spriteRenderer;
  protected Color originalColor;
  protected List<float> appliedShades;

  void Start() {
    spriteRenderer = GetComponent<SpriteRenderer>();
    originalColor = spriteRenderer.color;
    appliedShades = new List<float>();
  }

  //Shades this BoardNode to a given factor
  public void Shade(float factor) {
    Shade(factor, true);
  }

  private void Shade(float factor, bool addShade) {
    Color color = spriteRenderer.color;
    spriteRenderer.color = new Color(color.r * factor, color.g * factor, color.b * factor);
    appliedShades.Add(factor);
  }

  // Removes all shade
  public void RemoveShade() {
    spriteRenderer.color = originalColor;
    appliedShades = new List<float>();
  }

  // Removes a specific shade (if it has applied this shade)
  public void RemoveShade(float shade) {
    spriteRenderer.color = originalColor;
    appliedShades.Remove(shade);
    foreach (float s in appliedShades)
      Shade(shade, false);
  }
}