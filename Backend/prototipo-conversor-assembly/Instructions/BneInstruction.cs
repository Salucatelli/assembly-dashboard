using System;
using prototipo_conversor_assembly.Bases; 

namespace prototipo_conversor_assembly
{
    public class BneInstruction : MipsInstruction
    {
        private int _rsIndex;
        private int _rtIndex;
        private int _offset; 

        public BneInstruction(string assemblyLine, int address, int rsIndex, int rtIndex, int offset)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I;
            _rsIndex = rsIndex;
            _rtIndex = rtIndex;
            _offset = offset;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            int rsValue = cpu.bancoDeRegistradores.GetValue(_rsIndex);
            int rtValue = cpu.bancoDeRegistradores.GetValue(_rtIndex);

            if (rsValue != rtValue)
            {
                return Address + 4 + (_offset * 4);
            }
            else
            {
                return Address + 4;
            }
        }

        public override string ToBinaryString()
        {

            string opcodeBinary = Convert.ToString(0b000101, 2).PadLeft(6, '0');
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');

            string offsetBinary = Convert.ToString((ushort)_offset, 2).PadLeft(16, '0');

            return $"{opcodeBinary}{rsBinary}{rtBinary}{offsetBinary}";
        }

        public override string ToHexString()
        {
            uint binaryValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{binaryValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config)
        {
            return config.BranchCycles;
        }
    }
}