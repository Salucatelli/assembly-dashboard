using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly 
{
    public class BeqInstruction : MipsInstruction
    {
        private int _rsIndex;   
        private int _rtIndex;   
        private int _offset;    

        public BeqInstruction(string assemblyLine, int address, int rsIndex, int rtIndex, int offset)
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

            int nextPc = cpu.pc + 4;


            if (rsValue == rtValue)
            {
                int branchTargetAddress = nextPc + (_offset * 4);

                return branchTargetAddress;
            }
            else
            {
                return nextPc;
            }
        }

        public override string ToBinaryString()
        {
            string opcode = "000100";
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');

            string offsetBinary = Convert.ToString((short)_offset, 2).PadLeft(16, (_offset < 0 ? '1' : '0'));

            return $"{opcode}{rsBinary}{rtBinary}{offsetBinary}";
        }

        public override string ToHexString()
        {
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config) => config.BranchCycles; 
    }
}