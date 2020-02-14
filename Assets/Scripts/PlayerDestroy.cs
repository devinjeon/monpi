using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDestroy : MonoBehaviour
{
    private GameController gameController;
    public GameObject explosion;
    private bool isSafe;

    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").
            GetComponent<GameController>();
        isSafe = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Not Conflict only within a safezone
        if (other.tag == "SafeZone")
            isSafe = true;

        if (isSafe == true)
            return;

        if (other.tag.Contains("Enemy") == true)
        {
            GameObject enemyExplosion = other.GetComponent<Enemy>().explosion;
            Instantiate(enemyExplosion,
                    other.transform.position,
                    other.transform.rotation
            );
            Destroy(other.gameObject);
        }

        Instantiate(explosion,
                gameObject.transform.position,
                gameObject.transform.rotation
        );

        Destroy(gameObject);
        gameController.GameOver(false);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "SafeZone")
            isSafe = false;
    }
}
