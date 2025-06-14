

namespace OsSandbox.Simulation;

public class IllegalPCException(int pc, string message) : InterruptException(message)
{
    int PC { get; } = pc;
}

public class IllegalOperationException(string message) : InterruptException(message);

public class CPU
{
    public int[] Registers { get; } = new int[10];

    public int Zero { get => 0; set => throw new IllegalOperationException("Zero register cannot be set"); }

    public int Save1 { get => Registers[1]; set => Registers[1] = value; }
    public int Save2 { get => Registers[2]; set => Registers[2] = value; }
    public int Save3 { get => Registers[3]; set => Registers[3] = value; }
    public int Save4 { get => Registers[4]; set => Registers[4] = value; }

    public int Call1 { get => Registers[5]; set => Registers[5] = value; }
    public int Call2 { get => Registers[6]; set => Registers[6] = value; }
    public int Call3 { get => Registers[7]; set => Registers[7] = value; }
    public int Call4 { get => Registers[8]; set => Registers[8] = value; }

    public int ProgramCounter
    {
        get => Registers[9];
        set
        {
            if (value < 0)
            {
                throw new IllegalPCException(value, "PC must be >= 0");
            }
            if (value % 4 != 0)
            {
                throw new IllegalPCException(value, "PC must be 4-byte aligned");
            }
            Registers[9] = value;
        }
    }
    public int StackPointer { get => Registers[10]; set => Registers[10] = value; }

    // Interrupt tables, etc

}