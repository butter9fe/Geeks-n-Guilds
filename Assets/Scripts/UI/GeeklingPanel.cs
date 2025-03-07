using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using AYellowpaper.SerializedCollections;
using static GeeklingManager;
using TMPro;
using UnityEngine.SceneManagement;
using PlayFab.ClientModels;

public class GeeklingPanel : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup eggCanvasGroup;

    [SerializeField]
    private List<GameObject> cracks;
    [SerializeField]
    private GameObject godray;

    [SerializeField]
    private GameObject geekling;
    [SerializeField]
    private GameObject nextButton;

    [SerializeField]
    [SerializedDictionary("Key", "Models")]
    private SerializedDictionary<string, CharacterModel> characterModels;
    [SerializeField]
    [SerializedDictionary("Key", "Models")]
    private SerializedDictionary<int, OutfitModel> outfitModels;

    [SerializeField]
    private TMP_Text characterText, characterBio;


    int currSelectedHelmetIndex = 0;
    int currCrackIndex = 0;

    private float shakeDetectionThreshold = 2f;
    private float accelerometerUpdateInterval = 1f / 60f;
    private float lowPassKernelWidthInSeconds = 1f;
    private float lowPassFilterFactor;
    private Vector3 lowPassValue;

    public void UpdateCharacter()
    {
        CloudScriptManager.Instance.ExecGetExpertise(expertise =>
        {
            if (!characterModels.ContainsKey(expertise))
            {
                Debug.LogWarning("No experience found!");
                return;
            }

            foreach (var model in characterModels)
            {
                model.Value.head.SetActive(model.Key == expertise);
                model.Value.leftArm.SetActive(model.Key == expertise);
                model.Value.rightArm.SetActive(model.Key == expertise);
                model.Value.body.SetActive(model.Key == expertise);
            }

            characterText.text = expertise switch
            {
                "Software Engineer" => "Byte-Bear",
                "Hardware Engineer" => "Canine Envoy",
                _ => "Stalwart Bill-der"
            };

            // Update helmet
            CloudScriptManager.Instance.ExecGetHelmetIndex(helmet =>
            {
                UpdateHelmet(helmet);

            }, e => Debug.LogError(e));
        }, e => Debug.LogError(e));
    }

    private void Start()
    {
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        shakeDetectionThreshold *= shakeDetectionThreshold;
        lowPassValue = Input.acceleration;
    }

    private void Update()
    {
        if (Input.GetKeyDown("space")) // For debugging!
            OnNextCrack();

        Vector3 acceleration = Input.acceleration;
        lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
        Vector3 deltaAccel = acceleration - lowPassValue;

        if (deltaAccel.sqrMagnitude >= 0.1f && eggCanvasGroup.transform.localScale.x < 1.5f)
            eggCanvasGroup.transform.localScale *= 1.01f;

        // Shake Detection
        if (deltaAccel.sqrMagnitude >= shakeDetectionThreshold)
            OnNextCrack();
    }

    private void OnNextCrack()
    {
        Debug.LogWarning("CRACK");
        if (currCrackIndex > 3)
        {
            return;
        }
        else if (currCrackIndex == 3)
        {
            OnCrackedEgg();
        }
        else
        {
            cracks[currCrackIndex].SetActive(true);
            Handheld.Vibrate();
        }

        currCrackIndex++;
    }

    private void OnCrackedEgg()
    {
        godray.gameObject.SetActive(true);
        geekling.gameObject.SetActive(true);

        eggCanvasGroup.DOFade(0f, 0.5f)
            .OnComplete(() =>
            {
                nextButton.gameObject.SetActive(true);
                characterText.gameObject.SetActive(true);
                characterBio.gameObject.SetActive(true);
            });
    }

    public void UpdateHelmet(int index)
    {
        currSelectedHelmetIndex = index;
        foreach (var model in outfitModels)
        {
            if (model.Value.hat == null)
                continue;

            model.Value.hat.gameObject.SetActive(model.Key == index);
            model.Value.body.gameObject.SetActive(false);
        }
    }
}
