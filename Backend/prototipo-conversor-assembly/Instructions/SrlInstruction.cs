// Instructions/SrlInstruction.cs
using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class SrlInstruction : MipsInstruction
    {
        private int _rdIndex;   // Registrador de destino
        private int _rtIndex;   // Registrador a ser deslocado
        private int _shamt;     // Valor do deslocamento (0-31, int para consistência)

        public SrlInstruction(string assemblyLine, int address, int rdIndex, int rtIndex, int shamt)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.R;
            _rdIndex = rdIndex;
            _rtIndex = rtIndex;
            _shamt = shamt; // O parser deve garantir que esteja entre 0 e 31
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            // 1. Obter o valor do registrador a ser deslocado
            int rtValue = cpu.bancoDeRegistradores.GetValue(_rtIndex);

            // 2. Realizar o deslocamento lógico para a direita
            // Em C#, o operador '>>' para 'int' é um deslocamento ARITMÉTICO (preserva o bit de sinal).
            // Para um deslocamento LÓGICO (preenche com zeros), precisamos converter para 'uint' (unsigned int) primeiro.
            uint unsignedRtValue = (uint)rtValue;
            uint result = unsignedRtValue >> _shamt; // Deslocamento lógico para uint

            // 3. Converter o resultado de volta para int (para armazenar no registrador)
            cpu.bancoDeRegistradores.SetValue(_rdIndex, (int)result);

            // Retorna o próximo PC sequencial
            return cpu.pc + 4;
        }

        public override string ToBinaryString()
        {
            // Formato R-Type: opcode (6) | rs (5) | rt (5) | rd (5) | shamt (5) | funct (6)
            // Opcode para SRL é 000000 (0 em decimal)
            // Funct para SRL é 000010 (2 em decimal)
            string opcode = "000000";
            string rsBinary = "00000"; // Campo RS é sempre 0 para SRL
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');
            string rdBinary = Convert.ToString(_rdIndex, 2).PadLeft(5, '0');

            // Shamt é um valor de 5 bits
            string shamtBinary = Convert.ToString(_shamt, 2).PadLeft(5, '0');

            string funct = "000010"; // Funct para SRL

            return $"{opcode}{rsBinary}{rtBinary}{rdBinary}{shamtBinary}{funct}";
        }

        public override string ToHexString()
        {
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config) => config.RTypeCycles;
    }
}