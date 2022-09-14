/*==============================================================================
Author: James Burness
Last modified: 14 - 09 - 2022
Created for ARPLACER Honours project - University of Cape Town
==============================================================================*/

using UnityEngine;
using UnityEngine.UI;

public class UIBoxBehaviour : MonoBehaviour
{
    //Settings menu checkbox to toggle the model on/off
    public Toggle UItoggle;
    //Background circle (to hide or show with the model)
    private GameObject bgCircle;
    
    void Start()
    {
        bgCircle = transform.GetChild(transform.childCount - 1).gameObject;
        Clear();
    }

    private void Update()
    {
        //Seetings menu toggle is inactive before anchor is placed on ground plane
        UItoggle.interactable = FindObjectOfType<ARGuideSessionController>().GetPlaced();

        //Check if steps have started and not all steps have been completed
        int currentStep = FindObjectOfType<ARGuideSessionController>().GetStep();
        if ((!FindObjectOfType<ARGuideSessionController>().GetPlaced()) || (currentStep > 4))
        {
            Clear();
            return;
        }
        bgCircle.SetActive(UItoggle.isOn);
        //Clear all boxes
        Reset();
        //Only show current step's model
        transform.GetChild(currentStep).gameObject.SetActive(UItoggle.isOn);
    }
    
    //Hide all box models but leave background shown (for changing step)
    public void Reset()
    {
        //Reset all model representations to invisible
        for(int child = 0; child < transform.childCount - 1; child++)
        {
            transform.GetChild(child).gameObject.SetActive(false);
        }
    }

    //Hide all box models inlcuding background
    private void Clear()
    {
        Reset();
        bgCircle.SetActive(false);
    }
}
