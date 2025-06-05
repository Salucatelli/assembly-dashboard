// Instructions/StoreByteInstruction.cs
using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class StoreByteInstruction : MipsInstruction
    {
        private int _rtIndex;     // Registrador fonte (rt)
        private int _baseRegIndex; // Registrador base para o endereço (rs)
        private int _offset;    // Offset (usando int conforme sua diretriz)

        public StoreByteInstruction(string assemblyLine, int address, int rtIndex, int baseRegIndex, int offset)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I;
            _rtIndex = rtIndex;
            _baseRegIndex = baseRegIndex;
            _offset = offset;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory) // Certifique-se de que é MemoryMips
        {
            // 1. Calcular o endereço efetivo na memória
            int baseAddress = cpu.bancoDeRegistradores.GetValue(_baseRegIndex);
            int effectiveAddress = baseAddress + _offset;

            // 2. Obter o valor do registrador fonte.
            // Apenas o byte (8 bits) menos significativo é armazenado.
            int valueFromRt = cpu.bancoDeRegistradores.GetValue(_rtIndex);
            byte valueToStore = (byte)(valueFromRt & 0xFF); // Pega apenas os 8 bits menos significativos

            // 3. Armazenar o byte na memória de dados
            // Não há restrição de alinhamento para bytes.
            dataMemory.WriteByte(effectiveAddress, valueToStore);

            // Retorna o próximo PC sequencial
            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            // Formato I-Type: opcode (6) | rs (5) | rt (5) | immediate (16)
            // Opcode para SB é 101000 (40 em decimal)
            string opcode = "101000";
            string rsBinary = Convert.ToString(_baseRegIndex, 2).PadLeft(5, '0'); // Base register é o rs
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');     // Source register (valor a ser armazenado) é o rt
            // Para o offset/imediato (16 bits), usamos o tipo 'short' para gerar a representação binária correta
            // com extensão de sinal, e então garantimos 16 bits.
            string immediateBinary = Convert.ToString((short)_offset, 2).PadLeft(16, (_offset < 0 ? '1' : '0'));

            return $"{opcode}{rsBinary}{rtBinary}{immediateBinary}";
        }

        public override string ToHexString()
        {
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config) => config.ITypeCycles; // Ou um valor específico
    }
}