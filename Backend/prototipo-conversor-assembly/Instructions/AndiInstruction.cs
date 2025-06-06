using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly 
{
    public class AndiInstruction : MipsInstruction
    {
        private int _rtIndex;     
        private int _rsIndex;     
        private int _immediate;   

        public AndiInstruction(string assemblyLine, int address, int rtIndex, int rsIndex, int immediate)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I;
            _rtIndex = rtIndex;
            _rsIndex = rsIndex;
            _immediate = immediate;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            int rsValue = cpu.bancoDeRegistradores.GetValue(_rsIndex);

            int zeroExtendedImmediate = (int)(ushort)_immediate;

            int result = rsValue & zeroExtendedImmediate;

            cpu.bancoDeRegistradores.SetValue(_rtIndex, result);

            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            string opcode = "001100";
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');

            string immediateBinary = Convert.ToString((ushort)_immediate, 2).PadLeft(16, '0');

            return $"{opcode}{rsBinary}{rtBinary}{immediateBinary}";
        }

        public override string ToHexString()
        {
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config) => config.ITypeCycles;
    }
}