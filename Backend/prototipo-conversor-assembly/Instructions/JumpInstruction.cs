using System;
using prototipo_conversor_assembly.Bases; 

namespace prototipo_conversor_assembly
{
    public class JumpInstruction : MipsInstruction
    {
        private int _targetAddress;

        public JumpInstruction(string assemblyLine, int address, int targetAddress)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.J; 
            _targetAddress = targetAddress;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            return _targetAddress;
        }

        public override string ToBinaryString()
        {

            string opcodeBinary = Convert.ToString(0b000010, 2).PadLeft(6, '0'); 

            uint targetWordAddress = (uint)_targetAddress / 4;
            string targetBinary = Convert.ToString(targetWordAddress, 2).PadLeft(26, '0'); 


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