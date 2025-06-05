// Instructions/LoadByteInstruction.cs
using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class LoadByteInstruction : MipsInstruction
    {
        private int _rtIndex;     // Registrador destino (rt)
        private int _baseRegIndex; // Registrador base para o endereço (rs)
        private int _offset;    // Offset (agora usando int conforme sua diretriz)

        public LoadByteInstruction(string assemblyLine, int address, int rtIndex, int baseRegIndex, int offset)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I;
            _rtIndex = rtIndex;
            _baseRegIndex = baseRegIndex;
            _offset = offset; // Agora aceita int
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory) // Certifique-se de que é MemoryMips
        {
            // 1. Calcular o endereço efetivo na memória
            int baseAddress = cpu.bancoDeRegistradores.GetValue(_baseRegIndex);
            int effectiveAddress = baseAddress + _offset;

            // 2. Carregar o byte da memória de dados
            // Não há restrição de alinhamento para bytes, mas a memória deve ser acessível.
            byte loadedByte = dataMemory.ReadByte(effectiveAddress);

            // 3. Estender o sinal do byte para 32 bits (int)
            // Para extensão de sinal, convertemos para 'sbyte' (byte assinado) primeiro.
            // C# converte 'sbyte' para 'int' com extensão de sinal automaticamente.
            int result = (sbyte)loadedByte;

            // 4. Escrever o valor carregado no registrador de destino
            cpu.bancoDeRegistradores.SetValue(_rtIndex, result);

            // Retorna o próximo PC sequencial
            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            // Formato I-Type: opcode (6) | rs (5) | rt (5) | immediate (16)
            // Opcode para LB é 100000 (32 em decimal)
            string opcode = "100000";
            string rsBinary = Convert.ToString(_baseRegIndex, 2).PadLeft(5, '0'); // Base register é o rs
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');     // Destination register é o rt
            // Para o offset/imediato (16 bits), usamos o tipo 'short' para gerar a representação binária correta
            // com extensão de sinal, e então garantimos 16 bits.
            string immediateBinary = Convert.ToString((short)_offset, 2).PadLeft(16, (_offset < 0 ? '1' : '0'));

            return $"{opcode}{rsBinary}{rtBinary}{immediateBinary}";
        }

        public override string ToHexString()
        {
            // O erro anterior era aqui. Garanta que ToBinaryString retorna 32 bits.
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config) => config.ITypeCycles; // Ou um valor específico
    }
}