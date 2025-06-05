// Instructions/AndInstruction.cs
using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class AndInstruction : MipsInstruction
    {
        private int _rdIndex; // Registrador de destino
        private int _rsIndex; // Primeiro registrador operando
        private int _rtIndex; // Segundo registrador operando

        public AndInstruction(string assemblyLine, int address, int rdIndex, int rsIndex, int rtIndex)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.R;
            _rdIndex = rdIndex;
            _rsIndex = rsIndex;
            _rtIndex = rtIndex;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory) // Certifique-se de que é MemoryMips
        {
            // 1. Obter os valores dos registradores operando
            int rsValue = cpu.bancoDeRegistradores.GetValue(_rsIndex);
            int rtValue = cpu.bancoDeRegistradores.GetValue(_rtIndex);

            // 2. Realizar a operação AND lógica bit a bit
            int result = rsValue & rtValue; // O operador '&' em C# é o AND bit a bit

            // 3. Escrever o resultado no registrador de destino
            cpu.bancoDeRegistradores.SetValue(_rdIndex, result);

            // Retorna o próximo PC sequencial
            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            // Formato R-Type: opcode (6) | rs (5) | rt (5) | rd (5) | shamt (5) | funct (6)
            // Opcode para instruções R-Type é 000000 (0 em decimal)
            // Funct para AND é 100100 (36 em decimal)
            string opcode = "000000";
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');
            string rdBinary = Convert.ToString(_rdIndex, 2).PadLeft(5, '0');
            string shamt = "00000"; // Campo shamt não é usado para AND, então é 0.
            string funct = "100100";

            return $"{opcode}{rsBinary}{rtBinary}{rdBinary}{shamt}{funct}";
        }

        public override string ToHexString()
        {
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config) => config.RTypeCycles; // Instrução R-Type
    }
}