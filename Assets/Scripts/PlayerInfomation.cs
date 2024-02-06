using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
public class PlayerInfomation : MonoBehaviour
{
    public Image background;
    public Color topColor;
    public Color otherColor;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI deathText;
    public Material topPlayerMaterial;
    public GameObject crownIcon;
    public GameObject myIcon;
    public GameObject enemyIcon;
    public GameObject line;
    public GameObject otherLine;

    public void SetTopPlayer()
    {
        background.color = topColor;
        playerNameText.fontMaterial = topPlayerMaterial;
        killsText.fontMaterial = topPlayerMaterial;
        deathText.fontMaterial = topPlayerMaterial;
        myIcon.SetActive(false);
        enemyIcon.SetActive(false);
        crownIcon.SetActive(true);
        otherLine.SetActive(false);
        line.SetActive(true);
    }

    public void SetOtherPlayer()
    {
        background.color = otherColor;
        line.SetActive(false);
        otherLine.SetActive(true);
    }
    public void SetPlayerDetails(string name, int kills, int death, GameState state)
    {
        playerNameText.text = name;
        killsText.text = kills.ToString();
        deathText.text = death.ToString();

        if (state == GameState.Playing)
        {
            if (name == PhotonNetwork.LocalPlayer.NickName)
            {
                myIcon.SetActive(true);
                enemyIcon.SetActive(false);
            }
            else
            {
                enemyIcon.SetActive(true);
                myIcon.SetActive(false);
            }
        }
        else if (state == GameState.Ending)
        {
            myIcon.SetActive(false);
            enemyIcon.SetActive(false);
        }
    }

}
