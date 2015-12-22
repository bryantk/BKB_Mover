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
    MasterMover master;

    // Scroll position of Command window
    Vector2 scrollPos;
	bool guts = false;
	bool quick_commands = false;
	List<bool> viewPathEntries = new List<bool>();

    // settings
    float button_width = 80;


    void OnEnable() {
		myScript = target as Mover;
		int master_count = FindObjectsOfType<MasterMover>().Length;
		if (master_count == 0) {
			Debug.LogWarning("No MoverMasters found... Creating");
			GameObject obj = new GameObject();
			obj.AddComponent<MasterMover>();
			obj.name = "MasterMover";
		}
		if (master_count > 1)
			Debug.LogWarning(master_count.ToString() + " MoverMasters found, should be 1.");
		master = FindObjectOfType<MasterMover>();
		viewPathEntries = new List<bool>();
		if (myScript.commands == null)
			myScript.commands = new List<MovementCommand>();
		for (int i = 0; i < myScript.commands.Count; i++) {
			viewPathEntries.Add(false);
		}
        if (myScript.startPosition == Vector3.zero)
            myScript.startPosition = myScript.transform.position;
    }

	// manage handlers and arrow display of path
	void OnSceneGUI() {
		// TODO
		Vector3 lastPos = myScript.startPosition;
        Handles.Label(lastPos - Vector3.right * 0.25f, "Start");
        for (int i = 0; i < myScript.commands.Count; i++) {
            MovementCommand command = myScript.commands[i];
            // --- Command Switch -----------------------------------------------
			switch (command.command_type) {
			case MovementCommand.CommandTypes.Move:
                // --- Move Switch -----------------------------------------------
                Vector3 target = lastPos;
                switch (command.move_type) {
                case MovementCommand.MoverTypes.Relative:
                    target += (Vector3)command.myVector2;
                    Utils.DrawHandlesArrow(lastPos, target, Color.blue);
                    Vector3 temp = target;
                    temp = Handles.FreeMoveHandle(temp, Quaternion.identity, 0.75f, Vector3.one, Handles.ArrowCap);
                    command.myVector2 = temp - lastPos;
                    break;
                case MovementCommand.MoverTypes.Absolute:
                    target = (Vector3)command.myVector2;
                    Utils.DrawHandlesArrow(lastPos, target, Color.red);
                    // Draw Handle
                    command.myVector2 = Handles.FreeMoveHandle(command.myVector2, Quaternion.identity, 0.75f, Vector3.one, Handles.ArrowCap);
					break;
                case MovementCommand.MoverTypes.To_transform:
                case MovementCommand.MoverTypes.obj_name:
                    if (command.transformTarget != null)
                        target = command.transformTarget.position;
                    Utils.DrawHandlesArrow(lastPos, target, Color.green);
                    break;
				}
                
                lastPos = target;
                break;
			case MovementCommand.CommandTypes.Teleport:
				command.myVector2 = Handles.FreeMoveHandle(command.myVector2, Quaternion.identity, 0.75f, Vector3.one, Handles.ArrowCap);
                Utils.DrawHandlesArrow(lastPos, (Vector3)command.myVector2, Color.yellow);
                lastPos = (Vector3)command.myVector2;
                break;
			default:
				break;
			}
		}
		
	}

	public override void OnInspectorGUI() {
		// Follow this template
		serializedObject.Update();
		SerializedProperty prop = serializedObject.FindProperty("m_Script");
		EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
		serializedObject.ApplyModifiedProperties();

        DrawHeaderInfo();
        DrawCommands();
        
        DrawQuickCommands();
		SceneView.RepaintAll();
	}


    void DrawHeaderInfo() {
        GUILayout.Label("Status", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Move Speed:", GUILayout.MaxWidth(80));
        GUILayout.Label("", GUILayout.MaxWidth(0.0f));
        myScript.move_speed = (Mover.Speed)EditorGUILayout.EnumPopup(myScript.move_speed, GUILayout.Width(70));
        GUILayout.Label("Repeat:", GUILayout.MaxWidth(50.0f));
        myScript.Repeat = (Mover.RepeatBehavior)EditorGUILayout.EnumPopup(myScript.Repeat, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Forward:", GUILayout.MaxWidth(80));
        float y = GUILayoutUtility.GetLastRect().y;
        myScript.move_forward = GUILayout.Toggle(myScript.move_forward, "");
        GUI.Label(new Rect(176, y, 70, 15), "Current:");
        GUI.Label(new Rect(275, y, 70, 15), myScript.currentNode.ToString());
        GUILayout.EndHorizontal();

        myScript.ignore_impossible = EditorGUILayout.Toggle("Ignore Impossible:", myScript.ignore_impossible);
        myScript.facing = EditorGUILayout.FloatField("Facing:", myScript.facing);

        EditorGUI.indentLevel = 1;
        guts = EditorGUILayout.Foldout(guts, "Options");
        if (guts) {
            myScript.spread = EditorGUILayout.Slider("Spread %:", myScript.spread, 0.5f, 3);
            myScript.stop_range = EditorGUILayout.Slider("Stop Distance:", myScript.stop_range, 0, 4);
            myScript.radius = EditorGUILayout.Slider("Radius:", myScript.radius, 0, 10);
            myScript.ray_density = EditorGUILayout.IntSlider("Ray Count: ", myScript.ray_density, 1, 5);
            myScript.slide = EditorGUILayout.Toggle("Slide:", myScript.slide);
        }
    }


    void DrawCommands() {
        // setup a scroll box for movement commands
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        GUILayout.Label("Commands", EditorStyles.boldLabel);
        float y = GUILayoutUtility.GetLastRect().y;
        GUI.Box(new Rect(105, y, 15, 15), "");
        GUI.Label(new Rect(107, y, 18, 18), "?");
        myContextMenu(new Rect(105, y, 25, 25), 0, myScript.commands.Count, true);
        // -------------------------------------------------------------------------------------------------------------
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(200));
        EditorGUI.indentLevel = 1;
        // List each command
        for (int i = 0; i < myScript.commands.Count; i++) {
            MovementCommand command = myScript.commands[i];
            EditorGUILayout.BeginHorizontal(GUILayout.Width(50));
            viewPathEntries[i] = EditorGUILayout.Foldout(viewPathEntries[i], i.ToString() + ":");
            Rect rt = GUILayoutUtility.GetLastRect();
            GUI.Box(new Rect(43, rt.y, 15, 15), "");
            GUI.Label(new Rect(45, rt.y, 18, 18), "?");
            myContextMenu(new Rect(43, rt.y, 15, 15), 1, i);
            MovementCommand.CommandTypes commandType = (MovementCommand.CommandTypes)EditorGUILayout.EnumPopup(
                "", command.command_type, GUILayout.Width(75));
            // if user changed command type, show options
            if (commandType != command.command_type) {
                command.command_type = commandType;
                viewPathEntries[i] = true;
            }
            // Show quick info of command
            string stats = "";
            switch (command.command_type) {
            case MovementCommand.CommandTypes.Move:
                switch (command.move_type) {
                case MovementCommand.MoverTypes.To_transform:
                    if (command.transformTarget != null)
                        stats = "TO: " + command.transformTarget.name;
                    else
                        stats = "TO: NULL";
                    break;
                case MovementCommand.MoverTypes.obj_name:
                    stats = "TO: " + command.myString;
                    break;
                default:
                    Vector2 norm = command.myVector2.normalized;
                    if (norm == Vector2.up)
                        stats = "Up " + command.myVector2.y;
                    else if (norm == Vector2.right)
                        stats = "Right " + command.myVector2.x;
                    else if (norm == Vector2.down)
                        stats = "Down " + Mathf.Abs(command.myVector2.y);
                    else if (norm == Vector2.left)
                        stats = "Left " + Mathf.Abs(command.myVector2.x);
                    else
                        stats = command.myVector2.ToString() + " " + command.move_type.ToString();
                    break;
                }
                break;
            case MovementCommand.CommandTypes.Wait:
                stats = command.myFloat1.ToString() + " seconds";
                break;
            case MovementCommand.CommandTypes.Teleport:
                stats = "Teleport to " + command.myVector2.ToString();
                break;
            case MovementCommand.CommandTypes.GoTo:
                stats = "GoTo command " + command.myInt.ToString();
                break;
            default:
                stats = command.command_type.ToString() + " not implemented";
                break;
            }
            EditorGUILayout.LabelField(stats, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            // Draw layout for each type
            if (viewPathEntries[i]) {
                //------------------------------------------------------------------------------------------------------
                // C O M M A N D   E D I T I N G
                //------------------------------------------------------------------------------------------------------
                switch (command.command_type) {
                // Move command
                case MovementCommand.CommandTypes.Move:
                    command.move_type = (MovementCommand.MoverTypes)EditorGUILayout.EnumPopup("", command.move_type);
                    switch (command.move_type) {
                    case MovementCommand.MoverTypes.Relative:
                    case MovementCommand.MoverTypes.Absolute:
                        command.myVector2 = EditorGUILayout.Vector2Field("Destination:", command.myVector2);
                        break;
                    case MovementCommand.MoverTypes.To_transform:
                        command.transformTarget = EditorGUILayout.ObjectField("Target",
                                                                              command.transformTarget,
                                                                              typeof(Transform), true) as Transform;
                        command.myBool = EditorGUILayout.Toggle("Re-adjust Target: ", command.myBool);
                        break;
                    case MovementCommand.MoverTypes.obj_name:
                        command.myString = EditorGUILayout.TextField("Target Name: ", command.myString);
                        if (command.myString != "") {
                            GameObject obj = GameObject.Find(command.myString);
                            if (obj != null)
                                command.transformTarget = obj.transform;
                        }
                        command.myBool = EditorGUILayout.Toggle("Re-adjust Target: ", command.myBool);
                        break;
                    default:
                        break;
                    }

                    break;
                // Wait Command
                case MovementCommand.CommandTypes.Wait:
                    command.myFloat1 = EditorGUILayout.FloatField("Seconds:", command.myFloat1, GUILayout.Width(185));
                    break;
                case MovementCommand.CommandTypes.GoTo:
                    command.myInt = EditorGUILayout.IntField("Command:", command.myInt, GUILayout.Width(185));
                    command.myInt = Mathf.Clamp(command.myInt, 0, myScript.commands.Count - 1);
                    break;
                case MovementCommand.CommandTypes.Teleport:
                    command.myVector2 = EditorGUILayout.Vector2Field("Destination", command.myVector2);
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


	void _DrawRow(ref int index, int buttons_per_row) {
        index++;
        if (index % buttons_per_row != 0)
            return;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
	}


    void DrawQuickCommands() {
        quick_commands = EditorGUILayout.Foldout(quick_commands, "Quick Commands");
        if (quick_commands) {
            int index = -1;
            
            int buttons_per_row = (int)(Screen.width / (button_width + 7));
            EditorGUILayout.BeginHorizontal();
            _DrawRow(ref index, buttons_per_row);
            if (GUILayout.Button("Move Up", GUILayout.Width(button_width))) {
                QuickCommand(new int[2] { myScript.commands.Count, 1 });
            }
            _DrawRow(ref index, buttons_per_row);
            if (GUILayout.Button("Move Right", GUILayout.Width(button_width))) {
                QuickCommand(new int[2] { myScript.commands.Count, 2 });
            }
            _DrawRow(ref index, buttons_per_row);
            if (GUILayout.Button("Move Down", GUILayout.Width(button_width))) {
                QuickCommand(new int[2] { myScript.commands.Count, 3 });
            }
            _DrawRow(ref index, buttons_per_row);
            if (GUILayout.Button("Move Left", GUILayout.Width(button_width))) {
                QuickCommand(new int[2] { myScript.commands.Count, 4 });
            }
            _DrawRow(ref index, buttons_per_row);
            if (GUILayout.Button(Screen.width.ToString(), GUILayout.Width(button_width))) {

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
				menu.AddItem (new GUIContent ("New Command"), false, InsertCommand, id);
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

				menu.AddItem (new GUIContent ("Insert/Above"), false, InsertCommand, (id));
				menu.AddItem (new GUIContent ("Insert/Below"), false, InsertCommand, (id+1));
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
		for (int i = 0; i < viewPathEntries.Count; i++) {
			viewPathEntries[i] = visable;
		}
	}

	void ClearAll() {
		myScript.commands = new List<BKB_RPG.MovementCommand>();
	}

	void RemoveAt(object data) {
		int id = System.Convert.ToInt32(data);
		myScript.commands.RemoveAt(id);
		viewPathEntries.RemoveAt(id);
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
		viewPathEntries.Insert(id, false);
	}

	void InsertCommand(object data) {
		int id = System.Convert.ToInt32(data);
		myScript.commands.Insert(id, new MovementCommand(MovementCommand.CommandTypes.Move));
		viewPathEntries.Insert(id, true);
	}

	void QuickCommand(object data) {
		int[] args = data as int[];
		int index = args[0];
		if (index == myScript.commands.Count) {
			myScript.commands.Insert(index, new MovementCommand(MovementCommand.CommandTypes.Move));
			viewPathEntries.Insert(index, false);
		}
		MovementCommand command = new MovementCommand(MovementCommand.CommandTypes.Move);
		switch(args[1]) {
		case 1:	// move up
			command.move_type = MovementCommand.MoverTypes.Relative;
			command.myVector2 = Vector2.up * master.unitDistance;
			break;
		case 2:	// move right
			command.move_type = MovementCommand.MoverTypes.Relative;
			command.myVector2 = Vector2.right * master.unitDistance;
			break;
		case 3:	// move right
			command.move_type = MovementCommand.MoverTypes.Relative;
			command.myVector2 = Vector2.down * master.unitDistance;
			break;
		case 4:	// move right
			command.move_type = MovementCommand.MoverTypes.Relative;
			command.myVector2 = Vector2.left * master.unitDistance;
			break;
		}
		myScript.commands[index] = command;
	}


}