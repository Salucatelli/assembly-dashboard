using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly
{
    public class LuiInstruction : MipsInstruction
    {
        public int RtIndex { get; private set; }     
        public int Immediate { get; private set; }   

        private readonly byte _opcode; 

        public LuiInstruction(string assemblyLine, int address, int rtIndex, int immediate)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I;
            _opcode = 0x0F; 

            RtIndex = rtIndex;
            Immediate = immediate; 
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            uint value = (uint)(Immediate << 16); 
            cpu.bancoDeRegistradores.SetValue(RtIndex, (int)value);

            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            string opcodeBin = Convert.ToString(_opcode, 2).PadLeft(6, '0');
            string rsBin = Convert.ToString(0, 2).PadLeft(5, '0'); 
            string rtBin = Convert.ToString(RtIndex, 2).PadLeft(5, '0');
            string immediateBin = Convert.ToString(Immediate, 2).PadLeft(16, '0'); 

            return $"{opcodeBin}{rsBin}{rtBin}{immediateBin}";
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