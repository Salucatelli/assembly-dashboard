// Instructions/StoreWordInstruction.cs
using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly
{
    public class StoreWordInstruction : MipsInstruction
    {
        private int _rtIndex;     // Registrador fonte (ex: $s1)
        private int _baseRegIndex; // Registrador base (ex: $s2)
        private short _offset;    // Offset (ex: 20)

        public StoreWordInstruction(string assemblyLine, int address, int rtIndex, int baseRegIndex, short offset)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I;
            _rtIndex = rtIndex;
            _baseRegIndex = baseRegIndex;
            _offset = offset;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            // 1. Calcular o endereço efetivo na memória
            int baseAddress = cpu.bancoDeRegistradores.GetValue(_baseRegIndex); // Obtém o valor de $s2
            int effectiveAddress = baseAddress + _offset; // Calcula $s2 + 20

            // 2. Obter o valor do registrador fonte
            int valueToStore = cpu.bancoDeRegistradores.GetValue(_rtIndex); // Obtém o valor de $s1

            // 3. Armazenar a palavra na memória de dados
            dataMemory.WriteWord(effectiveAddress, valueToStore); // Escreve na Memória[$s2 + 20]

            // Retorna o próximo PC sequencial
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