using UnityEngine;
using EasyButtons;
using System.Collections.Generic;
using System;

namespace MiniHexMap
{
    public class HexCell : MonoBehaviour, IEquatable<HexCell>
    {
        [Range(-2, 7)]
        public int Elevation;

        public HexCoordinates coordinates;
        public HexMaterial material;

        [HideInInspector, SerializeField]
        HexCell[] neighbors = new HexCell[6];

        internal RectTransform uiRect;
        internal Canvas uiCanvas;
        internal HexGridBase grid;

        // @todo use scriptable object
        #region Runtime
        internal int gridIndex;
        internal Unit unit;
        public GameObject town;
        internal GameObject grass;
        #endregion

        public static readonly int MaxMoveCost = 4194304;

        public int MoveCost
        {
            get
            {
                if (this.Unpassable())
                    return MaxMoveCost;

                switch (material)
                {
                    case HexMaterial.White:
                    case HexMaterial.LightBlue:
                        return 1;
                    case HexMaterial.Yellow:
                    case HexMaterial.Brown:
                    case HexMaterial.Black:
                        return 2;
                    case HexMaterial.Green:
                    case HexMaterial.Emerald:
                        return 4;
                    default:
                        return MaxMoveCost;
                }
            }
        }

        [Button]
        public void Refresh()
        {
            grid.Refresh(this);
        }

        public void SetColor(HexMaterial material)
        {
            this.material = material;

            GetComponent<MeshRenderer>().material = material.GetMaterial();
        }

        public void SetElevation(int elev)
        {
            Elevation = elev;

            if (uiRect)
            {
                Vector3 uiPosition = uiRect.localPosition;
                uiPosition.z = Elevation * -HexMetrics.elevationStep;
                uiRect.localPosition = uiPosition;
            }
        }

        public HexCell GetNeighbor(HexDirection direction)
        {
            if (direction < HexDirection.NE || direction > HexDirection.NW)
                return null;
            return neighbors[(int)direction];
        }

        public HexCell[] GetAllNeighbors()
        {
            List<HexCell> cells = new List<HexCell>();

            for (HexDirection direction = HexDirection.NE; direction <= HexDirection.NW; direction++)
            {
                HexCell cell = GetNeighbor(direction);
                if (cell != null)
                {
                    cells.Add(cell);
                }
            }
            return cells.ToArray();
        }

        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            neighbors[(int)direction] = cell;
            cell.neighbors[(int)direction.Opposite()] = this;
        }

        public override string ToString()
        {
            return name + ", " + coordinates.ToString();
        }

        public bool Equals(HexCell other)
        {
            return gridIndex == other.gridIndex && grid == other.grid && Elevation == other.Elevation;
        }

        public override bool Equals(object other)
        {
            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}