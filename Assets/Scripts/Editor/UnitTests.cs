using MiniHexMap;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class UnitTests
    {
        [Test]
        public void TestCreateCell()
        {
            HexCell cell = CreateCell(new HexCoordinates(0, 0));
            CreateNeighbours(cell);

            Assert.AreEqual(cell.GetAllNeighbors().Length, 6);
        }

        [Test]
        public void TestFormationSlot()
        {
            Formation form = CreateFormation();
            Unit u1 = CreateUnit(form, CreateArcherStats());
            Unit u2 = CreateUnit(form, CreateArcherStats());
            Unit u3 = CreateUnit(form, CreateSwordStats());
            Unit u4 = CreateUnit(form, CreateSwordStats());

            Assert.AreEqual(u1.Row, 0);
            Assert.AreEqual(u2.Row, 0);
            Assert.AreEqual(u3.Row, 0);
            Assert.AreEqual(u4.Row, 0);

            Assert.AreEqual(u1.Col, 0);
            Assert.AreEqual(u2.Col, 1);
            Assert.AreEqual(u3.Col, 2);
            Assert.AreEqual(u4.Col, 3);

            // create 2 layer depth cells network
            HexCell cell = CreateCell(new HexCoordinates(0, 0));
            CreateNeighbours(cell);

            foreach (HexCell c in cell.GetAllNeighbors())
            {
                CreateNeighbours(c);
            }
            form.cell = cell;

            Assert.AreEqual(u1.FormationSlot.coordinates, new HexCoordinates(0, 0));
            Assert.AreEqual(u2.FormationSlot.coordinates, new HexCoordinates(1, 0));
            Assert.AreEqual(u3.FormationSlot.coordinates, new HexCoordinates(2, 0));

            //Debug.Log(u4.FormationSlot);
        }

        static Formation CreateFormation()
        {
            GameObject go = new GameObject();
            return go.AddComponent<Formation>();
        }

        static Unit CreateUnit(Formation formation, UnitStats stats)
        {
            Unit unit = CreateBaseUnit();
            unit.stats = stats;

            formation.AddUnit(unit);

            return unit;
        }

        static Unit CreateBaseUnit()
        {
            GameObject go = new GameObject();
            Unit unit = go.AddComponent<Unit>();
            unit.runtime = ScriptableObject.CreateInstance<UnitRuntime>();
            return unit;
        }

        static UnitStats CreateArcherStats()
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.unitType = UnitType.Arrow;
            return stats;
        }

        static UnitStats CreateSwordStats()
        {
            var stats = ScriptableObject.CreateInstance<UnitStats>();
            stats.unitType = UnitType.Sword;
            return stats;
        }

        static HexCell CreateCell(HexCoordinates coord)
        {
            GameObject go = new GameObject();
            HexCell cell = go.AddComponent<HexCell>();
            cell.coordinates = coord;
            return cell;
        }

        static void CreateNeighbours(HexCell cell)
        {
            HexCoordinates coord = cell.coordinates;
            int dir = 0;
            for (int i = 0; i < 6; i++)
            {
                HexCoordinates coordinate = new HexCoordinates(coord.X + i, coord.Z);
                HexCell next = CreateCell(coordinate);
                cell.SetNeighbor((HexDirection)dir, next);
                dir++;
            }
        }
    }
}
