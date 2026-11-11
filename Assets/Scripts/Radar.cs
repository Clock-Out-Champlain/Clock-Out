using UnityEngine;
using UnityEngine.UI;

public class Radar : MonoBehaviour
{
    [Header("Points")]
    public GameObject point1;
    public GameObject point2A;
    public GameObject point2B;
    public GameObject point3;
    public GameObject point4;
    public GameObject point4S;
    public GameObject pointFinal;
    public GameObject pointOffice;
    [Header("Active Point Array")]
    string[] activePoints = new string[] { "0", "0", "0", "0" };

    public void Update()
    {
        var ram = FindAnyObjectByType<MovementAI>();
        var mal = FindAnyObjectByType<MalMovementAI>();
        //var eye = FindAnyObjectByType<EyeMovementAI>();
        var clock = FindAnyObjectByType<ClockMovementAI>();
        ClearPoints();
        // Rampage current point
        activePoints[0] = ram.currentPoint;
        // Malwerewolf current point
        activePoints[1] = mal.currentPoint;
        // Eyesore current point
        //activePoints[2] = eye.currentPoint;
        // Clock'N current point
        activePoints[3] = clock.currentPoint;
        for (int i = 0; i < activePoints.Length; i++)
        {
            string tempPointString = activePoints[i];
            switch (tempPointString)
            {
                case "1":
                    ChangePoint1(true);
                    break;
                case "2A":
                    ChangePoint2A(true);
                    break;
                case "2B":
                    ChangePoint2B(true);
                    break;
                case "3":
                    ChangePoint3(true);
                    break;
                case "4":
                    ChangePoint4(true);
                    break;
                case "4s":
                    ChangePoint4S(true);
                    break;
                case "Final":
                    ChangePointFinal(true);
                    break;
                case "Office":
                    ChangePointOffice(true);
                    break;

            }
        }
    }
    public void ClearPoints()
    {
        ChangePoint1(false);
        ChangePoint2A(false);
        ChangePoint2B(false);
        ChangePoint3(false);
        ChangePoint4(false);
        ChangePoint4S(false);
        ChangePointFinal(false);
        ChangePointOffice(false);
    }
    public void ChangePoint1(bool active)
    {
        if (active)
        {
            point1.SetActive(true);
        }
        else
        {
            point1.SetActive(false);
        }
    }

    public void ChangePoint2A(bool active)
    {
        if (active)
        {
            point2A.SetActive(true);
        }
        else
        {
            point2A.SetActive(false);
        }
    }

    public void ChangePoint2B(bool active)
    {
        if (active)
        {
            point2B.SetActive(true);
        }
        else
        {
            point2B.SetActive(false);
        }
    }

    public void ChangePoint3(bool active)
    {
        if (active)
        {
            point3.SetActive(true);
        }
        else
        {
            point3.SetActive(false);
        }
    }

    public void ChangePoint4(bool active)
    {
        if (active)
        {
            point4.SetActive(true);
        }
        else
        {
            point4.SetActive(false);
        }
    }

    public void ChangePoint4S(bool active)
    {
        if (active)
        {
            point4S.SetActive(true);
        }
        else
        {
            point4S.SetActive(false);
        }
    }

    public void ChangePointFinal(bool active)
    {
        if (active)
        {
            pointFinal.SetActive(true);
        }
        else
        {
            pointFinal.SetActive(false);
        }
    }

    public void ChangePointOffice(bool active)
    {
        if (active)
        {
            pointOffice.SetActive(true);
        }
        else
        {
            pointOffice.SetActive(false);
        }
    }
}
