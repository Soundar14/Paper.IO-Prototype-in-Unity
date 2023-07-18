using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AIEnemyBehaviour : Player
{
    bool isNextTurn;

    private void OnEnable()
    {
        isNextTurn = true;
    }    

    public override void Update()
    {

        PlayerLogicPaperMethod();

        transform.position += transform.forward * speed * Time.deltaTime;

        if(isNextTurn)
        {
            isNextTurn = false;
            StartCoroutine(AIMovement());
        }

        
    }
    IEnumerator AIMovement()
    {
        Quaternion rot = Quaternion.AngleAxis(Random.Range(70, 170), Vector3.up);
        transform.DORotateQuaternion(rot,Random.Range(0.1f,1));
        yield return new WaitForSeconds(Random.Range(0.5f, 2.5f));
        isNextTurn = true;
    }
}
