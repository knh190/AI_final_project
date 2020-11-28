using System.Collections.Generic;
using MiniHexMap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class TrackClick : MonoBehaviour
{
    public static TrackClick instance;

    public struct ClickCtx
    {
        public Formation currFormation;
    }

    [HideInInspector]
    public ClickCtx ctx = new ClickCtx();
    public InputConfig config;

    private HexGrids grids;
    private readonly HashSet<Image> activeUI = new HashSet<Image>();

    private void Start()
    {
        if (instance != null && instance != this)
        {
            DestroyImmediate(this);
            return;
        }
        instance = this;
    }

    private void Update()
    {
        // single click, only once per frame
        if (Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            HandleMouseClick();
        }
        // update UI
        if (ctx.currFormation != null)
        {
            ClearHighlight();
            HighlightCurrFormation();
        }
    }

    void HandleMouseClick()
    {
        Vector3 screenPos = Vector2ControlToVector3(Mouse.current.position);
        // click on UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Click on UI. Ignore click on map.");
            return;
        }
        // click on map
        Ray inputRay = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(inputRay, out RaycastHit hit))
        {
            ClickCell(hit.point);
        }
    }

    void ClickCell(Vector3 position)
    {
        if (grids == null)
            grids = HexGrids.instance;

        position = grids.transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);

        HexCell cell = grids.GetCell(coordinates);
        if (cell != null)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                ClickL(cell);
            }
            if (Mouse.current.rightButton.isPressed)
            {
                ClickR(cell);
            }
            ClearHighlight();
            HighlightCurrFormation();
        }
    }

    void ClickL(HexCell cell)
    {
        // Cancel selection
        ctx.currFormation = null;
        // highlight button
        FormationClick.ToggleButtonEvent?.Invoke();

        // Select formation
        if (cell.unit != null)
        {
            ctx.currFormation = cell.unit.runtime.formation;
            // highlight button
            FormationClick.ToggleButtonEvent?.Invoke();
        }
    }

    void ClickR(HexCell cell)
    {
        // Move formation to target
        if (ctx.currFormation != null)
        {
            //Debug.Log("right click on " + cell);
            ctx.currFormation.cell = cell;
            ctx.currFormation.MakeFormation();
        }
    }

    void ClearHighlight()
    {
        foreach (Image ui in activeUI)
        {
            DestroyImmediate(ui.gameObject);
        }
        activeUI.Clear();
    }

    void HighlightCurrFormation()
    {
        if (ctx.currFormation == null)
            return;

        foreach (HexCell cell in ctx.currFormation.Cells)
        {
            if (cell != null) HighlightCell(cell);
        }
    }

    void HighlightCell(HexCell cell)
    {
        Image ui = Instantiate(config.outlineFormation);
        ui.rectTransform.SetParent(cell.uiCanvas.transform, false);
        ui.rectTransform.anchoredPosition = Vector2.zero;
        activeUI.Add(ui);
    }

    static Vector3 Vector2ControlToVector3(Vector2Control vec)
    {
        Vector3 pos = new Vector3(vec.x.ReadValue(), vec.y.ReadValue(), 0);
        return pos;
    }
}
