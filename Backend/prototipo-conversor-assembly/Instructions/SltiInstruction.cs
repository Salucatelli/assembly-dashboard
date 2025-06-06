using System;
using prototipo_conversor_assembly.Bases; 

namespace prototipo_conversor_assembly
{
    public class SltiInstruction : MipsInstruction
    {
        private int _rtIndex;      
        private int _rsIndex;      
        private short _immediate;  

        public SltiInstruction(string assemblyLine, int address, int rtIndex, int rsIndex, short immediate)
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

            int result = 0; 

            if (rsValue < _immediate)
            {
                result = 1;
            }

            cpu.bancoDeRegistradores.SetValue(_rtIndex, result);

            return Address + 4;
        }

        public override string ToBinaryString()
        {

            string opcodeBinary = Convert.ToString(0b001010, 2).PadLeft(6, '0'); 
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');

            string immediateBinary = Convert.ToString((ushort)_immediate, 2).PadLeft(16, '0');

            return $"{opcodeBinary}{rsBinary}{rtBinary}{immediateBinary}";
        }

        public override string ToHexString()
        {
            uint binaryValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{binaryValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config)
        {
            return config.ITypeCycles;
        }
    }
}