/*==============================================================================
Author: James Burness
Last modified: 14 - 09 - 2022
Created for ARPLACER Honours project - University of Cape Town
==============================================================================*/

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Vuforia;

public class ARGuideSessionController : MonoBehaviour
{

    //Public
    //Buttons and toggles
    public GameObject[] box_shells;
    public GameObject[] UI_boxes;
    public Toggle greenShellToggle;
    public Toggle progressPanelToggle;
    public TMP_Dropdown difficulty;
    public Button resetButton;

    //Indicators and objects for ground plane
    public GameObject easyIndicator;
    public GameObject mediumIndicator;
    public GameObject hardIndicator;
    public GameObject groundPlane;
    public GameObject planeFinder;

    [SerializeField] private Text onScreenMessage;

    //Private
    private int step_number;
    private bool placed;
    private int difficultyVal;
    private int[] steps;
    private int step_idx;

    //Set on-screen Messages
    private const string START_MSG = "Welcome to ARPLACER\nAim the camera at the ground";
    private const string SEARCH_MSG = "Point the camera at the required box";
    private const string PLACEMENT_MSG = "Tap the screen to anchor the target";
    private const string COMPLETE_MSG = "Assembly complete";

    //Set POSE configurations for the 3 box structures
    private readonly Pose[] EASY = { new Pose(new Vector3(0f, 0.135f, 0.2f), new Quaternion(0f,-0.707106829f,-0.707106829f,0f)),
                                    new Pose(new Vector3(0.29f,0.1625f,0.115f), new Quaternion(-0.5f,-0.5f,-0.5f,0.5f)),
                                    new Pose(new Vector3(-0.3f,0.1625f,0.057f), new Quaternion(0.405579805f,-0.579227984f,-0.579227984f,-0.405579805f)),
                                    new Pose(new Vector3(0.195f,0.0825f,-0.216f), new Quaternion(0f,0.42261827f,0f,0.906307876f)),
                                    new Pose(new Vector3(-0.136f,0.13f,-0.237f), new Quaternion(0.270598054f,-0.65328151f,-0.65328151f,-0.270598054f))
                                    };

    //First pose is HARD's pose for box 4 - box 4 is not shown.
    private readonly Pose[] MEDIUM = { new Pose(new Vector3(0f, 0.135f, 0.2f), new Quaternion(0f, -0.707106829f, -0.707106829f, 0f)),
                                    new Pose(new Vector3(0.115f,0.1625f,0f), new Quaternion(-0.5f,-0.5f,-0.5f,0.5f)),
                                    new Pose(new Vector3(-0.12f,0.1625f,0f), new Quaternion(-0.5f,-0.5f,-0.5f,0.5f)),
                                    new Pose(new Vector3(0f,0.6675f,0f), new Quaternion(0f,0.42261827f,0f,0.906307876f)),
                                    new Pose(new Vector3(0f,0.455f,0f), new Quaternion(0.270598054f,-0.65328151f,-0.65328151f,-0.270598054f))
                                    };

    private readonly Pose[] HARD = { new Pose(new Vector3(0f, 0.135f, 0.2f), new Quaternion(0f, -0.707106829f, -0.707106829f, 0f)),
                                    new Pose(new Vector3(0.25f,0.1625f,0.1f), new Quaternion(-0.241844729f,-0.664463043f,-0.664463043f,0.241844729f)),
                                    new Pose(new Vector3(-0.25f,0.1625f,0.1f), new Quaternion(0.241844788f,-0.664463043f,-0.664463043f,-0.241844788f)),
                                    new Pose(new Vector3(0f,0.4075f,0.125f), new Quaternion(0,0,0,1)),
                                    new Pose(new Vector3(0f,0.62f,0.125f), new Quaternion(0.270598054f,-0.65328151f,-0.65328151f,-0.270598054f))
                                    };

