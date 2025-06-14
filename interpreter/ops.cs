using System.ComponentModel;
using System.Data.SqlTypes;
using System.Text;
using OsSandbox.Simulation;

namespace OsSandbox.Interpreter;

public class IllegalInstructionException(InstructionContext inst, string message) : InterruptException(message)
{
    public byte[] Bytes { get; } = inst.Bytes;
    public int Address { get; } = inst.Address;
}

public enum Register
{
    Zero,

    Save1,
    Save2,
    Save3,
    Save4,

    Call1,
    Call2,
    Call3,
    Call4,

    ProgramCounter,
    StackPointer,
}

public enum OpCode
{
    AddN,
    SubN,
    MulN,
    DivN,

    AddI,
    SubI,
    MulI,
    DivI,

    LoadI,
    StoreI,

    SetILow,
    SetIHigh,

    Jump,
    JumpZero,
    JumpGT,

}

public struct InstructionContext(byte[] bytes, int address)
{
    public byte[] Bytes { get; } = bytes;
    public int Address { get; } = address;
}

public abstract class Instruction
{
    public abstract OpCode OpCode { get; }

    public abstract void Encode(Stream writer);

    public static Instruction Decode(InstructionContext inst)
    {
        var opCode = (OpCode)inst.Bytes[0];

        return opCode switch
        {
            OpCode.AddN => new AddN(inst),
            OpCode.SubN => new SubN(inst),
            OpCode.MulN => new MulN(inst),
            OpCode.DivN => new DivN(inst),
            OpCode.AddI => new AddI(inst),
            OpCode.SubI => new SubI(inst),
            OpCode.MulI => new MulN(inst),
            OpCode.DivI => new DivN(inst),

            OpCode.LoadI => new LoadI(inst),
            OpCode.StoreI => new StoreI(inst),

            OpCode.SetILow => new SetILow(inst),
            OpCode.SetIHigh => new SetIHigh(inst),

            OpCode.Jump => new Jump(inst),
            OpCode.JumpZero => new JumpZero(inst),
            OpCode.JumpGT => new JumpGT(inst),

            _ => throw new IllegalInstructionException(inst, "Invalid OpCode")

        };
    }

    public static Register DecodeRegister(InstructionContext inst, int i)
    {
        var register = (Register)inst.Bytes[i];
        if (!Enum.IsDefined(register))
        {
            throw new IllegalInstructionException(inst, $"{register} is not a valid register");
        }

        return register;
    }

    public static short DecodeShort(InstructionContext inst, int i)
    {
        if (!BitConverter.IsLittleEndian)
        {
            var s = new byte[2];
            s[0] = inst.Bytes[i + 1];
            s[1] = inst.Bytes[i];

            return BitConverter.ToInt16(s);
        }
        return BitConverter.ToInt16(inst.Bytes, i);
    }

    public static void EncodeShort(Stream writer, short s)
    {
        var bytes = BitConverter.GetBytes(s);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        writer.Write(bytes);
    }

}

public class AddI : Instruction
{
    public AddI() { }

    public AddI(InstructionContext inst)
    {
        RD = DecodeRegister(inst, 1);
        R1 = DecodeRegister(inst, 2);
        R2 = DecodeRegister(inst, 2);
    }

    public override OpCode OpCode => OpCode.AddI;

    // RD = R2 + R1
    public Register RD { get; set; }
    public Register R1 { get; set; }
    public Register R2 { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RD);
        writer.WriteByte((byte)R1);
        writer.WriteByte((byte)R2);
    }
}

public class SubI : Instruction
{
    public SubI() { }


    public SubI(InstructionContext inst)
    {
        RD = DecodeRegister(inst, 1);
        R1 = DecodeRegister(inst, 2);
        R2 = DecodeRegister(inst, 2);
    }

    public override OpCode OpCode => OpCode.SubI;

    // RD = R2 - R1
    public Register RD { get; set; }
    public Register R1 { get; set; }
    public Register R2 { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RD);
        writer.WriteByte((byte)R1);
        writer.WriteByte((byte)R2);
    }
}

public class MulI : Instruction
{
    public MulI() { }

    public MulI(InstructionContext inst)
    {
        RD = DecodeRegister(inst, 1);
        R1 = DecodeRegister(inst, 2);
        R2 = DecodeRegister(inst, 2);
    }

    public override OpCode OpCode => OpCode.MulI;

    // RD = R2 * R1
    public Register RD { get; set; }
    public Register R1 { get; set; }
    public Register R2 { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RD);
        writer.WriteByte((byte)R1);
        writer.WriteByte((byte)R2);
    }
}

public class DivI : Instruction
{
    public DivI() { }

    public DivI(InstructionContext inst)
    {
        RD = DecodeRegister(inst, 1);
        R1 = DecodeRegister(inst, 2);
        R2 = DecodeRegister(inst, 2);
    }

    public override OpCode OpCode => OpCode.DivI;

    // RD = R2 / R1
    public Register RD { get; set; }
    public Register R1 { get; set; }
    public Register R2 { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RD);
        writer.WriteByte((byte)R1);
        writer.WriteByte((byte)R2);
    }
}

public class AddN : Instruction
{
    public AddN() { }


    public AddN(InstructionContext inst)
    {
        RD = DecodeRegister(inst, 1);
        R1 = DecodeRegister(inst, 2);
        Immediate = (sbyte)inst.Bytes[3];
    }

    public override OpCode OpCode => OpCode.AddN;

