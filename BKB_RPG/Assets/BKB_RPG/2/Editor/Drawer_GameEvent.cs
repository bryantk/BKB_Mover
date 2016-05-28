using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using BKB_RPG;

[CustomEditor(typeof(GameEvent))]
public class Drawer_GameEvent : Editor {

    private ReorderableList list;
    private GameEvent myScript;

    HashSet<int> selected = new HashSet<int>();
    static Color highlightBlue = new Color(0, 0, 1, 0.1f);


    void OnEnable() {
        myScript = target as GameEvent;
        ReordableList();
        Debug.Log("setupssss");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        list.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
        SceneView.RepaintAll();
    }

    void NewCommand(object data) {
        int type = System.Convert.ToInt32(data);
        switch ((GameEventCommand.CommandTypes)type)
        {
        case GameEventCommand.CommandTypes.Pause:
            //myScript.commands.Add(CreateInstance<EventPause>());//, myScript.commands.Count);
            //AddCommand(CreateInstance<BKB_RPG.EventPause>(), myScript.commands.Count);
            break;
        case GameEventCommand.CommandTypes.UnPause:
            //myScript.commands.Add(CreateInstance<EventUnPause>());//, myScript.commands.Count);
            //AddCommand(CreateInstance<BKB_RPG.EventPause>(), myScript.commands.Count);
            break;
        }
        serializedObject.ApplyModifiedProperties();
    }

    void AddCommand(GameEventCommand command, int index = 0) {
        myScript.commands.Add(command);
        //AddCommand(new List<GameEventCommand>() { command }, index);
    }

    void AddCommand(List<GameEventCommand> commands, int index = 0) {
        HashSet<int> selection = new HashSet<int>();
        for (int i = 0; i < myScript.commands.Count; i++)
        {
            if (selected.Contains(i))
            {
                int id = i;
                if (i >= index)
                    id += commands.Count;
                selection.Add(id);
            }
        }
        selected = selection;
        //Add Commands.
        for (int i = 0; i < commands.Count; i++)
        {
            myScript.commands.Insert(index + i, commands[i]);
        }
        if (myScript.commands.Count == commands.Count)
            selected.Clear();
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
            //return 21 * myScript.commands[index].lines;
            return 21;
        };
        // Draw Commands
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            #region drawElementCallback
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            GameEventCommand command = myScript.commands[index];
            command.lines = 1;
            //rect.y += 3;
            int helpBoxX = 30;
            // 'Highlight' box if targeted for copy
            if (selected.Contains(index))
            {
                GUI.Box(new Rect(helpBoxX, rect.y - 2, 19, 19), "");
                EditorGUI.DrawRect(new Rect(18, rect.y - 2, rect.width + 16, 20), highlightBlue);
            }
            GUI.Label(new Rect(helpBoxX, rect.y, 18, 18), index.ToString());
            GUI.Label(new Rect(helpBoxX + 50, rect.y, 60, 18), command.CommandID.ToString());
            /*
            GUI.Box(new Rect(helpBoxX + 2, rect.y, 15, 15), "");
            float offset = 4;
            if (index > 9)
                offset = 0;
            GUI.Label(new Rect(helpBoxX + offset, rect.y, 18, 18), index.ToString());

            SubMenu(new Rect(helpBoxX + 2, rect.y, 15, 15), index);
            // Command Type Enum
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 80, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("commandType"), GUIContent.none);
            // Expand Inspector?
            SerializedProperty expand = element.FindPropertyRelative("expandedInspector");
            expand.boolValue = EditorGUI.Foldout(new Rect(115, rect.y, 20, 20), expand.boolValue, "");
            // Draw summary
            EditorGUI.LabelField(new Rect(130, rect.y, rect.width - 100, 20), command.BuildSummary());
            rect.y += 3;
            Rect r = new Rect(rect);
            if (!expand.boolValue)
                return;
            command.lines++;
            rect.y += EditorGUIUtility.singleLineHeight + 1;
            DrawCommand(rect, element, command, index);
            if (command.commandType != MovementCommand.CommandTypes.Script)
            {
                EditorGUI.DrawRect(new Rect(117, r.y - 2, 25, 17), highlightGrey);
                EditorGUI.DrawRect(new Rect(r.x + 10, r.y + 15, r.width - 10, (command.lines - 1) * 20), highlightGrey);
            }
            */

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



}
