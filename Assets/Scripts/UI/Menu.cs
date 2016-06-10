using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Texture2D BackgroundTexture;

    public GUIStyle UnitPortraitStyle;
    public EntityControl EntityController;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void OnGUI ()
	{
	    var rtsEntities = EntityController.SelectedEntities;

	    CreateUnitPortraits(rtsEntities);
	    CreateAbilities(rtsEntities);
    }
    private void CreateAbilities(IEnumerable<RtsEntity> rtsEntities)
    {
        GUIStyle style = new GUIStyle()
        {
            fixedWidth = 300,
            fixedHeight = 300,
        };
        style.normal.background = BackgroundTexture;

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, Screen.height - 300, 300, 300), style);
        {
            foreach (RtsEntity entity in rtsEntities)
            {
                foreach (IAbility ability in entity.Abilities)
                {
                    GUIContent content = new GUIContent(entity.portraitImage);
                    //content.text = ability.Name;
                    //content.tooltip = ability.Lore;
                    if (GUILayout.Button(content, UnitPortraitStyle) 
                        && ability.CanExecute)
                    {
                        ability.Execute();
                    }
                    
                }
            }
        }
        GUILayout.EndArea();
    }

    private void CreateUnitPortraits(IEnumerable<RtsEntity> rtsEntities)
    {
        GUIStyle style = new GUIStyle()
        {
            fixedWidth = 400,
            fixedHeight = 300,
        };
        style.normal.background = BackgroundTexture;

        GUILayout.BeginArea(new Rect(0, Screen.height - 300, 400, 300), style);
        {
            foreach (RtsEntity entity in rtsEntities)
            {
                GUILayout.Box(entity.portraitImage, UnitPortraitStyle);
            }
        }
        GUILayout.EndArea();
    }
}