    // RD = R1 + Immediate
    public Register RD { get; set; }
    public Register R1 { get; set; }
    public sbyte Immediate { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RD);
        writer.WriteByte((byte)R1);
        writer.WriteByte((byte)Immediate);
    }
}

public class SubN : Instruction
{
    public SubN() { }


    public SubN(InstructionContext inst)
    {
        RD = DecodeRegister(inst, 1);
        R1 = DecodeRegister(inst, 2);
        Immediate = (sbyte)inst.Bytes[3];
    }

    public override OpCode OpCode => OpCode.SubN;

    // RD = R1 - Immediate
    public Register RD { get; set; }
    public Register R1 { get; set; }
    public sbyte Immediate { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RD);
        writer.WriteByte((byte)R1);
        writer.WriteByte((byte)Immediate);
    }
}

public class MulN : Instruction
{
    public MulN() { }


    public MulN(InstructionContext inst)
    {
        RD = DecodeRegister(inst, 1);
        R1 = DecodeRegister(inst, 2);
        Immediate = (sbyte)inst.Bytes[3];
    }

    public override OpCode OpCode => OpCode.MulN;

    // RD = R1 * Immediate
    public Register RD { get; set; }
    public Register R1 { get; set; }
    public sbyte Immediate { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RD);
        writer.WriteByte((byte)R1);
        writer.WriteByte((byte)Immediate);
    }
}

public class DivN : Instruction
{
    public DivN() { }


    public DivN(InstructionContext inst)
    {
        RD = DecodeRegister(inst, 1);
        R1 = DecodeRegister(inst, 2);
        Immediate = (sbyte)inst.Bytes[3];
    }

    public override OpCode OpCode => OpCode.DivN;

    // RD = R1 / Immediate
    public Register RD { get; set; }
    public Register R1 { get; set; }
    public sbyte Immediate { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RD);
        writer.WriteByte((byte)R1);
        writer.WriteByte((byte)Immediate);
    }
}

public class LoadI : Instruction
{
    public LoadI() { }


    public LoadI(InstructionContext inst)
    {
        RD = DecodeRegister(inst, 1);
        RA = DecodeRegister(inst, 2);
        Offset = (sbyte)inst.Bytes[3];
    }

    public override OpCode OpCode => OpCode.LoadI;

    // RD = Memory[RA + 4*Offset]
    public Register RD { get; set; }
    public Register RA { get; set; }
    public sbyte Offset { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RD);
        writer.WriteByte((byte)RA);
    }
}

public class StoreI : Instruction
{
    public StoreI() { }

    public StoreI(InstructionContext inst)
    {
        R1 = DecodeRegister(inst, 1);
        RA = DecodeRegister(inst, 2);
        Offset = (sbyte)inst.Bytes[3];
    }

    public override OpCode OpCode => OpCode.StoreI;

    // Memory[RA + 4*Offset] = R1
    public Register R1 { get; set; }
    public Register RA { get; set; }
    public sbyte Offset { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RA);
        writer.WriteByte((byte)R1);
    }
}

public class SetILow : Instruction
{
    public SetILow() { }

    public SetILow(InstructionContext inst)
    {
        RD = DecodeRegister(inst, 1);
        Immediate = DecodeShort(inst, 2);
    }

    public override OpCode OpCode => OpCode.SetILow;

    // RD = Immediate
    public Register RD { get; set; }
    public short Immediate { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RD);
        EncodeShort(writer, Immediate);
    }

}

public class SetIHigh : Instruction
{
    public SetIHigh() { }


    public SetIHigh(InstructionContext inst)
    {
        RD = DecodeRegister(inst, 1);
        Immediate = DecodeShort(inst, 2);
    }

    public override OpCode OpCode => OpCode.SetIHigh;

    // RD = (Immediate << 16)
    public Register RD { get; set; }
    public short Immediate { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RD);
        EncodeShort(writer, Immediate);
    }

}

public class Jump : Instruction
{
    public Jump() { }

    public Jump(InstructionContext inst)
    {
        RA = DecodeRegister(inst, 1);
        Offset = DecodeShort(inst, 2);
    }

    public override OpCode OpCode => OpCode.Jump;

    // PC = RA + 4*Offset
    public Register RA { get; set; }
    public short Offset { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RA);
        EncodeShort(writer, Offset);
    }
}

public class JumpZero : Instruction
{
    public JumpZero() { }


    public JumpZero(InstructionContext inst)
    {
        RA = DecodeRegister(inst, 1);
        RC = DecodeRegister(inst, 2);
        Offset = (sbyte)inst.Bytes[3];
    }

    public override OpCode OpCode => OpCode.JumpZero;

    // PC = RA + 4*Offset if RC == 0
    public Register RA { get; set; }
    public Register RC { get; set; }
    public sbyte Offset { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RA);
        writer.WriteByte((byte)RC);
        writer.WriteByte((byte)Offset);
    }

}

public class JumpGT : Instruction
{
    public JumpGT() { }

    public JumpGT(InstructionContext inst)
    {
        RA = DecodeRegister(inst, 1);
        RC = DecodeRegister(inst, 2);
        Offset = (sbyte)inst.Bytes[3];
    }

    public override OpCode OpCode => OpCode.JumpGT;

    // PC = RA + 4*Offset if RC > 0
    public Register RA { get; set; }
    public Register RC { get; set; }
    public sbyte Offset { get; set; }

    public override void Encode(Stream writer)
    {
        writer.WriteByte((byte)OpCode);
        writer.WriteByte((byte)RA);
        writer.WriteByte((byte)RC);
        writer.WriteByte((byte)Offset);
    }

}