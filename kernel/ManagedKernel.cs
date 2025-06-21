using OsSandbox.Interpreter;
using OsSandbox.Simulation;

namespace OsSandbox.Kernel;

public class ManagedKernel
{

    public void SysCall(CPU cpu, MMU mmu)
    {
        var sysCallNumber = (SysCallCode)cpu.Call1;

        SysCallResult result = SysCallResult.Success;
        switch (sysCallNumber)
        {
            case SysCallCode.PutChar:
                var c = (char)cpu.Call2;

                if (!char.IsAscii(c))
                {
                    result = SysCallResult.OutOfRange;
                    break;
                }

                Console.Write(c);
                break;


            default:
                throw new UnknownSysCallException((int)sysCallNumber);
        }

        cpu.Call4 = (int)result;
    }

}