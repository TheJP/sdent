using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utility;

public class Menu : MonoBehaviour
{
    public Texture2D BackgroundTexture;
    public Texture2D ResourceBackgroundTexture;
    public Texture2D Map;

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
    private Rect minimapRect = new Rect();
    private Rect actualMapPos;

    private float mapScaleFactor;

    private const int MAP_WIDTH = 1000;
    private const int MAP_HEIGHT = 2000;
    private const int MAP_OFFSET_X = -500;
    private const int MAP_OFFSET_Y = -1900;

    private CameraControl cameraControl;

    public string HintText { get; set; }

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
	    if (cameraControl == null)
	    {
	        cameraControl = FindObjectOfType<CameraControl>();
	    }

	    var rtsEntities = EntityController.SelectedEntities;

	    float scaleFactor = CalculateScaleFactor();
	    DrawUnitPortraits(scaleFactor, rtsEntities);
	    DrawAbilities(scaleFactor, rtsEntities.Get(EntityController.ActiveType));
	    DrawSelectedUnitInfo(scaleFactor, rtsEntities.Get(EntityController.ActiveType));
	    DrawResources(scaleFactor, EntityController.Entities
            .Where(e => e.hasAuthority)
            .OfType<IHasInventory>());
	    DrawMinimap(scaleFactor, EntityController.Entities);

	    DrawUnitSelectionBox();
	    DrawHintText(scaleFactor);

