using System.Collections.Generic;
using MiniHexMap;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.EventSystems;

enum HighLightType
{
    Defaut, Unit, Town
}

public class TrackInput : MonoBehaviour
{
    public static TrackInput instance;

    public InputConfig config;

    private float heightLimitMin;
    private float heightLimitMax;
    private float widthLimitMin;
    private float widthLimitMax;

    private HexGrids grids;
    private readonly HashSet<HexCell> touchedCells = new HashSet<HexCell>();
    private readonly HashSet<Image> activeUI = new HashSet<Image>();

    private Vector3 panMovement;
    private float panIncrease;

    void Start()
    {
        if (instance != null && instance != this)
        {
            DestroyImmediate(this);
            return;
        }
        instance = this;

        grids = HexGrids.instance;

        if (config.outline == null)
            Debug.LogWarning("You should set outline image to make input system work.");
    }

    void FixedUpdate()
    {
        if (config.outline == null)
            return;

        HandleMouseMove();
        HandleCameraZoom();
        HandleCameraPan();

        LimitPosition();
    }

    void HandleMouseMove()
    {
        Vector3 screenPos = Vector2ControlToVector3(Mouse.current.position);
        // hover over ui
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Ray inputRay = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(inputRay, out RaycastHit hit))
        {
            TouchCell(hit.point);
        }
    }

    void HandleCameraZoom()
    {
        float delta = Mouse.current.scroll.ReadValue().y;
        Camera.main.fieldOfView -= delta * config.zoomSpeed;
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, config.zoomLimitMin, config.zoomLimitMax);
    }

    void HandleCameraPan()
    {
        Keyboard kb = Keyboard.current;
        panMovement = Vector3.zero;
        bool pressed = false;

        // @todo Mouse middle pressed + mouse movement
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)
        {
            panMovement += Vector3.forward * config.panSpeed * Time.deltaTime;
            pressed = true;
        }
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)
        {
            panMovement += Vector3.back * config.panSpeed * Time.deltaTime;
            pressed = true;
        }
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
        {
            panMovement += Vector3.left * config.panSpeed * Time.deltaTime;
            pressed = true;
        }
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)
        {
            panMovement += Vector3.right * config.panSpeed * Time.deltaTime;
            pressed = true;
        }
        // increase speed
        if (pressed)
        {
            panIncrease += Time.deltaTime / config.secondsToMaxSpeed;
            config.panSpeed = Mathf.Lerp(config.panSpeedMin, config.panSpeedMax, panIncrease);
        }
        else
        {
            panIncrease = 0;
            config.panSpeed = config.panSpeedMin;
        }
        Camera.main.transform.Translate(panMovement, Space.World);
    }

    void LimitPosition()
    {
        float zoomScale = config.zoomLimitMin / Camera.main.fieldOfView;

        heightLimitMin = grids.Center.z - config.panBorderLimit * (grids.height * .5f) * zoomScale;
        heightLimitMax = grids.Center.z + config.panBorderLimit * (grids.height * .2f) * zoomScale;
        widthLimitMin = grids.Center.x - config.panBorderLimit * (grids.width * .3f) * zoomScale;
        widthLimitMax = grids.Center.x + config.panBorderLimit * (grids.width * .3f) * zoomScale;

        Vector3 pos = Camera.main.transform.position;
        pos.z = Mathf.Clamp(pos.z, heightLimitMin, heightLimitMax);
        pos.x = Mathf.Clamp(pos.x, widthLimitMin, widthLimitMax);
        transform.position = pos;
    }

    #region Mouse Position

    // Highlight all same type cells
    void TouchCell(Vector3 position)
    {
        if (grids == null)
            grids = HexGrids.instance;

        position = grids.transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);

        HexCell cell = grids.GetCell(coordinates);
        if (cell != null)
        {
            ResetLastTouch();
            GetHighlightCells(cell, GetHighLightType(cell));
            HighlightTouch();
        }
    }

    HighLightType GetHighLightType(HexCell cell)
    {
        if (cell.unit)
            return HighLightType.Unit;
        if (cell.town)
            return HighLightType.Town;
        return HighLightType.Defaut;
    }

    // Add all same type neighbours
    void GetHighlightCells(HexCell cell, HighLightType type)
    {
        if (cell.Unpassable())
            return;

        touchedCells.Add(cell);

        switch (type)
        {
            case HighLightType.Defaut:
                return;

            case HighLightType.Town:
                foreach (HexCell c in cell.GetAllNeighbors())
                {
                    if (touchedCells.Contains(c))
                        continue;

                    if (GetHighLightType(c) == type)
                    {
                        touchedCells.Add(c);
                        GetHighlightCells(c, type);
                    }
                }
                return;

            case HighLightType.Unit:
                foreach (Unit u in cell.unit.runtime.formation.Units)
                {
                    if (touchedCells.Contains(u.runtime.currCell))
                        continue;
                    touchedCells.Add(u.runtime.currCell);
                }
                return;
        }
    }

    void HighlightTouch()
    {
        foreach (HexCell cell in touchedCells)
        {
            if (cell == null) continue;
            Image ui = Instantiate(config.outline);
            ui.rectTransform.SetParent(cell.uiCanvas.transform, false);
            ui.rectTransform.anchoredPosition = Vector2.zero;
            activeUI.Add(ui);
        }
    }

    void ResetLastTouch()
    {
        foreach (Image ui in activeUI)
        {
            if (ui != null)
                DestroyImmediate(ui.gameObject);
        }
        touchedCells.Clear();
        activeUI.Clear();
    }
    #endregion

    static Vector3 Vector2ControlToVector3(Vector2Control vec)
    {
        Vector3 pos = new Vector3(vec.x.ReadValue(), vec.y.ReadValue(), 0);
        return pos;
    }
}
