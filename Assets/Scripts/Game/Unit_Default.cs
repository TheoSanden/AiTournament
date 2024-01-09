using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Unit_Default : Unit
    {
        protected override Unit SelectTarget(List<Unit> enemiesInRange)
        {
            // pick a random target!
            return enemiesInRange != null && enemiesInRange.Count > 0 ? 
                   enemiesInRange[Random.Range(0, enemiesInRange.Count)] : null;
        }

        protected override void Start()
        {
            base.Start();

            StartCoroutine(StupidLogic());
        }

        IEnumerator StupidLogic()
        {
            while (true)
            {
                // wait (or take cover)
                TargetNode = null;
                yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));

                // move randomly
                TargetNode = Battlefield.Instance.GetRandomNode();
                yield return new WaitForSeconds(Random.Range(4.0f, 6.0f));
            }
        }
    }
}