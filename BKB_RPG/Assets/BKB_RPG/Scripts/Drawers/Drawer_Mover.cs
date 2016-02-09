using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using BKB_RPG;

[CustomEditor(typeof(Mover))]
public class Drawer_Mover : Editor
{
    //important
    Mover myScript;
    EntityMaster masterMover;

    // settings
    GUIStyle style;
    float buttonWidth = 80;
    MovementCommand toCopy;


    void OnEnable() {
		myScript = target as Mover;
        style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 8;

        int master_count = FindObjectsOfType<EntityMaster>().Length;
		if (master_count == 0) {
			Debug.LogWarning("No MoverMasters found... Creating");
			GameObject obj = new GameObject();
			obj.AddComponent<EntityMaster>();
			obj.name = "MasterMover";
		}
		if (master_count > 1)
			Debug.LogWarning(master_count.ToString() + " MoverMasters found, should be 1.");
		masterMover = FindObjectOfType<EntityMaster>();
		if (myScript.commands == null) {
            Debug.LogWarning("Created command list");
            myScript.commands = new List<MovementCommand>();
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
                    Vector3 temp2 = target;
                    temp2 = Handles.FreeMoveHandle(temp2, Quaternion.identity, 0.125f, Vector3.one, Handles.SphereCap);
                    temp2 = temp2 - lastPos;
                    command.maxStep = temp2.magnitude;
                    command.offsetAngle = Utils.Vector2toAngle((Vector2)temp2);
                    if (command.instant)
                        Utils.DrawDottedArrow(lastPos, target, Color.cyan);
                    else
                        Utils.DrawArrow(lastPos, target, Color.cyan);
                    break;
                }
                lastPos = target;
                if (myScript.advanceDebugDraw && command.move_type != MovementCommand.MoverTypes.Angle)
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
                Handles.Label(lastPos + textOffset, i.ToString() + ": GOTO: " + command.gotoId, style);
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
                Handles.Label(lastPos + textOffset, i.ToString() + ": Pop" + command.gotoId, style);
                textOffset += offsetAmount;
            }
            else if (command.commandType == MovementCommand.CommandTypes.Set)
            {
                Handles.Label(lastPos + textOffset, i.ToString() + ": Set " + command.setType.ToString(), style);
                textOffset += offsetAmount;
            }
            // ----------------------------------
            // NEW COMMAND SCENE DISPLAY LOGIC
            // ----------------------------------
        }
        Repaint();
    }

	public override void OnInspectorGUI() {
		// Follow this template
		serializedObject.Update();
        DrawHeaderInfo();
        DrawCommands();    
        DrawQuickCommands();
        serializedObject.ApplyModifiedProperties();
        SceneView.RepaintAll();
    }


    void DrawHeaderInfo() {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"),
            true, new GUILayoutOption[0]);
        GUILayout.Label("Status", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("directions"),
            new GUIContent("Directions", "4 cardinal directions, 8, or 360 degrees of freedom."));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("move_speed"),
            new GUIContent("Move Speed", "How quick to move."));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("animation_speed"),
            new GUIContent("Animation Speed", "Animation speed multiplier."));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("facing"),
            new GUIContent("Facing Angle", "0 is up/north, 90 is right/east"));
        EditorGUI.indentLevel = 1;
        myScript.showSettings = EditorGUILayout.Foldout(myScript.showSettings, "Options");
        if (myScript.showSettings)
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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slide"), new GUIContent("Slide on hit?"));
        }
        // Advanced Options
        myScript.showOptions = EditorGUILayout.Foldout(myScript.showOptions, "Advanced");
        if (myScript.showOptions)
        {
            myScript.compelete = EditorGUILayout.Toggle("Paused", myScript.compelete);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("advanceDebugDraw"),
                new GUIContent("Advacned Debug", "Show advanced options in inspector GUI."));
            EditorGUILayout.LabelField("Collision Settings:");
            EditorGUI.indentLevel = 2;
            serializedObject.FindProperty("spread").floatValue =
                EditorGUILayout.Slider("Spread %:", serializedObject.FindProperty("spread").floatValue, 0.5f, 3);
            serializedObject.FindProperty("stop_range").floatValue =
                EditorGUILayout.Slider("Stop At", serializedObject.FindProperty("stop_range").floatValue, 0, 4);
            serializedObject.FindProperty("radius").floatValue =
                EditorGUILayout.Slider("Radius", serializedObject.FindProperty("radius").floatValue, 0, 10);
            serializedObject.FindProperty("ray_density").intValue =
                EditorGUILayout.IntSlider("Ray Count", serializedObject.FindProperty("ray_density").intValue, 1, 5);
            EditorGUI.indentLevel = 1;
        }

    }

    void DrawCommands() {
        // setup a scroll box for movement commands
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Label("Commands", EditorStyles.boldLabel);
        float y = GUILayoutUtility.GetLastRect().y;
        GUI.Label(new Rect(176, y, 70, 15), "Current: " + myScript.currentNode.ToString());
        GUI.Box(new Rect(105, y, 15, 15), "");
        GUI.Label(new Rect(107, y, 18, 18), "?");
        myContextMenu(new Rect(105, y, 25, 25), 0, myScript.commands.Count, true);
        // -------------------------------------------------------------------------------------------------------------
        myScript.scrollPos = EditorGUILayout.BeginScrollView(myScript.scrollPos, false, true, GUILayout.Height(200));
        EditorGUI.indentLevel = 1;
        // List each command
        for (int i = 0; i < myScript.commands.Count; i++) {
            MovementCommand command = myScript.commands[i];
            EditorGUILayout.BeginHorizontal(GUILayout.Width(50));
            command.expandedInspector = EditorGUILayout.Foldout(command.expandedInspector, i.ToString() + ":");
            Rect rt = GUILayoutUtility.GetLastRect();
            GUI.Box(new Rect(43, rt.y, 15, 15), "");
            GUI.Label(new Rect(45, rt.y, 18, 18), "?");
            myContextMenu(new Rect(43, rt.y, 15, 15), 1, i);
            MovementCommand.CommandTypes commandType = (MovementCommand.CommandTypes)EditorGUILayout.EnumPopup(
                "", command.commandType, GUILayout.Width(75));
            // if user changed command type, show options
            if (commandType != command.commandType) {
                myScript.commands[i].SetMovementCommand(commandType);
                return;
            }
            // Show quick info of command
            string stats = "";
            switch (command.commandType) {
            case MovementCommand.CommandTypes.Move:
            case MovementCommand.CommandTypes.Face:
                switch (command.move_type) {
                case MovementCommand.MoverTypes.To_transform:
                    if (command.transformTarget != null)
                        stats = "TO: " + command.transformTarget.name;
                    else
                        stats = "TO: NULL";
                    break;
                case MovementCommand.MoverTypes.ObjName:
                    stats = "TO: " + command.targetName;
                    break;
                default:
                    Vector2 norm = command.target.normalized;
                    if (norm == Vector2.up)
                        stats = "Up " + command.target.y;
                    else if (norm == Vector2.right)
                        stats = "Right " + command.target.x;
                    else if (norm == Vector2.down)
                        stats = "Down " + Mathf.Abs(command.target.y);
                    else if (norm == Vector2.left)
                        stats = "Left " + Mathf.Abs(command.target.x);
                    else
                        stats = command.target.ToString() + " " + command.move_type.ToString();
                    break;
                case MovementCommand.MoverTypes.Angle:
                    if (command.offsetAngle == 0)
                        stats = "Forward " + command.maxStep;
                    else if (command.offsetAngle == 90)
                        stats = "Right " + command.maxStep;
                    else if (command.offsetAngle == 180)
                        stats = "Back " + command.maxStep;
                    else if (command.offsetAngle == 270)
                        stats = "Left " + command.maxStep;
                    else
                        stats =  "Move " + command.maxStep + " at " + command.offsetAngle + " degrees" ;
                    break;
                }
                if (command.withinDistance > 0)
                    stats = "*" + stats;
                if (command.instant)
                    stats = "!" + stats;
                break;
            case MovementCommand.CommandTypes.Wait:
                stats = command.time.ToString() + " seconds";
                break;
            case MovementCommand.CommandTypes.Boolean:
                stats = string.Format("{0} : {1}", command.flag, command.Bool);
                // TODO - remove
                if (command.flag == MovementCommand.FlagType.Script)
                    stats = string.Format("Call Events {0}", command.Bool ? "A" : "B");
                break;
            case MovementCommand.CommandTypes.GoTo:
                stats = "GoTo command " + command.gotoId.ToString();
                break;
            case MovementCommand.CommandTypes.Script:
                stats = "Call scripts: " + command.scriptCalls.GetPersistentEventCount();
                break;
            case MovementCommand.CommandTypes.Remove:
                stats = "Remove " + command.gotoId + " commands";
                break;
            case MovementCommand.CommandTypes.Set:
                stats = "Set " + command.setType.ToString() + " - " + command.gotoId.ToString();
                break;
            // ----------------------------------
            // COMMAND QUICK VIEW
            // ----------------------------------
            default:
                stats = command.commandType.ToString() + " not implemented";
                break;
            }
            EditorGUILayout.LabelField(stats, GUILayout.Width(175));
            EditorGUILayout.EndHorizontal();
            // Draw layout for each type
            if (command.expandedInspector) {
                //------------------------------------------------------------------------------------------------------
                // C O M M A N D   E D I T I N G
                //------------------------------------------------------------------------------------------------------
                // TODO - move all names/tooltips to definitions file?
                switch (command.commandType) {
                // Move command
                case MovementCommand.CommandTypes.Move:
                    command.move_type = (MovementCommand.MoverTypes)EditorGUILayout.EnumPopup(
                        "", command.move_type, GUILayout.Width(135));
                    // Random choices and values
                    string[] displayText = { "None", "Linear", "Area" };
                    int[] index = { 0, 1, 2 };
                    // Types
                    switch (command.move_type) {
                    case MovementCommand.MoverTypes.Relative:
                    case MovementCommand.MoverTypes.Absolute:
                        command.target = EditorGUILayout.Vector2Field("Destination", command.target);
                        break;
                    case MovementCommand.MoverTypes.To_transform:
                        command.transformTarget = EditorGUILayout.ObjectField("Target",
                                                                              command.transformTarget,
                                                                              typeof(Transform), true) as Transform;
                        command.recalculate = EditorGUILayout.Toggle("Re-adjust Target", command.recalculate);
                        break;
                    case MovementCommand.MoverTypes.ObjName:
                        command.targetName = EditorGUILayout.TextField("Target Name", command.targetName);
                        if (command.targetName != "") {
                            GameObject obj = GameObject.Find(command.targetName);
                            if (obj != null)
                                command.transformTarget = obj.transform;
                        }
                        command.recalculate = EditorGUILayout.Toggle("Re-adjust Target", command.recalculate);
                        break;
                    case MovementCommand.MoverTypes.Angle:
                        // TODO - angle property field?
                        displayText = new string[] { "None", "Linear" };
                        index = new int[] { 0, 1 };
                        if (command.randomType == MovementCommand.RandomTypes.Area)
                            command.randomType = MovementCommand.RandomTypes.Linear;
                        string[] display = { "Forward", "Right", "Left", "Backwards"};
                        int[] degrees = { 0, 90, -90, 180 };
                        command.offsetAngle = (float)EditorGUILayout.IntPopup("Direction", (int)command.offsetAngle, display, degrees);
                        command.offsetAngle = EditorGUILayout.FloatField("Angle", command.offsetAngle);
                        command.offsetAngle = Utils.ClampAngle(command.offsetAngle, (int)myScript.directions);
                        command.maxStep = EditorGUILayout.FloatField(new GUIContent("Distance", "Distance to move."), command.maxStep);
                        break;
                    default:
                        break;
                    }
                    // Extra Move Command options
                    if (command.move_type != MovementCommand.MoverTypes.Angle)
                    {
                        command.maxStep = Mathf.Max(EditorGUILayout.FloatField(new GUIContent("Max Distance", "Maximum distance to move. 0 = no limit."), command.maxStep), 0);
                        command.withinDistance = Mathf.Max(EditorGUILayout.FloatField("Stop Within", command.withinDistance), 0);
                        // TODO -  debug and clean this up
                        command.instant = EditorGUILayout.Toggle("Teleport", command.instant);
                    }
                    command.randomType = (MovementCommand.RandomTypes)EditorGUILayout.IntPopup(
                        "Random", (int)command.randomType, displayText, index);

                    if (command.randomType != MovementCommand.RandomTypes.None)
                    {
                        EditorGUI.indentLevel = 2;
                        command.random = EditorGUILayout.Vector2Field(new GUIContent("", "Move an extra X to Y from target destination."), command.random);
                        EditorGUI.indentLevel = 1;
                    }
                    
                    break;

                // Facing commands
                case MovementCommand.CommandTypes.Face:
                    command.move_type = (MovementCommand.MoverTypes)EditorGUILayout.EnumPopup(
                        "", command.move_type, GUILayout.Width(135));
                    // Random choices and values
                    string[] displayTextFace = { "Constant", "Between", "Between and Between" };
                    int[] indexFace = { 0, 1, 2 };
                    // Types
                    switch (command.move_type)
                    {
                    case MovementCommand.MoverTypes.Relative:
                    case MovementCommand.MoverTypes.Absolute:
                        command.target = EditorGUILayout.Vector2Field("Destination", command.target);
                        break;
                    case MovementCommand.MoverTypes.To_transform:
                        command.transformTarget = EditorGUILayout.ObjectField("Target",
                                                                              command.transformTarget,
                                                                              typeof(Transform), true) as Transform;
                        
                        break;
                    case MovementCommand.MoverTypes.ObjName:
                        command.targetName = EditorGUILayout.TextField("Target Name", command.targetName);
                        if (command.targetName != "")
                        {
                            GameObject obj = GameObject.Find(command.targetName);
                            if (obj != null)
                                command.transformTarget = obj.transform;
                        }
                        break;
                    case MovementCommand.MoverTypes.Angle:
                        // TODO - angle property field?
                        string[] display = { "Forward", "Right", "Left", "Backwards" };
                        int[] degrees = { 0, 90, -90, 180 };
                        command.offsetAngle = (float)EditorGUILayout.IntPopup("Direction", (int)command.offsetAngle, display, degrees);
                        break;
                    default:
                        break;
                    }
                    command.maxStep = 1;
                    command.randomType = (MovementCommand.RandomTypes)EditorGUILayout.IntPopup(
                        "Offset", (int)command.randomType, displayTextFace, indexFace);
                    EditorGUI.indentLevel = 2;
                    if (command.randomType == MovementCommand.RandomTypes.None)
                    {
                        command.offsetAngle = EditorGUILayout.FloatField("Angle", command.offsetAngle);
                        command.offsetAngle = Utils.ClampAngle(command.offsetAngle, (int)myScript.directions);
                    } else if (command.randomType == MovementCommand.RandomTypes.Linear)
                    {
                        command.random = EditorGUILayout.Vector2Field(new GUIContent("between", "Choose random offset between X and Y."), command.random);
                    }
                    else
                    {
                        command.random = EditorGUILayout.Vector2Field(new GUIContent("between", "Choose random offset between X and Y."), command.random);
                        EditorGUI.indentLevel = 3;
                        EditorGUILayout.LabelField("and");
                        EditorGUI.indentLevel = 2;
                        command.random2 = EditorGUILayout.Vector2Field(new GUIContent("between", "Choose random offset between X and Y."), command.random2);
                    }
                    EditorGUI.indentLevel = 1;
                    break;

                // Wait Command
                case MovementCommand.CommandTypes.Wait:
                    command.time = EditorGUILayout.FloatField("Seconds:", command.time, GUILayout.Width(185));
                    break;
                case MovementCommand.CommandTypes.Boolean:
                    GUILayout.BeginHorizontal();
                    command.flag = (MovementCommand.FlagType)EditorGUILayout.EnumPopup("", command.flag, GUILayout.Width(135));
                    if (command.flag == MovementCommand.FlagType.Pause)
                        command.Bool = true;
                    else
                        command.Bool = EditorGUILayout.Toggle("", command.Bool);
                    GUILayout.EndHorizontal();
                    break;
                case MovementCommand.CommandTypes.GoTo:
                    command.gotoId = EditorGUILayout.IntField("Command:", command.gotoId, GUILayout.Width(185));
                    command.gotoId = Mathf.Clamp(command.gotoId, 0, myScript.commands.Count - 1);
                    break;
                case MovementCommand.CommandTypes.Script:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("commands").GetArrayElementAtIndex(i).FindPropertyRelative("scriptCalls"));
                    break;
                case MovementCommand.CommandTypes.Remove:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("commands").GetArrayElementAtIndex(i).FindPropertyRelative("gotoId"),
                        new GUIContent("Remove:"));
                    command.gotoId = Mathf.Clamp(command.gotoId, 0, i);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("commands").GetArrayElementAtIndex(i).FindPropertyRelative("Bool"),
                        new GUIContent("Remove Self"));
                    break;
                case MovementCommand.CommandTypes.Set:
                    command.setType = (MovementCommand.SetTypes)EditorGUILayout.EnumPopup(
                        "", command.setType, GUILayout.Width(135));
                    string[] displays = System.Enum.GetNames(typeof(Mover.Speed));
                    int[] values = { 5, 15, 30, 50, 100, 200 };
                    string text = "Speed";
                    if (command.setType == MovementCommand.SetTypes.Animation)
                    {
                        displays = System.Enum.GetNames(typeof(Mover.aSpeed));
                        values = new int[] { 0, 5, 10, 20, 30, 40 };
                    }
                    command.gotoId = EditorGUILayout.IntPopup(text, command.gotoId, displays, values, GUILayout.Width(220));
                    break;
                // ----------------------------------
                // NEW COMMAND DISPLAY LOGIC HERE
                // ----------------------------------
                default:
                    EditorGUILayout.LabelField("ERROR");
                    break;
                }
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space();
    }

	void _DrawRow(ref int index, int buttons_per_row) {
        index++;
        if (index % buttons_per_row != 0)
            return;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
	}


    void DrawQuickCommands() {
        myScript.showQuickCommands = EditorGUILayout.Foldout(myScript.showQuickCommands, "Quick Commands");
        if (myScript.showQuickCommands) {
            int index = -1;
            
            int buttons_per_row = (int)(Screen.width / (buttonWidth + 7));
            EditorGUILayout.BeginHorizontal();
            _DrawRow(ref index, buttons_per_row);
            if (GUILayout.Button("Move Up", GUILayout.Width(buttonWidth))) {
                QuickCommand(new int[2] { myScript.commands.Count, 1 });
            }
            _DrawRow(ref index, buttons_per_row);
            if (GUILayout.Button("Move Right", GUILayout.Width(buttonWidth))) {
                QuickCommand(new int[2] { myScript.commands.Count, 2 });
            }
            _DrawRow(ref index, buttons_per_row);
            if (GUILayout.Button("Move Down", GUILayout.Width(buttonWidth))) {
                QuickCommand(new int[2] { myScript.commands.Count, 3 });
            }
            _DrawRow(ref index, buttons_per_row);
            if (GUILayout.Button("Move Left", GUILayout.Width(buttonWidth))) {
                QuickCommand(new int[2] { myScript.commands.Count, 4 });
            }
            _DrawRow(ref index, buttons_per_row);
            if (GUILayout.Button(Screen.width.ToString(), GUILayout.Width(buttonWidth))) {

            }
            EditorGUILayout.EndHorizontal();
        }
    }


	void myContextMenu(Rect area, int type, int id, bool left_click_allowed=false) {
		Event e = Event.current;
		// Did user right click in the target area?
		if (area.Contains(e.mousePosition) && e.type == EventType.MouseDown) {
			if (!left_click_allowed && e.button != 1)
				return;
			Event.current.Use();
			GenericMenu menu = new GenericMenu ();
			switch (type) {
			case 0:
				// 'New' Menu
				menu.AddItem (new GUIContent ("New Command"), false, InsertNewCommand, id);
				menu.AddItem (new GUIContent ("New Command/Move/Up"), false, QuickCommand, new int[2] {id, 1});
				menu.AddItem (new GUIContent ("New Command/Move/Right"), false, QuickCommand, new int[2] {id, 2});
				menu.AddItem (new GUIContent ("New Command/Move/Down"), false, QuickCommand, new int[2] {id, 3});
				menu.AddItem (new GUIContent ("New Command/Move/Left"), false, QuickCommand, new int[2] {id, 4});
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Show All"), false, VisiblityControlls, true);
				menu.AddItem (new GUIContent ("Hide All"), false, VisiblityControlls, false);
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Delete All"), false, ClearAll);
				break;
			case 1:
				// 'Modify' Button
				menu.AddItem (new GUIContent ("Move/Up"), false, Move, new int[2] {id, 1});
				menu.AddItem (new GUIContent ("Move/Down"), false, Move, new int[2] {id, 0});

				menu.AddItem (new GUIContent ("Insert/Above"), false, InsertNewCommand, (id));
				menu.AddItem (new GUIContent ("Insert/Below"), false, InsertNewCommand, (id+1));

                menu.AddItem(new GUIContent("Copy"), false, Copy, (id));
                if (toCopy == null)
                    menu.AddItem(new GUIContent("Paste"), true, Paste, (id));
                else
                {
                    menu.AddItem(new GUIContent("Paste/Above"), false, Paste, (id));
                    menu.AddItem(new GUIContent("Paste/Below"), false, Paste, (id+1));
                }
                menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Set To/Move/Up"), false, QuickCommand, new int[2] {id, 1});
				menu.AddItem (new GUIContent ("Set To/Move/Right"), false, QuickCommand, new int[2] {id, 2});
				menu.AddItem (new GUIContent ("Set To/Move/Down"), false, QuickCommand, new int[2] {id, 3});
				menu.AddItem (new GUIContent ("Set To/Move/Left"), false, QuickCommand, new int[2] {id, 4});
				menu.AddItem (new GUIContent ("Remove"), false, RemoveAt, id);
				break;
			}
			menu.ShowAsContext ();
			//SceneView.RepaintAll();
		}
	}


    //menu helpers
	void VisiblityControlls(object show) {
		bool visable = System.Convert.ToBoolean(show);
		for (int i = 0; i < myScript.commands.Count; i++) {
            myScript.commands[i].expandedInspector = visable;
		}
	}

	void ClearAll() {
		myScript.commands = new List<MovementCommand>();
	}

    void Copy(object data) {
        int id = System.Convert.ToInt32(data);
        toCopy = myScript.commands[id];
    }

    void Paste(object data) {
        int id = System.Convert.ToInt32(data);
        myScript.commands.Insert(id, toCopy.Copy());
    }

	void RemoveAt(object data) {
		int id = System.Convert.ToInt32(data);
		myScript.commands.RemoveAt(id);
	}

	void Move(object data) {
		if (myScript.commands.Count < 2)
			return;
		int[] args = data as int[];
		int id = args[0];
		bool up = args[1]==1 ? true : false;
		if (up && id == 0)
			return;
		if (!up && id == myScript.commands.Count-1)
			return;
		MovementCommand command = myScript.commands[id];
		RemoveAt(id);
		id = id - (up ? 1 : -1);
		myScript.commands.Insert(id, command);
        command.expandedInspector = false;
    }

	void InsertNewCommand(object data) {
		int id = System.Convert.ToInt32(data);
        MovementCommand newCommand = new MovementCommand(MovementCommand.CommandTypes.Move);
        myScript.commands.Insert(id, newCommand);
	}

    // for move commands so far
	void QuickCommand(object data) {
		int[] args = data as int[];
		int index = args[0];

        MovementCommand command = new MovementCommand(MovementCommand.CommandTypes.Move);
        command.SetMovementCommand(MovementCommand.CommandTypes.Move);
        command.move_type = MovementCommand.MoverTypes.Relative;
        switch (args[1]) {
		case 1:	// move up
			command.target = Vector2.up * masterMover.unitDistance;
			break;
		case 2:	// move right
			command.target = Vector2.right * masterMover.unitDistance;
			break;
		case 3:	// move right
			command.target = Vector2.down * masterMover.unitDistance;
			break;
		case 4:	// move right
			command.target = Vector2.left * masterMover.unitDistance;
			break;
		}
        myScript.commands.Insert(index, command);
    }


}