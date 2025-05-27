using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalBoss_AnimationController : MonoBehaviour
{
    [SerializeField] private Grade[] grades;
    [SerializeField] private float openingSpeed;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        for (int x = 0; x < grades.Length; x++)
        {
            if(grades[x].isOpen & grades[x].gradeState != GradeState.Open)
            {
                grades[x].gradeState = GradeState.Open;
                if (grades[x].openCoroutine != null)
                    StopCoroutine(grades[x].openCoroutine);
                StartCoroutine(grades[x].openCoroutine = OpenGrade(grades[x]));
                continue;
            }
            if(!grades[x].isOpen & grades[x].gradeState != GradeState.Closed)
            {
                grades[x].gradeState = GradeState.Closed;

                if (grades[x].openCoroutine != null)
                    StopCoroutine(grades[x].openCoroutine);
                StartCoroutine(grades[x].openCoroutine = OpenGrade(grades[x]));
                continue;
            }
        }
    }

    private IEnumerator OpenGrade(Grade grade)
    {
        Vector3 targetRotation = grade.isOpen? grade.openRotation : Vector3.zero;
        Vector3 startRotation = grade.isOpen? Vector3.zero: grade.openRotation;

        Debug.Log("abre fecha " + grade.isOpen + " - " + startRotation + " ---> " + targetRotation);

        float lerp = 0;
        while (lerp < 1)
        {
            Vector3 rotation = Vector3.Lerp(startRotation, targetRotation, lerp);
            lerp += Time.deltaTime * openingSpeed;
            grade.gradeTransform.localEulerAngles = rotation;

            yield return new WaitForEndOfFrame();
        }
    }
}

public enum GradeState
{
    Closed,
    Open
}

[Serializable]
public struct Grade
{
    public string side;
    public Transform gradeTransform;
    public bool isOpen;
    [NonSerialized] public GradeState gradeState;
    public Vector3 openRotation;

    [NonSerialized] public IEnumerator openCoroutine;
}
