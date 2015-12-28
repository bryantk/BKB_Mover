using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using BKB_RPG;

[CustomEditor(typeof(Mover))]
public class Drawer_Mover : Editor
{
    //important
    Mover myScript;
    MasterMover masterMover;

    // settings
    GUIStyle style;
    float buttonWidth = 80;


    void OnEnable() {
		myScript = target as Mover;
        style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 8;
        int master_count = FindObjectsOfType<MasterMover>().Length;
		if (master_count == 0) {
			Debug.LogWarning("No MoverMasters found... Creating");
			GameObject obj = new GameObject();
			obj.AddComponent<MasterMover>();
			obj.name = "MasterMover";
		}
		if (master_count > 1)
			Debug.LogWarning(master_count.ToString() + " MoverMasters found, should be 1.");
		masterMover = FindObjectOfType<MasterMover>();
		if (myScript.commands == null) {
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
        for (int i = 0; i < myScript.commands.Count; i++) {
            // --- Command Switch -----------------------------------------------

            var command_type = myScript.commands[i].GetType();
            if (command_type == typeof(MovementCommand_Wait))
            {
                MovementCommand command = ((MovementCommand_Wait)myScript.commands[i]);
                Handles.Label(lastPos + Vector3.right * 0.15f + Vector3.down * 0.1f, 
                    ("Wait: " + ((MovementCommand_Wait)command).time.ToString()), style);
            }
            else if (command_type == typeof(MovementCommand_Move))
            {
                MovementCommand_Move command = ((MovementCommand_Move)myScript.commands[i]);
                //TO DO
                /*
                command.myVector2 = Handles.FreeMoveHandle(command.myVector2, Quaternion.identity, 0.75f, Vector3.one, Handles.ArrowCap);
                Utils.DrawHandlesArrow(lastPos, (Vector3)command.myVector2, Color.yellow);
                lastPos = (Vector3)command.myVector2;
                */
                // change color of arrow if teleport
                Vector3 target = lastPos;
                switch (command.move_type)
                {
                case MovementCommand_Move.MoverTypes.Relative:
                    target += (Vector3)command.target;
                    Utils.DrawHandlesArrow(lastPos, target, Color.blue);
                    Vector3 temp = target;
                    temp = Handles.FreeMoveHandle(temp, Quaternion.identity, 0.125f, Vector3.one, Handles.SphereCap);
                    command.target = temp - lastPos;
                    break;
                case MovementCommand_Move.MoverTypes.Absolute:
                    target = (Vector3)command.target;
                    Utils.DrawHandlesArrow(lastPos, target, Color.red);
                    // Draw Handle
                    command.target = Handles.FreeMoveHandle(command.target, Quaternion.identity, 0.125f, Vector3.one, Handles.SphereCap);
                    break;
                case MovementCommand_Move.MoverTypes.To_transform:
                case MovementCommand_Move.MoverTypes.ObjName:
                    if (command.transformTarget != null)
                        target = command.transformTarget.position;
                    Utils.DrawHandlesArrow(lastPos, target, Color.green);
                    break;
                }
                lastPos = target;
                Handles.Disc(Quaternion.identity, lastPos, Vector3.forward, command.withinDistance, true, 1);
                Handles.DrawDottedLine(lastPos, lastPos + new Vector3(command.withinDistance, 0, 0), 2.5f);
                Handles.Label(lastPos + Vector3.right * 0.15f + Vector3.down * 0.1f, i.ToString(), style);
            }
            else if (command_type == typeof(MovementCommand_GOTO))
            {
      
            }
            else if (command_type == typeof(MovementCommand_Script))
            {

            }
		}

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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true, new GUILayoutOption[0]);
        GUILayout.Label("Status", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("move_speed"), new GUIContent("Move Speed"));
        

        EditorGUILayout.PropertyField(serializedObject.FindProperty("facing"), new GUIContent("Facing Angle"));

        EditorGUI.indentLevel = 1;
        myScript.showSettings = EditorGUILayout.Foldout(myScript.showSettings, "Settings");
        if (myScript.showSettings)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("repeat"), new GUIContent("Repeat"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reverse"), new GUIContent("Reverse"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("lockFacing"), new GUIContent("Lock Facing"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ignore_impossible"), new GUIContent("Ignore Impossible"));
            myScript.Pause = EditorGUILayout.Toggle("Paused", myScript.Pause);
        }
        myScript.showOptions = EditorGUILayout.Foldout(myScript.showOptions, "Options");
        if (myScript.showOptions) {
            serializedObject.FindProperty("spread").floatValue =
                EditorGUILayout.Slider("Spread %:", serializedObject.FindProperty("spread").floatValue, 0.5f, 3);
            serializedObject.FindProperty("stop_range").floatValue =
                EditorGUILayout.Slider("Stop At", serializedObject.FindProperty("stop_range").floatValue, 0, 4);
            serializedObject.FindProperty("radius").floatValue =
                EditorGUILayout.Slider("Radius", serializedObject.FindProperty("radius").floatValue, 0, 10);
            serializedObject.FindProperty("ray_density").intValue =
                EditorGUILayout.IntSlider("Ray Count", serializedObject.FindProperty("ray_density").intValue, 1, 5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slide"), new GUIContent("Slide?"));
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
                }
                if (move_command.withinDistance > 0)
                    stats = "*" + stats;
                break;
            case MovementCommand.CommandTypes.Wait:
                stats = ((MovementCommand_Wait)command).time.ToString() + " seconds";
                break;
            // TODO
            //case MovementCommand.CommandTypes.Teleport:
            //    stats = "Teleport to " + command.myVector2.ToString();
            //    break;
            case MovementCommand.CommandTypes.Boolean:
                MovementCommand_Bool boolCommand = (MovementCommand_Bool)command;
                stats = string.Format("{0} : {1}", boolCommand.flag, boolCommand.Bool);
                break;
            case MovementCommand.CommandTypes.GoTo:
                stats = "GoTo command " + ((MovementCommand_GOTO)command).gotoId.ToString();
                break;
            case MovementCommand.CommandTypes.Script:
                MovementCommand_Script events = (MovementCommand_Script)command;
                stats = ((events.events != null) ? events.events.GetPersistentEventCount() : 0).ToString() + " calls";
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
                switch (command.command_type) {
                // Move command
                case MovementCommand.CommandTypes.Move:
                    MovementCommand_Move move_command = (MovementCommand_Move)command;
                    move_command.move_type = (MovementCommand_Move.MoverTypes)EditorGUILayout.EnumPopup(
                        "", move_command.move_type, GUILayout.Width(135));
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
                    default:
                        break;
                    }
                    move_command.instant = EditorGUILayout.Toggle("Teleport", move_command.instant);
                    move_command.withinDistance = Mathf.Max(EditorGUILayout.FloatField("Stop Within", move_command.withinDistance), 0);
                    
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
                //TODO
                //case MovementCommand.CommandTypes.Teleport:
                //    command.myVector2 = EditorGUILayout.Vector2Field("Destination", command.myVector2);
                //    break;
                case MovementCommand.CommandTypes.Script:
                    MovementCommand_Script scriptCommand = (MovementCommand_Script)command;
                    SerializedObject o = new SerializedObject(scriptCommand);
                    EditorGUILayout.PropertyField(o.FindProperty("events"), new GUIContent("calls"), GUILayout.Width(275));
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
        case MovementCommand.CommandTypes.Wait:
            myScript.commands.Insert(index, ScriptableObject.CreateInstance<MovementCommand_Wait>());
            break;
        case MovementCommand.CommandTypes.Boolean:
            myScript.commands.Insert(index, ScriptableObject.CreateInstance<MovementCommand_Bool>());
            break;
        case MovementCommand.CommandTypes.GoTo:
            myScript.commands.Insert(index, ScriptableObject.CreateInstance<MovementCommand_GOTO>());
            break;
        case MovementCommand.CommandTypes.Script:
            myScript.commands.Insert(index, ScriptableObject.CreateInstance<MovementCommand_Script>());
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