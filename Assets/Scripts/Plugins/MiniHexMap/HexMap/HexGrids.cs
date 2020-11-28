﻿using UnityEngine;
using UnityEngine.UI;
using EasyButtons;

namespace MiniHexMap
{
    [RequireComponent(typeof(HexGenConfig))]
    public class HexGrids : HexGridBase
    {
        public static HexGrids instance;

        public Text cellLabelPrefab;

        private HexGenConfig config;

        private Transform cellsTransform;
        private Transform watersTransform;
        private Transform waters;

        private float[] tribeNoise;

        private void Awake()
        {
            cellsTransform = transform.GetChild(0).transform;
            watersTransform = transform.GetChild(1).transform;
        }

        private void Start()
        {
            RegenerateCells();
        }

        private void Initialize()
        {
            if (instance == null)
                instance = this;

            else if (instance != this)
                DestroyImmediate(gameObject);
        }


        #region Generate

        [Button("Regenerate")]
        public override void RegenerateCells()
        {
            config = GetComponent<HexGenConfig>();

            Clear();

            if (pool == null)
                pool = new NoisePool();

            cells = new HexCell[height * width];

            Initialize();
            InitializeNoise();

            for (int z = 0, i = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int elevation = (int)(noises[i] * config.maxElevation) % config.maxElevation - 2;
                    // force to create unpassable tile
                    if (elevation >= 7) elevation += pool.Next() > .5 ? 1 : 2;
                    if (elevation <= 0) elevation -= 1;

                    CreateCell(x, z, i, elevation);
                    SetHexCellColor(cells[i]);
                    SetGrass(cells[i]);

                    i++;
                }
            }

            if (config.generateTribe)
            {
                InitializeTribeNoise();

                foreach (HexCell cell in cells)
                    SetTribe(cell);

                foreach (HexCell cell in cells)
                    SetWall(cell);
            }

            SetWaterSurface();
        }

        [Button("Clear")]
        public override void Clear()
        {
            base.Clear();

            tribeNoise = null;

            if (waters != null)
            {
                DestroyImmediate(waters.gameObject);
            }
        }

        protected override HexCell CreateCell(int x, int z, int i, int elevation = 0)
        {
            if (cellsTransform == null)
            {
                cellsTransform = transform.GetChild(0).transform;
            }

            Vector3 position;
            position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
            position.y = elevation < 0 ? elevation * HexMetrics.elevationStep : 0;
            position.z = z * 15f;

            // position cells
            int prefabIndex = elevation <= 0 ? 1 : elevation;
            //Debug.Log("Load prefab: " + prefabIndex + ", " + GetCellPrefab(prefabIndex));

            HexCell cell = cells[i] = Instantiate(GetCellPrefab(prefabIndex));
            cell.transform.SetParent(cellsTransform, false);
            cell.transform.localPosition = position;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

            // set material
            cell.SetColor(HexMaterial.White);

            // set grid
            cell.grid = this;
            cell.gridIndex = i;

            // set elevation;
            cell.Elevation = elevation;

            // set neightbours
            SetNeighbors(cell, x, z, i);

            // set canvas
            SetCanvas(cell);

            return cell;
        }

        protected void InitializeNoise()
        {
            noises = new float[width * height];

            float layerWeight = NoisePool.NoiseLayerInitialWeight(config.noiseLayers);

            for (int layer = 1; layer <= config.noiseLayers; layer++)
            {
                float scale = config.noiseScale * layer * layer;
                float weight = layerWeight * Mathf.Pow(.25f, layer - 1);

                float[] layered;
                float seed = pool.Next() * 1000;

                switch (config.noiseType)
                {
                    case NoiseType.Perlin:
                        layered = NoisePool.Perlin(width, height, scale, seed);
                        break;
                    case NoiseType.Simplex:
                        layered = NoisePool.Simplex(width, height, scale, seed);
                        break;
                    case NoiseType.Cellular:
                        layered = NoisePool.Cellular(width, height, scale, seed);
                        break;
                    default:
                        layered = NoisePool.Perlin(width, height, scale, seed);
                        break;
                }

                for (int i = 0; i < noises.Length; i++)
                {
                    noises[i] += layered[i] * weight;
                }
            }
        }

        protected void InitializeTribeNoise()
        {
            float layerWeight = NoisePool.NoiseLayerInitialWeight(config.noiseLayers);

            tribeNoise = new float[width * height];

            for (int layer = 1; layer <= config.noiseLayers; layer++)
            {
                float scale = config.noiseScale * layer * layer * 3;
                float weight = layerWeight * Mathf.Pow(.25f, layer - 1);

                float[] layered;
                float seed = pool.Next() * 1000;

                layered = NoisePool.Cellular(width, height, scale, seed);

                for (int i = 0; i < noises.Length; i++)
                {
                    tribeNoise[i] += layered[i] * weight;
                }
            }
        }

