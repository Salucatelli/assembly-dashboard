using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly
{
    public class MulInstruction : MipsInstruction
    {
        public int RdIndex { get; private set; }
        public int RsIndex { get; private set; }
        public int RtIndex { get; private set; }

        private readonly byte _opcode;
        private readonly byte _function;

        public MulInstruction(string assemblyLine, int address, int rdIndex, int rsIndex, int rtIndex)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.R;

            _opcode = 0x1C; 
            _function = 0x2; 

            RdIndex = rdIndex;
            RsIndex = rsIndex;
            RtIndex = rtIndex;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            int rsValue = cpu.bancoDeRegistradores.GetValue(RsIndex);
            int rtValue = cpu.bancoDeRegistradores.GetValue(RtIndex);

            long result64 = (long)rsValue * rtValue;
            int result = (int)result64;

            cpu.bancoDeRegistradores.SetValue(RdIndex, result);

            return cpu.pc + 4; 
        }

        public override string ToBinaryString()
        {
            string opcodeBin = Convert.ToString(_opcode, 2).PadLeft(6, '0');
            string rsBin = Convert.ToString(RsIndex, 2).PadLeft(5, '0');
            string rtBin = Convert.ToString(RtIndex, 2).PadLeft(5, '0');
            string rdBin = Convert.ToString(RdIndex, 2).PadLeft(5, '0');
            string shamtBin = Convert.ToString(0, 2).PadLeft(5, '0'); 
            string funcBin = Convert.ToString(_function, 2).PadLeft(6, '0');

            return $"{opcodeBin}{rsBin}{rtBin}{rdBin}{shamtBin}{funcBin}";
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