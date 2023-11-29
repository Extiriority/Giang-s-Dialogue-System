using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public sealed class GDSGroup : Group
{
    public string ID { get; set; }
    public string oldTitle { get; set; }

    private readonly Color defaultBorderColor;
    private readonly float defaultBorderWidth;

    public GDSGroup(string groupTitle, Vector2 position)
    {
        ID = Guid.NewGuid().ToString();

        title = groupTitle;
        oldTitle = groupTitle;

        SetPosition(new Rect(position, Vector2.zero));

        defaultBorderColor = contentContainer.style.borderBottomColor.value;
        defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
    }

    public void setErrorStyle(Color color)
    {
        contentContainer.style.borderBottomColor = color;
        contentContainer.style.borderBottomWidth = 2f;
    }

    public void resetStyle()
    {
        contentContainer.style.borderBottomColor = defaultBorderColor;
        contentContainer.style.borderBottomWidth = defaultBorderWidth;
    }
}