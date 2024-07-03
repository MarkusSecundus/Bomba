using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkingEffect : MonoBehaviour
{
    [SerializeField] int BlinkCount = 3;
    [SerializeField] float VisiblePerBlinkSeconds = 0.6f;
    [SerializeField] float  HiddenPerBlinkSeconds = 0.6f;
    [SerializeField] float EndVisibilityDuration = -1f;

    [SerializeField] bool _runOnStart = false;

    void Start()
    {
        if (_runOnStart) RunEffect();    
    }

    bool isRunning = false;
    public void RunEffect()
    {
        if (isRunning)
        {
            Debug.LogWarning($"Blinking effect is already running!", this);
            this.StopAllCoroutines();
        }
        isRunning = true;
        this.gameObject.SetActive(true);
        var hideable = GetComponentsInChildren<Graphic>();

        StartCoroutine(impl());
        IEnumerator impl()
        {
            for(int t = 0; t < BlinkCount; ++t)
            {
                foreach (var h in hideable) h.enabled = true;
                yield return new WaitForSeconds(VisiblePerBlinkSeconds);
                foreach (var h in hideable) h.enabled = false;
                yield return new WaitForSeconds(VisiblePerBlinkSeconds);
            }
            foreach (var h in hideable) h.enabled = true;
            if (EndVisibilityDuration > 0f)
            {
                yield return new WaitForSeconds(EndVisibilityDuration);
                foreach (var h in hideable) h.enabled = false;
            }

            this.isRunning = false;
            //this.gameObject.SetActive(EndVisibilityDuration >= 0);
        }
    }
}
