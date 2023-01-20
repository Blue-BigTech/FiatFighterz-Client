using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Net.Web;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterPreview : MonoBehaviour
{
    public ClassPreview classPreview;

    public TextMeshProUGUI className;

    public ItemDisplay[] equips;

    private WebCharacterInfo info;



    public void SetCharacter(WebCharacterInfo info)
    {
        //Debug.Log(info.metadata);

        this.info = info;
       
        var charInfo = GameData.objects[info.type];     // this gets the data from the xml document
        GameObjectInfo skinInfo = null;
        if (info.skin != 0)
            skinInfo = GameData.objects[info.skin];


        classPreview.SetClass(skinInfo ?? charInfo);
        NFTmetadata metadata = new NFTmetadata(info.metadata);
        
        //className.text = charInfo.name; //change it to info metadata name
        className.text = metadata.name;

        // fix this also they wont have any items at all because of this 
        
        for (int i = 0; i < equips.Length && i < info.equips.Length; i++)
        {
            var item = info.equips[i];
            equips[i].SetItem(item);
        }
    }

    public void OnClick()
    {
        GameManager.characterToLoad = info.id;
        GameManager.characterToCreate = 0;
        Debug.Log(info.id);
        SceneManager.LoadScene(Constants.Game_Scene);
    }
}