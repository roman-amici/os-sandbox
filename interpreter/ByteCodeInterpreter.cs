using OsSandbox.Simulation;

namespace OsSandbox.Interpreter;

public class ByteCodeInterpreter(CPU cpu, MMU mmu)
{

    Instruction NextInstruction()
    {
        var bytes = mmu.Fetch(cpu.ProgramCounter);

        var inst = Instruction.Decode(new(bytes, cpu.ProgramCounter));

        return inst;

    }

    public int Interpret()
    {
        while (mmu.HasNextInstruction(cpu.ProgramCounter))
        {
            bool advancePC = true;

            switch (NextInstruction())
            {
                case AddI addi:
                    cpu.Registers[(int)addi.RD] = cpu.Registers[(int)addi.R2] + cpu.Registers[(int)addi.R1];

                    break;
                case SubI subi:
                    cpu.Registers[(int)subi.RD] = cpu.Registers[(int)subi.R2] - cpu.Registers[(int)subi.R1];

                    break;

                case AddN addn:
                    cpu.Registers[(int)addn.RD] = cpu.Registers[(int)addn.R1] + addn.Immediate;

                    break;
                case SubN subn:
                    cpu.Registers[(int)subn.RD] = cpu.Registers[(int)subn.R1] - subn.Immediate;

                    break;
                case Jump jmp:
                    cpu.ProgramCounter = cpu.Registers[(int)jmp.RA] + 4 * jmp.Offset;

                    advancePC = false;
                    break;
                    
                case JumpZero jz:
                    if (cpu.Registers[(int)jz.RC] == 0)
                    {
                        cpu.ProgramCounter = cpu.Registers[(int)jz.RA] + 4 * jz.Offset;

                        advancePC = false;
                    }
                    break;
                case JumpGT jGT:
                    if (cpu.Registers[(int)jGT.RC] > 0)
                    {
                        var newPC = cpu.Registers[(int)jGT.RA] + 4 * jGT.Offset;
                        cpu.ProgramCounter = newPC;

                        advancePC = false;
                    }
                    break;
                case SetILow setiLow:
                    cpu.Registers[(int)setiLow.RD] |= (ushort)setiLow.Immediate;

                    break;
                case SetIHigh setiHigh:
                    cpu.Registers[(int)setiHigh.RD] |= (int)((uint)setiHigh.Immediate << 16);

                    break;

                case LoadI loadi:
                    cpu.Registers[(int)loadi.RD + 4 * loadi.Offset] = mmu.GetI(cpu.Registers[(int)loadi.RD]);

                    break;

                case StoreI storei:
                    mmu.StoreI(cpu.Registers[(int)storei.RA + 4 * storei.Offset], cpu.Registers[(int)storei.R1]);
                    break;
                default:
                    throw new NotImplementedException("Instruction type not implemented");

            }

            if (advancePC)
            {
                cpu.ProgramCounter += 4;
            }
        }

        return cpu.Call1;

    }


}