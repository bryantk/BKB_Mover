using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using BKB_RPG;
using SimpleJSON;

[CustomEditor(typeof(Mover))]
public class Drawer_Mover : Editor
{

    private const float DefaultUnitDistance = 1f;
    private ReorderableList list;

    //important
    Mover myScript;

    // settings
    public bool showOptions;
    public bool showCollisionOptions;
    public bool showAdvancedOptions;

    GUIStyle style;
    string json;
    HashSet<int> selected = new HashSet<int>();

    int timestamp = 0;
    int indexSelected = -1;

    static Color highlightBlue = new Color(0, 0, 1, 0.1f);
    static Color highlightGrey = new Color(0.3f, 0.3f, 0.3f, 0.1f);

    void OnEnable() {
        myScript = target as Mover;
        ReordableList();
        
        style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 8;

        Rigidbody2D r = myScript.GetComponent<Rigidbody2D>();
        r.gravityScale = 0;

        int master_count = FindObjectsOfType<EntityMaster>().Length;
		if (master_count == 0) {
			Debug.LogWarning("No MoverMasters found... Creating");
			GameObject obj = new GameObject();
			obj.AddComponent<EntityMaster>();
			obj.name = "MasterMover";
		}
		if (master_count > 1)
			Debug.LogWarning(master_count.ToString() + " MoverMasters found, should be 1.");
		if (myScript.commands == null) {
            Debug.LogWarning("Created command list");
            myScript.commands = new List<MovementCommand>();
            selected = new HashSet<int>();
        }
    }

