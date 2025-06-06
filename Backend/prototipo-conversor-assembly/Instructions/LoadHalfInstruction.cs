using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly
{
    public class LoadHalfInstruction : MipsInstruction
    {
        private int _rtIndex;     
        private int _baseRegIndex; 
        private short _offset;    

        public LoadHalfInstruction(string assemblyLine, int address, int rtIndex, int baseRegIndex, short offset)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I;
            _rtIndex = rtIndex;
            _baseRegIndex = baseRegIndex;
            _offset = offset;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {

            int baseAddress = cpu.bancoDeRegistradores.GetValue(_baseRegIndex); 
            int effectiveAddress = baseAddress + _offset; 

            short loadedHalf = dataMemory.ReadHalf(effectiveAddress); 

            int result = loadedHalf; 

            cpu.bancoDeRegistradores.SetValue(_rtIndex, result); 

            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            string opcode = "100011";
            string rsBinary = Convert.ToString(_baseRegIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');
            string immediateBinary = Convert.ToString(_offset, 2).PadLeft(16, _offset < 0 ? '1' : '0');

            return $"{opcode}{rsBinary}{rtBinary}{immediateBinary}";
        }

        public override string ToHexString()
        {
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config)
        {
            return config.ITypeCycles; 
        }
    }
}