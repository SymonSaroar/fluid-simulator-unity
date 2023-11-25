using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public float size = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.localScale = new Vector2(size / 0.5f, size / 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject);
    }

    public void updateSize(float size)
    {
        gameObject.transform.localScale = new Vector2(this.size / size, this.size / size);
        this.size = size;
    }
}
