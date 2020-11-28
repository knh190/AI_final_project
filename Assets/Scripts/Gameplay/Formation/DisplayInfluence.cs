using System.Collections.Generic;
using MiniHexMap;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FormationGenerator))]
public class DisplayInfluence : MonoBehaviour
{
    public static DisplayInfluence instance;

    public InputConfig config;
    public Color friendColor = new Color(1f, .45f, .8f);
    public Color enemyColor = new Color(.1f, .2f, .8f);

    [Range(1, 10)]
    public int displayThreshold = 4;

    public bool displayEnemy = true;
    public bool displayPlayer = true;

    private FormationGenerator generator;
    private readonly HashSet<HexCell> friendCells = new HashSet<HexCell>();
    private readonly HashSet<HexCell> enemyCells = new HashSet<HexCell>();
    private readonly HashSet<Image> activeUI = new HashSet<Image>();

    private void Awake()
    {
        generator = GetComponent<FormationGenerator>();
    }

    private void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
            return;
        }
        instance = this;
    }

    public void UpdateUI()
    {
        ResetLastTouch();
        SetTouchCells();
        HighlightTouch();
    }

    void SetTouchCells()
    {
        friendCells.Clear();
        enemyCells.Clear();

        for (int i = 0; i < HexGrids.instance.Cells.Length; i++)
        {
            HexCell cell = HexGrids.instance.Cells[i];
            if (cell.Unpassable())
                continue;

            int inf1 = generator.playerInfluence.overallMap[i];
            int inf2 = generator.enemyInfluence.overallMap[i];

            if (inf1 > displayThreshold) friendCells.Add(cell);
            if (inf2 > displayThreshold) enemyCells.Add(cell);
        }
    }

    void ResetLastTouch()
    {
        foreach (Image ui in activeUI)
        {
            if (ui != null)
                DestroyImmediate(ui.gameObject);
        }
        friendCells.Clear();
        enemyCells.Clear();
        activeUI.Clear();
    }

    void HighlightTouch()
    {
        if (displayPlayer)
        {
            foreach (HexCell cell in friendCells)
            {
                if (cell == null) continue;
                int inf = generator.playerInfluence.friendMap[cell.gridIndex];

                HighlightCell(cell, inf, friendColor);
            }
        }

        if (displayEnemy)
        {
            foreach (HexCell cell in enemyCells)
            {
                if (cell == null) continue;
                int inf = generator.enemyInfluence.friendMap[cell.gridIndex];

                HighlightCell(cell, inf, enemyColor);
            }
        }
    }

    void HighlightCell(HexCell cell, int inf, Color color)
    {
        Color baseColor = inf < 0 ? Color.black : Color.white;
        Image ui = Instantiate(config.outlineInfluence);
        ui.rectTransform.SetParent(cell.uiCanvas.transform, false);
        ui.rectTransform.anchoredPosition = Vector2.zero;
        ui.color = Color.Lerp(color, baseColor, Mathf.Abs(inf) / 15f);
        activeUI.Add(ui);
    }
}
