using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHolder : MonoBehaviour
{
    public int posX;

    public int posY;

    public int cellIndex;

    //[HideInInspector]
    public TileElement tile;

    public SawElement saw;

    public RotatorElement rotator;

    [HideInInspector]
    public TileElement tilePrefab;

    public SawElement sawPrefab;

    public RotatorElement rotatorPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadTileElement(int direction)
    {
        TileElement element = Instantiate(tilePrefab);
        element.tileHolder = this;
        element.InitTile(direction);
        element.transform.SetParent(transform);
        element.transform.localPosition = Vector3.zero;
        tile = element;
    }

    public void LoadSaw()
    {
        SawElement sawElement = Instantiate(sawPrefab);
        sawElement.tileHolder = this;
        sawElement.transform.SetParent(transform);
        sawElement.transform.localPosition = Vector3.zero;
        saw = sawElement;
    }

    public void LoadRotator(RotatorComponent rotComponent)
    {
        RotatorElement rotElement = Instantiate(rotatorPrefab);
        rotElement.tileHolder = this;
        rotElement.InitRotator(rotComponent);
        rotElement.transform.SetParent(transform);
        rotElement.transform.localPosition = Vector3.zero;
        rotator = rotElement;
    }

    
}
