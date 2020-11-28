using System.Collections.Generic;
using MiniHexMap;
using UnityEngine;
using UnityEngine.UI;

public class Debugger : MonoBehaviour
{
    public static Debugger instance;

    public Image outline;

    private readonly HashSet<Image> activeUI = new HashSet<Image>();
    private Color originColor;

    private void Start()
    {
        if (instance != null && instance != this)
        {
            DestroyImmediate(this);
            return;
        }
        instance = this;
    }

    public void HighlightCells(HexCell[] cells)
    {
        foreach (HexCell cell in cells) HighlightCell(cell);
    }

    public void HighlightCell(HexCell cell, float intensity = 1f)
    {
        if (outline == null) return;
        originColor = outline.color;

        Image ui = Instantiate(outline);
        ui.rectTransform.SetParent(cell.uiCanvas.transform, false);
        ui.rectTransform.anchoredPosition = Vector2.zero;
        ui.color = Color.Lerp(new Color(0,0,0,.5f), originColor, intensity);
        activeUI.Add(ui);
    }

    public void ClearHighlight()
    {
        foreach (Image ui in activeUI)
        {
            Destroy(ui.gameObject);
        }
        activeUI.Clear();
    }
}