	// manage handlers and arrow display of path
	void OnSceneGUI() {
        // redraw path in editor
        if (!Application.isPlaying)
            myScript.startPosition = myScript.transform.position;
        Vector3 lastPos = myScript.startPosition;
        Handles.Label(lastPos - Vector3.right * 0.25f, "Start");
        Vector3 offsetAmount = Vector3.right * 0.1f + Vector3.down * 0.25f;
        Vector3 textOffset = offsetAmount;
        for (int i = 0; i < myScript.commands.Count; i++) {
            // --- Command Switch -----------------------------------------------
            MovementCommand command = myScript.commands[i];
            if (command.commandType == MovementCommand.CommandTypes.Wait)
            {
                Handles.Label(lastPos + textOffset, i.ToString() + " : Wait: " + command.time.ToString(), style);
                textOffset += offsetAmount;
            }
            else if (command.commandType == MovementCommand.CommandTypes.Move || command.commandType == MovementCommand.CommandTypes.Face)
            {
                // Reset Text Offset
                textOffset = Vector3.right * 0.15f + Vector3.down * 0.1f;
                Vector3 target = lastPos;
                switch (command.move_type)
                {
                case MovementCommand.MoverTypes.Relative:
                    target += (Vector3)command.target;
                    if (command.instant)
                        Utils.DrawDottedArrow(lastPos, target, Color.blue);
                    else
                        Utils.DrawArrow(lastPos, target, Color.blue);
                    Vector3 temp = target;
                    temp = Handles.FreeMoveHandle(temp, Quaternion.identity, 0.125f, Vector3.one, Handles.SphereCap);
                    command.target = temp - lastPos;
                    break;
                case MovementCommand.MoverTypes.Absolute:
                    target = (Vector3)command.target;
                    if (command.instant)
                        Utils.DrawDottedArrow(lastPos, target, Color.red);
                    else
                        Utils.DrawArrow(lastPos, target, Color.red);
                    command.target = Handles.FreeMoveHandle(command.target, Quaternion.identity, 0.125f, Vector3.one, Handles.SphereCap);
                    break;
                case MovementCommand.MoverTypes.To_transform:
                case MovementCommand.MoverTypes.ObjName:
                    if (command.transformTarget != null)
                        target = command.transformTarget.position;
                    if (command.instant)
                        Utils.DrawDottedArrow(lastPos, target, Color.green);
                    else
                        Utils.DrawArrow(lastPos, target, Color.green);
                    break;
                case MovementCommand.MoverTypes.Angle:
                    target += (Vector3)Utils.AngleMagnitudeToVector2(command.offsetAngle, command.maxStep);

                    //Vector3 t = (Vector3)Utils.AngleMagnitudeToVector2(command.offsetAngle, 1);
                    //Vector3 te = Handles.Slider(target, t, 0.125f, Handles.SphereCap, 1);
                    //command.maxStep = (target - te).magnitude;
                    // TODO
                    Vector3 temp2 = target;
                    temp2 = Handles.FreeMoveHandle(temp2, Quaternion.identity, 0.125f, Vector3.one, Handles.SphereCap);
                    temp2 = temp2 - lastPos;
                    command.maxStep = temp2.magnitude;
                    //command.offsetAngle = Utils.Vector2toAngle((Vector2)temp2);

                    if (command.instant)
                        Utils.DrawDottedArrow(lastPos, target, Color.cyan);
                    else
                        Utils.DrawArrow(lastPos, target, Color.cyan);
                    break;
                }
                lastPos = target;
                if ( command.move_type != MovementCommand.MoverTypes.Angle)
                {
                    // Within Distance.
                    Vector3 withinRadius = lastPos + new Vector3(command.withinDistance, 0, 0);
                    Vector3 withinSlider = Handles.Slider(withinRadius, Vector3.right, 0.5f, Handles.ArrowCap, 1);
                    command.withinDistance = Mathf.Max((withinSlider - lastPos).x, 0);
                    if (command.withinDistance > 0)
                    {
                        Handles.Disc(Quaternion.identity, lastPos, Vector3.forward, command.withinDistance, true, 1);
                        Handles.DrawDottedLine(lastPos, withinRadius, 2.5f);
                    }
                    // Within Random.
                    if (command.randomType == MovementCommand.RandomTypes.Area)
                    {
                        Handles.color = Color.yellow;
                        Vector3 randomRadiusMax = lastPos + Vector3.up * command.random.y;
                        Vector3 randomSliderMax = Handles.Slider(randomRadiusMax, Vector3.up, 0.75f, Handles.ArrowCap, 1);
                        command.random.y = Mathf.Max((randomSliderMax - lastPos).y, 0);
                        if (command.random.y < command.random.x)
                            command.random.x = command.random.y;
                        Handles.color = Color.red;
                        Vector3 randomRadiusMin = lastPos + Vector3.up * command.random.x;
                        Vector3 randomSliderMin = Handles.Slider(randomRadiusMin, Vector3.down, 0.5f, Handles.ArrowCap, 1);
                        command.random.x = Mathf.Max((randomSliderMin - lastPos).y, 0);
                        if (command.random.x > command.random.y)
                            command.random.y = command.random.x;
                        if (command.random.magnitude > 0)
                        {
                            Handles.color = Color.red;
                            Handles.Disc(Quaternion.identity, lastPos, Vector3.forward, command.random.x, true, 1);
                            Handles.color = Color.yellow;
                            Handles.Disc(Quaternion.identity, lastPos, Vector3.forward, command.random.y, true, 1);
                            Handles.DrawDottedLine(randomSliderMin, randomRadiusMax, 2.5f);
                            Handles.color = Color.white;
                        }
                    }
                    else if (command.randomType == MovementCommand.RandomTypes.Linear)
                    {
                        // TODO - Draw line alone path
                    }
                    
                }
                // Labels.
                Handles.Label(lastPos + textOffset, i.ToString() + " : Move", style);
                textOffset += offsetAmount;
            }
            else if (command.commandType == MovementCommand.CommandTypes.GoTo)
            {
                Handles.Label(lastPos + textOffset, i.ToString() + ": GOTO: " + command.int_1, style);
                textOffset += offsetAmount;
            }
            else if (command.commandType == MovementCommand.CommandTypes.Boolean)
            {
                Handles.Label(lastPos + textOffset, i.ToString() + ": Bool", style);
                textOffset += offsetAmount;
            }
            else if (command.commandType == MovementCommand.CommandTypes.Script)
            {
                Handles.Label(lastPos + textOffset, i.ToString() + ": Script", style);
                textOffset += offsetAmount;
            }
            else if (command.commandType == MovementCommand.CommandTypes.Remove)
            {
                Handles.Label(lastPos + textOffset, i.ToString() + ": Pop" + command.int_1, style);
                textOffset += offsetAmount;
            }
            else if (command.commandType == MovementCommand.CommandTypes.Set)
            {
                Handles.Label(lastPos + textOffset, i.ToString() + ": Set " + command.setType.ToString(), style);
                textOffset += offsetAmount;
            }
            else if (command.commandType == MovementCommand.CommandTypes.Note)
            {
                Handles.Label(lastPos + textOffset, i.ToString() + ": " + command.targetName.ToString(), style);
                textOffset += offsetAmount;
            }
            // ----------------------------------
            // NEW COMMAND SCENE DISPLAY LOGIC
            // ----------------------------------

            // Hint selected command
            if (timestamp > 0 && i == indexSelected)
            {
                timestamp--;
                float size = (float)timestamp / 100;
                size = Mathf.Min(size, 0.75f);
                Handles.CircleCap(0, lastPos, Quaternion.identity, size);
            }
            // TODO - keep this????
            if (selected.Contains(i))
            {
                Handles.color = Color.white;
                Handles.RectangleCap(-1, lastPos, Quaternion.identity, 0.2f);
                Handles.color = Color.black;
                Handles.RectangleCap(-1, lastPos, Quaternion.identity, 0.25f);
                Handles.color = Color.white;
            }
        }
        Repaint();
    }

