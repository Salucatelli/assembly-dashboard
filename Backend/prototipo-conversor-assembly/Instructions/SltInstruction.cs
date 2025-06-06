using System;
using prototipo_conversor_assembly.Bases; 

namespace prototipo_conversor_assembly
{
    public class SltInstruction : MipsInstruction
    {
        private int _rdIndex; 
        private int _rsIndex; 
        private int _rtIndex; 

        public SltInstruction(string assemblyLine, int address, int rdIndex, int rsIndex, int rtIndex)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.R; 
            _rdIndex = rdIndex;
            _rsIndex = rsIndex;
            _rtIndex = rtIndex;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            int rsValue = cpu.bancoDeRegistradores.GetValue(_rsIndex);
            int rtValue = cpu.bancoDeRegistradores.GetValue(_rtIndex);

            int result = 0; 

            if (rsValue < rtValue)
            {
                result = 1;
            }

            cpu.bancoDeRegistradores.SetValue(_rdIndex, result);

            return Address + 4;
        }

        public override string ToBinaryString()
        {

            string opcodeBinary = Convert.ToString(0b000000, 2).PadLeft(6, '0'); 
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');
            string rdBinary = Convert.ToString(_rdIndex, 2).PadLeft(5, '0');
            string shamtBinary = Convert.ToString(0b00000, 2).PadLeft(5, '0'); 
            string functBinary = Convert.ToString(0b101010, 2).PadLeft(6, '0'); 

            return $"{opcodeBinary}{rsBinary}{rtBinary}{rdBinary}{shamtBinary}{functBinary}";
        }

        public override string ToHexString()
        {
            uint binaryValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{binaryValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config)
        {
            return config.RTypeCycles;
        }
    }
}