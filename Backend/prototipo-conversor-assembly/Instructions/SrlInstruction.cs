using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly 
{
    public class SrlInstruction : MipsInstruction
    {
        private int _rdIndex;   
        private int _rtIndex;   
        private int _shamt;     

        public SrlInstruction(string assemblyLine, int address, int rdIndex, int rtIndex, int shamt)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.R;
            _rdIndex = rdIndex;
            _rtIndex = rtIndex;
            _shamt = shamt; 
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            int rtValue = cpu.bancoDeRegistradores.GetValue(_rtIndex);

            uint unsignedRtValue = (uint)rtValue;
            uint result = unsignedRtValue >> _shamt; 

            cpu.bancoDeRegistradores.SetValue(_rdIndex, (int)result);

            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            string opcode = "000000";
            string rsBinary = "00000"; 
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');
            string rdBinary = Convert.ToString(_rdIndex, 2).PadLeft(5, '0');

            string shamtBinary = Convert.ToString(_shamt, 2).PadLeft(5, '0');

            string funct = "000010";

            return $"{opcode}{rsBinary}{rtBinary}{rdBinary}{shamtBinary}{funct}";
        }

        public override string ToHexString()
        {
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config) => config.RTypeCycles;
    }
}