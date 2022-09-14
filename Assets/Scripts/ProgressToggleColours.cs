/*==============================================================================
Author: James Burness
Last modified: 14 - 09 - 2022
Created for ARPLACER Honours project - University of Cape Town
==============================================================================*/

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class ProgressToggleColours : MonoBehaviour
{
    //Three checkboxes which show step progress
    public Toggle[] progressToggles;
    //Toggle for hiding/showing progress panel (in settings menu)
    public Toggle activeToggle;

    private void Awake()
    {
        //Reset to default values to start
        Reset();
    }

    private void Update()
    {
        //Seetings menu toggle is inactive before anchor is placed on ground plane
        activeToggle.interactable = FindObjectOfType<ARGuideSessionController>().GetPlaced();
        //Progress panel is hidden until anchor is placed
        gameObject.SetActive(activeToggle.isOn && FindObjectOfType<ARGuideSessionController>().GetPlaced());    
    }

    //Specific toggle (0,1,2) is toggled on or off based on the boolean argument
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void toggle(int toggle, bool b)
    {
        progressToggles[toggle].isOn = b;
        ColorBlock cb = progressToggles[toggle].colors;
        if (b)
        {
            cb.disabledColor = Color.green;
        }
        else
        {
            cb.disabledColor = Color.red;
        }
        progressToggles[toggle].colors = cb;
    }

    //Resets toggles to red and unchecked
    public void Reset()
    {
        foreach (Toggle t in progressToggles)
        {
            t.isOn = false;
            ColorBlock cb = t.colors;
            cb.disabledColor = Color.red;
            t.colors = cb;
        }
    }
}
