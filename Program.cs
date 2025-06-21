using OsSandbox.Interpreter;
using OsSandbox.Kernel;
using OsSandbox.Simulation;

string path;
if (args.Length == 0)
{
    Console.WriteLine("Usage: [filename]");
    path = "../../../programs/hello_world.bsm";

    // return;
}
else
{
    path = Path.GetFullPath(args[0]);
}


List<Instruction> program;
var assembler = new Assembler();
using (var programText = File.OpenText(path))
{
    program = assembler.Assemble(programText);
}

var buffer = new byte[program.Count * 4];
var writer = new MemoryStream(buffer);

foreach (var instruction in program)
{
    instruction.Encode(writer);
}

var kernel = new ManagedKernel();
var interpreter = new ByteCodeInterpreter(new(), new SegregatedMemoryMMU(buffer, 1024), kernel);

Console.WriteLine(interpreter.Interpret());