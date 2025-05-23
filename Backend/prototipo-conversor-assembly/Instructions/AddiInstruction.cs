// Em Instructions/AddiInstruction.cs
using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class AddiInstruction : MipsInstruction
    {
        private int _rtIndex; // Registrador destino/fonte
        private int _rsIndex; // Registrador fonte
        private short _immediate; // Imediato (16 bits, pode ser negativo)

        public AddiInstruction(string assemblyLine, int address, int rtIndex, int rsIndex, short immediate)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I;
            _rtIndex = rtIndex;
            _rsIndex = rsIndex;
            _immediate = immediate;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            int rsValue = cpu.bancoDeRegistradores.GetValue(_rsIndex);
            int result = rsValue + _immediate;
            cpu.bancoDeRegistradores.SetValue(_rtIndex, result);

            // Retorna o próximo PC sequencial
            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            // Formato I-Type: opcode (6) | rs (5) | rt (5) | immediate (16)
            // Opcode para addi é 001000 (8 em decimal)
            string opcode = "001000";
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');
            string immediateBinary = Convert.ToString(_immediate, 2).PadLeft(16, '0'); // Garante 16 bits com sinal

            return $"{opcode}{rsBinary}{rtBinary}{immediateBinary}";
        }

        public override string ToHexString()
        {
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config) => config.ITypeCycles;
    }
}