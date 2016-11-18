#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BKB_RPG;

using System.IO;

[CustomEditor(typeof(Entity))]
[CanEditMultipleObjects]
public class Drawer_Entity : Editor
{
    Entity myScript;
    int selectedPage = 0;
    int firstPage = 0;

    float buttonWidth = 40;
    float buttonHeight = 21;

    private int pagesToShow;

    void OnEnable() {
		myScript = target as Entity;
        if (myScript.eventPages == null)
            myScript.eventPages = new System.Collections.Generic.List<Entity.EntityPageData>();
        if (myScript.eventPages.Count < 1)
            myScript.eventPages.Add(new Entity.EntityPageData(myScript.GetComponent<GameEvent>(), myScript.GetComponent<Mover>()));
	}

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true, new GUILayoutOption[0]);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Active Page", myScript.activePage.ToString(), EditorStyles.boldLabel);
        pagesToShow = (int)(Screen.width / buttonWidth) - 2;
        EditorGUILayout.LabelField("Page", string.Format("Showing {0}-{1} of {2}", firstPage, Mathf.Min(firstPage + pagesToShow, myScript.eventPages.Count)-1, myScript.eventPages.Count));

        Rect header = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(21));
        float end = header.x + buttonWidth * (pagesToShow + 1) + 2;
        Rect button = new Rect(header.x + 2, header.y, buttonWidth-2, buttonHeight);
        // Show left arrow
        if (firstPage > 0)
        {
            GUI.Box(new Rect(button.x + 16, button.y + 2, button.width - 16, button.height - 4), "<");
            SubMenu(button, () => {
                firstPage -= 1;
            });
        }
        button.x += buttonWidth;

        for (int i = 0; i < pagesToShow; i++)
        {
            int index = firstPage + i;
            if (index < myScript.eventPages.Count)
            {
                GUI.Box(button, index.ToString());
                SubMenu(button, index);
                if (index == selectedPage)
                {
                    GUI.Box(new Rect(button.x + 1, button.y + 1, button.width - 2, button.height - 2), index.ToString());
                    GUI.Box(new Rect(button.x + 2, button.y + 2, button.width - 4, button.height - 4), index.ToString());
                }
                button.x += buttonWidth;
            }
            else
            {
                // '+' New page
                GUI.Box(new Rect(button.x + 2, button.y + 1, button.width - 4, button.height - 2), "+");
                SubMenu(button, () => {
                    myScript.eventPages.Add(new Entity.EntityPageData());
                    if ((firstPage + pagesToShow) <= myScript.eventPages.Count)
                        firstPage++;
                });
                button.x += buttonWidth;
                break;
            }
        }
        button.x = end;
        // Show Right arrow
        if (firstPage + pagesToShow < myScript.eventPages.Count + 1)
        {
            GUI.Box(new Rect(button.x, button.y + 2, button.width - 16, button.height - 4), ">");
            SubMenu(button, () => {
                firstPage += 1;
            });
        }
        // Show page info
        EditorGUI.indentLevel = 1;
        EditorGUILayout.LabelField("Page " + selectedPage, EditorStyles.boldLabel);
        SerializedProperty page = serializedObject.FindProperty("eventPages").GetArrayElementAtIndex(selectedPage);
        EditorGUILayout.PropertyField(page.FindPropertyRelative("condition"));
        EditorGUILayout.PropertyField(page.FindPropertyRelative("trigger"));

        EditorGUILayout.PropertyField(page.FindPropertyRelative("controller"));
        EditorGUILayout.PropertyField(page.FindPropertyRelative("sprite"));
        EditorGUILayout.PropertyField(page.FindPropertyRelative("facing"));

        if (GUI.Button(new Rect(100, header.y + 135, 30, 15), "Get"))
            myScript.eventPages[selectedPage].gameEvent = myScript.GetComponent<GameEvent>();
        EditorGUILayout.PropertyField(page.FindPropertyRelative("gameEvent"));
        if (GUI.Button(new Rect(100, header.y + 155, 30, 15), "Get"))
            myScript.eventPages[selectedPage].mover = myScript.GetComponent<Mover>();
        var moverTooltip = "Assign a mover script.";
        var mover = myScript.eventPages[selectedPage].mover;
        if (mover != null)
        {
            moverTooltip = string.Format("'{0}' on game object '{1}'", mover.moverName, mover.name);
        }
        EditorGUILayout.PropertyField(page.FindPropertyRelative("mover"),
            new GUIContent("Mover", moverTooltip));
        EditorGUILayout.PropertyField(page.FindPropertyRelative("useCollider"));
        serializedObject.ApplyModifiedProperties();

    }

    void SubMenu(Rect area, int id) {
        Event e = Event.current;
        // Did user right click in the target area?
        if (area.Contains(e.mousePosition) && e.type == EventType.MouseDown)
        {
            Event.current.Use();
            if (e.button == 0)
            {
                // Left Click
                selectedPage = id;
            }else
            {
                // Right Click
                GenericMenu menu = new GenericMenu();
                if (myScript.eventPages.Count > 1)
                    menu.AddItem(new GUIContent("Cut"), false, Copy, new object[] { id, true });
                //menu.AddItem(new GUIContent("Copy"), false, Copy, new object[] { id, false });
                if (EditorPrefs.GetString("BKBMoverCopyData") == "")
                    menu.AddDisabledItem(new GUIContent("Paste"));
                else
                {
                    //menu.AddItem(new GUIContent("Paste/Above"), false, Paste, (id));
                    //menu.AddItem(new GUIContent("Paste/Below"), false, Paste, (id + 1));
                }
                menu.ShowAsContext();
            }
        }
    }

    void SubMenu(Rect area, Callback callback) {
        Event e = Event.current;
        // Did user right click in the target area?
        if (area.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
        {
            Event.current.Use();
            callback();
        }
    }

    void Copy(object data)
    {
        object[] datum = data as object[];
        int page = System.Convert.ToInt32(datum[0]);
        bool delete = System.Convert.ToBoolean(datum[1]);
        // TODO - implement serialization of page

        if (delete)
        {
            if (selectedPage == myScript.eventPages.Count - 1)
            {
                selectedPage--;
            }
            myScript.eventPages.RemoveAt(page);
            if (myScript.eventPages.Count - 1 < pagesToShow)
            {
                firstPage = 0;
            }
        }
    }

}
#endif