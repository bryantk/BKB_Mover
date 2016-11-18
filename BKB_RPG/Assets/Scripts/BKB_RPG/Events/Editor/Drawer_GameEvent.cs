using System;
using System.CodeDom;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditorInternal;
using BKB_RPG;
using BKB_TEXT;
using SimpleJSON;

[CustomEditor(typeof(GameEvent))]
public class Drawer_GameEvent : Editor {

    private ReorderableList list;
    private GameEvent myScript;
    private int tabLevel = 0;
    private const int tabAmount = 8;

    HashSet<int> selected = new HashSet<int>();
    static Color highlightBlue = new Color(0, 0, 1, 0.1f);


    void OnEnable() {
        myScript = target as GameEvent;
        if (myScript.commands == null)
        {
            Debug.LogWarning("Created command list");
            myScript.commands = new List<GameEventCommand>();
            selected = new HashSet<int>();
        }
        ReordableList();
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"),
            true, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("runGlobaly"), new GUIContent("Global?"));
        tabLevel = 0;
        list.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
        SceneView.RepaintAll();
    }

    void NewCommand(object data) {
        int type = System.Convert.ToInt32(data);
        myScript.commands.Add(new GameEventCommand((GameEventCommand.CommandTypes)type));
        if ((GameEventCommand.CommandTypes)type == GameEventCommand.CommandTypes.If)
            myScript.commands.Add(new GameEventCommand(GameEventCommand.CommandTypes.EndIf));
        serializedObject.ApplyModifiedProperties();
    }

    void ReordableList() {
        list = new ReorderableList(serializedObject,
                serializedObject.FindProperty("commands"),
                true, true, true, true);
        // Draw Header
        list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Commands");
        };
        // Expand as needed
        list.elementHeightCallback = (index) => {
            return 21 * (myScript.commands[index].expanded ? myScript.commands[index].lines : 1);
        };
        // Draw Commands
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            #region drawElementCallback
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            GameEventCommand command = myScript.commands[index];
            if (command.CommandID == GameEventCommand.CommandTypes.EndIf || command.CommandID == GameEventCommand.CommandTypes.Else)
                tabLevel--;
            // indention
            for (int i = 0; i < tabLevel; i++)
            {
                GUI.Box(new Rect(rect.x, rect.y, 1, 21 * (command.expanded ? command.lines : 1)), "");
                rect.x += tabAmount;
            }
            rect.y += 3;
            int helpBoxX = 30 + tabLevel * tabAmount;
            // 'Highlight' box if targeted for copy
            if (selected.Contains(index))
            {
                GUI.Box(new Rect(helpBoxX, rect.y - 2, 19, 19), "");
                EditorGUI.DrawRect(new Rect(18, rect.y - 2, rect.width + 16, 20), highlightBlue);
            }
            GUI.Box(new Rect(helpBoxX + 2, rect.y, 15, 15), "");
            float offset = 4;
            if (index > 9)
                offset = 0;
            GUI.Label(new Rect(helpBoxX + offset, rect.y, 18, 18), index.ToString());
            // Click box
            SubMenu(new Rect(helpBoxX + 2, rect.y, 15, 15), index);
            // expand inspector?
            if (command.lines > 1)
                command.expanded = EditorGUI.Foldout(new Rect(rect.x + 115, rect.y, 20, 20), command.expanded, "");
            // Command Type Enum
            GameEventCommand.CommandTypes previousType = command.CommandID;
            EditorGUI.PropertyField(new Rect(rect.x+30, rect.y, 70, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("CommandID"), GUIContent.none);
            serializedObject.ApplyModifiedProperties();
            if (previousType != command.CommandID)
                command.SetEventCommand(command.CommandID);
            // Draw command data   
            DrawCommand(rect, element, command, index);
            serializedObject.ApplyModifiedProperties();
            if (command.CommandID == GameEventCommand.CommandTypes.If || command.CommandID == GameEventCommand.CommandTypes.Else)
                tabLevel++;
            #endregion
        };
        // Context Menu
        list.onAddDropdownCallback = (Rect rect, ReorderableList l) => {
            GenericMenu menu = new GenericMenu();
            foreach (var type in System.Enum.GetValues(typeof(GameEventCommand.CommandTypes)))
            {
                menu.AddItem(new GUIContent(((GameEventCommand.CommandTypes)type).ToString()), false, NewCommand, (int)type);
            }
            menu.ShowAsContext();
        };
        // On Select
        list.onSelectCallback = (ReorderableList l) => {
            Event e = Event.current;
            if (e.control)
            {
                if (!selected.Remove(l.index))
                    selected.Add(l.index);
            }
            else if (e.clickCount > 1)
            {
                selected.Clear();
                selected.Add(l.index);
            }

        };
        // On Remove
        list.onRemoveCallback = (ReorderableList l) => {
            if (selected.Count == 0)
                myScript.commands.RemoveAt(l.index);
            else
            {
                for (int i = myScript.commands.Count - 1; i >= 0; i--)
                {
                    if (selected.Remove(i))
                        myScript.commands.RemoveAt(i);
                }
            }
            selected.Clear();
        };
    }


    void SubMenu(Rect area, int id, bool left_click_allowed = false) {
        Event e = Event.current;
        // Did user right click in the target area?
        if (area.Contains(e.mousePosition) && e.type == EventType.MouseDown)
        {
            if (!left_click_allowed && e.button != 1)
                return;
            Event.current.Use();
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Cut"), false, Copy, new object[] { id, true });
            menu.AddItem(new GUIContent("Copy"), false, Copy, new object[] { id, false });
            if (EditorPrefs.GetString("BKBMoverCopyData") == "")
                menu.AddDisabledItem(new GUIContent("Paste"));
            else
            {
                menu.AddItem(new GUIContent("Paste/Above"), false, Paste, (id));
                menu.AddItem(new GUIContent("Paste/Below"), false, Paste, (id + 1));
            }
            menu.ShowAsContext();
        }
    }

    void DrawCommand(Rect rect, SerializedProperty element, GameEventCommand command, int i) {
        //------------------------------------------------------------------------------------------------------
        // C O M M A N D   E D I T I N G
        //------------------------------------------------------------------------------------------------------
        // TODO - move all names/tooltips to definitions file?
        float tabedWidth = rect.width - tabLevel * tabAmount;
        Rect rect2;
        switch (command.CommandID)
        {
        case GameEventCommand.CommandTypes.If:
        case GameEventCommand.CommandTypes.Debug:
        case GameEventCommand.CommandTypes.Label:
            rect.x += 120;
            Rect r = new Rect(rect.x, rect.y, tabedWidth - 120, EditorGUIUtility.singleLineHeight);
            if (command.expanded)
                r.height *= command.lines;
            command.string_1 = EditorGUI.TextArea(r, command.string_1);
            command.lines = command.string_1.Split('\n').Length;
            break;

        case GameEventCommand.CommandTypes.GoTo:
            rect.x += 120;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth - 120, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("int_1"), new GUIContent("Command"));
            command.int_1 = Mathf.Clamp(command.int_1, -1, myScript.commands.Count - 1);
            break;

        case GameEventCommand.CommandTypes.Wait:
            rect.x += 120;
            rect2 = rect;
            rect2.width = 70;
            command.executionType = EditorGUI.IntPopup(rect2, command.executionType,
                new string[] { "For", "Random" }, new int[] { 0, 1 });
            if (command.executionType == 0)
                EditorGUI.PropertyField(new Rect(rect.x + 80, rect.y, tabedWidth - 200, EditorGUIUtility.singleLineHeight),
                                      element.FindPropertyRelative("float_1"), new GUIContent(""));
            else
            {
                EditorGUI.PropertyField(new Rect(rect.x + 80, rect.y, tabedWidth - 250, EditorGUIUtility.singleLineHeight),
                                      element.FindPropertyRelative("float_2"), new GUIContent(""));
                EditorGUI.PropertyField(new Rect(rect.x + 130, rect.y, tabedWidth - 250, EditorGUIUtility.singleLineHeight),
                                      element.FindPropertyRelative("float_1"), new GUIContent(""));
                command.float_2 = Mathf.Clamp(command.float_2, 0, command.float_1);
                command.float_1 = Mathf.Max(command.float_1, 0);
            }
            command.float_1 = Mathf.Clamp(command.float_1, 0, Mathf.Infinity);
            break;

        case GameEventCommand.CommandTypes.Script:
            if (!command.expanded)
                return;
            rect.y += EditorGUIUtility.singleLineHeight + 5;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("scriptCalls"), GUIContent.none);
            break;

        case GameEventCommand.CommandTypes.Pause:
            rect.x += 120;
            rect2 = rect;
            rect2.width = tabedWidth - 120;
            command.executionType = EditorGUI.IntPopup(rect2, command.executionType,
                new string[] { "All", "NPCs", "Player", "Enemies"}, new int[] { 0, 1, 2, 3 });
            break;

        case GameEventCommand.CommandTypes.Teleport:
            rect.x += 120;
            rect2 = rect;
            rect2.width = tabedWidth - 120;
            command.executionType = EditorGUI.IntPopup(rect2, command.executionType,
                new string[] { "By Label", "By Absolute" }, new int[] { 0, 1 }); 

            if (!command.expanded)
                return;
            rect.x -= 120;
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            command.lines = 3;
            if (command.executionType == 1)
            {
                command.lines = 4;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("string_1"), new GUIContent("Map(optional)"));
                rect.y += EditorGUIUtility.singleLineHeight + 3;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("vector3_1"), new GUIContent("Target"));
            } else
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("string_1"), new GUIContent("Label.Map(optional)"));
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("bool_1"), new GUIContent("Instant?"));
            break;

        case GameEventCommand.CommandTypes.Shake:
            rect.x += 120;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("Block"), new GUIContent("Block:"));
            if (!command.expanded)
                return;
            rect.x -= 120;
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("float_1"), new GUIContent("Time"));
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            command.int_1 = EditorGUI.IntField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                                "Power", command.int_1);
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 25, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("vector3_1"), new GUIContent("Scale"));
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            command.lines = 5;
            if (tabedWidth < 288)
            {
                rect.y += EditorGUIUtility.singleLineHeight + 3;
                command.lines = 7;
            }
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 25, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("vector3_2"), new GUIContent("Rotation"));
            break;

        case GameEventCommand.CommandTypes.Tint:
            rect.x += 120;
            //EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth - 120, EditorGUIUtility.singleLineHeight),
            //                        element.FindPropertyRelative("Block"), new GUIContent("Block:"));

            EditorGUI.LabelField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), "Block");
            EditorGUI.PropertyField(new Rect(tabedWidth + 15, rect.y, 10, EditorGUIUtility.singleLineHeight),
                                element.FindPropertyRelative("Block"), new GUIContent(""));

            if (!command.expanded)
                return;
            rect.x -= 120;
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            tabedWidth -= 50;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("float_1"), new GUIContent("Time"));
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            rect2 = rect;
            rect2.width = 80;
            command.executionType = EditorGUI.IntPopup(rect2, command.executionType,
                new string[] { "Color", "Gradient"}, new int[] { 0, 1 });
            rect.x += 120;
            switch (command.executionType)
            {
            case 0:
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth-120, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("color"), new GUIContent(""));
                break;
            case 1:
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth-120, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("gradient"), new GUIContent(""));
                break;
            }
            break;

        case GameEventCommand.CommandTypes.Transition:
            rect.x += 120;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("Block"), new GUIContent("Block:"));
            if (!command.expanded)
                return;
            rect.x -= 120;
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            tabedWidth -= 50;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("float_1"), new GUIContent("Time"));
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth - 50, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("bool_1"), new GUIContent("Distort"));
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            rect2 = rect;
            rect2.width = 80;
            command.executionType = EditorGUI.IntPopup(rect2, command.executionType,
                new string[] { "Fade Out", "Fade In" }, new int[] { 0, 1 });
            if (command.executionType == 0)
                EditorGUI.PropertyField(new Rect(rect.x + 120, rect.y, tabedWidth - 120, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("color"), new GUIContent(""));
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("texture"), new GUIContent("Optional"));
            command.lines = 5;
            if (command.texture != null)
            {
                command.lines = 6;
                rect.y += EditorGUIUtility.singleLineHeight + 3;

                rect2 = rect;
                rect2.width = 80;
                command.int_1 = EditorGUI.IntPopup(rect2, command.int_1,
                                new string[] { "No Offset", "Player", "Absolute" }, new int[] { 0, 1, 2 });
                if (command.int_1 == 2)
                    EditorGUI.PropertyField(new Rect(rect.x + 120, rect.y, tabedWidth - 120, EditorGUIUtility.singleLineHeight),
                                            element.FindPropertyRelative("vector2_1"), new GUIContent(""));
            }
            break;

        case GameEventCommand.CommandTypes.Letterbox:
            rect.x += 120;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("Block"), new GUIContent("Block:"));
            if (!command.expanded)
                return;
            rect.x -= 120;
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            tabedWidth -= 50;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("float_1"), new GUIContent("Time"));
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            rect2 = rect;
            rect2.width = 80;
            command.executionType = EditorGUI.IntPopup(rect2, command.executionType,
                new string[] { "Show", "Hide" }, new int[] { 0, 1 });
            if (command.executionType == 0)
                EditorGUI.PropertyField(new Rect(rect.x + 120, rect.y, tabedWidth - 120, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("float_2"), new GUIContent(""));
            break;

        case GameEventCommand.CommandTypes.Globals:
            rect.x += 120;
            rect2 = rect;
            rect2.width = tabedWidth - 120;
            command.executionType = EditorGUI.IntPopup(rect2, command.executionType,
                new string[] { "Set Bool", "Set Float", "Set String", "Set Bool with eval", "Set Float with eval", "Set String with eval" }, new int[] { 0, 1, 2, 3, 4, 5 });
            if (!command.expanded)
                return;
            rect.x -= 120;
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            EditorGUI.PropertyField(new Rect(rect.x + 30, rect.y, 80, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("string_2"), new GUIContent(""));
            string target = "bool_1";
            if (command.executionType == 1)
                target = "float_1";
            else if (command.executionType >= 2)
                target = "string_1";
            EditorGUI.PropertyField(new Rect(rect.x + 120, rect.y, tabedWidth - 120, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative(target), new GUIContent(""));
            break;

        case GameEventCommand.CommandTypes.Local:
            rect.x += 120;
            rect2 = rect;
            rect2.width = tabedWidth - 120;
            command.executionType = EditorGUI.IntPopup(rect2, command.executionType,
                new string[] { "Set Bool", "Set Float", "Set String", "Set Bool with eval", "Set Float with eval", "Set String with eval" }, new int[] { 0, 1, 2, 3, 4, 5 });
            if (!command.expanded)
                return;
            rect.x -= 120;
            rect.y += EditorGUIUtility.singleLineHeight + 3;

            string[] lookup = { "A", "B", "C", "D" };
            command.int_1 = EditorGUI.IntPopup(new Rect(rect.x + 30, rect.y, 80, EditorGUIUtility.singleLineHeight),
                command.int_1, lookup, new int[] { 0, 1, 2, 3 });
            command.string_2 = lookup[command.int_1];

            string target2 = "bool_1";
            if (command.executionType == 1)
                target2 = "float_1";
            else if (command.executionType >= 2)
                target2 = "string_1";
            command.string_2 = target2[0] + "-" + command.string_2;
            EditorGUI.PropertyField(new Rect(rect.x + 120, rect.y, tabedWidth - 120, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative(target2), new GUIContent(""));
            break;

        case GameEventCommand.CommandTypes.EntityEvent:
            rect.x += 120;
            rect2 = rect;
            rect2.width = tabedWidth - 120;
            command.executionType = EditorGUI.IntPopup(rect2, command.executionType,
                new string[] { "Set Execution", "Erase", "Move Command", "Sprite or Anim" }, new int[] { 0, 1, 2, 3 });
            if (!command.expanded)
                return;
            rect.x -= 120;
            rect.y += EditorGUIUtility.singleLineHeight + 3;

            EditorGUI.PropertyField(new Rect(rect.x + 30, rect.y, 140, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("entity"), new GUIContent(""));
            command.lines = 2;
            switch (command.executionType)
            {
            case 0:
                command.int_1 = EditorGUI.IntField(new Rect(rect.x + 175, rect.y, 40, EditorGUIUtility.singleLineHeight), command.int_1);
                int limit = 0;
                if (command.entity == null)
                    limit = myScript.gameObject.GetComponent<Entity>().eventPages.Count - 1;
                else
                    limit = command.entity.eventPages.Count - 1;
                command.int_1 = Mathf.Clamp(command.int_1, -2, limit);
                command.int_2 = EditorGUI.IntPopup(new Rect(rect.x + 220, rect.y, tabedWidth - 220, EditorGUIUtility.singleLineHeight),
                command.int_2, new string[] { "OnButtonPress", "PlayerTouch", "EventTouch", "Always", "Once", "Each", "None" }, new int[] { 0, 1, 2, 3, 4, 5, 6 });
                break;
            case 2:
                command.lines = 3;
                EditorGUI.LabelField(new Rect(rect.x + 175, rect.y, 50, EditorGUIUtility.singleLineHeight), "Block");
                EditorGUI.PropertyField(new Rect(tabedWidth + 15, rect.y,  10, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("Block"), new GUIContent(""));
                rect.y += EditorGUIUtility.singleLineHeight + 3;
                command.string_1 = EditorGUI.TextField(new Rect(rect.x + 30, rect.y, tabedWidth - 30, EditorGUIUtility.singleLineHeight), command.string_1);
                break;
            case 3:
                command.lines = 4;
                rect.y += EditorGUIUtility.singleLineHeight + 3;
                rect.x += 30;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth - 30, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("animationOverride"), new GUIContent("Anim"));
                rect.y += EditorGUIUtility.singleLineHeight + 3;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, tabedWidth - 30, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("sprite"), new GUIContent("Sprite"));
                break;
            }
            break;

        case GameEventCommand.CommandTypes.Message:
            rect.x += 120;
            Rect r2 = new Rect(rect.x, rect.y, tabedWidth - 120, EditorGUIUtility.singleLineHeight - 2);
            if (command.expanded)
                r2.height *= 4;
            element = element.FindPropertyRelative("voxData");
            VoxData data = command.voxData;
            data.message = EditorGUI.TextArea(r2, data.message);
            if (!command.expanded)
                break;

            rect.x -= 120;
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            data.position = (MessageLocation)EditorGUI.IntPopup(new Rect(rect.x + 40, rect.y, 60, EditorGUIUtility.singleLineHeight),
                            (int)data.position, new string[] { "Top", "Middle", "Bottom", "Auto", "Custom" }, new int[] { 0, 1, 2, 3, 4});
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            rect.y += EditorGUIUtility.singleLineHeight + 3;

            EditorGUI.LabelField(new Rect(rect.x + 30, rect.y,  50, EditorGUIUtility.singleLineHeight), new GUIContent("Name", "Leave blank to not show."));
            EditorGUI.PropertyField(new Rect(rect.x + 80, rect.y, 100, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("name"), new GUIContent(""));
            data.nameLocation = (HorizontalAlignment)EditorGUI.IntPopup(new Rect(rect.x + tabedWidth - 60, rect.y, 60, EditorGUIUtility.singleLineHeight),
                                (int)data.nameLocation, new string[] { "Left", "Center", "Right" }, new int[] { 0, 1, 2 });
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            // BG
            EditorGUI.LabelField(new Rect(rect.x + 30, rect.y, 20, EditorGUIUtility.singleLineHeight), new GUIContent("BG", "Use window BG, or leave transparent?"));
            EditorGUI.PropertyField(new Rect(rect.x + 55, rect.y, 20, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("useTexture"), new GUIContent());
            // Text Sound
            EditorGUI.LabelField(new Rect(rect.x + 80, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Sound", "Use sound per character?"));
            EditorGUI.PropertyField(new Rect(rect.x + 125, rect.y, 20, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("useSound"), new GUIContent());
            // No Tear Down
            EditorGUI.LabelField(new Rect(rect.x + 150, rect.y, 80, EditorGUIUtility.singleLineHeight), new GUIContent("No TearDown", "Use sound per character?"));
            EditorGUI.PropertyField(new Rect(rect.x + 230, rect.y, 30, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("noTearDown"), new GUIContent());
            rect.y += EditorGUIUtility.singleLineHeight + 3;

            if (command.voxData.hasChoices)
            {
                GUI.Box(new Rect(rect.x + 10, rect.y, tabedWidth - 20, (EditorGUIUtility.singleLineHeight + 2) * (command.voxData.choices.Count + 2)), "");
            }
                

            command.voxData.hasChoices = EditorGUI.Foldout(new Rect(rect.x + 30, rect.y, 20, 20), command.voxData.hasChoices, "Choices");
            if (command.voxData.hasChoices)
            {
                command.lines = 8 + command.voxData.choices.Count;
                
                EditorGUI.LabelField(new Rect(rect.x + 120, rect.y, 70, EditorGUIUtility.singleLineHeight), new GUIContent("Default", "Use sound per character?"));
                EditorGUI.PropertyField(new Rect(rect.x + 190, rect.y, 40, EditorGUIUtility.singleLineHeight),
                                            element.FindPropertyRelative("defaultChoice"), new GUIContent());
                command.voxData.defaultChoice = Mathf.Clamp(command.voxData.defaultChoice, -1, command.voxData.choices.Count - 1);
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                for (int j = 0; j < command.voxData.choices.Count; j++)
                {
                    command.voxData.choices[j] = EditorGUI.TextField(new Rect(rect.x + 30, rect.y, tabedWidth - 50, EditorGUIUtility.singleLineHeight), j.ToString(), command.voxData.choices[j]);
                    if (GUI.Button(new Rect(rect.x + 70, rect.y, 70, EditorGUIUtility.singleLineHeight), "remove"))
                    {
                        command.voxData.choices.RemoveAt(j);
                    }
                    rect.y += EditorGUIUtility.singleLineHeight + 2;
                }
                if (GUI.Button(new Rect(tabedWidth - 10, rect.y, 20, EditorGUIUtility.singleLineHeight), "+"))
                {
                    command.voxData.choices.Add("NEW");
                }
            }
            else
            {
                command.lines = 7;
            }
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            EditorGUI.PropertyField(new Rect(rect.x + 30, rect.y, tabedWidth - 30, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("JSON"), new GUIContent("JSON"));

            break;

        }
    }


    void Copy(object data) {
        // Data = (index to start copying, bool should cut)
        // index of -1 implys copy/cut all
        object[] datum = data as object[];
        int index = System.Convert.ToInt32(datum[0]);
        bool cut = System.Convert.ToBoolean(datum[1]);
        HashSet<int> selection = new HashSet<int>(selected);
        if (selection.Count == 0)
        {
            if (index == -1)
                for (int i = 0; i < myScript.commands.Count; i++)
                    selection.Add(i);
            else
                selection.Add(index);
        }
        JSONArray json = new JSONArray();
        for (int i = 0; i < myScript.commands.Count; i++)
        {
            if (selection.Contains(i))
            {
                string jsonStr = JsonUtility.ToJson(myScript.commands[i]);
                json.Add(JSON.Parse(jsonStr));
            }
        }
        EditorPrefs.SetString("BKBEventCopyData", json.ToString());
        if (cut)
        {
            for (int i = myScript.commands.Count - 1; i >= 0; i--)
                if (selection.Remove(i))
                    myScript.commands.RemoveAt(i);
            selected.Clear();
        }
    }


    void Paste(object data) {
        // Data contains an index (int) to begin inserting at. Editor prefs contains JSON data of commands to create.
        int index = System.Convert.ToInt32(data);
        JSONNode json = JSON.Parse(EditorPrefs.GetString("BKBEventCopyData"));
        foreach (JSONNode j in json.Children)
        {
            myScript.commands.Insert(index, JsonUtility.FromJson<GameEventCommand>(j.ToString()));
            index++;
        }
        selected.Clear();
    }


}
