using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerscript : MonoBehaviour
{

    public float Speed;
    Rigidbody rb;
    Vector3 Movement;
    public GameObject Block;
    public GameObject Spawner;
    public Gridscript gs;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gs = Camera.main.GetComponent<Gridscript>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Move(h, v);
        Ins();
        Rotate();
    }

    void Move(float h, float v)
    {
        Movement.Set(h, 0, v);
        Movement = Movement.normalized * Speed * Time.deltaTime;
        rb.MovePosition(transform.position + Movement);    
    }

    void Ins()
    {
        Vector3 obj = Spawner.transform.position;

        if (Input.GetKey(KeyCode.Space))
        {
            int cn = gs.pos2Cell(roundVec3(obj));
            if (gs.world[cn] != Gridscript.TileType.Block)
            {
                gs.world[cn] = Gridscript.TileType.Block;
                Instantiate(Block, roundVec3(obj), Quaternion.identity);
            }
        }
    }

    public static Vector3 roundVec3(Vector3 v)
    {
        return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z)) + new Vector3(0.5f, 0, 0.5f);
    }

    void Rotate()
    {
        Quaternion obj = gameObject.transform.rotation;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.rotation = Quaternion.Euler(0, 270, 0);
        }
    }
}
