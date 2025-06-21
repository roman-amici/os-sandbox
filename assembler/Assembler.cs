using System.ComponentModel;
using OsSandbox.Interpreter;
using OsSandbox.Simulation;

public class ParseException(uint lineNumber, string line, string message) : Exception(message)
{
    public uint LineNumber { get; } = lineNumber;
    public string Line { get; } = line;

    public override string ToString()
    {
        return $"{LineNumber}: {message}";
    }
}

public class Assembler
{

    private uint LineNumber { get; set; }
    private string Line { get; set; } = string.Empty;

    public List<Instruction> Assemble(StreamReader reader)
    {
        var instructions = new List<Instruction>();

        LineNumber = 1;
        while ((Line = reader.ReadLine()!) != null)
        {
            var instruction = ParseLine();

            if (instruction == null)
            {
                continue;
            }

            instructions.Add(instruction);
            LineNumber++;
        }

        return instructions;
    }

    void AssertNumArgs(string[] args, int length)
    {
        if (length != args.Length)
        {
            throw new ParseException(LineNumber, Line, $"Expected {length} args");
        }
    }

    Register ParseRegister(string reg)
    {
        if (!Enum.TryParse(reg, true, out Register register))
        {
            throw new ParseException(LineNumber, Line, $"{reg} is not a register");
        }

        return register;
    }

    short ParseShort(string s)
    {
        if (!short.TryParse(s, out var immediate))
        {
            throw new ParseException(LineNumber, Line, $"{s} is not a short");
        }

        return immediate;
    }

    sbyte ParseSbyte(string s)
    {
        if (!sbyte.TryParse(s, out var immediate))
        {
            throw new ParseException(LineNumber, Line, $"{s} is not an sbyte");
        }

        return immediate;
    }

    Instruction? ParseLine()
    {
        var commentIndex = Line.IndexOf('#');

        var line = Line;
        if (commentIndex >= 0)
        {
            line = Line.Substring(0, commentIndex);
        }

        if (string.IsNullOrWhiteSpace(Line))
        {
            return null;
        }

        var parts = line.Split(" ");
        if (parts.Length < 1)
        {
            throw new ParseException(LineNumber, Line, "Expected opcode");
        }

        if (!Enum.TryParse(parts[0], true, out OpCode opCode))
        {
            throw new ParseException(LineNumber, Line, "Unknown opcode");
        }

        string argsString = string.Empty;
        if (parts.Length > 1)
        {
            argsString = line.Substring(parts[0].Length + 1);
        }

        var args = argsString
            .Split(',')
            .Select(a => a.Trim())
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToArray();

        switch (opCode)
        {
            case OpCode.AddI:
                AssertNumArgs(args, 3);

                return new AddI()
                {
                    RD = ParseRegister(args[0]),
                    R1 = ParseRegister(args[1]),
                    R2 = ParseRegister(args[2]),
                };
            case OpCode.AddN:
                AssertNumArgs(args, 3);

                return new AddN()
                {
                    RD = ParseRegister(args[0]),
                    R1 = ParseRegister(args[1]),
                    Immediate = ParseSbyte(args[2]),
                };

            case OpCode.JmpGT:
                AssertNumArgs(args, 3);

                return new JumpGT()
                {
                    RC = ParseRegister(args[0]),
                    RA = ParseRegister(args[1]),
                    Offset = ParseSbyte(args[2])
                };

            case OpCode.End:
                AssertNumArgs(args, 1);

                return new End()
                {
                    OutRegister = ParseRegister(args[0])
                };

            case OpCode.SysCall:
                AssertNumArgs(args, 0);
                return new SysCall();

            default:
                throw new NotImplementedException();
        }

    }
}