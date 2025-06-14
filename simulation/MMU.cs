
using System.ComponentModel;
using OsSandbox.Interpreter;

namespace OsSandbox.Simulation;


public class MemoryOutOfRangeException(int address, string message) : InterruptException(message)
{
    public int Address { get; } = address;
}

public class MemoryAlignmentException(int address, string message) : InterruptException(message)
{
    public int Address { get; } = address;
}

public abstract class MMU(uint size)
{
    protected byte[] Memory { get; } = new byte[size];

    public void CheckAddressI(int address)
    {
        if (address < 0)
        {
            throw new MemoryOutOfRangeException(address, $"Memory out of range {size}");
        }

        if (address + 3 >= Memory.Length)
        {
            throw new MemoryOutOfRangeException(address, $"Memory out of range {size}");
        }

        if (address % 4 != 0)
        {
            throw new MemoryAlignmentException(address, "Int access must be 4-byte aligned");
        }
    }

    public int GetI(int address)
    {
        CheckAddressI(address);
        var mem = new byte[] { Memory[address], Memory[address + 1], Memory[address + 2], Memory[address + 3] };
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(mem);
        }

        return BitConverter.ToInt32(mem);
    }

    public void StoreI(int address, int value)
    {
        CheckAddressI(address);

        var bytes = BitConverter.GetBytes(value);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        Memory[address] = bytes[0];
        Memory[address + 1] = bytes[1];
        Memory[address + 2] = bytes[2];
        Memory[address + 3] = bytes[3];
    }

    public abstract byte[] Fetch(int pc);
}

public class UnifiedMemoryMMU(uint size) : MMU(size)
{
    public override byte[] Fetch(int pc)
    {
        CheckAddressI(pc);

        return [Memory[pc], Memory[pc + 1], Memory[pc + 2], Memory[pc + 3]];
    }
}

public class SegregatedMemoryMMU(byte[] instructions, uint size) : MMU(size)
{
    protected byte[] Instructions { get; } = instructions;

    public override byte[] Fetch(int pc)
    {
        if (pc < 0)
        {
            throw new MemoryOutOfRangeException(pc, $"Memory out of range {Instructions.Length}");
        }

        if (pc + 3 >= Instructions.Length)
        {
            throw new MemoryOutOfRangeException(pc, $"Memory out of range {Instructions.Length}");
        }

        if (pc % 4 != 0)
        {
            throw new MemoryAlignmentException(pc, "Int access must be 4-byte aligned");
        }

        return [Instructions[pc], Instructions[pc + 1], Instructions[pc + 2], Instructions[pc + 3]];
    }
}