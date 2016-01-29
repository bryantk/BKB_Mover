﻿using UnityEngine;
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
        // Check if InstanceID changed, if so deep copy Command List
        int id = myScript.GetInstanceID();
        if (myScript.myID != id)
        {
            myScript.myID = id;
            DeepCloneCommands();
        }
    }


    void DeepCloneCommands() {
        List<MovementCommand> commands = new List<MovementCommand>();
        foreach (MovementCommand c in myScript.commands)
        {
            commands.Add(ScriptableObject.Instantiate(c));
        }
        myScript.commands = commands;
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

            var command_type = myScript.commands[i].GetType();
            if (command_type == typeof(MovementCommand_Wait))
            {
                MovementCommand_Wait command = ((MovementCommand_Wait)myScript.commands[i]);
                Handles.Label(lastPos + textOffset, i.ToString() + " : Wait: " + command.time.ToString(), style);
                textOffset += offsetAmount;
            }
            else if (command_type == typeof(MovementCommand_Move) || command_type == typeof(MovementCommand_Face))
            {
                // Reset Text Offset
                textOffset = Vector3.right * 0.15f + Vector3.down * 0.1f;
                MovementCommand_Move command = ((MovementCommand_Move)myScript.commands[i]);
                Vector3 target = lastPos;
                switch (command.move_type)
                {
                case MovementCommand_Move.MoverTypes.Relative:
                    target += (Vector3)command.target;
                    if (command.instant)
                        Utils.DrawDottedArrow(lastPos, target, Color.blue);
                    else
                        Utils.DrawArrow(lastPos, target, Color.blue);
                    Vector3 temp = target;
                    temp = Handles.FreeMoveHandle(temp, Quaternion.identity, 0.125f, Vector3.one, Handles.SphereCap);
                    command.target = temp - lastPos;
                    break;
                case MovementCommand_Move.MoverTypes.Absolute:
                    target = (Vector3)command.target;
                    if (command.instant)
                        Utils.DrawDottedArrow(lastPos, target, Color.red);
                    else
                        Utils.DrawArrow(lastPos, target, Color.red);
                    command.target = Handles.FreeMoveHandle(command.target, Quaternion.identity, 0.125f, Vector3.one, Handles.SphereCap);
                    break;
                case MovementCommand_Move.MoverTypes.To_transform:
                case MovementCommand_Move.MoverTypes.ObjName:
                    if (command.transformTarget != null)
                        target = command.transformTarget.position;
                    if (command.instant)
                        Utils.DrawDottedArrow(lastPos, target, Color.green);
                    else
                        Utils.DrawArrow(lastPos, target, Color.green);
                    break;
                case MovementCommand_Move.MoverTypes.Angle:
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
                if (myScript.advanceDebugDraw && command.move_type != MovementCommand_Move.MoverTypes.Angle)
                {
                    // Within Distance.
                    Vector3 withinRadius = lastPos + new Vector3(command.withinDistance, 0, 0);
                    Vector3 withinSlider = Handles.Slider(withinRadius, Vector3.right, 0.75f, Handles.ArrowCap, 1);
                    command.withinDistance = Mathf.Max((withinSlider - lastPos).x, 0);
                    if (command.withinDistance > 0)
                    {
                        Handles.Disc(Quaternion.identity, lastPos, Vector3.forward, command.withinDistance, true, 1);
                        Handles.DrawDottedLine(lastPos, withinRadius, 2.5f);
                    }
                    // Within Random.
                    if (command.randomType == MovementCommand_Move.RandomTypes.Area)
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
                    else if (command.randomType == MovementCommand_Move.RandomTypes.Linear)
                    {
                        // TODO - Draw line alone path
                    }
                    
                }
                // Labels.
                Handles.Label(lastPos + textOffset, i.ToString() + " : Move", style);
                textOffset += offsetAmount;
            }
            else if (command_type == typeof(MovementCommand_GOTO))
            {
                MovementCommand_GOTO command = ((MovementCommand_GOTO)myScript.commands[i]);
                Handles.Label(lastPos + textOffset, i.ToString() + ": GOTO: " + command.gotoId, style);
                textOffset += offsetAmount;
            }
            else if (command_type == typeof(MovementCommand_Bool))
            {
                Handles.Label(lastPos + textOffset, i.ToString() + ": Bool", style);
                textOffset += offsetAmount;
            }
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
            myScript.Pause = EditorGUILayout.Toggle("Paused", myScript.Pause);
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
        // Script options
        myScript.showScripts = EditorGUILayout.Foldout(myScript.showScripts, "Script Calls");
        if (myScript.showScripts)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onComplete"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eventA"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eventB"));
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
                "", command.command_type, GUILayout.Width(75));
            // if user changed command type, show options
            if (commandType != command.command_type) {
                _SwapCommand(commandType, i);
                return;
            }
            // Show quick info of command
            string stats = "";
            switch (command.command_type) {
            case MovementCommand.CommandTypes.Move:
            case MovementCommand.CommandTypes.Face:
                MovementCommand_Move move_command = (MovementCommand_Move)command;
                switch (move_command.move_type) {
                case MovementCommand_Move.MoverTypes.To_transform:
                    if (move_command.transformTarget != null)
                        stats = "TO: " + move_command.transformTarget.name;
                    else
                        stats = "TO: NULL";
                    break;
                case MovementCommand_Move.MoverTypes.ObjName:
                    stats = "TO: " + move_command.targetName;
                    break;
                default:
                    Vector2 norm = move_command.target.normalized;
                    if (norm == Vector2.up)
                        stats = "Up " + move_command.target.y;
                    else if (norm == Vector2.right)
                        stats = "Right " + move_command.target.x;
                    else if (norm == Vector2.down)
                        stats = "Down " + Mathf.Abs(move_command.target.y);
                    else if (norm == Vector2.left)
                        stats = "Left " + Mathf.Abs(move_command.target.x);
                    else
                        stats = move_command.target.ToString() + " " + move_command.move_type.ToString();
                    break;
                case MovementCommand_Move.MoverTypes.Angle:
                    if (move_command.offsetAngle == 0)
                        stats = "Forward " + move_command.maxStep;
                    else if (move_command.offsetAngle == 90)
                        stats = "Right " + move_command.maxStep;
                    else if (move_command.offsetAngle == 180)
                        stats = "Back " + move_command.maxStep;
                    else if (move_command.offsetAngle == 270)
                        stats = "Left " + move_command.maxStep;
                    else
                        stats =  "Move " + move_command.maxStep + " at " + move_command.offsetAngle + " degrees" ;
                    break;
                }
                if (move_command.withinDistance > 0)
                    stats = "*" + stats;
                if (move_command.instant)
                    stats = "!" + stats;
                break;
            case MovementCommand.CommandTypes.Wait:
                stats = ((MovementCommand_Wait)command).time.ToString() + " seconds";
                break;
            case MovementCommand.CommandTypes.Boolean:
                MovementCommand_Bool boolCommand = (MovementCommand_Bool)command;
                stats = string.Format("{0} : {1}", boolCommand.flag, boolCommand.Bool);
                if (boolCommand.flag == MovementCommand_Bool.FlagType.Script)
                    stats = string.Format("Call Events {0}", boolCommand.Bool ? "A" : "B");
                break;
            case MovementCommand.CommandTypes.GoTo:
                stats = "GoTo command " + ((MovementCommand_GOTO)command).gotoId.ToString();
                break;
            default:
                stats = command.command_type.ToString() + " not implemented";
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
                switch (command.command_type) {
                // Move command
                case MovementCommand.CommandTypes.Move:
                    MovementCommand_Move move_command = (MovementCommand_Move)command;
                    move_command.move_type = (MovementCommand_Move.MoverTypes)EditorGUILayout.EnumPopup(
                        "", move_command.move_type, GUILayout.Width(135));
                    // Random choices and values
                    string[] displayText = { "None", "Linear", "Area" };
                    int[] index = { 0, 1, 2 };
                    // Types
                    switch (move_command.move_type) {
                    case MovementCommand_Move.MoverTypes.Relative:
                    case MovementCommand_Move.MoverTypes.Absolute:
                        move_command.target = EditorGUILayout.Vector2Field("Destination", move_command.target);
                        break;
                    case MovementCommand_Move.MoverTypes.To_transform:
                        move_command.transformTarget = EditorGUILayout.ObjectField("Target",
                                                                              move_command.transformTarget,
                                                                              typeof(Transform), true) as Transform;
                        move_command.recalculate = EditorGUILayout.Toggle("Re-adjust Target", move_command.recalculate);
                        break;
                    case MovementCommand_Move.MoverTypes.ObjName:
                        move_command.targetName = EditorGUILayout.TextField("Target Name", move_command.targetName);
                        if (move_command.targetName != "") {
                            GameObject obj = GameObject.Find(move_command.targetName);
                            if (obj != null)
                                move_command.transformTarget = obj.transform;
                        }
                        move_command.recalculate = EditorGUILayout.Toggle("Re-adjust Target", move_command.recalculate);
                        break;
                    case MovementCommand_Move.MoverTypes.Angle:
                        // TODO - angle property field?
                        displayText = new string[] { "None", "Linear" };
                        index = new int[] { 0, 1 };
                        if (move_command.randomType == MovementCommand_Move.RandomTypes.Area)
                            move_command.randomType = MovementCommand_Move.RandomTypes.Linear;
                        string[] display = { "Forward", "Right", "Left", "Backwards"};
                        int[] degrees = { 0, 90, -90, 180 };
                        move_command.offsetAngle = (float)EditorGUILayout.IntPopup("Direction", (int)move_command.offsetAngle, display, degrees);
                        move_command.offsetAngle = EditorGUILayout.FloatField("Angle", move_command.offsetAngle);
                        move_command.offsetAngle = Utils.ClampAngle(move_command.offsetAngle, (int)myScript.directions);
                        move_command.maxStep = EditorGUILayout.FloatField(new GUIContent("Distance", "Distance to move."), move_command.maxStep);
                        break;
                    default:
                        break;
                    }
                    // Extra Move Command options
                    if (move_command.move_type != MovementCommand_Move.MoverTypes.Angle)
                    {
                        move_command.maxStep = Mathf.Max(EditorGUILayout.FloatField(new GUIContent("Max Distance", "Maximum distance to move. 0 = no limit."), move_command.maxStep), 0);
                        move_command.withinDistance = Mathf.Max(EditorGUILayout.FloatField("Stop Within", move_command.withinDistance), 0);
                        // TODO -  debug and clean this up
                        move_command.instant = EditorGUILayout.Toggle("Teleport", move_command.instant);
                    }
                    move_command.randomType = (MovementCommand_Move.RandomTypes)EditorGUILayout.IntPopup(
                        "Random", (int)move_command.randomType, displayText, index);

                    if (move_command.randomType != MovementCommand_Move.RandomTypes.None)
                    {
                        EditorGUI.indentLevel = 2;
                        move_command.random = EditorGUILayout.Vector2Field(new GUIContent("", "Move an extra X to Y from target destination."), move_command.random);
                        EditorGUI.indentLevel = 1;
                    }
                    
                    break;

                // Facing commands
                case MovementCommand.CommandTypes.Face:
                    MovementCommand_Face face_command = (MovementCommand_Face)command;
                    face_command.move_type = (MovementCommand_Move.MoverTypes)EditorGUILayout.EnumPopup(
                        "", face_command.move_type, GUILayout.Width(135));
                    // Random choices and values
                    string[] displayTextFace = { "Constant", "Between", "Between and Between" };
                    int[] indexFace = { 0, 1, 2 };
                    // Types
                    switch (face_command.move_type)
                    {
                    case MovementCommand_Move.MoverTypes.Relative:
                    case MovementCommand_Move.MoverTypes.Absolute:
                        face_command.target = EditorGUILayout.Vector2Field("Destination", face_command.target);
                        break;
                    case MovementCommand_Move.MoverTypes.To_transform:
                        face_command.transformTarget = EditorGUILayout.ObjectField("Target",
                                                                              face_command.transformTarget,
                                                                              typeof(Transform), true) as Transform;
                        
                        break;
                    case MovementCommand_Move.MoverTypes.ObjName:
                        face_command.targetName = EditorGUILayout.TextField("Target Name", face_command.targetName);
                        if (face_command.targetName != "")
                        {
                            GameObject obj = GameObject.Find(face_command.targetName);
                            if (obj != null)
                                face_command.transformTarget = obj.transform;
                        }
                        break;
                    case MovementCommand_Move.MoverTypes.Angle:
                        // TODO - angle property field?
                        string[] display = { "Forward", "Right", "Left", "Backwards" };
                        int[] degrees = { 0, 90, -90, 180 };
                        face_command.offsetAngle = (float)EditorGUILayout.IntPopup("Direction", (int)face_command.offsetAngle, display, degrees);
                        break;
                    default:
                        break;
                    }
                    face_command.maxStep = 1;
                    face_command.randomType = (MovementCommand_Move.RandomTypes)EditorGUILayout.IntPopup(
                        "Offset", (int)face_command.randomType, displayTextFace, indexFace);
                    EditorGUI.indentLevel = 2;
                    if (face_command.randomType == MovementCommand_Move.RandomTypes.None)
                    {
                        face_command.offsetAngle = EditorGUILayout.FloatField("Angle", face_command.offsetAngle);
                        face_command.offsetAngle = Utils.ClampAngle(face_command.offsetAngle, (int)myScript.directions);
                    } else if (face_command.randomType == MovementCommand_Move.RandomTypes.Linear)
                    {
                        face_command.random = EditorGUILayout.Vector2Field(new GUIContent("between", "Choose random offset between X and Y."), face_command.random);
                    }
                    else
                    {
                        face_command.random = EditorGUILayout.Vector2Field(new GUIContent("between", "Choose random offset between X and Y."), face_command.random);
                        EditorGUI.indentLevel = 3;
                        EditorGUILayout.LabelField("and");
                        EditorGUI.indentLevel = 2;
                        face_command.random2 = EditorGUILayout.Vector2Field(new GUIContent("between", "Choose random offset between X and Y."), face_command.random2);
                    }
                    EditorGUI.indentLevel = 1;
                    break;

                // Wait Command
                case MovementCommand.CommandTypes.Wait:
                    MovementCommand_Wait wait_command = (MovementCommand_Wait)command;
                    wait_command.time = EditorGUILayout.FloatField("Seconds:", wait_command.time, GUILayout.Width(185));
                    break;
                case MovementCommand.CommandTypes.Boolean:
                    MovementCommand_Bool boolCommand = (MovementCommand_Bool)command;
                    GUILayout.BeginHorizontal();
                    boolCommand.flag = (MovementCommand_Bool.FlagType)EditorGUILayout.EnumPopup("", boolCommand.flag, GUILayout.Width(135));
                    if (boolCommand.flag == MovementCommand_Bool.FlagType.Pause)
                        boolCommand.Bool = true;
                    else
                        boolCommand.Bool = EditorGUILayout.Toggle("", boolCommand.Bool);
                    GUILayout.EndHorizontal();
                    break;
                case MovementCommand.CommandTypes.GoTo:
                    MovementCommand_GOTO goto_command = (MovementCommand_GOTO)command;
                    goto_command.gotoId = EditorGUILayout.IntField("Command:", goto_command.gotoId, GUILayout.Width(185));
                    goto_command.gotoId = Mathf.Clamp(goto_command.gotoId, 0, myScript.commands.Count - 1);
                    break;
                default:
                    EditorGUILayout.LabelField("ERROR");
                    break;
                }
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space();
    }

    void _SwapCommand(MovementCommand.CommandTypes commandType, int index = -1) {
        if (index >= 0 && index <= myScript.commands.Count)
            myScript.commands.RemoveAt(index);
        switch (commandType)
        {
        case MovementCommand.CommandTypes.Move:
            myScript.commands.Insert(index, ScriptableObject.CreateInstance<MovementCommand_Move>());
            break;
        case MovementCommand.CommandTypes.Face:
            myScript.commands.Insert(index, ScriptableObject.CreateInstance<MovementCommand_Face>());
            break;
        case MovementCommand.CommandTypes.Wait:
            myScript.commands.Insert(index, ScriptableObject.CreateInstance<MovementCommand_Wait>());
            break;
        case MovementCommand.CommandTypes.Boolean:
            myScript.commands.Insert(index, ScriptableObject.CreateInstance<MovementCommand_Bool>());
            break;
        case MovementCommand.CommandTypes.GoTo:
            myScript.commands.Insert(index, ScriptableObject.CreateInstance<MovementCommand_GOTO>());
            break;
        }
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
		myScript.commands.Insert(id, ScriptableObject.CreateInstance<MovementCommand_Move>());
	}

    // for move commands so far
	void QuickCommand(object data) {
		int[] args = data as int[];
		int index = args[0];

        MovementCommand_Move command = ScriptableObject.CreateInstance<MovementCommand_Move>();
        command.move_type = MovementCommand_Move.MoverTypes.Relative;
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