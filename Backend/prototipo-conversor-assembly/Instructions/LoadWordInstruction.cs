// Em Instructions/LoadWordInstruction.cs

using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly
{
    public class LoadWordInstruction : MipsInstruction
    {
        private int _rtIndex;
        private int _baseRegIndex;
        private short _offset;

        public LoadWordInstruction(string assemblyLine, int address, int rtIndex, int baseRegIndex, short offset)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I;
            _rtIndex = rtIndex;
            _baseRegIndex = baseRegIndex;
            _offset = offset;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory) // Assegure-se de que é MemoryMips
        {
            int baseAddress = cpu.bancoDeRegistradores.GetValue(_baseRegIndex);
            int effectiveAddress = baseAddress + _offset;
            int loadedValue = dataMemory.ReadWord(effectiveAddress);
            cpu.bancoDeRegistradores.SetValue(_rtIndex, loadedValue);
            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            // Formato I-Type: opcode (6) | rs (5) | rt (5) | immediate (16)
            // Opcode para LW é 100011 (35 em decimal)
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
            return config.ITypeCycles; // Usa o valor configurado para instruções tipo I
        }
    }
}