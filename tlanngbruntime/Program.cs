// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TubbyLangRuntime
{
    public enum Argtype
    {
        STATIC,
        VARIABLE
    }
    public class Command
    {
        public Byte id;
        public int argsLength;

        public Command(Byte id, int argslength)
        {
            this.id = id;
            this.argsLength = argslength;
        }
    }

    public class Argument
    {
        public Argtype type;
        public int value;

        public Argument(Argtype type, int value)
        {
            this.type = type;
            this.value = value;
        }
    }

    public class Instruction
    {
        public Command command;
        public Argument[] arguments;

        public Instruction(Command command, Argument[] args)
        {
            this.command = command;
            this.arguments = args;
        }
    }
    
    class Program
    {
        static void Main(String[] args)
        {
            String inPath = args[0];
            Console.WriteLine(inPath);
            Instruction[] Instructions = Reader.ReadCompiledToinstructions(File.ReadAllBytes(inPath));
            Interpreter interp = new Interpreter(Instructions);
            while (interp.Cursor < Instructions.Length)
            {
                interp.Next();
            }
        }
    }

    public class Interpreter
    {
        public Stack<int>  MainStack   = new Stack<int>();
        public Stack<int>  CallStack   = new Stack<int>();
        public Dictionary<int, int>  Accumulators = new Dictionary<int, int>();
        public Instruction[] Instructions;
        public int Cursor = 0;

        public Interpreter(Instruction[] instructions)
        {
            this.Instructions = instructions;
        }

        private void EditAccumulator(int id, int value)
        {
            if (!Accumulators.ContainsKey(id))
                Accumulators.Add(id, value);
            else
                Accumulators[id] = value;
        }

        private int GetAccumulator(int id)
        {
            if (Accumulators.ContainsKey(id))
                return Accumulators[id];
            return 0;
        }
        
        public void Next()
        {
            Instruction currentInstruction = Instructions[Cursor];
            List<int> ArgValues = new List<int>();
            for (int i = 0; i < currentInstruction.arguments.Length; i++)
            {
                int arg = 0;
                if (currentInstruction.arguments[i].type == Argtype.VARIABLE)
                    arg = GetAccumulator(currentInstruction.arguments[i].value);
                else
                    arg = currentInstruction.arguments[i].value;
                ArgValues.Add(arg);
            }

            switch (this.Instructions[Cursor].command.id)
            {
                case 0x10: //INC
                    EditAccumulator(ArgValues[0], GetAccumulator(ArgValues[0]) + ArgValues[1]);
                    break;
                case 0x11: //DEC
                    EditAccumulator(ArgValues[0], GetAccumulator(ArgValues[0]) - ArgValues[1]);
                    break;
                case 0x12: //PRINTA
                    Console.Write(ArgValues[0]);
                    break;
                case 0x13: //PRINTB
                    Console.Write((char)ArgValues[0]);
                    break;
                case 0x14: //GOTO
                    Cursor = ArgValues[0] - 1;
                    break;
                case 0x15: //TEST
                    if(ArgValues[0] == ArgValues[1])
                        Cursor++;
                    break;
                    
            }
            Cursor++;
        }
        
    }

    class Reader
    {
        public static Dictionary<Byte, int> commands = new Dictionary<byte, int>()
        {
            {0x10, 2},
            {0x11, 2},
            {0x12, 1},
            {0x13, 1},
            {0x14, 1},
            {0x15, 2}
        };

        public static Instruction[] ReadCompiledToinstructions(Byte[] compiled)
        {
            List<Byte> leftover = compiled.ToList();
            List<Instruction> instructions = new List<Instruction>();
            while (leftover.Count > 0)
            {
                Instruction currentInstruction;
                List<Argument> args = new List<Argument>();
                int commandlength = commands[leftover[0]];
                Byte commandid = leftover[0];
                leftover = leftover.Skip(1).ToList();
                for (int i = 0; i < commandlength; i++)
                {
                    Byte argType = leftover[0];
                    Byte argValue = leftover[1];
                    leftover = leftover.Skip(2).ToList();
                    args.Add(new Argument((argType == 0x45) ? Argtype.STATIC : Argtype.VARIABLE, argValue));
                }

                Command cmd = new Command(commandid, commandlength);
                currentInstruction = new Instruction(cmd, args.ToArray());
                instructions.Add(currentInstruction);
            }

            return instructions.ToArray();
        }
    }
}
