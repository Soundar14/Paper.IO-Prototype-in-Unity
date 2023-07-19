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
        gameManagerRef = FindAnyObjectByType<GameManager>();
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
    public override void OnTriggerEnter(Collider other)
    {
        PlayerArea characterArea = other.GetComponent<PlayerArea>();


        if (characterArea && characterArea != area && !attackedCharacters.Contains(characterArea.player))
        {
            attackedCharacters.Add(characterArea.player);
        }

        //Debug.Log("Trigger Entered :" + other.name);
        

        if (other.gameObject.layer == 8)
        {
            Debug.Log("This Enemy thing is problem " + other.name);
            
            characterArea = other.transform.parent.GetComponent<PlayerArea>();
            characterArea.player.Die();
        }
    }
    private void OnDisable()
    {
        if(gameManagerRef.EnemyCount > 0)
        {
            gameManagerRef.EnemyCount--;            
        }
    }
}
