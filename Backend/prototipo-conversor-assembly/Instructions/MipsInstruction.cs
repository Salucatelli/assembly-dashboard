using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly 
{
    public enum MipsInstructionType { R, I, J, Unknown }

    public abstract class MipsInstruction
    {
        public MipsInstructionType Type { get; protected set; }
        public string AssemblyLine { get; protected set; } 
        public int Address { get; protected set; }         

        protected MipsInstruction(string assemblyLine, int address)
        {
            AssemblyLine = assemblyLine;
            Address = address;
        }

        public abstract int Execute(MipsCPU cpu, MemoryMips dataMemory);

        public abstract string ToBinaryString();
        public abstract string ToHexString();

        public abstract int GetClockCycles(CpuConfig config);
    }
}
