using UnityEngine;
using MiniStrategy;
using System.Collections;

public class CombatMan : MonoBehaviour
{
    [Range(0, 1000)]
    public int delayMS = 100;

    private RealTimeSequence sequence;
    private bool running;

    private void Start()
    {
        sequence = new RealTimeSequence { delayMS = 0 };
    }

    private void FixedUpdate()
    {
        if (sequence.nWait > 0 && !running)
        {
            StartCoroutine(NextCoro());
        }
    }

    IEnumerator NextCoro()
    {
        running = true;
        yield return new WaitForSeconds(delayMS / 1000f);
        sequence.NextAction();
        running = false;
    }

    public void RegisterAction(ActionBase action)
    {
        if (sequence == null) return;
        sequence.Register(action);
    }
}