    //Start with default values
    private void Awake()
    {
        step_idx = -1;
        difficultyVal = 0;
        Reset();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public int GetStep()
    {
        return step_number;   
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void NextStep()
    {
        //If number of steps exhausted - completed
        if (step_idx >= steps.Length) return;
        
        //Otherwise go to next step - hide red, show green for old step
        box_shells[step_number].transform.GetChild(0).gameObject.SetActive(false);
        box_shells[step_number].transform.GetChild(1).gameObject.SetActive(greenShellToggle.isOn);
        //Increment step number to next step (box value)
        ++step_idx;

        //Check index is valid (still another step to come)
        if(step_idx < steps.Length)
        {
            //Update step number
            step_number = steps[step_idx];
            //Set red target of next step to visible
            box_shells[step_number].transform.GetChild(0).gameObject.SetActive(true);
            //Reset colour toggles if possible
            if (progressPanelToggle.isOn) FindObjectOfType<ProgressToggleColours>().Reset();
            //Reset on screen message to first message of step
            onScreenMessage.text = SEARCH_MSG;
        }
        else
        {
            //Assembly complete
            step_number = box_shells.Length;
            onScreenMessage.text = COMPLETE_MSG;
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Reset()
    {
        //Start new run - waiting for target to be anchored (default values)
        placed = false;
        greenShellToggle.interactable = placed;
        onScreenMessage.text = START_MSG;
        
        //Check which difficulty the new restart will run
        switch (difficulty.value)
        {
            case 0:
                SetupEasy();
                break;
            case 1:
                SetupMedium();
                break;
            case 2:
                SetupHard();
                break;
            default:
                SetupHard();
                break;
        }

        //Show new base outline for target
        ShowOutline(difficultyVal);
        //Default values once step list has been set for difficulty
        step_idx = 0;
        step_number = steps[step_idx];

        //Hide green and red targets
        foreach (GameObject go in box_shells)
        {
            go.transform.GetChild(0).gameObject.SetActive(false);
            go.transform.GetChild(1).gameObject.SetActive(false);
        }
        //Set first step red target to visible
        box_shells[step_number].transform.GetChild(0).gameObject.SetActive(true);
    }

    //Refresh the green-target visibility asynchronously when setting changed
    public void StepRefresh()
    {
        step_idx = Mathf.Min(step_idx, steps.Length);
        for (int x = step_idx-1; x >= 0; x--)
        {
            box_shells[steps[x]].transform.GetChild(1).gameObject.SetActive(greenShellToggle.isOn);
        }
    }

    //Set new message once ground plane detected
    public void GroundPlaneFound()
    {
        onScreenMessage.text = PLACEMENT_MSG;
    }

    //Set shared placed variable once anchor is placed
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void SetPlaced()
    {
        placed = true;
        greenShellToggle.interactable = placed;
        onScreenMessage.text = SEARCH_MSG;

    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool GetPlaced()
    {
        return placed;
    }

    //Setup method for assigning new poses to targets and new step list for ordering of targets
    private void SetupEasy()
    {
        for (int p = 0; p < EASY.Length; p++)
        {
            box_shells[p].transform.localPosition = EASY[p].position;
            box_shells[p].transform.localRotation = EASY[p].rotation;
        }
        steps = new int[] { 4,3,2,1,0};
        step_number = steps[0];

        //GROUND INDICATOR
        FindObjectOfType<PlaneFinderBehaviour>().PlaneIndicator = easyIndicator;
    }

    //Setup method for assigning new poses to targets and new step list for ordering of targets
    private void SetupMedium()
    {
        for (int p = 0; p < MEDIUM.Length; p++)
        {
            box_shells[p].transform.localPosition = MEDIUM[p].position;
            box_shells[p].transform.localRotation = MEDIUM[p].rotation;
        }
        steps = new int[] { 1, 2, 4, 3 };
        step_number = steps[0];

        //GROUND INDICATOR
        FindObjectOfType<PlaneFinderBehaviour>().PlaneIndicator = mediumIndicator;
    }

    //Setup method for assigning new poses to targets and new step list for ordering of targets
    private void SetupHard()
    {
        for (int p = 0; p < HARD.Length; p++)
        {
            box_shells[p].transform.localPosition = HARD[p].position;
            box_shells[p].transform.localRotation = HARD[p].rotation;
        }
        steps = new int[]{0, 1, 2, 3, 4};
        step_number = steps[0];

        //GROUND INDICATOR
        FindObjectOfType<PlaneFinderBehaviour>().PlaneIndicator = hardIndicator;
    }

    //Change the difficulty of the structure and trigger a reset
    public void NewDifficulty()
    {
        if (difficulty.value != difficultyVal)
        {
            difficultyVal = difficulty.value;
            resetButton.onClick.Invoke();
        }
    }

    //Activating required indicator and target outline
    private void ShowOutline(int v)
    {
        groundPlane.transform.GetChild(0).gameObject.SetActive(false);
        groundPlane.transform.GetChild(1).gameObject.SetActive(false);
        groundPlane.transform.GetChild(2).gameObject.SetActive(false);
        groundPlane.transform.GetChild(v).gameObject.SetActive(true);

        planeFinder.transform.GetChild(0).gameObject.SetActive(false);
        planeFinder.transform.GetChild(1).gameObject.SetActive(false);
        planeFinder.transform.GetChild(2).gameObject.SetActive(false);
        planeFinder.transform.GetChild(v).gameObject.SetActive(true);
    }
}
