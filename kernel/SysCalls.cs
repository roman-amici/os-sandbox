using OsSandbox.Simulation;

namespace OsSandbox.Kernel;

public class UnknownSysCallException(int code) : InterruptException($"Invalid SysCall: {code}")
{
    public int Code { get; } = code;
}

public enum SysCallCode
{
    PutChar = 1,
}

public enum SysCallResult
{
    Success = 0,
    OutOfRange = 1,
}