	    DrawTooltip();
    }

    private void DrawHintText(float scaleFactor)
    {
        if (!string.IsNullOrEmpty(HintText))
        {
            GUI.skin.box.wordWrap = true;

            Rect hintTextPos = new Rect(unitPortraitRect);
            hintTextPos.y -= 65;
            hintTextPos.height = 60;
            GUI.Box(hintTextPos, HintText);
        }
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

        if (unitPortraitRect.Contains(relPos)
            || abilitiesRect.Contains(relPos)
            || resourcesRect.Contains(relPos)
            || selectedUnitInfoRect.Contains(relPos))
        {
            return true;
        }
        else if (minimapRect.Contains(relPos))
        {
            float scaleFactor = CalculateScaleFactor();

            // hack, Don't look at it!
            Vector3 scaledMapPos = new Vector3(relPos.x - (minimapRect.x + actualMapPos.x), 0, actualMapPos.height - (relPos.y - (minimapRect.y + actualMapPos.y)));

            Vector3 mapPos = scaledMapPos/mapScaleFactor;
            mapPos.x += MAP_OFFSET_X;
            mapPos.z += MAP_OFFSET_Y;
            cameraControl.MoveToMapPos(mapPos);

            return true;
        }
        return false;
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
            .OrderBy(p => p.Key)
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

                DrawSingleResource(res.Resource, res.Amount, scaledResIconStyle, scaledResTextStyle, " in all Inventories.");
            }

            if (counter > 0)
            {
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndArea();
    }

    private void DrawSingleResource(ResourceTypes resource, int amount, GUIStyle scaledResIconStyle, GUIStyle scaledResTextStyle, string toolTipSuffix)
    {
        GUIContent content = new GUIContent(ResourceBackgroundTexture);
        //content.text = amount.ToString();
        content.tooltip = resource.ToString() + toolTipSuffix;

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
        float guiWidth =500 * scaleFactor;
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
                RtsTrainingBuilding building = entity as RtsTrainingBuilding;

                if (building != null)
                {
                    GUILayout.BeginHorizontal();
                    {
                        foreach (GameObject o in building.TrainingQueue)
                        {
                            var inTrain = o.GetComponent<RtsEntity>();
                            DrawSingleUnitPortrait(inTrain, scaledPortraitStyle);
                        }
                        if (!building.TrainingQueue.Any())
                        {
                            GUILayout.BeginVertical();
                            GUILayout.Space(scaledPortraitStyle.fixedHeight + scaledPortraitStyle.margin.top/2F);
                            GUILayout.EndVertical();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                else if (entity is RtsCraftingBuilding)
                {
                    var craftingBuilding = (RtsCraftingBuilding) entity;
                    GUIStyle scaledResIconStyle = GUIHelper.ScaleStyle(scaleFactor, ResourceIconStyle);
                    GUIStyle scaledResTextStyle = GUIHelper.ScaleStyle(scaleFactor, ResourceTextStyle);

                    GUILayout.BeginHorizontal();
                    bool hasInput = false;
                    foreach (var res in craftingBuilding.Recipes.SelectMany(recipe => recipe.Input).OrderBy(p => p.Resource))
                    {
                        hasInput = true;
                        DrawSingleResource(res.Resource, res.Amount, scaledResIconStyle, scaledResTextStyle, " needed per recipe.");
                    }
                    if (!hasInput)
                    {
                        GUILayout.BeginVertical();
                        GUILayout.Space(scaledResIconStyle.fixedHeight + scaledResIconStyle.margin.top / 2F);
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    foreach (var recipe in craftingBuilding.Recipes)
                    {
                        foreach (var res in recipe.Output.OrderBy(p => p.Resource))
                        {
                            DrawSingleResource(res.Resource, res.Amount, scaledResIconStyle, scaledResTextStyle, " crafted per Recipe. \n\n(Time: " + recipe.CraftingTime.ToString("0.0") + "s)");
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                else if (entity is ConstructionSite)
                {
                    var constructionSite = (ConstructionSite)entity;

                    int counter = 0;
                    GUIStyle scaledResIconStyle = GUIHelper.ScaleStyle(scaleFactor, ResourceIconStyle);
                    GUIStyle scaledResTextStyle = GUIHelper.ScaleStyle(scaleFactor, ResourceTextStyle);
                    GUILayout.BeginHorizontal();
                    {
                        foreach (var res in constructionSite.NeededResources.OrderBy(p => p.Key))
                        {
                            if (counter == 5)
                            {
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                            }
                            DrawSingleResource(res.Key, res.Value, scaledResIconStyle, scaledResTextStyle, " needed for finishing the building.");
                            counter++;
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (counter < 5)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.BeginVertical();
                        GUILayout.Space(scaledResIconStyle.fixedHeight + scaledResIconStyle.margin.top / 2F);
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    GUILayout.Space(scaledPortraitStyle.fixedHeight + scaledPortraitStyle.margin.top / 2F);
                    GUILayout.BeginVertical();
                    GUILayout.EndHorizontal();
                }

                // Inventory
                GUILayout.BeginHorizontal();
                {
                    var entityWithInv = entity as IHasInventory;
                    if (entityWithInv != null)
                    {
                        GUIStyle scaledResIconStyle = GUIHelper.ScaleStyle(scaleFactor, ResourceIconStyle);
                        GUIStyle scaledResTextStyle = GUIHelper.ScaleStyle(scaleFactor, ResourceTextStyle);

                        int counter = 0;
                        foreach (var res in entityWithInv.Inventory.OrderBy(p => p.Key))
                        {
                            if (counter > 0 && counter % 5 == 0)
                            {
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                            }
                            DrawSingleResource(res.Key, res.Value, scaledResIconStyle, scaledResTextStyle, " in local Inventory.");
                            counter++;
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

        if (!ability.CanExecute)
        {
            Rect abilityRect = GUILayoutUtility.GetLastRect();
            GUI.DrawTexture(abilityRect, GUIHelper.DisableTexture);
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
        content.tooltip = entity.name.Replace("(Clone)", "");

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

    #region Minimap

    private void DrawMinimap(float scaleFactor, EntityContainer entities)
    {
        float border = 10*scaleFactor;
        float guiWidth = 320 * scaleFactor;
        float guiHeight = 620 * scaleFactor;
        minimapRect = new Rect(Screen.width - guiWidth, Screen.height - guiHeight, guiWidth, guiHeight);

        GUIStyle guiStyle = new GUIStyle()
        {
            fixedWidth = guiWidth,
            fixedHeight = guiHeight,
        };
        guiStyle.normal.background = BackgroundTexture;

        mapScaleFactor = Mathf.Min((guiWidth-2*border)/MAP_WIDTH, (guiHeight-2*border)/MAP_HEIGHT);
        float actualMapWidth = MAP_WIDTH* mapScaleFactor;
        float actualMapHeight = MAP_HEIGHT* mapScaleFactor;

        GUILayout.BeginArea(minimapRect, guiStyle);
        {
            Vector2 offset = new Vector2((guiWidth - actualMapWidth) / 2, (guiHeight - actualMapHeight) / 2);
            actualMapPos = new Rect(offset.x, offset.y, actualMapWidth, actualMapHeight);

            GUI.DrawTexture(actualMapPos, Map);

            foreach (var entity in entities)
            {
                var resource = entity as RtsResource;
                if (resource != null)
                {
                    Rect unitPos = GetMapUnitPos(scaleFactor, actualMapHeight, offset, entity.transform.position, new Vector2(16, 16));
                    RtsResource res = resource;
                    GUI.DrawTexture(unitPos, res.ResourceType.GetIcon());
                }
                else if (entity.hasAuthority)
                {
                    Rect unitPos = GetMapUnitPos(scaleFactor, actualMapHeight, offset, entity.transform.position, new Vector2(5, 5));
                    GUI.DrawTexture(unitPos, GUIHelper.FriendlyUnitTexture);
                }
                else
                {
                    Rect unitPos = GetMapUnitPos(scaleFactor, actualMapHeight, offset, entity.transform.position, new Vector2(5,5));
                    GUI.DrawTexture(unitPos, GUIHelper.EnemyUnitTexture);
                }
            }

            var cameraPos = Utility.RayCameraToGround() ?? Vector3.zero;
            Rect curLocPos = GetMapUnitPos(scaleFactor, actualMapHeight, offset, cameraPos, new Vector2(192/5F, 108/5F));
            GUIHelper.DrawScreenRectBorder(curLocPos, 2, new Color(0.8f, 0.8f, 0.95f));
        }
        GUILayout.EndArea();
    }

    private Rect GetMapUnitPos(float scaleFactor, float actualMapHeight, Vector2 offset, Vector3 entityPos, Vector2 entitySize)
    {
        Vector2 scaledSize = entitySize*scaleFactor;

        float posX = entityPos.x - MAP_OFFSET_X;
        float posY = entityPos.z - MAP_OFFSET_Y; // Units move on the (x-z)-plane

        return new Rect(offset.x + posX*mapScaleFactor - scaledSize.x/2, offset.y + actualMapHeight - (posY*mapScaleFactor + scaledSize.y/2), scaledSize.x, scaledSize.y);
    }

    #endregion
}
