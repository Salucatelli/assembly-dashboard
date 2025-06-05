// Instructions/StoreHalfInstruction.cs
using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class StoreHalfInstruction : MipsInstruction
    {
        private int _rtIndex;     // Registrador fonte (rt)
        private int _baseRegIndex; // Registrador base para o endereço (rs)
        private short _offset;    // Offset (imediato de 16 bits com sinal)

        public StoreHalfInstruction(string assemblyLine, int address, int rtIndex, int baseRegIndex, short offset)
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
            // Apenas os 16 bits menos significativos são armazenados.
            int valueFromRt = cpu.bancoDeRegistradores.GetValue(_rtIndex);
            short valueToStore = (short)(valueFromRt & 0xFFFF); // Pega apenas os 16 bits menos significativos

            // 3. Armazenar a meia-palavra na memória de dados
            // A memória deve garantir que o endereço seja alinhado à meia-palavra (múltiplo de 2)
            dataMemory.WriteHalf(effectiveAddress, valueToStore);

            // Retorna o próximo PC sequencial
            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            // Formato I-Type: opcode (6) | rs (5) | rt (5) | immediate (16)
            // Opcode para SH é 101001 (41 em decimal)
            string opcode = "101001";
            string rsBinary = Convert.ToString(_baseRegIndex, 2).PadLeft(5, '0'); // Base register é o rs
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');     // Source register é o rt
            string immediateBinary = Convert.ToString(_offset, 2).PadLeft(16, _offset < 0 ? '1' : '0'); // Offset é o immediate

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