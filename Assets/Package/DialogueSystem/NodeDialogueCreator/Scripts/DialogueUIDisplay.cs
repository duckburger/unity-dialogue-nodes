using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueUIDisplay : MonoBehaviour
{
    [SerializeField]
    ScriptableEvent onDialogueStarted;
    [SerializeField]
    ScriptableEvent onDialogueEnded;
    [Space(10)]
    [SerializeField]
    ConversationAsset activeConversation;
    [Space(10)]
    [Header("UI Components")]
    [SerializeField]
    RectTransform mainDialogueBox;
    [SerializeField]
    TextMeshProUGUI speakerName;
    [SerializeField]
    TextMeshProUGUI dialogueLine;
    [SerializeField]
    Image speakerIcon;
    [SerializeField]
    Transform repliesParent;
    [SerializeField]
    CanvasGroup continueButtonCG;
    [SerializeField]
    GameObject skipButton;

    [Space(10)]
    [SerializeField]
    GameObject responsePrefab;
    IDialogueNode currentNode;
    Vector2 originalBoxPosition;
    Vector2 originalRepliesPosition;
    Action onCurrentConvoCompleted = null;
    CanvasGroup mainCG;
    AudioSource audioSource;

    bool onScreen = false;


    private void Awake()
    {
        originalBoxPosition = mainDialogueBox.localPosition;
        originalRepliesPosition = repliesParent.localPosition;
        _ = AnimateOutAsync();
        mainCG = GetComponent<CanvasGroup>();
        audioSource = GetComponent<AudioSource>();
    }

    public void AnimateIn()
    {
        if (!mainDialogueBox)
        {
            Debug.LogError($"No main dialogue box connected");
            return;
        }

        onScreen = true;
        if (mainCG)
        {
            mainCG.alpha = 1;
            mainCG.blocksRaycasts = true;
            mainCG.interactable = true;
        }
        LeanTween.value(mainDialogueBox.anchoredPosition.y, 0, 0.23f).setEase(LeanTweenType.easeOutSine)
            .setOnUpdate((float val) =>
            {
                mainDialogueBox.anchoredPosition = new Vector2(mainDialogueBox.anchoredPosition.x, val);
            });
    }

    public async Task AnimateOutAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        if (!mainDialogueBox)
        {
            Debug.LogError($"No main dialogue box connected");
            return;
        }

        onScreen = false;

        LeanTween.value(mainDialogueBox.anchoredPosition.y, mainDialogueBox.anchoredPosition.y - Screen.height / 2, 0.15f).setEase(LeanTweenType.easeInSine)
            .setOnUpdate((float val) =>
            {
                mainDialogueBox.anchoredPosition = new Vector2(mainDialogueBox.anchoredPosition.x, val);
            })
            .setOnComplete(() =>
            {
                if (mainCG)
                {
                    mainCG.alpha = 0;
                    mainCG.blocksRaycasts = false;
                    mainCG.interactable = false;
                }
                tcs.TrySetResult(true);
            });
            await tcs.Task;
    }

    void AnimateRepliesIn()
    {
        if (!repliesParent)
        {
            Debug.LogError($"No replies parent connected");
            return;
        }

        LeanTween.moveLocalY(repliesParent.gameObject, originalRepliesPosition.y, 0.23f).setEase(LeanTweenType.easeOutSine);
    }

    void AnimateRepliesOut()
    {
        if (!repliesParent)
        {
            Debug.LogError($"No replies parent connected");
            return;
        }

        LeanTween.moveLocalY(repliesParent.gameObject, originalRepliesPosition.y - Screen.height / 2, 0.15f).setEase(LeanTweenType.easeOutSine)
            .setOnComplete(() =>
            {
                DestroyAllReplyNodes();
            });
    }

    public void AcceptConversationAssetFromEvent(object convoAsset)
    {
        ConversationAsset asset = (ConversationAsset)convoAsset;
        AssignActiveConversation(asset);
        if (!onScreen)
            ProcessActiveConversation();
        else
            Debug.Log("Cannot load a new conversation, while the previous one is still on screen");
    }

    public void AssignActiveConversation(ConversationAsset asset)
    {
        activeConversation = asset;
    }

    public void ProcessActiveConversation()
    {
        _ = ProcessActiveConversation(null);
    }

    public async Task ProcessActiveConversation(Action onConversationCompleted = null)
    {
        onDialogueStarted?.Raise();
        if (!activeConversation)
        {
            Debug.LogError($"No active conversation detected, can't play dialogue");
            return;
        }

        if (activeConversation.allNPCNodes.Count <= 0)
        {
            Debug.Log($"No nodes in the active conversation, can't play dialogue");
            return;
        }

        if (activeConversation.skippable)
        {
            skipButton.SetActive(true);
        }
        else
        {
            skipButton.SetActive(false);
        }

        onCurrentConvoCompleted = onConversationCompleted;

        if (!onScreen)
        {
            AnimateIn();
        }
        else
        {
            await CloseAsync();
            AnimateIn();
        }

        for (int i = 0; i < activeConversation.allNPCNodes.Count; i++)
        {
            if (activeConversation.allNPCNodes[i].incomingTransitions.Count <= 0 && activeConversation.allNPCNodes[i].outgoingTransitions.Count > 0)
            {
                // Found a node with only outbound transitions - this will be our first node
                currentNode = activeConversation.GetNPCNodyByID(activeConversation.allNPCNodes[i].id);
                DisplayLine(currentNode);
            }
        }
    }

    private void DisplayLine(IDialogueNode node)
    {
        StartCoroutine(RollOutLine(node.DialogueLine()));
    }

    IEnumerator RollOutLine(string line)
    {
        NPCDialogueNode npcNode = currentNode as NPCDialogueNode;

        if (npcNode != null)
        {
            if (audioSource && npcNode.lineSoundEffect)
            {
                audioSource.clip = npcNode.lineSoundEffect;
                audioSource.Play();
            }
            npcNode?.attachedEvent?.Raise();
        }

        if (npcNode != null)
        {
            if (npcNode.speaker && npcNode.speaker.icon && !speakerIcon)
            {
                Debug.LogError($"Connect speaker icon to the Dialogue UI Display");
            }
            speakerIcon.sprite = npcNode.speaker.icon;
            speakerName.text = npcNode.speaker.name;
        }
        dialogueLine.text = "";
        foreach (char character in line)
        {
            dialogueLine.text += character;
            yield return null;
        }

        if (currentNode.GetConnectedPlayerResponses().Count > 0)
        {
            // Display player responses
            ShowReplies(currentNode.GetConnectedPlayerResponses());
        }
        else if (currentNode.GetConnectedNPCLines().Count > 0)
        {
            ShowContinueButton(true);
        }
        else
        {
            SpawnDoneButton();
        }
    }

    void ShowReplies(List<int> replyIDs)
    {
        if (!responsePrefab)
        {
            Debug.LogError($"No response prefab connected");
            return;
        }

        for (int i = 0; i < replyIDs.Count; i++)
        {
            PlayerDialogueNode playerResponseNode = activeConversation.GetPlayerNodeByID(replyIDs[i]);

            if (playerResponseNode.outgoingTransitions.Count == 0)
            {
                SpawnButton(playerResponseNode.dialogueLine, () =>
                {
                    _ = CloseAsync();
                    onCurrentConvoCompleted?.Invoke();
                });
                continue;
            }

            NPCDialogueNode connectedNPCNode = activeConversation.GetNPCNodyByID(activeConversation.GetTransitionByID(playerResponseNode.outgoingTransitions[0]).endNPCNode.id);
            SpawnButton(playerResponseNode.dialogueLine, () =>
            {
                AnimateRepliesOut();
                IDialogueNode savedNode = connectedNPCNode;
                currentNode = savedNode;
                DisplayLine(currentNode);
            });

        }

        AnimateRepliesIn();
    }

    void SpawnButton(string buttonTitle, Action buttonAction)
    {
        GameObject spawnedReply = Instantiate(responsePrefab, Vector3.zero, Quaternion.identity, repliesParent);
        TextMeshProUGUI replyText = spawnedReply.GetComponentInChildren<TextMeshProUGUI>();
        Button replyButton = spawnedReply.GetComponent<Button>();

        replyText.text = buttonTitle;
        replyButton.onClick.AddListener(buttonAction.Invoke);
    }

    void SpawnDoneButton()
    {
        SpawnButton("Done", () =>
        {
            _ = CloseAsync();
            onCurrentConvoCompleted?.Invoke();
        });

        AnimateRepliesIn();
    }

    void ShowContinueButton(bool enabled)
    {
        if (enabled)
        {
            LeanTween.alphaCanvas(continueButtonCG, 1, 0.23f).setOnComplete(() =>
            {
                continueButtonCG.blocksRaycasts = enabled;
                continueButtonCG.interactable = enabled;
            });
        }
        else
        {
            LeanTween.alphaCanvas(continueButtonCG, 0, 0.18f).setOnComplete(() =>
            {
                continueButtonCG.blocksRaycasts = enabled;
                continueButtonCG.interactable = enabled;
            });
        }

    }

    private void DestroyAllReplyNodes()
    {
        for (int i = repliesParent.childCount - 1; i >= 0; i--)
        {
            Destroy(repliesParent.GetChild(i).gameObject);
        }
    }

    public async Task CloseAsync()
    {
        await AnimateOutAsync();
        DestroyAllReplyNodes();
        onCurrentConvoCompleted?.Invoke();
        onDialogueEnded?.Raise();
    }

    public void Continue()
    {
        // Display NPC responses
        currentNode = activeConversation.GetNPCNodyByID(currentNode.GetConnectedNPCLines()[0]);
        DisplayLine(currentNode);
        ShowContinueButton(false);
    }

}