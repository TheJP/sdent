    using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
    using Assets.Scripts.Utility;
    using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Texture2D BackgroundTexture;
    public Texture2D ResourceBackgroundTexture;

    public GUIStyle PortraitStyle;
    public GUIStyle ResourceIconStyle;
    public GUIStyle ResourceTextStyle;

    public EntityControl EntityController;


    private static readonly Color fullColor = Color.green;
    private static readonly Color emptyColor = Color.red;

    private Rect unitPortraitRect = new Rect();
    private Rect abilitiesRect = new Rect();
    private Rect resourcesRect = new Rect();
    private Rect selectedUnitInfoRect = new Rect();

    private static Texture2D whiteTexture;
    public static Texture2D WhiteTexture
    {
        get
        {
            if (whiteTexture == null)
            {
                whiteTexture = new Texture2D(1, 1);
                whiteTexture.SetPixel(0, 0, Color.white);
                whiteTexture.wrapMode = TextureWrapMode.Repeat;
                whiteTexture.Apply();
            }

            return whiteTexture;
        }
    }

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
	    DrawAbilities(scaleFactor, rtsEntities.Get(EntityController.ActiveType));
	    DrawSelectedUnitInfo(scaleFactor, rtsEntities.Get(EntityController.ActiveType));
	    DrawResources(scaleFactor, EntityController.Entities.OfType<IHasInventory>());
	    DrawUnitSelectionBox();

	    DrawTooltip();
    }

    private void DrawUnitSelectionBox()
    {
        if (EntityController.Selecting)
        {
            var rect = GUIHelper.GetScreenRect(EntityController.SelectionStart, Input.mousePosition);
            GUIHelper.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            GUIHelper.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
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

        return unitPortraitRect.Contains(relPos)
               || abilitiesRect.Contains(relPos)
               || resourcesRect.Contains(relPos)
               || selectedUnitInfoRect.Contains(relPos);
    }

    #region Helpers

    private float CalculateScaleFactor()
    {
        float heightFactor = Screen.height/1080f;
        float widthFactor = Screen.width/1920f;

        return Mathf.Min(heightFactor, widthFactor, 1);
    }

    #endregion

    #region Resources

    private void DrawResources(float scaleFactor, IEnumerable<IHasInventory> rtsEntity)
    {
        float guiWidth = 500 * scaleFactor;
        float guiHeight = 100 * scaleFactor;
        resourcesRect = new Rect(Screen.width - guiWidth, 0, guiWidth, guiHeight);

        GUIStyle guiStyle = new GUIStyle()
        {
            fixedWidth = guiWidth,
            fixedHeight = guiHeight,
        };
        guiStyle.normal.background = BackgroundTexture;


        var resources = rtsEntity
            .Where(entity => true)
            .SelectMany(entity => entity.Inventory)
            .GroupBy(keyValue => keyValue.Key, keyValue => keyValue.Value)
            .Select(p => new {Resource = p.Key, Amount= p.Sum()});

        GUILayout.BeginArea(resourcesRect, guiStyle);
        {
            GUIStyle scaledResIconStyle = GUIHelper.ScaleStyle(scaleFactor, ResourceIconStyle);
            GUIStyle scaledResTextStyle = GUIHelper.ScaleStyle(scaleFactor, ResourceTextStyle);

            int counter = 0;
            foreach (var res in resources)
            {
                if (counter % 5 == 0)
                {
                    if (counter > 0)
                    {
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.BeginHorizontal();
                }
                counter++;

                DrawSingleResource(res.Resource, res.Amount, scaledResIconStyle, scaledResTextStyle);
            }

            if (counter > 0)
            {
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndArea();
    }

    private void DrawSingleResource(ResourceTypes resource, int amount, GUIStyle scaledResIconStyle, GUIStyle scaledResTextStyle)
    {
        GUIContent content = new GUIContent(ResourceBackgroundTexture);
        //content.text = amount.ToString();
        content.tooltip = resource.ToString();

        GUILayout.Box(content, scaledResIconStyle);
        Rect resourcePos = GUILayoutUtility.GetLastRect();
        Rect resIconPos = new Rect(resourcePos);
        resIconPos.width /= 2;

        GUI.DrawTexture(resIconPos, resource.GetIcon(), ScaleMode.ScaleToFit );
        GUI.Label(resourcePos, amount.ToString(), scaledResTextStyle);
    }

    #endregion

    #region SelectedUnit

    private void DrawSelectedUnitInfo(float scaleFactor, IEnumerable<RtsEntity> rtsEntities)
    {
        float guiWidth = 400 * scaleFactor;
        float guiHeight = 300 * scaleFactor;
        selectedUnitInfoRect = new Rect(Screen.width / 2 + 2, Screen.height - guiHeight, guiWidth, guiHeight);

        GUIStyle guiStyle = new GUIStyle()
        {
            fixedWidth = guiWidth,
            fixedHeight = guiHeight,
        };
        guiStyle.normal.background = BackgroundTexture;

        GUILayout.BeginArea(selectedUnitInfoRect, guiStyle);
        {
            GUIStyle scaledPortraitStyle = GUIHelper.ScaleStyle(scaleFactor, PortraitStyle);
            
            RtsEntity entity = rtsEntities.FirstOrDefault();
            if (entity != null)
            {
                // Generic info
                GUILayout.BeginHorizontal();
                {
                    // ToDo: Generic Info
                    DrawSingleUnitPortrait(entity, scaledPortraitStyle);
                }
                GUILayout.EndHorizontal();

                // Build Queue
                GUILayout.BeginHorizontal();
                {
                    // ToDo: Build Queue
                    DrawSingleUnitPortrait(entity, scaledPortraitStyle);
                }
                GUILayout.EndHorizontal();

                // Inventory
                GUILayout.BeginHorizontal();
                {
                    var entityWithInv = entity as IHasInventory;
                    if (entityWithInv != null)
                    {
                        GUIStyle scaledResIconStyle = GUIHelper.ScaleStyle(scaleFactor, ResourceIconStyle);
                        GUIStyle scaledResTextStyle = GUIHelper.ScaleStyle(scaleFactor, ResourceTextStyle);

                        int counter = 0;
                        foreach (var res in entityWithInv.Inventory)
                        {
                            if (counter > 0 && counter % 4 == 0)
                            {
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                            }
                            DrawSingleResource(res.Key, res.Value, scaledResIconStyle, scaledResTextStyle);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndArea();
    }

    #endregion

    #region Abilities

    private void DrawAbilities(float scaleFactor, IEnumerable<RtsEntity> rtsEntities)
    {
        float guiWidth = 400*scaleFactor;
        float guiHeight = 300*scaleFactor;
        abilitiesRect = new Rect(Screen.width/2 - guiWidth - 2, Screen.height - guiHeight, guiWidth, guiHeight);

        GUIStyle guiStyle = new GUIStyle()
        {
            fixedWidth = guiWidth,
            fixedHeight = guiHeight,
        };
        guiStyle.normal.background = BackgroundTexture;

        GUILayout.BeginArea(abilitiesRect, guiStyle);
        {
            GUIStyle scaledPortraitStyle = GUIHelper.ScaleStyle(scaleFactor, PortraitStyle);

            int counter = 0;
            RtsEntity entity = rtsEntities.FirstOrDefault(e => e.hasAuthority);
            if(entity != null)
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
        unitPortraitRect = new Rect(0, Screen.height - guiHeight, guiWidth, guiHeight);

        GUIStyle guiStyle = new GUIStyle()
        {
            fixedWidth = guiWidth,
            fixedHeight = guiHeight,
        };
        guiStyle.normal.background = BackgroundTexture;

        GUILayout.BeginArea(unitPortraitRect, guiStyle);
        {
            GUIStyle scaledPortraitStyle = GUIHelper.ScaleStyle(scaleFactor, PortraitStyle);
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