        private void SetNeighbors(HexCell cell, int x, int z, int i)
        {
            if (x > 0)
            {
                cell.SetNeighbor(HexDirection.W, cells[i - 1]);
            }
            if (z > 0)
            {
                if ((z & 1) == 0)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - width]);
                    if (x > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - width]);
                    if (x < width - 1)
                    {
                        cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
                    }
                }
            }
        }

        private void SetCanvas(HexCell cell)
        {
            GameObject canvas = SetPrefabAtTop(gridCanvasPrefab.gameObject, cell);
            Vector3 pos = canvas.transform.position;
            pos.y += .1f;
            canvas.transform.position = pos;
            canvas.transform.rotation = Quaternion.identity;
            canvas.transform.Rotate(90, 0, 0);
            cell.uiCanvas = canvas.GetComponent<Canvas>();

            if (config.noText)
                return;

            Text label = Instantiate(cellLabelPrefab);
            label.rectTransform.SetParent(canvas.transform, false);
            label.rectTransform.anchoredPosition = Vector2.zero;
            label.text = cell.coordinates.ToStringOnSeparateLines();

            cell.uiRect = label.rectTransform;
        }

        private void SetWaterSurface()
        {
            GameObject waterPrefab = GetWaterPrefab();

            if (waterPrefab == null)
            {
                Debug.LogWarning("Water prefab not set, cannot instantiate water surface.");
                return;
            }
            if (watersTransform == null)
            {
                watersTransform = transform.GetChild(1).transform;
            }

            Vector3 position = Center;
            position.y = transform.position.y + 0.1f - 12.5f;

            GameObject water = Instantiate(waterPrefab);
            water.transform.position = position;
            water.transform.SetParent(watersTransform);

            water.transform.localScale = new Vector3(width * 18.75f, 25, height * 16f);

            waters = water.transform;
        }

        private void SetTribe(HexCell cell)
        {
            if (!config.generateTribe)
                return;

            if (Vector3.Distance(cell.transform.position, Center) > config.tribeRadius)
                return;

            // tribes
            if (cell.material == HexMaterial.Brown || cell.material == HexMaterial.Black)
            {
                int index = (int)(tribeNoise[cell.gridIndex] * 10 % 6.5);

                GameObject tribePrefab = GetTribePrefab(index);
                if (tribePrefab == null)
                {
                    Debug.LogWarning("Tribe prefab not set, cannot instantiate tribe.");
                    return;
                }
                GameObject building = SetPrefabAtTop(tribePrefab, cell, 2.5f);

                if (building.CompareTag("Town"))
                    cell.town = building;
            }

            // fields
            if (cell.material == HexMaterial.Yellow || cell.material == HexMaterial.Brown)
            {
                int index = (int)(tribeNoise[cell.gridIndex] * 10 % 6);

                GameObject fieldPrefab = GetFieldPrefab(index);
                if (fieldPrefab == null)
                {
                    Debug.LogWarning("Field prefab not set, cannot instantiate field.");
                    return;
                }
                SetPrefabAtTop(fieldPrefab, cell, 2.5f, false);
            }
        }

        private void SetWall(HexCell cell)
        {
            if (!config.generateTribe)
                return;

            if (cell.town == null)
                return;

            GameObject wallPrefab = GetWallPrefab();
            if (wallPrefab == null)
            {
                Debug.LogWarning("Wall prefab not set, cannot instantiate wall.");
                return;
            }

            for (HexDirection direction = HexDirection.NE; direction <= HexDirection.NW; direction++)
            {
                HexCell next = cell.GetNeighbor(direction);

                if (next == null || (next.town == null && next.Elevation - cell.Elevation <= 1))
                {
                    SetPrefabAtTopEdge(wallPrefab, cell, 3f, direction);
                }
            }
        }

        private void SetGrass(HexCell cell)
        {
            if (!config.generateGrass)
                return;

            if (cell.material == HexMaterial.Green || cell.material == HexMaterial.Emerald)
            {
                float rnd = pool.Next();

                if (cell.Elevation <= 5 && rnd < 1 - config.grassDensity)
                        return;

                GameObject grassPrefab = GetGrassPrefab();
                if (grassPrefab == null)
                {
                    Debug.LogWarning("Grass prefab not set, cannot instantiate grass.");
                    return;
                }
                float scale = 10 * rnd % 3 + 5;

                cell.grass = SetPrefabAtTop(grassPrefab, cell, scale);
            }
        }

        private void SetHexCellColor(HexCell cell)
        {
            float rnd = noises[cell.gridIndex];
            int index = Mathf.Clamp((int)(rnd * 10 % 10), 0, 10);

            if (cell.Elevation >= config.maxElevation - 3)
                index = pool.Next() < .7f ? 8 : 9;

            //Debug.Log("material index: " + index);
            cell.SetColor((HexMaterial)index);
        }
        #endregion



        #region Save/Load

        [Button("Save", ButtonSpacing.Before)]
        public void Save()
        {
            Debug.Log("save file");
        }

        [Button("Load", ButtonSpacing.After)]
        public void Load()
        {
            Debug.Log("load file");
        }
        #endregion
    }
}