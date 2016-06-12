using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Texture2D BackgroundTexture;

    public GUIStyle PortraitStyle;
    public EntityControl EntityController;


    private static readonly Color fullColor = Color.green;
    private static readonly Color emptyColor = Color.red;

    private Rect UnitPortraitRect = new Rect();
    private Rect AbilitiesRect = new Rect();
    private Rect ResourcesRect = new Rect();

    // Use this for initialization
    void Start ()
    {
	
	}
	
	// Update is called once per frame
	void OnGUI ()
	{
	    var rtsEntities = EntityController.SelectedEntities;

	    float scaleFactor = CalculateScaleFactor();

	    DrawUnitPortraits(scaleFactor, rtsEntities);
	    DrawAbilities(scaleFactor, rtsEntities);
        // ToDo: Where are Recources
	    DrawResources(scaleFactor, null);

	    DrawTooltip();
    }

    private void DrawTooltip()
    {
        if (!string.IsNullOrEmpty(GUI.tooltip))
        {
            GUI.skin.box.wordWrap = true;
            var mousePos = Event.current.mousePosition;
            GUI.Box(new Rect(mousePos.x, mousePos.y, 200, 80), GUI.tooltip);
        }
    }

    /// <summary>
    /// Check if the position of the mouse is above the menu and handles a click event if yes.
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <returns>Wether the click was handled by the menu or not</returns>
    public bool HandleMouseClick(Vector3 mousePosition)
    {
        Vector2 relPos = new Vector2(mousePosition.x, Screen.height - mousePosition.y);

        return UnitPortraitRect.Contains(relPos)
               || AbilitiesRect.Contains(relPos)
               || ResourcesRect.Contains(relPos);
    }

    #region Helpers

    private float CalculateScaleFactor()
    {
        float heightFactor = Screen.height/1080f;
        float widthFactor = Screen.width/1920f;

        return Mathf.Min(heightFactor, widthFactor, 1);
    }

    private GUIStyle CreateScaledPortraitStyle(float scaleFactor)
    {
        GUIStyle scaledPorttraitStyle = new GUIStyle(PortraitStyle);
        scaledPorttraitStyle.fixedWidth *= scaleFactor;
        scaledPorttraitStyle.fixedHeight *= scaleFactor;
        scaledPorttraitStyle.padding = new RectOffset
        {
            top = (int)(PortraitStyle.padding.top * scaleFactor),
            bottom = (int)(PortraitStyle.padding.bottom * scaleFactor),
            left = (int)(PortraitStyle.padding.left * scaleFactor),
            right = (int)(PortraitStyle.padding.right * scaleFactor)
        };
        scaledPorttraitStyle.margin = new RectOffset
        {
            top = (int)(PortraitStyle.margin.top * scaleFactor),
            bottom = (int)(PortraitStyle.margin.bottom * scaleFactor),
            left = (int)(PortraitStyle.margin.left * scaleFactor),
            right = (int)(PortraitStyle.margin.right * scaleFactor)
        };

        return scaledPorttraitStyle;
    }

    #endregion

    #region Resources

    private void DrawResources(float scaleFactor, IEnumerable<RtsResource> rtsResources)
    {
        float guiWidth = 500 * scaleFactor;
        float guiHeight = 100 * scaleFactor;
        ResourcesRect = new Rect(Screen.width - guiWidth, 0, guiWidth, guiHeight);

        GUIStyle guiStyle = new GUIStyle()
        {
            fixedWidth = guiWidth,
            fixedHeight = guiHeight,
        };
        guiStyle.normal.background = BackgroundTexture;

        GUILayout.BeginArea(ResourcesRect, guiStyle);
        {
            GUIStyle scaledPortraitStyle = CreateScaledPortraitStyle(scaleFactor);
            
        }
        GUILayout.EndArea();
    }

    #endregion

    #region Abilities

    private void DrawAbilities(float scaleFactor, IEnumerable<RtsEntity> rtsEntities)
    {
        float guiWidth = 400*scaleFactor;
        float guiHeight = 300*scaleFactor;
        AbilitiesRect = new Rect((Screen.width - guiWidth)/2, Screen.height - guiHeight, guiWidth, guiHeight);

        GUIStyle guiStyle = new GUIStyle()
        {
            fixedWidth = guiWidth,
            fixedHeight = guiHeight,
        };
        guiStyle.normal.background = BackgroundTexture;

        GUILayout.BeginArea(AbilitiesRect, guiStyle);
        {
            GUIStyle scaledPortraitStyle = CreateScaledPortraitStyle(scaleFactor);

            int counter = 0;
            foreach (RtsEntity entity in rtsEntities)
            {
                foreach (IAbility ability in entity.Abilities)
                {
                    if (counter % 4 == 0)
                    {
                        if (counter > 0)
                        {
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.BeginHorizontal();
                    }
                    counter++;

                    DrawSingleAbility(entity, ability, scaledPortraitStyle);
                }
            }

            if (counter > 0)
            {
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndArea();
    }

    private static void DrawSingleAbility(RtsEntity entity, IAbility ability, GUIStyle scaledPortraitStyle)
    {
        GUIContent content = new GUIContent(ability.Icon);
        //content.text = ability.Name;
        content.tooltip = string.Format("{0} ({1})\n\n{2}", ability.Name, ability.Key, ability.Lore);

        if (GUILayout.Button(content, scaledPortraitStyle)
            && Event.current.button == 0
            && ability.CanExecute)
        {
            ability.Execute();
        }
    }

    #endregion

    #region UnitPortraits

    private void DrawUnitPortraits(float scaleFactor,  IEnumerable<RtsEntity> rtsEntities)
    {
        float guiWidth = 400*scaleFactor;
        float guiHeight = 300*scaleFactor;
        UnitPortraitRect = new Rect(0, Screen.height - guiHeight, guiWidth, guiHeight);

        GUIStyle guiStyle = new GUIStyle()
        {
            fixedWidth = guiWidth,
            fixedHeight = guiHeight,
        };
        guiStyle.normal.background = BackgroundTexture;

        GUILayout.BeginArea(UnitPortraitRect, guiStyle);
        {
            GUIStyle scaledPortraitStyle = CreateScaledPortraitStyle(scaleFactor);
            int counter = 0;

            foreach (RtsEntity entity in rtsEntities)
            {
                if (counter % 4 == 0)
                {
                    if (counter > 0)
                    {
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.BeginHorizontal();
                }
                counter++;

                DrawSingleUnitPortrait(entity, scaledPortraitStyle);
            }

            if (counter > 0)
            {
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndArea();
    }

    private void DrawSingleUnitPortrait(RtsEntity entity, GUIStyle scaledPortraitStyle)
    {
        GUIContent content = new GUIContent(entity.portraitImage);
        content.tooltip = entity.name;

        float relativeState = entity.state/entity.MaxState;
        float healthBarHeight = scaledPortraitStyle.fixedHeight / 8F;
        float healthBarWidth = scaledPortraitStyle.fixedWidth*relativeState;
        Texture2D healthTexture = new Texture2D(1, 1);
        healthTexture.SetPixel(0, 0, Color.Lerp(emptyColor, fullColor, relativeState));
        healthTexture.wrapMode = TextureWrapMode.Repeat;
        healthTexture.Apply();

        GUILayout.Box(content, scaledPortraitStyle);
        var healthBarRect =  GUILayoutUtility.GetLastRect();
        healthBarRect.height = healthBarHeight;
        healthBarRect.width = healthBarWidth;
        GUI.DrawTexture(healthBarRect, healthTexture, ScaleMode.StretchToFill);
    }

    #endregion
}