	public override void OnInspectorGUI() {
		// Follow this template
		serializedObject.Update();
        DrawHeaderInfo();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        SceneView.RepaintAll();
    }

    void ReordableList() {
        list = new ReorderableList(serializedObject,
                serializedObject.FindProperty("commands"),
                true, true, true, true);
        // Draw Header
        list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Commands");
            int helpBoxX = 120;
            // 'Highlight' box if targeted for copy
            GUI.Box(new Rect(helpBoxX + 2, rect.y, 15, 15), "");
            GUI.Label(new Rect(helpBoxX + 4, rect.y, 18, 18), "?");
            ContextMenu(new Rect(helpBoxX + 2, rect.y, 15, 15), true);
            // Draw info command box
        };
        // Expand as needed
        list.elementHeightCallback = (index) => {
            return 21 * myScript.commands[index].lines;
        };
        // Draw Commands
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            #region drawElementCallback
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            MovementCommand command = myScript.commands[index];
            command.lines = 1;
            //rect.y += 3;
            int helpBoxX = 30;
            // 'Highlight' box if targeted for copy
            if (selected.Contains(index))
            {
                GUI.Box(new Rect(helpBoxX, rect.y - 2, 19, 19), "");
                EditorGUI.DrawRect(new Rect(18, rect.y - 2, rect.width + 16, 20), highlightBlue);
            }
                
