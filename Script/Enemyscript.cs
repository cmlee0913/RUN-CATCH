using UnityEngine;
using System.Collections;


public class Enemyscript : MonoBehaviour
{
    Gridscript gs = null;
    Coroutine move_coroutine = null;
    public GameObject player;
    public GameObject text;

    // Use this for initialization
    void Start()
    {

        gs = Camera.main.GetComponent<Gridscript>() as Gridscript;
        gs.BuildWorld();
    }

    // Update is called once per frame
    void Update()
    {
        if (move_coroutine != null) StopCoroutine(move_coroutine);
        move_coroutine = StartCoroutine(gs.Move(gameObject, player.transform.position));
    }

    private void OnCollisionEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            text.SetActive(true);
        }
    }
}