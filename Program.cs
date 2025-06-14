using OsSandbox.Interpreter;
using OsSandbox.Simulation;

var simpleProgram = new List<Instruction>()
{
    new AddN()
    {
        RD = Register.Call1,
        R1 = Register.Zero,
        Immediate = 5
    },
    new SubN()
    {
        RD = Register.Call1,
        R1 = Register.Call1,
        Immediate = 1,
    },
    new JumpGT()
    {
        RA = Register.ProgramCounter,
        RC = Register.Call1,
        Offset = -2 * 4
    }
};

var buffer = new byte[simpleProgram.Count * 4];
var writer = new MemoryStream(buffer);

foreach (var instruction in simpleProgram)
{
    instruction.Encode(writer);
}

var interpreter = new ByteCodeInterpreter(new(), new SegregatedMemoryMMU(buffer, 1024));

interpreter.Interpret();