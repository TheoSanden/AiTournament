using Graphs;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR;

namespace Game
{
    public class Link_Normal : Battlefield.Link
    {
        #region Properties

        #endregion

        public Link_Normal(Battlefield.Node source, Battlefield.Node target) : base(source, target)
        {
        }

        public override IEnumerator MoveUnit(Unit unit)
        {
            float fProgress = 0.0f;
            Quaternion qTarget = Quaternion.LookRotation(Vector3.Normalize(Target.WorldPosition - Source.WorldPosition));

            while(fProgress < 1.0f) 
            {
                // calculate move
                float fMoveSpeed = Mathf.Lerp(Source.UnitMoveSpeed, Target.UnitMoveSpeed, fProgress);
                float fMove = fMoveSpeed * Time.deltaTime; 
                fProgress += fMove;

                // move unit
                unit.transform.position = Vector3.Lerp(Source.WorldPosition, Target.WorldPosition, fProgress);
                unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, qTarget, Time.deltaTime * 3.0f);
                yield return null;
            }
        }
    }
}