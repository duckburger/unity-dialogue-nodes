using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class DialogueUIDisplay : MonoBehaviour
{
    [SerializeField] ConversationAsset activeConversation;
    [Space(10)]
    [Header("UI Components")]
    [SerializeField] RectTransform mainDialogueBox;
    [SerializeField] TextMeshProUGUI speakerName;
    [SerializeField] TextMeshProUGUI dialogueLine;
    [SerializeField] Image speakerIcon;
    [SerializeField] Transform repliesParent;
    [SerializeField] CanvasGroup continueButtonCG;

    [Space(10)]
    [SerializeField] GameObject responsePrefab;
    IDialogueNode currentNode;
    Vector2 originalBoxPosition;
    Vector2 originalRepliesPosition;

    bool onScreen = false;

#region Awake / Start

    private void Awake()
    {
        originalBoxPosition = mainDialogueBox.localPosition;
        originalRepliesPosition = repliesParent.localPosition;
        AnimateOut();
    }

#endregion


#region Animate In / Out

    public void AnimateIn()
    {
        if (!mainDialogueBox)
        {
            Debug.LogError($"No main dialogue box connected");
            return;
        }

        onScreen = true;

        LeanTween.moveLocalY(mainDialogueBox.gameObject, originalBoxPosition.y, 0.23f).setEase(LeanTweenType.easeOutSine);
    }

    public void AnimateOut()
    {
        if (!mainDialogueBox)
        {
            Debug.LogError($"No main dialogue box connected");
            return;
        }

        onScreen = false;

        LeanTween.moveLocalY(mainDialogueBox.gameObject, originalBoxPosition.y - Screen.height / 2, 0.15f).setEase(LeanTweenType.easeInSine);
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

#endregion

#region Processing Conversation Asset

    public void ProcessActiveConversation()
    {
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

        if (!onScreen)
            AnimateIn();

        for (int i = 0; i < activeConversation.allNPCNodes.Count; i++)
        {
            if (activeConversation.allNPCNodes[i].incomingTransitions.Count <= 0 && activeConversation.allNPCNodes[i].outgoingTransitions.Count > 0)
            {
                // Found a node with only outbound transitions - this will be our first node
                currentNode = activeConversation.GetNPCNodyByID( activeConversation.allNPCNodes[i].id );
                
                DisplayLine(currentNode);
            }
        }
    }

    void DisplayLine(IDialogueNode node)
    {
        StartCoroutine(RollOutLine(node.DialogueLine()));
    }   


    IEnumerator RollOutLine(string line)
    {
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
        SpawnButton("Done", Close);

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


#endregion


    void DestroyAllReplyNodes()
    {
        for (int i = repliesParent.childCount - 1; i >= 0; i--)
        {
            Destroy(repliesParent.GetChild(i).gameObject);
        }
    }

    public void Close()
    {
        AnimateOut();
        DestroyAllReplyNodes();
    }

    public void Continue()
    {
        // Display NPC responses
        currentNode = activeConversation.GetNPCNodyByID(currentNode.GetConnectedNPCLines()[0]);
        DisplayLine(currentNode);
        ShowContinueButton(false);
    }

}
