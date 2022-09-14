/*==============================================================================
Author: James Burness
Last modified: 14 - 09 - 2022
Created for ARPLACER Honours project - University of Cape Town
==============================================================================*/

using UnityEngine;
using UnityEngine.UI;

public class BoxTargetBehaviour : MonoBehaviour
{
    //public
    //Differentiaitng variables between each target
    public GameObject targetParent;
    public int desiredStep;

    [SerializeField] private Text onScreenMessage;

    //private
    //Status values for control
    private int currentStep;
    private bool upright, position, rotation, found, nearby;

    //Set messages for on-screen message text
    private const string POSITION_MSG = "Place the required box inside the red target";
    private const string ROTATE_CLOCK_MSG = "Rotate the box clockwise";
    private const string ROTATE_COUNTER_MSG = "Rotate the box counter-clockwise";
    private const string SEARCH_MSG = "Point the camera at the required box";

    //Reset to default values when application starts
    private void Awake()
    {
        upright = false; position = false; rotation = false; found = false; nearby = false;
        currentStep = 0;
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(true);
    }

    private void Update()
    {
        //Check if current step requires this box target
        currentStep = FindObjectOfType<ARGuideSessionController>().GetStep();
        if ((currentStep == desiredStep) && (FindObjectOfType<ARGuideSessionController>().GetPlaced()))
        {
            //Make sure occlusion for the current multitarget is active
            transform.GetChild(4).gameObject.SetActive(true);
            //Enable target corner indicators 
            transform.GetChild(5).gameObject.SetActive(true);

            //Checked upright position (x:270+-10)
            upright = CheckUpright();
            if (upright) onScreenMessage.text = POSITION_MSG;
            //Toggle progress toggle colour based on upright value
            FindObjectOfType<ProgressToggleColours>().toggle(0, upright);
            //Set black arrow child based on upright value
            transform.GetChild(1).gameObject.SetActive(!upright);

            //Check position in relation to camera
            position = CheckPosition();
            FindObjectOfType<ProgressToggleColours>().toggle(1, position);
            transform.GetChild(0).gameObject.SetActive(!position);
            
            //Rotation arrows shown if nearby to correct position
            nearby = ((Mathf.Abs(transform.position.x - targetParent.transform.position.x) <= 0.05f)
                && (Mathf.Abs(transform.position.y - targetParent.transform.position.y) <= 0.06f)
                && (Mathf.Abs(transform.position.z - targetParent.transform.position.z) <= 0.05f));
            
            //Check rotation in relation to target
            rotation = CheckRotation();
            FindObjectOfType<ProgressToggleColours>().toggle(2, rotation);

            if (position && rotation) CorrectPlacement();
        }
        else
        {
            //Not correct step
            OffTurn();
        }
    }

    //Round to nearest 2 decimals
    private float Rnd(float f)
    {
        return Mathf.Round(f * 100.0f) / 100.0f;
    }

    //When box judged as corrrectly placed - next step
    private void CorrectPlacement()
    {
        FindObjectOfType<ARGuideSessionController>().NextStep();
    }

    //Dynamically control arrow pointing direction.  
    private bool ArrowPointBehaviour()
    {
        //Hide arrow if box is near (8cm) the target
        if (Mathf.Abs(transform.position.x - targetParent.transform.position.x) <= 0.08f)
        {
            if (Mathf.Abs(transform.position.y - targetParent.transform.position.y) <= 0.08f)
            {
                if (Mathf.Abs(transform.position.z - targetParent.transform.position.z) <= 0.08f)
                {
                    transform.GetChild(0).gameObject.SetActive(false);
                    return true;
                }
            }
        }
        //Arrow always points to look at target position.
        if (!transform.GetChild(0).gameObject.activeSelf) transform.GetChild(0).gameObject.SetActive(true);
        Vector3 toTarget = targetParent.transform.position - transform.position;
        transform.GetChild(0).gameObject.transform.rotation = Quaternion.LookRotation(toTarget);
        return false;
    }

    //Return boolean of whether the box is upright or not
    private bool CheckUpright()
    {
        //Step 4 (index 3) doesn't have a 270* rotation to be upright
        if (targetParent.transform.localEulerAngles.x == 0) return ((transform.rotation.eulerAngles.x > 350.0f) || (transform.rotation.eulerAngles.x < 10.0f));
        else return (Mathf.Abs(transform.rotation.eulerAngles.x - targetParent.transform.localEulerAngles.x) <= 10.0f);
    }

    private bool CheckPosition()
    {
        //Update arrow
        ArrowPointBehaviour();
        //Check correct x,y,z position
        if ((Mathf.Abs(transform.position.x - targetParent.transform.position.x) <= 0.03f)
            && (Mathf.Abs(transform.position.y - targetParent.transform.position.y) <= 0.04f)
            && (Mathf.Abs(transform.position.z - targetParent.transform.position.z) <= 0.03f)) return true;
        return false;
    }

    private bool CheckRotation()
    {
        //Calculate most acute direction of rotation to orient box with target
        float orbit = (transform.rotation.eulerAngles.y + transform.rotation.eulerAngles.z + 180) % 360;
        float targetAngle = (targetParent.transform.rotation.eulerAngles.y + targetParent.transform.rotation.eulerAngles.z) % 360;

        bool clockwise = nearby && !(orbit < targetAngle);
        bool counter = nearby && (orbit < targetAngle);
        transform.GetChild(2).gameObject.SetActive(clockwise);
        transform.GetChild(3).gameObject.SetActive(counter);
        //Screen Message update
        if(clockwise) onScreenMessage.text = ROTATE_CLOCK_MSG;
        if (counter) onScreenMessage.text = ROTATE_COUNTER_MSG;

        //Angle between object and target facing direction
        float angle = Rnd(Quaternion.Angle(transform.rotation, targetParent.transform.rotation));
        //Current diff angle is 10* (5x2)
        if (angle <= 5.0f) return true;
        else return false;
    }

    //Keep all children/indicators hidden when not the correct step
    private void OffTurn()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(2).gameObject.SetActive(false);
        transform.GetChild(3).gameObject.SetActive(false);
        transform.GetChild(4).gameObject.SetActive(false);
        transform.GetChild(5).gameObject.SetActive(false);
    }

    //Change on screen message when target is found, if correct step
    public void Found() { 
        if (currentStep == desiredStep)
        {
            found = true;
            onScreenMessage.text = POSITION_MSG;
        }
    }

    //Change on screen message when target is lost, if correct step
    public void Lost() { 
        if ((currentStep == desiredStep) && found)
        {
            found = false;
            onScreenMessage.text = SEARCH_MSG;
        }
    }
}   
