using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GDSErrorData
{
    public Color Color { get; set; }

    public GDSErrorData()
    {
        generateRandomColor();
    }

    private void generateRandomColor()
    {
        Color = new Color32(
            (byte) Random.Range(65, 256),
            (byte) Random.Range(50, 176),
            (byte) Random.Range(50, 176),
            255
        );
    }
}
