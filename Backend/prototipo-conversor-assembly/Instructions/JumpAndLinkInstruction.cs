using System;
using prototipo_conversor_assembly.Bases; 

namespace prototipo_conversor_assembly
{
    public class JumpAndLinkInstruction : MipsInstruction
    {
        private int _targetAddress;

        private const int RA_REGISTER_INDEX = 31;

        public JumpAndLinkInstruction(string assemblyLine, int address, int targetAddress)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.J; 
            _targetAddress = targetAddress;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            int returnAddress = Address + 8;

            cpu.bancoDeRegistradores.SetValue(RA_REGISTER_INDEX, returnAddress);

            return _targetAddress;
        }

        public override string ToBinaryString()
        {

            string opcodeBinary = Convert.ToString(0b000011, 2).PadLeft(6, '0'); 

            uint adjustedTarget = ((uint)_targetAddress & 0x0FFFFFFF) >> 2; 
            string adjustedTargetBinary = Convert.ToString(adjustedTarget, 2).PadLeft(26, '0');

            return $"{opcodeBinary}{adjustedTargetBinary}";
        }

        public override string ToHexString()
        {
            uint binaryValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{binaryValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config)
        {
            return config.JTypeCycles;
        }
    }
}