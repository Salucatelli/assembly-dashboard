using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly 
{
    public class NorInstruction : MipsInstruction
    {
        private int _rdIndex; 
        private int _rsIndex; 
        private int _rtIndex; 

        public NorInstruction(string assemblyLine, int address, int rdIndex, int rsIndex, int rtIndex)
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

            int orResult = rsValue | rtValue;

            int norResult = ~orResult;

            cpu.bancoDeRegistradores.SetValue(_rdIndex, norResult);

            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            string opcode = "000000";
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');
            string rdBinary = Convert.ToString(_rdIndex, 2).PadLeft(5, '0');
            string shamt = "00000"; 
            string funct = "100111";

            return $"{opcode}{rsBinary}{rtBinary}{rdBinary}{shamt}{funct}";
        }

        public override string ToHexString()
        {
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config) => config.RTypeCycles; 
    }
}