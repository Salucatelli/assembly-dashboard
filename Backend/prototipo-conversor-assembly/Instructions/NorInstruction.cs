// Instructions/NorInstruction.cs
using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class NorInstruction : MipsInstruction
    {
        private int _rdIndex; // Registrador de destino
        private int _rsIndex; // Primeiro registrador operando
        private int _rtIndex; // Segundo registrador operando

        public NorInstruction(string assemblyLine, int address, int rdIndex, int rsIndex, int rtIndex)
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

            // 2. Realizar a operação OR lógica bit a bit
            int orResult = rsValue | rtValue;

            // 3. Realizar a operação NOT (negação bit a bit) no resultado do OR
            // O operador '~' em C# é o NOT bit a bit
            int norResult = ~orResult;

            // 4. Escrever o resultado no registrador de destino
            cpu.bancoDeRegistradores.SetValue(_rdIndex, norResult);

            // Retorna o próximo PC sequencial
            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            // Formato R-Type: opcode (6) | rs (5) | rt (5) | rd (5) | shamt (5) | funct (6)
            // Opcode para instruções R-Type é 000000 (0 em decimal)
            // Funct para NOR é 100111 (39 em decimal)
            string opcode = "000000";
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');
            string rdBinary = Convert.ToString(_rdIndex, 2).PadLeft(5, '0');
            string shamt = "00000"; // Campo shamt não é usado para NOR, então é 0.
            string funct = "100111";

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