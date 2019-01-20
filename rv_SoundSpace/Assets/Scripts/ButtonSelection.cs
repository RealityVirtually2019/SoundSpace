using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSelection : MonoBehaviour
{
    private Color32 NotSelectedColor = new Color32(106, 106, 106, 255);
    private Color32 SelectedColor = new Color32(255, 0, 255, 255);

    public void OnClick()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;
        Debug.Log(name);
    }
}
