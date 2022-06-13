using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandleInventory : MonoBehaviour
{
    public World world;
    public Color up;
    public Color down;
    public GameObject inventoryItem;
    public Image[] shortcutImages;
    public List<int> shortcutList = new List<int> ();
    public Transform outline;
    public RectTransform grids;
    public float gridsTop;
    public float gridsBottom;
    public Scrollbar scorllbar;

    int first = -10;
    List<GameObject> items = new List<GameObject> ();

    void Update()
    {
        if (world.isOpenInventory) {
            // scroll library
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0.0f) {
                grids.anchoredPosition = grids.anchoredPosition - new Vector2(0, scroll) * 100;
            }
            if (grids.anchoredPosition.y < gridsTop) {
                grids.anchoredPosition = new Vector3(0, gridsTop);
            }
            else if (grids.anchoredPosition.y > gridsBottom) {
                grids.anchoredPosition = new Vector3(0, gridsBottom);
            }
            scorllbar.value = (grids.anchoredPosition.y - gridsTop) / (gridsBottom - gridsTop);
        }
    }

    public void Init()
    {
        for (int i = 0; i < 9; i ++) {
            shortcutList.Add(i);
        }
        // shortcut
        for (int i = 0; i < 9; i ++) {
            GameObject item = GameObject.Instantiate(inventoryItem);
            item.transform.SetParent(outline);
            item.GetComponent<RectTransform>().anchoredPosition = new Vector3(-322 + i * 72, -203, 0);
            int index = i - 9;
            item.GetComponentsInChildren<Button>()[0].onClick.AddListener(() => this.ClickItem(index));
            item.GetComponentsInChildren<Image>()[1].sprite = world.previews[i];
            shortcutImages[i].sprite = world.previews[i];
            items.Add(item);
        }
        // library
        for (int i = 0; i < world.previews.Count; i ++) {
            GameObject item = GameObject.Instantiate(inventoryItem);
            item.transform.SetParent(grids);
            item.GetComponent<RectTransform>().anchoredPosition = new Vector3(-322 + i % 9 * 72, 171 - (i / 9) * 72, 0);
            int index = i;
            item.GetComponentsInChildren<Button>()[0].onClick.AddListener(() => this.ClickItem(index));
            item.GetComponentsInChildren<Image>()[1].sprite = world.previews[i];
            items.Add(item);
        }
    }

    void UpdateShortcut(int shortcutIndex, int libraryIndex)
    {
        // exchange if in shortcut
        for (int i = 0; i < 9; i ++) {
            if (shortcutList[i] == libraryIndex) {
                shortcutList[i] = shortcutList[shortcutIndex];
                shortcutImages[i].sprite = world.previews[shortcutList[shortcutIndex]];
                items[i].transform.GetChild(0).GetComponent<Image>().sprite = world.previews[shortcutList[shortcutIndex]];
            }            
        }
        shortcutList[shortcutIndex] = libraryIndex;
        shortcutImages[shortcutIndex].sprite = world.previews[libraryIndex];
        items[shortcutIndex].transform.GetChild(0).GetComponent<Image>().sprite = world.previews[libraryIndex];
    }

	public void ClickItem(int index)
    {
        // first click
        if (first == -10) {
            first = index;
            items[index+9].GetComponent<Image>().color = down;
        }
        // second click
        else {
            // first in shortcut
            if (-9 <= first && first < 0) {
                // second in shortcut
                if (-9 <= index && index < 0) {
                    UpdateShortcut(first+9, shortcutList[index+9]);
                }
                // second in library
                else if (index >= 0) {
                    UpdateShortcut(first+9, index);
                }
            }
            // second in shortcut
            else if (-9 <= index && index < 0) {
                UpdateShortcut(index+9, first);
            }
            // recover first
            items[first+9].GetComponent<Image>().color = up;
            first = -10;
        }
    }

    public void DropScrollBar()
    {
        grids.anchoredPosition = new Vector3(0, gridsTop + (gridsBottom - gridsTop) * scorllbar.value);
    }
}
