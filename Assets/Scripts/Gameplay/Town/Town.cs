using System.Collections.Generic;
using MiniHexMap;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem), typeof(AudioSource))]
public class Town : MonoBehaviour
{
    public Material burntMat;
    public TownStats stats;
    [HideInInspector]
    public TownRuntime runtime;
    public ParticleSystem explosion;

    internal HexCell cell;
    private ParticleSystem smoke;
    private AudioSource source;

    public delegate void OnDamage(int dmg);
    public static OnDamage DamageEvent;

    private Town[] neighbors;

    private void Awake()
    {
        runtime = ScriptableObject.CreateInstance<TownRuntime>();
        runtime.Health = stats.Health;

        smoke = GetComponent<ParticleSystem>();
        source = GetComponent<AudioSource>();

        cell = transform.GetComponentInParent<HexCell>();
    }

    private void Update()
    {
        if (neighbors == null)
        {
            InitializeNearby();
        }

        if (runtime.Health <= 0)
        {
            Destroy();
            return;
        }
        if (runtime.Health < stats.Health)
        {
            Burn();
        }
        else
        {
            smoke.Stop();
        }
        // recover after a few seconds
        //if (runtime.Health < stats.Health &&
        //    Time.time - runtime.LastDamageTime > stats.RecoveryAfterSeconds)
        //{
        //    Recover();
        //}
    }

    public void Recover()
    {
        runtime.Health += stats.RecoveryPerSecond * Time.fixedDeltaTime;
        runtime.Health = Mathf.Min(stats.Health, runtime.Health);
    }

    public void Burn()
    {
        if (!smoke.isPlaying) smoke.Play();
    }

    private void Explode()
    {
        if (!explosion) return;

        ParticleSystem exp = Instantiate(explosion, transform, false);
        exp.Play();
        Destroy(exp, exp.main.duration);

        if (!stats.burnSfx) return;
        source.PlayOneShot(stats.burnSfx);
    }

    public void Destroy()
    {
        if (cell.town != null)
        {
            // debug & safety
            if (!burntMat)
            {
                Debug.LogWarning("Burnt material not set!");
                return;
            }
            // change to burnt material
            for (int i = 0; i < transform.childCount; i++)
            {
                MeshRenderer render = transform.GetChild(i).GetComponent<MeshRenderer>();
                if (render != null) render.material = burntMat;
            }
            // erase map information
            cell.town = null;
        }
    }

    private void InitializeNearby()
    {
        if (cell == null)
        {
            cell = transform.GetComponentInParent<HexCell>();
        }
        // BFS find all adjacent towns
        Queue<HexCell> frontiers = new Queue<HexCell>();
        HashSet<HexCell> visited = new HashSet<HexCell>();

        frontiers.Enqueue(cell);
        while (frontiers.Count != 0)
        {
            HexCell curr = frontiers.Dequeue();
            if (curr == null || visited.Contains(curr))
                continue;

            visited.Add(curr);
            foreach (HexCell c in curr.GetAllNeighbors())
            {
                if (c.town != null) frontiers.Enqueue(c);
            }
        } 
        // store neaby including itself
        neighbors = new Town[visited.Count];
        int i = 0;
        foreach (HexCell c in visited)
        {
            neighbors[i] = c.town.GetComponent<Town>();
            i++;
        }
    }

    public void TakeDamage(int dmg)
    {
        foreach (Town town in neighbors)
        {
            Damage(town, dmg);
        }
    }

    private void Damage(Town town, int dmg)
    {
        if (town == null) return;
        town.runtime.Health -= dmg;
        if (town.runtime.Health < 0)
            town.runtime.Health = 0;

        // Animation & SFX
        Explode();
    }
}
