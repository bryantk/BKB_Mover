using System.Collections.Generic;

namespace BKB_RPG {
    [System.Serializable]
    public class CommandList {

        List<MovementCommand.CommandTypes> types;
        List<int> offsets;

        List<MovementCommand_Move> command_move;
        List<MovementCommand_Face> command_face;
        List<MovementCommand_Wait> command_wait;
        List<MovementCommand_GOTO> command_goto;
        List<MovementCommand_Bool> command_bool;
        List<MovementCommand_Script> command_script;


        public int Count {
            get { return types.Count; }
        }

        public MovementCommand this[int key] {
            get { return this.At(key); }
            set {
                this.RemoveAt(key);
                this.Insert(key, value);
            }
        }

        public IEnumerator<MovementCommand> GetEnumerator() {
            List<MovementCommand> c = new List<MovementCommand>();
            for (int x = 0; x < types.Count; x++)
                c.Add(this[x]);
            return c.GetEnumerator();
        }

        public CommandList() {
            types = new List<MovementCommand.CommandTypes>();
            offsets = new List<int>();

            command_move = new List<MovementCommand_Move>();
            command_face = new List<MovementCommand_Face>();
            command_wait = new List<MovementCommand_Wait>();

            command_goto = new List<MovementCommand_GOTO>();
            command_bool = new List<MovementCommand_Bool>();
            command_script = new List<MovementCommand_Script>();
        }

        public void Add(MovementCommand command) {
            Insert(types.Count, command);
        }

        public int OffsetAt(int index) {
            return offsets[index];
        }

        public void Insert(int index, MovementCommand command) {
            types.Insert(index, command.command_type);
            int count = 0;
            int x = 0;
            while (x < types.Count)
            {
                if (x < index && types[x] == command.command_type)
                    count++;
                else if (x == index)
                {
                    offsets.Insert(index, count);
                    switch (command.command_type)
                    {
                    case MovementCommand.CommandTypes.Move:
                        command_move.Insert(count, (MovementCommand_Move)command);
                        break;
                    case MovementCommand.CommandTypes.Face:
                        command_face.Insert(count, (MovementCommand_Face)command);
                        break;
                    case MovementCommand.CommandTypes.Wait:
                        command_wait.Insert(count, (MovementCommand_Wait)command);
                        break;
                    case MovementCommand.CommandTypes.GoTo:
                        command_goto.Insert(count, (MovementCommand_GOTO)command);
                        break;
                    case MovementCommand.CommandTypes.Boolean:
                        command_bool.Insert(count, (MovementCommand_Bool)command);
                        break;
                    case MovementCommand.CommandTypes.Script:
                        command_script.Insert(count, (MovementCommand_Script)command);
                        break;
                    }
                }
                else if (x > index && types[x] == command.command_type)
                    offsets[x]++;
                x++;
            }
        }

        public void RemoveAt(int index) {
            MovementCommand.CommandTypes type = types[index];
            types.RemoveAt(index);
            offsets.RemoveAt(index);
            for (int x = index; x < types.Count; x++)
            {
                if (types[x] == type)
                    offsets[x]--;
            }
        }

        public MovementCommand At(int index) {
            int offset = offsets[index];
            switch (types[index])
            {
            case MovementCommand.CommandTypes.Move:
                return command_move[offset];
            case MovementCommand.CommandTypes.Face:
                return command_face[offset];
            case MovementCommand.CommandTypes.Wait:
                return command_wait[offset];
            case MovementCommand.CommandTypes.GoTo:
                return command_goto[offset];
            case MovementCommand.CommandTypes.Boolean:
                return command_bool[offset];
            default:
                // case MovementCommand.CommandTypes.Script:
                return command_script[offset];
            }
        }

        public override string ToString() {
            string objs = "";
            for (int x = 0; x < types.Count; x++)
            {
                objs += x.ToString() + ": " + At(x).GetType() + ", ";
            }
            return objs;
        }
    }
}