            GUI.Box(new Rect(helpBoxX+2, rect.y, 15, 15), "");
            float offset = 4;
            if (index > 9)
                offset = 0;
            GUI.Label(new Rect(helpBoxX+ offset, rect.y, 18, 18), index.ToString());
            SubMenu(new Rect(helpBoxX+2, rect.y, 15, 15), index);
            // Command Type Enum
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 80, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("commandType"), GUIContent.none);
            // Expand Inspector?
            SerializedProperty expand = element.FindPropertyRelative("expandedInspector");
            expand.boolValue = EditorGUI.Foldout(new Rect(115, rect.y, 20, 20), expand.boolValue, "");
            // Draw summary
            EditorGUI.LabelField(new Rect(130, rect.y, rect.width-100, 20), command.BuildSummary() );
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
            #endregion
        };
        // Context Menu
        list.onAddDropdownCallback = (Rect rect, ReorderableList l) => {
            GenericMenu menu = new GenericMenu();
            foreach (var type in System.Enum.GetValues(typeof(MovementCommand.CommandTypes)))
            {
                menu.AddItem(new GUIContent(((MovementCommand.CommandTypes)type).ToString()), false, NewCommand, (int)type);
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
            else if(e.clickCount > 1)
            {
                selected.Clear();
                selected.Add(l.index);
                timestamp = 0;
            }
            else
            {
                timestamp = 100;
                indexSelected = l.index;
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


    void DrawCommand(Rect rect, SerializedProperty element, MovementCommand command, int i) {
        //------------------------------------------------------------------------------------------------------
        // C O M M A N D   E D I T I N G
        //------------------------------------------------------------------------------------------------------
        // TODO - move all names/tooltips to definitions file?
        switch (command.commandType)
        {
        // Move command
        case MovementCommand.CommandTypes.Move:
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("move_type"), GUIContent.none);
            // Types
            switch (command.move_type)
            {
            case MovementCommand.MoverTypes.Relative:
            case MovementCommand.MoverTypes.Absolute:
                EditorGUI.PropertyField(new Rect(rect.x + 90, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("target"), GUIContent.none);
                break;
            case MovementCommand.MoverTypes.To_transform:
                EditorGUI.ObjectField(new Rect(rect.x + 95, rect.y, rect.width - 170, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("transformTarget"), GUIContent.none);
                command.recalculate = EditorGUI.ToggleLeft(
                    new Rect(rect.width - 50, rect.y, 65, EditorGUIUtility.singleLineHeight), "recalc", command.recalculate);

                break;
            case MovementCommand.MoverTypes.ObjName:
                EditorGUI.DelayedTextField(new Rect(rect.x + 95, rect.y, rect.width - 170, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("targetName"), GUIContent.none);
                command.recalculate = EditorGUI.ToggleLeft(
                        new Rect(rect.width - 50, rect.y, 65, EditorGUIUtility.singleLineHeight), "recalc", command.recalculate);
                if (command.targetName != "" && command.targetName.ToUpper() != "PLAYER")
                {
                    GameObject obj = GameObject.Find(command.targetName);
                    if (obj != null)
                        command.transformTarget = obj.transform;
                    else
                        EditorGUI.DrawRect(new Rect(rect.x + 105, rect.y, rect.width - 185, EditorGUIUtility.singleLineHeight),
                            new Color(1f, 0, 0, 0.25f));
                }
                break;
            case MovementCommand.MoverTypes.Angle:
                // TODO - enum of facing directions?
                EditorGUI.PropertyField(new Rect(rect.x + 95, rect.y, rect.width - 185, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("maxStep"), GUIContent.none);
                EditorGUI.LabelField(new Rect(rect.width - 70, rect.y, 50, EditorGUIUtility.singleLineHeight), "at");
                command.offsetAngle = EditorGUI.FloatField(new Rect(rect.width - 50, rect.y, 55, EditorGUIUtility.singleLineHeight),
                    command.offsetAngle);
                break;
            default:
                break;
            }
            // Extra Move Command options
            if (command.move_type != MovementCommand.MoverTypes.Angle)
            {
                command.lines++;
                rect.y += EditorGUIUtility.singleLineHeight + 3;
                command.maxStep = Mathf.Max(EditorGUI.FloatField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                    new GUIContent("Max Distance", "Maximum distance to move. 0 = no limit."),
                    command.maxStep), 0);
                command.lines++;
                rect.y += EditorGUIUtility.singleLineHeight + 3;
                command.withinDistance = Mathf.Max(EditorGUI.FloatField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                    new GUIContent("Stop Within"), command.withinDistance), 0);

                command.lines++;
                rect.y += EditorGUIUtility.singleLineHeight + 3;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("instant"), new GUIContent("Teleport"));
            }
            command.lines++;
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 70, EditorGUIUtility.singleLineHeight), "Random");
            EditorGUI.PropertyField(new Rect(rect.x + 95, rect.y, 100, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("randomType"), GUIContent.none);
            if (command.randomType != MovementCommand.RandomTypes.None)
            {
                command.lines++;
                rect.y += EditorGUIUtility.singleLineHeight + 3;
                EditorGUI.PropertyField(new Rect(rect.x + 95, rect.y, 100, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("random"), GUIContent.none);
            }
            break;
        // Facing commands
        case MovementCommand.CommandTypes.Face:
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("move_type"), GUIContent.none);
            // Random choices and values
            string[] displayTextFace = { "Constant", "Between", "Between and Between" };
            int[] indexFace = { 0, 1, 2 };
            // Types
            switch (command.move_type)
            {
            case MovementCommand.MoverTypes.Relative:
            case MovementCommand.MoverTypes.Absolute:
                EditorGUI.PropertyField(new Rect(rect.x + 90, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("target"), GUIContent.none);
                break;
            case MovementCommand.MoverTypes.To_transform:
                EditorGUI.ObjectField(new Rect(rect.x + 95, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("transformTarget"), GUIContent.none);
                break;
            case MovementCommand.MoverTypes.ObjName:
                EditorGUI.DelayedTextField(new Rect(rect.x + 95, rect.y, rect.width - 170, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("targetName"), GUIContent.none);
                if (command.targetName != "")
                {
                    GameObject obj = GameObject.Find(command.targetName);
                    if (obj != null)
                        command.transformTarget = obj.transform;
                    else
                        EditorGUI.DrawRect(new Rect(rect.x + 105, rect.y, rect.width - 185, EditorGUIUtility.singleLineHeight),
                            new Color(1f, 0, 0, 0.25f));
                }
                break;
            case MovementCommand.MoverTypes.Angle:
                // TODO - intpopup
                // TODO - angle property field?
                EditorGUI.PropertyField(new Rect(rect.x + 105, rect.y, 55, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("offsetAngle"), GUIContent.none);
                EditorGUI.LabelField(new Rect(rect.x + 150, rect.y, 90, EditorGUIUtility.singleLineHeight), "from forward");

                break;
            default:
                break;
            }
            command.maxStep = 1;
            command.lines++;
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 70, EditorGUIUtility.singleLineHeight), "Offset");
            command.randomType = (MovementCommand.RandomTypes)EditorGUI.IntPopup(
                new Rect(rect.x + 95, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight),
                (int)command.randomType, displayTextFace, indexFace);
            if (command.randomType == MovementCommand.RandomTypes.None)
                return;
            command.lines++;
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            if (command.randomType == MovementCommand.RandomTypes.Linear)
            {
                EditorGUI.PropertyField(new Rect(rect.x + 95, rect.y, rect.width-100, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("random"), GUIContent.none);
            }
            else
            {
                EditorGUI.PropertyField(new Rect(rect.x + 95, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("random"), GUIContent.none);
                command.lines++;
                rect.y += EditorGUIUtility.singleLineHeight + 3;
                EditorGUI.PropertyField(new Rect(rect.x + 95, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight),
                                        element.FindPropertyRelative("random2"), GUIContent.none);
            }
            break;
        // Wait Command
        case MovementCommand.CommandTypes.Wait:
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("time"), new GUIContent("Seconds"));
            command.time = Mathf.Max(command.time, 0);
            break;
        case MovementCommand.CommandTypes.GoTo:
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("int_1"), new GUIContent("Command"));
            command.int_1 = Mathf.Clamp(command.int_1, 0, myScript.commands.Count - 1);
            break;
        case MovementCommand.CommandTypes.Boolean:
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("flag"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + 160, rect.y, 60, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("Bool"), GUIContent.none);
            break;
        case MovementCommand.CommandTypes.Script:
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("scriptCalls"), GUIContent.none);
            int extraLines = command.scriptCalls.GetPersistentEventCount() * 2 + 1;
            command.lines += Mathf.Max(extraLines, 3);

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("commands").GetArrayElementAtIndex(i).FindPropertyRelative("scriptCalls"));
            break;
        case MovementCommand.CommandTypes.Remove:
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("int_1"), new GUIContent("# to Remove"));
            command.int_1 = Mathf.Clamp(command.int_1, 1, myScript.commands.Count - 1);
            command.lines++;
            rect.y += EditorGUIUtility.singleLineHeight + 3;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("Bool"), new GUIContent("Remove Self"));
            break;
        case MovementCommand.CommandTypes.Set:
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("setType"), GUIContent.none);
            EditorGUI.LabelField(new Rect(rect.x + 90, rect.y, 40, EditorGUIUtility.singleLineHeight), "To");
            string[] displays = System.Enum.GetNames(typeof(Mover.Speed));
            int[] values = { 5, 15, 30, 50, 100, 200 };
            if (command.setType == MovementCommand.SetTypes.Animation)
            {
                displays = System.Enum.GetNames(typeof(Mover.aSpeed));
                values = new int[] { 0, 5, 10, 20, 30, 40 };
            }
            command.int_1 = EditorGUI.IntPopup(
                new Rect(rect.x + 120, rect.y, rect.width - 160, EditorGUIUtility.singleLineHeight),
                command.int_1, displays, values);
            break;
        case MovementCommand.CommandTypes.Note:
            GUIStyle s = new GUIStyle(GUI.skin.box);
            s.wordWrap = true;
            s.alignment = TextAnchor.UpperLeft;
            command.targetName = EditorGUI.TextField(new Rect(rect.x - 2, rect.y, rect.width, EditorGUIUtility.singleLineHeight * 3 + 9), 
                command.targetName, s);
            command.lines += 2;
            break;
        case MovementCommand.CommandTypes.Sync:
            Rect rect2 = rect;
            rect2.width = 100;
            command.int_1 = EditorGUI.IntPopup(rect2, command.int_1,
                new string[] { "Await", "Notify" }, new int[] { 0, 1 });
            if (command.int_1 == 1)
                EditorGUI.PropertyField(new Rect(rect.x + 100, rect.y, rect.width - 100, EditorGUIUtility.singleLineHeight),
                                      element.FindPropertyRelative("moverTarget"), new GUIContent(""));
            break;
        // ----------------------------------
        // NEW COMMAND DISPLAY LOGIC HERE
        // ----------------------------------
        default:
            EditorGUI.LabelField(rect, "ERROR");
            break;
        }
    }



    void DrawHeaderInfo() {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"),
            true, new GUILayoutOption[0]);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moverName"),
            new GUIContent("Tag", "Tag to distinguish from other movers."));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("directions"),
            new GUIContent("Directions", "4 cardinal directions, 8, or 360 degrees of freedom."));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("move_speed"),
            new GUIContent("Move Speed", "How quick to move."));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("animation_speed"),
            new GUIContent("Animation Speed", "Animation speed multiplier."));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("facing"),
            new GUIContent("Facing Angle", "0 is up/north, 90 is right/east"));
        EditorGUI.indentLevel = 1;
        

        showOptions = EditorGUILayout.Foldout(showOptions, "Options");
        if (showOptions)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("repeat"), new GUIContent("Repeat",
                "Repeat loops to start. Ping-pong advances to end, then start, then end. ResetAndLoop reverts to start position and then loops."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reverse"),
                new GUIContent("Reverse", "When True, nextNode-- rather than ++"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("alwaysAnimate"),
                new GUIContent("Always Animate", "Play animation even when not moving?"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lockFacing"),
                new GUIContent("Lock Facing", "Facing will not change when true."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ignore_impossible"),
                new GUIContent("Skip Impossible", "On impossible move commands, advance to next command."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("affectedBySlope"),
                new GUIContent("Affected by Slope", "Slope areas will affect this mover?"));
        }
        // Collider Options
        showCollisionOptions = EditorGUILayout.Foldout(showCollisionOptions, "Collision Options");
        if (showCollisionOptions)
        {
            serializedObject.FindProperty("spread").floatValue =
                EditorGUILayout.Slider("Spread %:", serializedObject.FindProperty("spread").floatValue, 0.5f, 3);
            serializedObject.FindProperty("stop_range").floatValue =
                EditorGUILayout.Slider("Stop At", serializedObject.FindProperty("stop_range").floatValue, 0, 4);
            serializedObject.FindProperty("radius").floatValue =
                EditorGUILayout.Slider("Radius", serializedObject.FindProperty("radius").floatValue, 0, 10);
            serializedObject.FindProperty("ray_density").intValue =
                EditorGUILayout.IntSlider("Ray Count", serializedObject.FindProperty("ray_density").intValue, 1, 5);
        }
        // Advanced Options
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options");
        if (showAdvancedOptions)
        {
            // myScript.collisionMask = EditorGUILayout.LayerField(new GUIContent("Collides", "Layers to collide with"), myScript.collisionMask);
            myScript.collisionLayerMask = Utils.LayerMaskField("Collision Layers", myScript.collisionLayerMask);

            EditorGUI.indentLevel = 2;

            EditorGUILayout.LabelField("Command id: " + myScript.currentCommandIndex);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("JSON");
            json = EditorGUILayout.TextArea(json, GUILayout.Height(24), GUILayout.Width(300));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("To JSON", GUILayout.Width(120)))
            {
                json = myScript.iSave();
                // Remove stored position
                int start = json.IndexOf(", \"savePosition\":");
                int end = json.IndexOf("}", start);
                json = json.Remove(start, end-start+1);
                // copy to clipboard
                TextEditor te = new TextEditor();
                te.text = json;
                te.SelectAll();
                te.Copy(); 
            }
            if (GUILayout.Button("From JSON", GUILayout.Width(120)) && json.Length > 200)
            {
                if (EditorUtility.DisplayDialog("Are you sure?", "Load from given JSON string?", "Yes", "No"))
                {
                    try { myScript.iLoad(json); }
                    catch (System.ArgumentException)
                    {
                        Debug.LogError("Corrupt JSON: " + json);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel = 1;
        }
        EditorGUILayout.Space();
    }


    void NewCommand(object data) {
        int type = System.Convert.ToInt32(data);
        MovementCommand newCommand = new MovementCommand((MovementCommand.CommandTypes)type);
        AddCommand(newCommand, myScript.commands.Count);
    }


    void AddCommand(MovementCommand command, int index=0) {
        AddCommand(new List<MovementCommand>() { command }, index);
    }

    void AddCommand(List<MovementCommand> commands, int index=0) {
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
        for (int i=0; i < commands.Count; i++)
        {
            myScript.commands.Insert(index+i, commands[i]);
        }
        if (myScript.commands.Count == commands.Count)
            selected.Clear();
        serializedObject.ApplyModifiedProperties();
    }

    #region todo
    void _DrawRow(ref int index, int buttons_per_row) {
        index++;
        if (index % buttons_per_row != 0)
            return;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
	}
    #endregion

    void ContextMenu(Rect area, bool left_click_allowed=false) {
		Event e = Event.current;
		// Did user right click in the target area?
		if (area.Contains(e.mousePosition) && e.type == EventType.MouseDown) {
			if (!left_click_allowed && e.button != 1)
				return;
			Event.current.Use();
			GenericMenu menu = new GenericMenu ();
            // 'Commands' Menu

            menu.AddItem(new GUIContent("Visibility/Show All"), false, VisiblityControls, true);
            menu.AddItem(new GUIContent("Visibility/Hide All"), false, VisiblityControls, false);
            // Selection
            menu.AddItem(new GUIContent("Select/All"), false, Select, true);
            menu.AddItem(new GUIContent("Select/None"), false, Select, false);
            // Cut
            if (selected.Count > 0)
                //TODO - add cut
                menu.AddItem(new GUIContent("Cut"), false, Copy, new object[] {-1, true});
            else if (myScript.commands.Count > 0)
                menu.AddItem(new GUIContent("Cut All"), false, Copy, new object[] { -1, true });
            else
                menu.AddDisabledItem(new GUIContent("Cut"));
            // Copy
            if (selected.Count > 0)
                //TODO - add cut
                menu.AddItem(new GUIContent("Copy"), false, Copy, new object[] { -1, false });
            else if (myScript.commands.Count > 0)
                menu.AddItem(new GUIContent("Copy All"), false, Copy, new object[] { -1, false });
            else
                menu.AddDisabledItem(new GUIContent("Copy"));
            // Paste    
            if (EditorPrefs.GetString("BKBMoverCopyData") != "")
            {
                if (myScript.commands.Count > 0)
                {
                    menu.AddItem(new GUIContent("Paste/Top"), false, Paste, (0));
                    menu.AddItem(new GUIContent("Paste/Bottom"), false, Paste, (myScript.commands.Count));
                }
                else
                    menu.AddItem(new GUIContent("Paste"), false, Paste, (0));

            }
            else
                menu.AddDisabledItem(new GUIContent("Paste"));

			menu.ShowAsContext ();
		}
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


    //menu helpers
    void VisiblityControls(object show) {
		bool visable = System.Convert.ToBoolean(show);
		for (int i = 0; i < myScript.commands.Count; i++) {
            myScript.commands[i].expandedInspector = visable;
		}
	}

    void Select(object data) {
        if (System.Convert.ToBoolean(data))
            for (int i = 0; i < myScript.commands.Count; i++)
                selected.Add(i);
        else
            selected.Clear();
    }

    void Copy(object data) {
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
        EditorPrefs.SetString("BKBMoverCopyData", json.ToString());
        if (cut)
        {
            for (int i = myScript.commands.Count-1; i >= 0 ; i--)
                if (selection.Remove(i))
                    myScript.commands.RemoveAt(i);
            selected.Clear();
        }
    }


    void Paste(object data) {
        int index = System.Convert.ToInt32(data);
        JSONNode json = JSON.Parse(EditorPrefs.GetString("BKBMoverCopyData"));
        List<MovementCommand> commands = new List<MovementCommand>();
        foreach(JSONNode j in json.Children)
            commands.Add(JsonUtility.FromJson<MovementCommand>(j.ToString()));
        AddCommand(commands, index);
    }

}