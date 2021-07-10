using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInfoUpdater : MonoBehaviour
{

    // required components and objects
    public GameObject tilePrefab;
    public TileMain tileClass;

    public Text nameText;
    public Text descText;
    public Text costAText;
    public Text costBText;

    public GameObject sceneController;  // Set by the tile list controller as varies depending on scene

	private Image imageComponent;

	private MainMenuTileListController mMenuTileListController;
	private EditMenuTileListController eMenuTileListController;
	private bool onEditMenu = false;    // if false, assume on Main Menu scene

	// Called immediatley
	void Awake()
	{
		// Get image compoennt of this gameobject
		imageComponent = this.gameObject.GetComponent<Image>();
		if (imageComponent == null)
		{
			Debug.LogError("Image Component not found");
		}
	}

	public void SetupControllerVars()   // This msut be done separetley so that the sceneController can be set first
	{
		// Get the tile list controller, attempting the editMenu verson, then the MainMneu one
		eMenuTileListController = sceneController.GetComponent<EditMenuTileListController>();

		if (eMenuTileListController != null)
		{
			onEditMenu = true;
		}
		else
		{
			mMenuTileListController = sceneController.GetComponent<MainMenuTileListController>();

			if (mMenuTileListController == null)
			{
				Debug.LogError("Neither a MainMenuTileListController or a EditMenuTileListCOntroller could be found");
			}
		}
	}

    // Start is called before the first frame update, setting the UI up based on a Prefab
    public void UpdateUI()
    {
        tileClass = tilePrefab.GetComponent<TileMain>();

        if (tileClass == null)
        {
            Debug.LogError("No TileClass Found, Looking for: " + tilePrefab.name.Split('_')[1]);
        }

        nameText.text = tileClass.tileName;
        descText.text = tileClass.description;



        // Set cost amoutns based on set    // WILL ADD ART CHANGES IN FUTURE
        if (tileClass.setName == "Forest")
        {
            costAText.text = "-" + tileClass.resourceCosts[0].ToString();
            costBText.text = "-" + tileClass.resourceCosts[1].ToString();
        }
        else if(tileClass.setName == "Medieval")
        {
            costAText.text = "-" + tileClass.resourceCosts[2].ToString();
            costBText.text = "-" + tileClass.resourceCosts[3].ToString();
        }
        else if (tileClass.setName == "Modern")
        {
            costAText.text = "-" + tileClass.resourceCosts[4].ToString();
            costBText.text = "-" + tileClass.resourceCosts[5].ToString();
        }
    }

	public void OnClick()
	{
		if(onEditMenu == true)
		{
			eMenuTileListController.AddDeckTile(this.gameObject);
		}
		else
		{
			mMenuTileListController.AddDeckTile(this.gameObject);
		}
	}
}
